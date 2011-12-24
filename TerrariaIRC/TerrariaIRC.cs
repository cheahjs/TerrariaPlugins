using System;
using Hooks;
using Meebey.SmartIrc4net;
using TShockAPI;

namespace TerrariaIRC
{
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
        #endregion

        #region Plugin Vars
        public static IrcClient irc = new IrcClient();
        #endregion

        #region Plugin overrides
        public override void Initialize()
        {
            irc.Encoding = System.Text.Encoding.ASCII;
            irc.SendDelay = 300;
            irc.ActiveChannelSyncing = true;

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion
    }
}