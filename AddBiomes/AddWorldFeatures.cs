using System;
using System.Collections.Generic;
using Terraria;
using TerrariaAPI;

namespace AddWorldFeatures
{
    public class AddWorldFeatures : TerrariaMod
    {
        #region Vars

        private Dictionary<string, bool> ConfirmAction = new Dictionary<string, bool>();
        private Toolkit toolkit;

        #endregion

        public override string GetName()
        {
            return "Add World Features";
        }

        public override string GetVersion()
        {
            return "1.1";
        }

        public override void Initialize(TehModAPI api)
        {
            toolkit = Toolkit.GetToolkit();
            api.RegisterMenu(this, "Add World Features");
            api.AddOption(this, "Add Snow Biome", "addSnow", 2);
            api.AddOption(this, "Convert Dirt to Snow", "convertSnow", 2);
            api.AddOption(this, "Convert World to Snow", "convertWorldSnow", 2);
            api.AddOption(this, "Add Dungeon", "addDungeon", 2);
            api.AddOption(this, "Add Hell House", "addHell", 2);
            api.AddOption(this, "Add Floating Island", "addIsland", 2);
            api.AddOption(this, "Add Floating Island House", "addIslandHouse", 2);
            api.AddOption(this, "Add Mine House", "addMineHouse", 2);
            api.AddOption(this, "Start Hardmode", "startHm", 2);
            api.AddOption(this, "Start Hardmode (No Biome Changes)", "startHm2", 2);
            api.AddOption(this, "Stop Hardmode", "stopHm", 2);
            api.AddOption(this, "Convert Corruption to Hallow", "convertCorruption", 2);
            api.AddOption(this, "Convert Hallow to Corruption", "convertHallow", 2);
            api.AddOption(this, "Quit Without Saving World", "exitNoSave", 2);
        }

        public override void ModMenuAction(TehModAPI api, MenuOption opt)
        {
            if (!toolkit.IsInGame() || Main.netMode != 0) return;
            try
            {
                switch (opt.action)
                {
                    case "addSnow":
                        WriteSnow();
                        UpdateFrames();
                        break;
                    case "convertSnow":
                        bool doet;
                        if (ConfirmAction.TryGetValue("convertSnow", out doet))
                        {
                            ConfirmAction.Remove("convertSnow");
                            OverwriteSnow();
                            UpdateFrames();
                        }
                        else
                        {
                            Main.NewText("Please click the button again to confirm that you want to do this.", 255, 0, 0);
                            ConfirmAction.Add("convertSnow", false);
                        }
                        break;
                    case "convertWorldSnow":
                        if (ConfirmAction.TryGetValue("convertWorldSnow", out doet))
                        {
                            ConfirmAction.Remove("convertWorldSnow");
                            OverwriteSnowTotal();
                            UpdateFrames();
                        }
                        else
                        {
                            Main.NewText("Please click the button again to confirm that you want to do this.", 255, 0, 0);
                            ConfirmAction.Add("convertWorldSnow", false);
                        }
                        break;
                    case "exitNoSave":
                        QuitGame();
                        break;
                    case "addDungeon":
                        WorldGen.MakeDungeon((int) (toolkit.GetPlayer().position.X/16), (int) (toolkit.GetPlayer().position.Y/16));
                        UpdateFrames();
                        Main.NewText("Made a dungeon.");
                        break;
                    case "addHell":
                        WorldGen.HellHouse((int) (toolkit.GetPlayer().position.X/16), (int) (toolkit.GetPlayer().position.Y/16));
                        UpdateFrames();
                        Main.NewText("Made a hell house.");
                        break;
                    case "addIsland":
                        WorldGen.FloatingIsland((int) (toolkit.GetPlayer().position.X/16), (int) (toolkit.GetPlayer().position.Y/16));
                        UpdateFrames();
                        Main.NewText("Made a floating island.");
                        break;
                    case "addIslandHouse":
                        WorldGen.IslandHouse((int) (toolkit.GetPlayer().position.X/16), (int) (toolkit.GetPlayer().position.Y/16));
                        UpdateFrames();
                        Main.NewText("Made a floating island house.");
                        break;
                    case "addMineHouse":
                        WorldGen.MineHouse((int)(toolkit.GetPlayer().position.X / 16), (int)(toolkit.GetPlayer().position.Y / 16));
                        UpdateFrames();
                        Main.NewText("Made a mine house.");
                        break;
                    case "startHm":
                        WorldGen.StartHardmode();
                        break;
                    case "startHm2":
                        Main.hardMode = true;
                        Main.NewText("Enabled hardmode.");
                        break;
                    case "stopHm":
                        Main.hardMode = false;
                        Main.NewText("Disabled hardmode.");
                        break;
                    case "convertCorruption":
                        ConvertCorruption();
                        UpdateFrames();
                        break;
                    case "convertHallow":
                        ConvertHallow();
                        UpdateFrames();
                        break;
                }
            }
            catch (Exception e)
            {
                Main.NewText("ERROR: " + e.Message, 255, 0, 0);
            }
        }

        public void WriteSnow()
        {
            int xpos = WorldGen.genRand.Next(Main.maxTilesX);
            while (xpos < Main.maxTilesX*0.35f || xpos > Main.maxTilesX*0.65f)
                xpos = WorldGen.genRand.Next(Main.maxTilesX);

            int num34 = WorldGen.genRand.Next(35, 90);
            float num35 = (Main.maxTilesX/4200);
            num34 += (int) (WorldGen.genRand.Next(20, 40)*num35);
            num34 += (int) (WorldGen.genRand.Next(20, 40)*num35);
            int num36 = xpos - num34;
            num34 = WorldGen.genRand.Next(35, 90);
            num34 += (int) (WorldGen.genRand.Next(20, 40)*num35);
            num34 += (int) (WorldGen.genRand.Next(20, 40)*num35);
            int num37 = xpos + num34;
            if (num36 < 0)
                num36 = 0;
            if (num37 > Main.maxTilesX)
                num37 = Main.maxTilesX;
            int num38 = WorldGen.genRand.Next(50, 100);
            for (int num39 = num36; num39 < num37; num39++)
            {
                if (WorldGen.genRand.Next(2) == 0)
                {
                    num38 += WorldGen.genRand.Next(-1, 2);
                    if (num38 < 50)
                        num38 = 50;
                    if (num38 > 100)
                        num38 = 100;
                }
                int num40 = 0;
                while (num40 < Main.worldSurface)
                {
                    if (Main.tile[num39, num40].active)
                    {
                        int num41 = num38;
                        if (num39 - num36 < num41)
                            num41 = num39 - num36;
                        if (num37 - num39 < num41)
                            num41 = num37 - num39;
                        num41 += WorldGen.genRand.Next(5);
                        for (int num42 = num40; num42 < num40 + num41; num42++)
                            if (num39 > num36 + WorldGen.genRand.Next(5) && num39 < num37 - WorldGen.genRand.Next(5))
                                if (TileValidSnowBiome(Main.tile[num39, num42].type))
                                    Main.tile[num39, num42].type = 147;
                        break;
                    }
                    num40++;
                }
            }
            Main.NewText("Generated snow biome somewhere in the middle of the map", 255, 0, 255);
        }

        public void OverwriteSnow()
        {
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                    if (TileValidSnow(Main.tile[x, y].type))
                        Main.tile[x, y].type = 147;
            }
            Main.NewText("Overwritten all dirt and grass with snow.", 0, 255, 0);
        }

        public void OverwriteSnowTotal()
        {
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                    if (Main.tileSolid[Main.tile[x, y].type])
                        Main.tile[x, y].type = 147;
            }
            Main.NewText("Overwritten all solid tiles to snow.", 0, 255, 0);
        }

        private void ConvertCorruption()
        {
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    switch (Main.tile[x, y].type)
                    {
                        case 25:
                            Main.tile[x, y].type = 117;
                            break;
                        case 23:
                            Main.tile[x, y].type = 109;
                            break;
                        case 112:
                            Main.tile[x, y].type = 116;
                            break;
                        default:
                            continue;
                    }
                }
            }
            WorldGen.CountTiles(0);
            WorldGen.CountTiles(0);
            Main.NewText("Corruption converted to hallow.");
        }

        private void ConvertHallow()
        {
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    switch (Main.tile[x, y].type)
                    {
                        case 117:
                            Main.tile[x, y].type = 25;
                            break;
                        case 109:
                            Main.tile[x, y].type = 23;
                            break;
                        case 116:
                            Main.tile[x, y].type = 112;
                            break;
                        default:
                            continue;
                    }
                }
            }
            WorldGen.CountTiles(0);
            WorldGen.CountTiles(0);
            Main.NewText("Hallow converted to corruption.");
        }

        public bool TileValidSnowBiome(int type)
        {
            switch (type)
            {
                case 0:
                case 1:
                case 2:
                    return true;
                default:
                    return false;
            }
        }

        public bool TileValidSnow(int type)
        {
            switch (type)
            {
                case 0:
                case 2:
                case 23:
                case 109:
                    return true;
                default:
                    return false;
            }
        }

        public void QuitGame()
        {
            Main.menuMode = 10;
            Main.gameMenu = true;
            Player.SavePlayer(Main.player[Main.myPlayer], Main.playerPathName);
            Main.PlaySound(10, -1, -1, 1);
            Main.menuMode = 0;
        }

        public void UpdateFrames()
        {
        }
    }
}