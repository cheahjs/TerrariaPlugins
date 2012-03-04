using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Hooks;
using Meebey.SmartIrc4net;
using TShockAPI;
using Terraria;
using ErrorEventArgs = Meebey.SmartIrc4net.ErrorEventArgs;
using Group = TShockAPI.Group;

namespace TerrariaIRC
{
    [APIVersion(1, 11)]
    public class TerrariaIRC : TerrariaPlugin
    {
        #region Plugin Properties

        public TerrariaIRC(Main game) : base(game)
        {
            Order = 10;
        }

        public override string Name
        {
            get { return "TerrariaIRC"; }
        }

        public override string Author
        {
            get { return "Deathmax"; }
        }

        public override string Description
        {
            get { return "Provides an interface between IRC and Terraria"; }
        }

        public override Version Version
        {
            get { return new Version(1, 2, 0, 0); }
        }

        #endregion

        #region Plugin Vars

        public static IrcClient irc = new IrcClient();
        private static readonly Settings settings = new Settings();
        public static string settingsfiles = Path.Combine(TShock.SavePath, "irc", "settings.txt");
        public static bool StayConnected = true;
        public static Dictionary<string, Group> Users = new Dictionary<string, Group>();

        #endregion

        #region Plugin overrides

        public override void Initialize()
        {
            ServerHooks.Chat += OnChat;
            ServerHooks.Join += OnJoin;
            ServerHooks.Leave += OnLeave;
            irc.Encoding = Encoding.ASCII;
            irc.SendDelay = 300;
            irc.ActiveChannelSyncing = true;
            irc.AutoRejoinOnKick = true;
            irc.OnQueryMessage += OnQueryMessage;
            irc.OnError += OnError;
            irc.OnChannelMessage += OnChannelMessage;
            irc.OnRawMessage += OnRawMessage;
            irc.OnNickChange += OnNickChange;
            if (!settings.Load())
            {
                Console.WriteLine("Settings failed to load, aborting IRC connection.");
                return;
            }
            new Thread(Connect).Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            StayConnected = false;
            if (irc.IsConnected)
                irc.Disconnect();
        }

        #endregion

        #region IRC methods

        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            string message = e.Data.Message;
            if (IsOpAllowed(e.Data.Nick))
            {
                if (message.StartsWith("!"))
                {
                    var user = new IRCPlayer(e.Data.Nick) {Group = new SuperAdminGroup()};
                    TShockAPI.Commands.HandleCommand(user, "/" + message.TrimStart('!'));
                    foreach (string t in user.Output)
                        irc.RfcPrivmsg(e.Data.Nick, t);
                }
            }
            else if (IsUserAllowed(e.Data.Nick))
            {
                if (message.StartsWith("!"))
                {
                    var user = new IRCPlayer(e.Data.Nick) {Group = new Group(Users[e.Data.Nick].Name)};
                    TShockAPI.Commands.HandleCommand(user, "/" + message.TrimStart('!'));
                    foreach (string t in user.Output)
                        irc.RfcPrivmsg(e.Data.Nick, t);
                }
            }
            else if (message.ToLower().StartsWith("login"))
            {
            }
            else
                irc.RfcPrivmsg(e.Data.Nick, "You are not allowed to perform that action.");
        }

        public static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("IRC Error: {0}", e.Data.RawMessage);
        }

        private static void OnChannelMessage(object sender, IrcEventArgs e)
        {
            string message = e.Data.Message;
            if (message.ToLower() == "!players")
            {
                string reply = TShock.Players.Where(player => player != null).Where(player => player.RealPlayer).Aggregate("", (current, player) => current + (current == "" ? player.Name : ", " + player.Name));
                irc.SendMessage(SendType.Message, settings["channel"], "Current Players: " + reply);
            }
            else
                TShock.Utils.Broadcast(string.Format("(IRC)<{0}> {1}", e.Data.Nick,
                    TShock.Utils.SanitizeString(Regex.Replace(message, (char) 3 + "[0-9]{1,2}(,[0-9]{1,2})?", String.Empty))), Color.Green);
        }

        private static void OnRawMessage(object sender, IrcEventArgs e)
        {
            Debug.Write(e.Data.RawMessage);
        }

        private static void OnNickChange(object sender, NickChangeEventArgs e)
        {
            if (!Users.ContainsKey(e.OldNickname)) return;
            Users.Add(e.NewNickname, Users[e.OldNickname]);
            Users.Remove(e.OldNickname);
        }

        public static void Connect()
        {
            while (StayConnected)
            {
                Console.WriteLine("Connecting to {0}:{1}...", settings["server"], settings["port"]);
                try
                {
                    irc.Connect(settings["server"], int.Parse(settings["port"]));
                    irc.ListenOnce();
                    Console.WriteLine("Connected to IRC server.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error connecting to IRC server.");
                    Console.WriteLine(e);
                    return;
                }
                irc.Login(settings["botname"], "TerrariaIRC");
                irc.ListenOnce();
                Console.WriteLine("Trying to join {0}...", settings["channel"]);
                irc.RfcJoin(settings["channel"]);
                irc.ListenOnce();
                Console.WriteLine("Joined {0}", settings["channel"]);
                if (settings.ContainsKey("nickserv") && settings.ContainsKey("password"))
                {
                    irc.RfcPrivmsg(settings["nickserv"], settings["password"]);
                    irc.ListenOnce();
                }
                irc.Listen();
                Console.WriteLine("Disconnected from IRC... Attempting to reconnect");
            }
        }

        public static bool IsOpAllowed(string nick)
        {
            //Theres an issue with IsOp, if the ircd has anything higher like +a, IsOp will return false;
            if (bool.Parse(settings["allowop"]))
                return (from user in (from DictionaryEntry channeluser in irc.GetChannel(settings["channel"]).Users select (ChannelUser) channeluser.Value) where user.Nick == nick select user.IsOp).FirstOrDefault();
            return false;
        }

        public static bool IsUserAllowed(string nick)
        {
            return Users.ContainsKey(nick);
        }

        public static bool CompareIrcUser(IrcUser user1, IrcUser user2)
        {
            return (user1.Host == user2.Host && user1.Ident == user2.Ident && user1.Realname == user2.Realname);
        }

        #endregion

        #region Plugin hooks

        private void OnChat(messageBuffer msg, int player, string text, HandledEventArgs e)
        {
            if (!irc.IsConnected) return;
            TSPlayer tsplr = TShock.Players[msg.whoAmI];
            if (tsplr == null)
                return;
            if (!TShock.Utils.ValidString(text))
                return;
            if (text.StartsWith("/"))
                return;
            if (tsplr.mute)
                return;
            irc.SendMessage(SendType.Message, settings["channel"], string.Format("({0}){1}: {2}",
                tsplr.Group.Name, tsplr.Name, text));
        }

        private void OnJoin(int player, HandledEventArgs e)
        {
            if (!irc.IsConnected) return;
            if (e.Handled) return;
            irc.SendMessage(SendType.Message, settings["channel"], string.Format("{0} joined the server.",
                Main.player[player].name));
        }

        private void OnLeave(int player)
        {
            if (!irc.IsConnected) return;
            irc.SendMessage(SendType.Message, settings["channel"], string.Format("{0} left the server.",
                Main.player[player].name));
        }

        #endregion
    }
}