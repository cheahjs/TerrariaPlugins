using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace TerrariaIRC
{
    class Commands
    {
        public static void Init()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("manageirc", Reconnect, "reconnectirc"));
        }

        public static void Reconnect(CommandArgs args)
        {
            TerrariaIRC.irc.Disconnect();
            TShock.Utils.SendLogs("Disconnected from IRC, reconnecting.", Color.Red);
        }
    }
}
