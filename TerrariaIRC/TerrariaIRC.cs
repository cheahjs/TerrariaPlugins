using System;
using System.IO;
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
        {}
        #endregion

        #region Plugin Vars
        public static IrcClient irc = new IrcClient();
        private static Settings settings = new Settings();
        public static string settingsfiles = Path.Combine(TShock.SavePath, "irc", "settings.txt");
        #endregion

        #region Plugin overrides
        public override void Initialize()
        {
            irc.Encoding = System.Text.Encoding.ASCII;
            irc.SendDelay = 300;
            irc.ActiveChannelSyncing = true;
            irc.OnQueryMessage += OnQueryMessage;
            irc.OnError += OnError;
            irc.OnRawMessage += OnRawMessage;
            if (!settings.Load())
            {
                Console.WriteLine("Settings failed to load, aborting IRC.");
                return;
            }
            Connect();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion

        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            Console.WriteLine("Query: {0}", e.Data.RawMessage);
        }

        public static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error: {0}", e.Data.RawMessage);
        }

        public static void OnRawMessage(object sender, IrcEventArgs e)
        {
            Console.WriteLine("Raw: {0}", e.Data.RawMessage);
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
    }
}