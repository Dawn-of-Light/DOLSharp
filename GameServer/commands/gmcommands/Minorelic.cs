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
using DOL.GS.PacketHandler;
using DOL.GS;
using DOL.Database2;
using DOL.GS.Utils;
using System.Collections;

namespace DOL.GS.Commands
{
    [Cmd(
     "&minorelic",
     ePrivLevel.GM,
     "Various Commands for Minotaur Relics",
     "'/Minorelic create <Name> <Model> <Group/Self/Realm> <SpellID> <Effect / 159 = red, 160 = white, 161 = gold>' creates a new Relic with the given arguments",
     "'/Minorelic movehere' moves the relic to player x, y, z and saves the coordinates as Spawn",
     "'/Minorelic name <Name>' changes the name",
     "'/Minorelic model <Model>' changes the Model",
     "'/Minorelic effect <Effect / 159 = red, 160 = white, 161 = gold>' changes the effect the player gets when he picks up the relic",
     "'/Minorelic info' browses relic Informations",
     "'/Minorelic despawn' despawns the Relic",
     "'/Minorelic remove' removes the relic from the World and Database",
     "'/Minorelic showall' shows all Relics",
     "'/Minorelic spawn <ID>' spawns the Relic with the ID")]

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
                        GameServer.Database.AddNewObject(relic);

                        MinotaurRelic rrelic = new MinotaurRelic(relic);
                        rrelic.AddToWorld();

                        MinotaurRelicManager.AddRelic(rrelic);

                        return;
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

                        return;
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

                        return;
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

                        return;
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
                            relic.RelicSpell = Convert.ToInt32(args[2]);
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        relic.SaveIntoDatabase();

                        return;
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

                        return;
                    }
                case "info":
					{
                        if (!(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

						ArrayList info = new ArrayList();
                        info.Add("===========================");
                        info.Add("Relic Informations");
                        info.Add("===========================");
                        info.Add("- Name: " + relic.Name);
                        info.Add("- ID: " + relic.RelicID);
                        info.Add("- Current Relic XP: " + relic.XP);
                        info.Add("- Level: " + relic.Level);
                        info.Add("- Effect: " + relic.Effect);
                        info.Add("===========================");
                        info.Add("Position Informations");
                        info.Add("===========================");
                        info.Add("- Spawn X: " + relic.X);
                        info.Add("- Spawn Y: " + relic.Y);
                        info.Add("- Spawn Z: " + relic.SpawnZ);
                        info.Add("- Spawnheading: " + relic.SpawnHeading);
                        info.Add("- Spawnregion: " + relic.SpawnRegion);
                        info.Add("===========================");
                        info.Add("Spell Informations");
                        info.Add("===========================");
						info.Add("- Spell (ID): " + relic.RelicSpell);
                        info.Add("- Spell Target: " + relic.RelicTarget);

                        Spell spell = SkillBase.GetSpellByID(relic.RelicSpell);
                        if (spell != null)
                        {
                            info.Add("- Spellname: " + spell.Name);
                            info.Add("- Spelltype: " + spell.SpellType);
                            info.Add("- Spellduration: " + spell.Duration / 1000 + " seconds");
                        }

						client.Out.SendCustomTextWindow("[ " + relic.Name + " ]", info);
					}
					break;
                case "despawn":
                    {
                        if (!(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;

                        relic.XP = 0;
                        relic.RemoveFromWorld();
                        relic.RelicDispose();
                    }
                    break;
                case "remove":
                    {
                        if (!(client.Player.TargetObject is MinotaurRelic))
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        MinotaurRelic relic = client.Player.TargetObject as MinotaurRelic;
                        
                        relic.RemoveFromWorld();
                        DatabaseObject obj = GameServer.Database.SelectObject(typeof(DBMinotaurRelic), "RelicID",relic.RelicID);
                        if (obj != null) GameServer.Database.DeleteObject(obj);
                    }
                    break;
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
                            relic.XP += Convert.ToInt32(args[2]);
                            if (relic.Owner != null)
                                relic.Owner.Out.SendMinotaurRelicBarUpdate(relic.Owner, (int)relic.XP);
                        }
                        catch (Exception)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        return;
                    }
                case "showall":
                    {
                        ArrayList info = new ArrayList();

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

                            foreach (MinotaurRelic relic in MinotaurRelicManager.m_minotaurrelics.Values)
                            {
                                if (relic != null && relic.CurrentRegionID == region)
                                {
                                    info.Add(relic.ToString());
                                    info.Add("===========================");
                                }
                            }

                            info.Add("Reliccount: " + info.Count);

                            client.Out.SendCustomTextWindow("Relic Infos", info);

                            return;
                        }

                        foreach (MinotaurRelic relic in MinotaurRelicManager.m_minotaurrelics.Values)
                        {
                            if (relic != null)
                            {
                                info.Add(relic.ToString());
                                info.Add("===========================");
                            }
                        }

                        info.Add("Reliccount: " + MinotaurRelicManager.m_minotaurrelics.Count);

                        client.Out.SendCustomTextWindow("Relic Infos", info);

                        return;
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

                        if (relic.respawntimer == null)
                        {
                            DisplayMessage(client, "Relic already spawned", new object[] { });
                            return;
                        }

                        relic.ManualRespawn();
                        
                        return;
                    }
            }

            return;
        }
    }
}
