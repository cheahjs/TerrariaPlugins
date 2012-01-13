using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Threading;
using Terraria;
using TerrariaAPI;

namespace PortableTerraria
{
    public class PortableTerraria : TerrariaMod
    {
        public override string GetName()
        {
            return "Portable Terraria";
        }

        public override string GetVersion()
        {
            return "v1";
        }

        public override void Initialize(TehModAPI api)
        {
            if (CheckPirate())
            {
                Console.WriteLine("Pirated copy detected, exiting Terraria.");
                Thread.Sleep(3000);
                Environment.Exit(6666);
            }
            Main.SavePath = Path.Combine(Environment.CurrentDirectory, "TerrariaData");
            Main.WorldPath = Path.Combine(Main.SavePath, "Worlds");
            Main.PlayerPath = Path.Combine(Main.SavePath, "Players");
        }

        public static bool CheckPirate()
        {
            if (File.Exists("TDU.exe"))
                return true;
            if (CheckSteamAPI())
                return true;
            if (Steam.SteamAPI_Init() && !IsSteamRunning())
                return true;
            return false;
        }

        public static bool IsSteamRunning()
        {
            var proc = Process.GetProcessesByName("Steam.exe");
            return proc.Length > 0;
        }

        public static bool CheckSteamAPI()
        {
            var files = (new DirectoryInfo(Environment.CurrentDirectory)).GetFiles("steam_api*");
            return files.Any(file => new SoapHexBinary(MD5.Create().ComputeHash(File.ReadAllBytes(file.FullName))).ToString().ToLower() == "fbeb939ec32ffa442a59b164f396d197");
        }
    }
}