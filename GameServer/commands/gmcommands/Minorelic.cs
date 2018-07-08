/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Language;

namespace DOL.GS.Commands
{
    [Cmd(
     "&minorelic",
     ePrivLevel.GM,
     "GMCommands.MinoRelic.Description",
     "GMCommands.MinoRelic.Usage.Create",
     "GMCommands.MinoRelic.Usage.MoveHere",
     "GMCommands.MinoRelic.Usage.Name",
     "GMCommands.MinoRelic.Usage.Spell",
     "GMCommands.MinoRelic.Usage.Model",
     "GMCommands.MinoRelic.Usage.Effect",
     "GMCommands.MinoRelic.Usage.Info",
     "GMCommands.MinoRelic.Usage.DeSpawn",
     "GMCommands.MinoRelic.Usage.Remove",
     "GMCommands.MinoRelic.Usage.XP",
     "GMCommands.MinoRelic.Usage.ShowAll",
     "GMCommands.MinoRelic.Usage.Spawn")]
    public class MinoRelicCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            switch (args[1].ToLower())
            {
                case "create":
                    {
                        if (args.Length != 7 || (!args[4].ToLower().Equals("group") && !args[4].ToLower().Equals("self") && !args[4].ToLower().Equals("realm")))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        DBMinotaurRelic relic = new DBMinotaurRelic();

                        relic.Name = args[2];

                        relic.SpawnHeading = client.Player.Heading;
                        relic.SpawnX = client.Player.X;
                        relic.SpawnY = client.Player.Y;
                        relic.SpawnZ = client.Player.Z;
                        relic.SpawnRegion = client.Player.CurrentRegionID;

                        relic.relicTarget = args[4].ToLower();

                        try
                        {
                            relic.relicSpell = Convert.ToInt32(args[5]);
                            relic.Model = Convert.ToUInt16(args[3]);
                            relic.Effect = Convert.ToInt32(args[6]);
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        relic.RelicID = MinotaurRelicManager.GetRelicCount() + 1;
                        GameServer.Database.AddObject(relic);

                        MinotaurRelic rrelic = new MinotaurRelic(relic);
                        rrelic.AddToWorld();

                        MinotaurRelicManager.AddRelic(rrelic);

                        break;
                    }

                case "movehere":
                    {
                        if (!(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        relic.Heading = client.Player.Heading;
                        relic.X = client.Player.X;
                        relic.Y = client.Player.Y;
                        relic.Z = client.Player.Z;
                        relic.CurrentRegionID = client.Player.CurrentRegionID;

                        relic.SpawnHeading = client.Player.Heading;
                        relic.SpawnX = client.Player.X;
                        relic.SpawnY = client.Player.Y;
                        relic.SpawnZ = client.Player.Z;
                        relic.SpawnRegion = client.Player.CurrentRegionID;

                        relic.SaveIntoDatabase();

                        break;
                    }

                case "model":
                    {
                        if (args.Length != 3 || !(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        try
                        {
                            relic.Model = Convert.ToUInt16(args[2]);
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        relic.SaveIntoDatabase();

                        break;
                    }

                case "name":
                    {
                        if (args.Length != 3 || !(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        relic.Name = args[2];

                        relic.SaveIntoDatabase();

                        break;
                    }

                case "spell":
                    {
                        if (args.Length != 3 || !(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        try
                        {
                            relic.RelicSpellId = Convert.ToInt32(args[2]);
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        relic.SaveIntoDatabase();

                        break;
                    }

                case "effect":
                    {
                        if (args.Length != 3 || !(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        try
                        {
                            relic.Effect = Convert.ToInt32(args[2]);
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        relic.SaveIntoDatabase();

                        break;
                    }

                case "info":
                    {
                        if (!(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        var info = new List<string>();
                        info.Add("===========================");
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.RelicInfo"));
                        info.Add("===========================");
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.Name", relic.Name));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.ID", relic.RelicId));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.CurrentXP", relic.Xp));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.Level", relic.Level));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.Effect", relic.Effect));
                        info.Add("===========================");
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.PositionInfo"));
                        info.Add("===========================");
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpawnX", relic.SpawnX));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpawnY", relic.SpawnX));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpawnZ", relic.SpawnZ));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpawnHeading" + relic.SpawnHeading));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpawnRegion", relic.SpawnRegion));
                        info.Add("===========================");
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpellInfo"));
                        info.Add("===========================");
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpellID", relic.RelicSpell));
                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpellTarget", relic.RelicTarget));

                        Spell spell = SkillBase.GetSpellByID(relic.RelicSpellId);
                        if (spell != null)
                        {
                            info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpellName", spell.Name));
                            info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpellType", spell.SpellType));
                            info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Info.SpellDuration", spell.Duration / 1000));
                        }

                        client.Out.SendCustomTextWindow("[ " + relic.Name + " ]", info);
                        break;
                    }

                case "despawn":
                    {
                        if (!(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        relic.Xp = 0;
                        relic.RemoveFromWorld();
                        relic.RelicDispose();
                        break;
                    }

                case "remove":
                    {
                        // Lifeflight: Added the option to remove a minorelic by supplying an ID.
                        if (args.Length == 3)
                        {
                            int minorelicID = 0;
                            try
                            {
                                minorelicID = Convert.ToInt32(args[2]);
                            }
                            catch (Exception)
                            {
                            }

                            if (minorelicID == 0)
                            {
                                DisplaySyntax(client);
                                return;
                            }
                            else
                            {

                                foreach (MinotaurRelic relic in MinotaurRelicManager.Minotaurrelics.Values)
                                {
                                    if (relic != null)
                                    {
                                        if (relic.RelicId == minorelicID)
                                        {
                                            // there is a match!
                                            // remove it from the world
                                            relic.RemoveFromWorld();
                                            client.Player.Out.SendMessage("Relic " + relic.RelicId + " has been removed from the world", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                            // remove it from the hashtable
                                            MinotaurRelicManager.RemoveRelic(relic);
                                            client.Player.Out.SendMessage("Relic " + relic.RelicId + " has been removed from the Minorelic Hash Table", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                                            DataObject obj = GameServer.Database.FindObjectByKey<DBMinotaurRelic>(relic.RelicId);
                                            if (obj != null)
                                            {
                                                GameServer.Database.DeleteObject(obj);
                                                client.Player.Out.SendMessage("Relic " + relic.RelicId + " has been removed from the database!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!(client.Player.TargetObject is MinotaurRelic))
                            {
                                DisplaySyntax(client);
                                return;
                            }

                            MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                            relic.RemoveFromWorld();
                            client.Player.Out.SendMessage("Relic " + relic.RelicId + " has been removed from the world", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            // remove it from the hashtable
                            MinotaurRelicManager.RemoveRelic(relic);
                            client.Player.Out.SendMessage("Relic " + relic.RelicId + " has been removed from the Minorelic Hash Table", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                            DataObject obj = GameServer.Database.FindObjectByKey<DBMinotaurRelic>(relic.RelicId);
                            if (obj != null)
                            {
                                GameServer.Database.DeleteObject(obj);
                                client.Player.Out.SendMessage("Relic " + relic.RelicId + " has been removed from the database!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            }
                        }

                        break;
                    }

                case "xp":
                    {
                        if (args.Length != 3 || !(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        try
                        {
                            relic.Xp += Convert.ToInt32(args[2]);
                            if (relic.Owner != null)
                            {
                                relic.Owner.Out.SendMinotaurRelicBarUpdate(relic.Owner, (int)relic.Xp);
                            }
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        break;
                    }

                case "showall":
                    {
                        var info = new List<string>();

                        if (args.Length > 2)
                        {
                            ushort region = 0;
                            try
                            {
                                region = Convert.ToUInt16(args[2]);
                            }
                            catch (Exception)
                            {
                                return;
                            }

                            foreach (MinotaurRelic relic in MinotaurRelicManager.Minotaurrelics.Values)
                            {
                                if (relic != null && relic.CurrentRegionID == region)
                                {
                                    info.Add(relic.ToString());
                                    info.Add("===========================");
                                }
                            }

                            info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.ShowAll.Count", info.Count));

                            client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.ShowAll.Infos"), info);

                            return;
                        }

                        foreach (MinotaurRelic relic in MinotaurRelicManager.Minotaurrelics.Values)
                        {
                            if (relic != null)
                            {
                                info.Add(relic.ToString());
                                info.Add("===========================");
                            }
                        }

                        info.Add(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.ShowAll.Count", MinotaurRelicManager.Minotaurrelics.Count));

                        client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.ShowAll.Infos"), info);

                        break;
                    }

                case "spawn":
                    {
                        if (args.Length != 3)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = MinotaurRelicManager.GetRelic(Convert.ToInt32(args[2]));

                        if (relic == null)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (relic.Respawntimer == null)
                        {
                            DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.MinoRelic.Spawn.AlreadySpawned"));
                            return;
                        }

                        relic.ManualRespawn();

                        break;
                    }
            }
        }
    }
}
