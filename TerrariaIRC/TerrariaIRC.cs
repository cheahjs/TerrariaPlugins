using System;
using System.IO;
using System.Threading;
using Hooks;
using Meebey.SmartIrc4net;
using TShockAPI;
using Terraria;
using ErrorEventArgs = Meebey.SmartIrc4net.ErrorEventArgs;

namespace TerrariaIRC
{
    [APIVersion(1, 10)]
    public class TerrariaIRC : TerrariaPlugin
    {
        #region Plugin Properties
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
            get { return new Version(1, 0, 0, 0); }
        }
        public TerrariaIRC(Main game) : base(game)
        {
            Order = 10;
        }
        #endregion

        #region Plugin Vars
        public static IrcClient irc = new IrcClient();
        private static Settings settings = new Settings();
        public static string settingsfiles = Path.Combine(TShock.SavePath, "irc", "settings.txt");
        #endregion

        #region Plugin overrides
        public override void Initialize()
        {
            ServerHooks.Chat += OnChat;
            TShock.Players = new TSPlayer[Main.maxNetPlayers + 1];
            TShock.Players[Main.maxNetPlayers] = new IRCPlayer {Group = new SuperAdminGroup()};
            irc.Encoding = System.Text.Encoding.ASCII;
            irc.SendDelay = 300;
            irc.ActiveChannelSyncing = true;
            irc.OnQueryMessage += OnQueryMessage;
            irc.OnError += OnError;
            irc.OnChannelMessage += OnChannelMessage;
            if (!settings.Load())
            {
                Console.WriteLine("Settings failed to load, aborting IRC.");
                return;
            }
            new Thread(Connect).Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion

        #region IRC methods
        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
        }

        public static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("IRC Error: {0}", e.Data.RawMessage);
        }

        void OnChannelMessage(object sender, IrcEventArgs e)
        {
            TShock.Utils.Broadcast(string.Format("(IRC)<{0}> {1}", e.Data.Nick, TShock.Utils.SanitizeString(e.Data.Message)));
        }

        public void Connect()
        {
            while (true)
            {
                Console.WriteLine("Connecting to {0}:{1}...", settings["server"], settings["port"]);
                try
                {
                    irc.Connect(settings["server"], int.Parse(settings["port"]));
                    Console.WriteLine("Connected to IRC server.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error connecting to IRC server.");
                    Console.WriteLine(e);
                    return;
                }
                irc.Login(settings["botname"], "TerrariaIRC");
                irc.RfcJoin(settings["channel"]);
                Console.WriteLine("Joined {0}", settings["channel"]);
                irc.Listen();
                Console.WriteLine("Disconnected from IRC... Attempting to reconnect");
            }
        }
        #endregion

        #region Plugin hooks
        void OnChat(messageBuffer msg, int player, string text, System.ComponentModel.HandledEventArgs e)
        {
            var tsplr = TShock.Players[msg.whoAmI];
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
        #endregion
    }
}