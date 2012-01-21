using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Meebey.SmartIrc4net;

namespace TerrariaIRC
{
    class IRCPlayer : TSPlayer
    {
        public List<string> Output = new List<string>();

        public IRCPlayer(string player) : base(player)
        {
        }

        public override void SendMessage(string msg, byte red, byte green, byte blue)
        {
            Output.Add(msg);
        }
    }
}
