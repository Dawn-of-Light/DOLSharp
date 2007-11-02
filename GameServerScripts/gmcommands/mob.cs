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
using System.Collections;
using System.Reflection;
using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;
using DOL.GS.Quests;
using DOL.GS.Housing;
using DOL.GS.Movement;

namespace DOL.GS.Scripts
{
	[Cmd("&mob", //command to handle
		(uint)ePrivLevel.GM, //minimum privelege level
		"Various mob creation commands!", //command description
		//Usage
		"'/mob nfastcreate <Model> <Realm> <Level> <Number> <Radius> <Name>' to create n mob with fixed level,model and name placed around creator within the provided radius",
		"'/mob nrandcreate <Number> <Radius>' to create <Number> mob in <Radius> around player",
		"'/mob fastcreate <Model> <Level> <Name>' to create mob with fixed level,model",
		"'/mob create <type> <realm>' to create an empty mob",
		"'/mob model <newMobModel>' to set the mob model to newMobModel",
		"'/mob size <newMobSize>' to set the mob size to newMobSize",
		"'/mob name <newMobName>' to set the mob name to newMobName",
		"'/mob guild <newMobGuild>' to set the mob guildname to newMobGuild",
		"'/mob aggro <level>' set mob aggro level 0..100%",
		"'/mob range <distance>' set mob aggro range",
		"'/mob distance <maxdistance>' set mob max distance from its spawn (>0=real, 0=no check, <0=procent)",
		"'/mob damagetype <eDamageType>' set mob damage type",
		"'/mob movehere'",
		"'/mob remove' to remove this mob from the DB",
		"'/mob ghost' makes this mob ghostlike",
		"'/mob transparent' makes this mob transparent",
		"'/mob fly' makes this mob able to fly by changing the Z coordinate",
		"'/mob noname' still possible to target this mob but removes the name from above mob",
		"'/mob notarget' makes it impossible to target this mob and removes the name from above it",
		"'/mob kill' Kills the mob without removing it from the DB",
		"'/mob regen' regenerates the mob health to maximum ",
		"'/mob info' amplified informations output about the mob",
		"'/mob realm' changing the mob's realm",
		"'/mob speed' changing the mob's max speed",
		"'/mob level' changing the mob's level",
		"'/mob brain' changing the mob's brain",
		"'/mob respawn <newDuration>' changing the time between each mob respawn",
		"'/mob questinfo' to show mob quests infos",
		"'/mob equipinfo' to show mob inventory infos",
		"'/mob equiptemplate create' to create an empty inventory template",
		"'/mob equiptemplate add <slot> <model> [color] [effect]' to add an item to this mob inventory template",
		"'/mob equiptemplate remove <slot>' to remove item from the specified slot in this mob inventory template",
		"'/mob equiptemplate clear' to remove the inventory template from mob",
		"'/mob equiptemplate close' to finish the inventory template you are creating",
		"'/mob equiptemplate save <templateID> [replace]' to save the inventory template with a new name",
		"'/mob equiptemplate load <templateID>' to load the inventory template from the database, it is open for modifications after",
		"'/mob addloot <ItemTemplateID> [chance]' to add loot to the mob's unique drop table",
		"'/mob viewloot' to view the selected mob's loot table",
		"'/mob removeloot <ItemTemplateID>' to remove loot from the mob's unique drop table",
		"'/mob copy' copies a mob exactly and places it at your location",
		"'/mob npctemplate <NPCTemplateID>' creates a mob with npc template, or modifies target",
		"'/mob path <PathID>' associate the mob to the specified path",
        "'/mob <stat> <ammount>' Set the mobs stats (str, con, etc)",
		"'/mob tether <tether range>' set mob tether range (>0=check, 0=no check, <0=no check)"
		)]
	public class MobCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}
			string param = "";
			if (args.Length > 2)
				param = String.Join(" ", args, 2, args.Length - 2);

			GameNPC targetMob = null;
			if (client.Player.TargetObject != null && client.Player.TargetObject is GameNPC)
				targetMob = (GameNPC)client.Player.TargetObject;

			if (args[1] != "create"
				&& args[1] != "fastcreate"
				&& args[1] != "nrandcreate"
				&& args[1] != "nfastcreate"
				&& args[1] != "npctemplate"
				&& targetMob == null)
			{
				if (client.Player.TargetObject != null)
				{
					client.Out.SendMessage("Cannot use " + client.Player.TargetObject + " for /mob command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch (args[1])
			{
				case "nrandcreate":
					{
						ushort number = 1;
						ushort radius = 10;
						if (args.Length < 4)
						{
							client.Out.SendMessage("Usage: /mob nrandcreate <Number> <Radius>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						try
						{
							radius = Convert.ToUInt16(args[3]);
							number = Convert.ToUInt16(args[2]);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Usage: /mob  nrandcreate <Number> <Radius>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						for (int i = 0; i < number; ++i)
						{
							GameNPC mob = new GameNPC();
							//Fill the object variables
							int x = client.Player.X + DOL.GS.Util.Random(-radius, radius);
							int y = client.Player.Y + DOL.GS.Util.Random(-radius, radius);
							mob.X = FastMath.Abs(x);
							mob.Y = FastMath.Abs(y);
							mob.Z = client.Player.Z;
							mob.CurrentRegion = client.Player.CurrentRegion;
							mob.Heading = client.Player.Heading;
							mob.Level = (byte)DOL.GS.Util.Random(10, 50);
							mob.Realm = (byte)DOL.GS.Util.Random(1, 3);
							mob.Name = "rand_" + i;
							mob.Model = (byte)DOL.GS.Util.Random(100, 200);

							//Fill the living variables
							if (mob.Brain is IAggressiveBrain)
							{
								((IAggressiveBrain)mob.Brain).AggroLevel = 100;
								((IAggressiveBrain)mob.Brain).AggroRange = 1500;
							}
							mob.CurrentSpeed = 0;
							mob.MaxSpeedBase = 200;
							mob.GuildName = "";
							mob.Size = 50;
							mob.AddToWorld();
						}
						client.Out.SendMessage("created " + number + " mobs", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "nfastcreate":
					{
						byte level = 1;
						ushort model = 408;
						byte realm = 0;
						ushort number = 1;
						string name = "New Mob";
						ushort radius = 10;
						if (args.Length < 8)
						{
							client.Out.SendMessage("Usage: /mob nfastcreate <Model> <Realm> <Level> <Number> <Radius> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						try
						{
							name = String.Join(" ", args, 7, args.Length - 7);
							radius = Convert.ToUInt16(args[6]);
							number = Convert.ToUInt16(args[5]);
							level = Convert.ToByte(args[4]);
							realm = Convert.ToByte(args[3]);
							model = Convert.ToUInt16(args[2]);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Usage: /mob nfastcreate <Model> <Level> <Number> <Radius> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						for (int i = 0; i < number; ++i)
						{
							GameNPC mob = new GameNPC();

							//Fill the object variables
							int x = client.Player.X + DOL.GS.Util.Random(-radius, radius);
							int y = client.Player.Y + DOL.GS.Util.Random(-radius, radius);
							mob.X = FastMath.Abs(x);
							mob.Y = FastMath.Abs(y);
							mob.Z = client.Player.Z;
							mob.CurrentRegion = client.Player.CurrentRegion;
							mob.Heading = client.Player.Heading;
							mob.Level = level;
							mob.Realm = realm;
							mob.Name = name;
							mob.Model = model;

							//Fill the living variables
							if (mob.Brain is IAggressiveBrain)
							{
								((IAggressiveBrain)mob.Brain).AggroLevel = 100;
								((IAggressiveBrain)mob.Brain).AggroRange = 1500;
							}
							mob.CurrentSpeed = 0;
							mob.MaxSpeedBase = 200;
							mob.GuildName = "";
							mob.Size = 50;
							mob.AddToWorld();
							client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						break;
					}
				case "fastcreate":
					{
						byte level = 1;
						ushort model = 408;
						string name = "New Mob";

						if (args.Length < 5)
						{
							client.Out.SendMessage("Usage: /mob fastcreate <Model> <Level> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						try
						{
							name = String.Join(" ", args, 4, args.Length - 4);
							level = Convert.ToByte(args[3]);
							model = Convert.ToUInt16(args[2]);
						}
						catch
						{
							client.Out.SendMessage("Usage: /mob fastcreate <Model> <Level> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						name = CheckName(name, client);
						GameNPC mob = new GameNPC();
						//Fill the object variables
						mob.X = client.Player.X;
						mob.Y = client.Player.Y;
						mob.Z = client.Player.Z;
						mob.CurrentRegion = client.Player.CurrentRegion;
						mob.Heading = client.Player.Heading;
						mob.Level = level;
						mob.Realm = 0;
						mob.Name = name;
						mob.Model = model;
						//Fill the living variables
						mob.CurrentSpeed = 0;
						mob.MaxSpeedBase = 200;
						mob.GuildName = "";
						mob.Size = 50;
						mob.AddToWorld();
						client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "info":
					{
						ArrayList info = new ArrayList();
						info.Add(" + Class: " + targetMob.GetType().ToString());
						info.Add(" + Realm: " + targetMob.Realm);
						info.Add(" + Level: " + targetMob.Level);
						info.Add(" + Brain: " + (targetMob.Brain == null ? "(null)" : targetMob.Brain.GetType().ToString()));
						IAggressiveBrain aggroBrain = targetMob.Brain as IAggressiveBrain;
						if (aggroBrain != null)
						{
							info.Add(" + Aggro level: " + aggroBrain.AggroLevel);
							info.Add(" + Aggro range: " + aggroBrain.AggroRange);
							if ( targetMob.MaxDistance < 0 )
								info.Add(" + MaxDistance: " + -targetMob.MaxDistance*aggroBrain.AggroRange/100);
							else
								info.Add(" + MaxDistance: " + targetMob.MaxDistance);
								
						}
						else
						{
							info.Add(" + Not aggressive brain");
						}
						//info.Add(" + Tether Range: " + targetMob.TetherRange);
						TimeSpan respawn = TimeSpan.FromMilliseconds(targetMob.RespawnInterval);
						if (targetMob.RespawnInterval <= 0)
							info.Add(" + Respawn: NPC will not respawn");
						else info.Add(" + Respawn: " + respawn.Minutes + " minutes " + respawn.Seconds + " seconds (Position: X=" + targetMob.SpawnX + " Y=" + targetMob.SpawnY + " Z=" + targetMob.SpawnZ + ")");

                        info.Add(" ");
                        info.Add(" + Mob Stats:");
                        info.Add(" + Constitution: " + targetMob.Constitution);
                        info.Add(" + Dexterity: " + targetMob.Dexterity);
                        info.Add(" + Strength: " + targetMob.Strength);
                        info.Add(" + Quickness: " + targetMob.Quickness);
                        info.Add(" + Intelligence: " + targetMob.Intelligence);
                        info.Add(" + Piety: " + targetMob.Piety);
                        info.Add(" + Empathy: " + targetMob.Empathy);
                        info.Add(" + Charisma: " + targetMob.Charisma);
                        info.Add(" ");

                        info.Add(" + Damage type: " + targetMob.MeleeDamageType);
						info.Add(" + Position: X=" + targetMob.X + " Y=" + targetMob.Y + " Z=" + targetMob.Z);
						info.Add(" + Guild: " + targetMob.GuildName);
						info.Add(" + Model: " + targetMob.Model + " sized to " + targetMob.Size);
						info.Add(string.Format(" + Flags: {0} (0x{1})", ((GameNPC.eFlags)targetMob.Flags).ToString("G"), targetMob.Flags.ToString("X")));
						info.Add(" + Active weapon slot: " + targetMob.ActiveWeaponSlot);
						info.Add(" + Visible weapon slot: " + targetMob.VisibleActiveWeaponSlots);
						info.Add(" + Speed(current/max): " + targetMob.CurrentSpeed + "/" + targetMob.MaxSpeedBase);
						info.Add(" + Health: " + targetMob.Health + "/" + targetMob.MaxHealth);
						info.Add(" + Equipment Template ID: " + targetMob.EquipmentTemplateID);
						info.Add(" + Inventory: " + targetMob.Inventory);
						info.Add(" + CanGiveQuest: " + targetMob.QuestListToGive.Count);
						info.Add(" + Path: " + targetMob.PathID);
                        if (targetMob.BoatOwnerID != null)
                        {
                            info.Add(" + Boat OwnerID: " + targetMob.BoatOwnerID);
                        }

						client.Out.SendCustomTextWindow("[ " + targetMob.Name + " ]", info);
					}
					break;
				case "stats":
					{
						if (targetMob == null)
						{
							client.Out.SendMessage("No target selected.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						ArrayList info = new ArrayList();
						info.Add("Modified stats:");
						info.Add("");
						for (eProperty property = eProperty.Stat_First; property <= eProperty.Stat_Last; ++property)
							info.Add(String.Format("{0}: {1}",
								GlobalConstants.PropertyToName(property),
								targetMob.GetModified(property)));
						info.Add("");
						info.Add("Modified resists:");
						info.Add("");
						for (eProperty property = eProperty.Resist_First + 1; property <= eProperty.Resist_Last; ++property)
							info.Add(String.Format("{0}: {1}",
								GlobalConstants.PropertyToName(property),
								targetMob.GetModified(property)));
						info.Add("");
						info.Add("Miscellaneous:");
						info.Add("");
						info.Add(String.Format("Maximum Health: {0}", targetMob.MaxHealth));
						info.Add(String.Format("Armor Factor (AF): {0}", targetMob.GetModified(eProperty.ArmorFactor)));
						info.Add(String.Format("Absorption (ABS): {0}", targetMob.GetModified(eProperty.ArmorAbsorbtion)));
						client.Out.SendCustomTextWindow("[ " + targetMob.Name + " ]", info);
						return 1;
					}
				case "level":
					{
						byte level;
						try
						{
							level = Convert.ToByte(args[2]);
							targetMob.Level = level;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob level changed to: " + targetMob.Level, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				#region TEST : CurrentHouse
				case "house":
					{
						int house;
						House H;
						try
						{
							house = Convert.ToInt32(args[2]);
							H = HouseMgr.GetHouse(house);

							if (H != null)
							{
								targetMob.HouseNumber = house;
								targetMob.CurrentHouse = H;
								targetMob.SaveIntoDatabase();
								client.Out.SendMessage("Mob house changed to: " + house, eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
								client.Out.SendMessage("House number "+house+" doesn't exists !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				#endregion
				case "realm":
					{
						byte realm;
						try
						{
							realm = Convert.ToByte(args[2]);
							targetMob.Realm = realm;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob realm changed to: " + targetMob.Realm, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				case "speed":
					{
						int maxSpeed;
						try
						{
							maxSpeed = Convert.ToUInt16(args[2]);
							targetMob.MaxSpeedBase = maxSpeed;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob MaxSpeed changed to: " + targetMob.MaxSpeedBase, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
                case "str":
                    {
                        short strength;
                        try
                        {
                            strength = Convert.ToInt16(args[2]);
                            targetMob.Strength = strength;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Strength changed to: " + targetMob.Strength, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "con":
                    {
                        short constitution;
                        try
                        {
                            constitution = Convert.ToInt16(args[2]);
                            targetMob.Constitution = constitution;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Constitution changed to: " + targetMob.Constitution, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "dex":
                    {
                        short dexterity;
                        try
                        {
                            dexterity = Convert.ToInt16(args[2]);
                            targetMob.Dexterity = dexterity;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Dexterity changed to: " + targetMob.Dexterity, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "qui":
                    {
                        short quickness;
                        try
                        {
                            quickness = Convert.ToInt16(args[2]);
                            targetMob.Quickness = quickness;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Quickness changed to: " + targetMob.Quickness, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "pie":
                    {
                        short piety;
                        try
                        {
                            piety = Convert.ToInt16(args[2]);
                            targetMob.Piety = piety;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Piety changed to: " + targetMob.Piety, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "cha":
                    {
                        short charisma;
                        try
                        {
                            charisma = Convert.ToInt16(args[2]);
                            targetMob.Charisma = charisma;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Charisma changed to: " + targetMob.Charisma, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "int":
                    {
                        short intelligence;
                        try
                        {
                            intelligence = Convert.ToInt16(args[2]);
                            targetMob.Intelligence = intelligence;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Intelligence changed to: " + targetMob.Intelligence, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "emp":
                    {
                        short empathy;
                        try
                        {
                            empathy = Convert.ToInt16(args[2]);
                            targetMob.Empathy = empathy;
                            targetMob.SaveIntoDatabase();
                            client.Out.SendMessage("Mob Empathy changed to: " + targetMob.Empathy, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        catch (Exception)
                        {
                            client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return 1;
                        }
                    }
                    break;
                case "regen":
					{
						try
						{
							targetMob.Health = targetMob.MaxHealth;
							targetMob.SaveIntoDatabase();
							client.Out.SendObjectUpdate(targetMob);
							client.Out.SendMessage("Mob health regenerated (" + targetMob.Health + "/" + targetMob.MaxHealth + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				case "kill":
					{
						try
						{
							targetMob.AddAttacker(client.Player);
							targetMob.AddXPGainer(client.Player, targetMob.Health);
							targetMob.Die(client.Player);
							targetMob.XPGainers.Clear();
							client.Out.SendMessage("Mob '" + targetMob.Name + "' killed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
				case "create":
					{
						string theType = "DOL.GS.GameNPC";
						byte realm = 0;
						if (args.Length > 2)
							theType = args[2];
						if (args.Length > 3)
							realm = Convert.ToByte(args[3]); 

						//Create a new mob
						GameNPC mob = null;
						try
						{
							client.Out.SendDebugMessage(Assembly.GetAssembly(typeof(GameServer)).FullName);
							mob = (GameNPC)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(theType, false);
						}
						catch (Exception e)
						{
							client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
						if (mob == null)
						{
							try
							{
								client.Out.SendDebugMessage(Assembly.GetExecutingAssembly().FullName);
								mob = (GameNPC)Assembly.GetExecutingAssembly().CreateInstance(theType, false);
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
						if (mob == null)
						{
							client.Out.SendMessage("There was an error creating an instance of " + theType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;
						}
						//Fill the object variables
						mob.X = client.Player.X;
						mob.Y = client.Player.Y;
						mob.Z = client.Player.Z;
						mob.CurrentRegion = client.Player.CurrentRegion;
						mob.Heading = client.Player.Heading;
						mob.Level = 1;
						mob.Realm = realm;
						mob.Name = "New Mob";
						mob.Model = 408;
						//Fill the living variables
						mob.CurrentSpeed = 0;
						mob.MaxSpeedBase = 200;
						mob.GuildName = "";
						mob.Size = 50;
						mob.Flags |= (uint)GameNPC.eFlags.PEACE;
						mob.AddToWorld();
						mob.SaveIntoDatabase();
						client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage("The mob has been created with the peace flag, so it can't be attacked, to remove type /mob peace", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
					break;
				case "model":
					{
						ushort model;
						try
						{
							model = Convert.ToUInt16(args[2]);
							targetMob.Model = model;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob model changed to: " + targetMob.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "respawn":
					{
						int interval;
						try
						{
							interval = Convert.ToInt32(args[2]);
							targetMob.RespawnInterval = interval;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob respawn interval changed to: " + interval, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "size":
					{
						ushort size;
						try
						{
							size = Convert.ToUInt16(args[2]);
							if (targetMob == null || size > 255)
							{
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 1;
							}

							targetMob.Size = (byte)size;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob size changed to: " + targetMob.Size, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "aggro":
					{
						try
						{
							IAggressiveBrain aggroBrain = targetMob.Brain as IAggressiveBrain;
							if (aggroBrain != null)
							{
								int aggro = int.Parse(args[2]);
								client.Out.SendMessage("Mob aggro changed to: " + aggro + " (was " + aggroBrain.AggroLevel + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								aggroBrain.AggroLevel = aggro;
								targetMob.SaveIntoDatabase();
							}
							else
							{
								DisplayMessage(client, "Not aggressive brain.");
							}
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "range":
					{
						try
						{
							IAggressiveBrain aggroBrain = targetMob.Brain as IAggressiveBrain;
							if (aggroBrain != null)
							{
								int range = int.Parse(args[2]);
								DisplayMessage(client, "Mob aggro range changed to: {0} (was {1})", range, aggroBrain.AggroRange);
								aggroBrain.AggroRange = range;
								targetMob.SaveIntoDatabase();
							}
							else
							{
								DisplayMessage(client, "Not aggressive brain.");
							}
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
							return 1;
						}
					}
					break;

				case "distance":
					{
						try
						{
							int distance = Convert.ToInt32(args[2]);
							targetMob.MaxDistance = distance;
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob max distance changed to: " + distance, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

					//Disabled tether for now
				/*case "tether":
					{
						try
						{
							int tether = Convert.ToInt32(args[2]);
							targetMob.TetherRange = tether;
							//targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob tether range changed to: " + tether, eChatType.CT_System, eChatLoc.CL_SystemWindow);
							client.Out.SendMessage("Keep in mind that this setting is volatile, it needs to be set in this NPC's template to become permanent.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;*/

				case "damagetype":
					{
						try
						{
							eDamageType damage = (eDamageType)Enum.Parse(typeof(eDamageType), args[2], true);
							DisplayMessage(client, "Mob damage type changed to: {0} (was {1})", damage, targetMob.MeleeDamageType);
							targetMob.MeleeDamageType = damage;
							targetMob.SaveIntoDatabase();
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
						}
					}
					break;

				case "name":
					{
						if (param != "" && targetMob != null)
						{
							targetMob.Name = CheckName(param, client);
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob name changed to: " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					break;

				case "brain":
					{
						try
						{
							ABrain brain = null;
							string brainType = args[2];
							try
							{
								client.Out.SendDebugMessage(Assembly.GetAssembly(typeof(GameServer)).FullName);
								brain = (ABrain)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(brainType, false);
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							if (brain == null)
							{
								try
								{
									client.Out.SendDebugMessage(Assembly.GetExecutingAssembly().FullName);
									brain = (ABrain)Assembly.GetExecutingAssembly().CreateInstance(brainType, false);
								}
								catch (Exception e)
								{
									client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								}
							}
							if (brain == null)
							{
								client.Out.SendMessage("There was an error creating an instance of " + brainType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							targetMob.SetOwnBrain(brain);
							targetMob.SaveIntoDatabase();
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
							return 1;
						}
					}
					break;
				
				case "path":
					{
						try
						{
							string pathname = String.Join(" ", args, 2, args.Length - 2);
							if (MovementMgr.LoadPath(pathname) == null)
								client.Out.SendMessage("The specified path does not exist", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							else
							{
								targetMob.PathID = pathname;
								targetMob.SaveIntoDatabase();
								if (targetMob.Brain.Stop())
									targetMob.Brain.Start();
								client.Out.SendMessage("The path has been assigned to this mob", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
							return 1;
						}
					}
					break;

				case "guild":
					{
						if (param != "")
						{
							targetMob.GuildName = CheckGuildName(param, client);
							targetMob.SaveIntoDatabase();
							client.Out.SendMessage("Mob guild changed to: " + targetMob.GuildName, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							if (targetMob.GuildName != "")
							{
								targetMob.GuildName = "";
								targetMob.SaveIntoDatabase();
								client.Out.SendMessage("Mob guild removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					break;

				case "movehere":
					{
						targetMob.MoveTo(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading);
						targetMob.SaveIntoDatabase();
						client.Out.SendMessage("Target Mob '" + targetMob.Name + "' moved to your location!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;

				case "attack":
					foreach (GamePlayer player in targetMob.GetPlayersInRadius(3000))
					{
						if (player.Name == args[2])
						{
							targetMob.StartAttack(player);
							break;
						}
					}
					break;

				case "remove":
					{
						targetMob.StopAttack();
						targetMob.StopCurrentSpellcast();
						targetMob.DeleteFromDatabase();
						targetMob.Delete();
						client.Out.SendMessage("Target Mob removed from DB!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;

				case "questinfo":
					{
						if (targetMob.QuestListToGive.Count == 0)
						{
							client.Out.SendMessage("Mob not have any quests!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						foreach (AbstractQuest quest in targetMob.QuestListToGive)
							client.Out.SendMessage("Quest Name: [" + quest.Name + "]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					break;

				case "equipinfo":
					{
						if (targetMob.Inventory == null)
						{
							client.Out.SendMessage("Mob inventory not found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						string closed = "";
						if (targetMob.Inventory is GameNpcInventoryTemplate)
						{
							GameNpcInventoryTemplate t = (GameNpcInventoryTemplate)targetMob.Inventory;
							closed = t.IsClosed ? " (closed)" : " (open)";
						}
						string message = string.Format("			         Inventory: {0}{1}, equip template ID: {2}", targetMob.Inventory.GetType().ToString(), closed, targetMob.EquipmentTemplateID);
						client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						foreach (InventoryItem item in targetMob.Inventory.AllItems)
						{
							client.Out.SendMessage("Slot Description : [" + GlobalConstants.SlotToName(item.SlotPosition) + "]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("         Slot: " + GlobalConstants.SlotToName(item.Item_Type), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Model: " + item.Model, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Color: " + item.Color, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("       Effect: " + item.Effect, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
					}
					break;

				case "equiptemplate":
					{
						if (args.Length < 3)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						if (args[2].ToLower() == "create")
						{
							try
							{
								if (targetMob.Inventory != null)
								{
									client.Out.SendMessage("Target mob inventory is set to " + targetMob.Inventory.GetType() + ", remove it first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 1;
								}

								targetMob.Inventory = new GameNpcInventoryTemplate();

								client.Out.SendMessage("Inventory template created.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							catch
							{
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 1;
							}
							break;
						}
						else if (args[2].ToLower() == "load")
						{
							if (args.Length > 3)
							{
								if (targetMob.Inventory != null && !(targetMob.Inventory is GameNpcInventoryTemplate))
								{
									client.Out.SendMessage("Target mob is not using GameNpcInventoryTemplate.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 1;
								}
								try
								{
									GameNpcInventoryTemplate load = new GameNpcInventoryTemplate();
									if (!load.LoadFromDatabase(args[3]))
									{
										client.Out.SendMessage("Error loading equipment template \"" + args[2] + "\"", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
									targetMob.EquipmentTemplateID = args[3];
									targetMob.Inventory = load;
									targetMob.SaveIntoDatabase();
									targetMob.UpdateNPCEquipmentAppearance();
									client.Out.SendMessage("Mob equipment loaded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								catch
								{
									client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 1;
								}
							}
							else
							{
								client.Out.SendMessage("Usage: /mob equiptemplate load <templateID>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}

						GameNpcInventoryTemplate template = targetMob.Inventory as GameNpcInventoryTemplate;
						if (template == null)
						{
							client.Out.SendMessage("Target mob is not using GameNpcInventoryTemplate.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						switch (args[2])
						{
							case "add":
								{
									if (args.Length >= 5)
									{
										try
										{
											int slot = GlobalConstants.NameToSlot(args[3]);
											if (slot == 0)
											{
												client.Out.SendMessage("No such slot available!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}

											int model = Convert.ToInt32(args[4]);
											int color = 0;
											int effect = 0;

											if (args.Length >= 6)
												color = Convert.ToInt32(args[5]);
											if (args.Length >= 7)
												effect = Convert.ToInt32(args[6]);


											if (!template.AddNPCEquipment((eInventorySlot)slot, model, color, effect))
											{
												client.Out.SendMessage("Couldn't add new item to slot " + slot + ". Template could be closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}

											client.Out.SendMessage("Item added to the mob inventory template!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										}
										catch
										{
											client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
									}
									else
									{
										client.Out.SendMessage("Usage: /mob equiptemplate add <slot> <model> [color] [effect]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
								}
								break;

							case "remove":
								{
									if (args.Length > 3)
									{
										try
										{
											int slot = GlobalConstants.NameToSlot(args[3]);
											if (slot == 0)
											{
												client.Out.SendMessage("No such slot available!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}

											if (!template.RemoveNPCEquipment((eInventorySlot)slot))
											{
												client.Out.SendMessage("Couldn't remove item from slot " + slot + ". Template could be closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}
											client.Out.SendMessage("Mob inventory template slot " + slot + " cleaned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										}
										catch
										{
											client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
									}
									else
									{
										client.Out.SendMessage("Usage: /mob equiptemplate remove <slot>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
								}
								break;

							case "close":
								{
									if (template.IsClosed)
									{
										client.Out.SendMessage("Template is already closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
									targetMob.Inventory = template.CloseTemplate();
									client.Out.SendMessage("Inventory template closed succesfully.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								break;

							case "clear":
								{
									targetMob.Inventory = null;
									targetMob.EquipmentTemplateID = null;
									targetMob.SaveIntoDatabase();
									client.Out.SendMessage("Mob equipment removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								break;

							case "save":
								{
									if (args.Length > 3)
									{
										bool replace = (args.Length > 4 && args[4].ToLower() == "replace") ? true : false;
										if (!replace && null != GameServer.Database.SelectObject(typeof(NPCEquipment), "TemplateID = '" + GameServer.Database.Escape(args[3]) + "'"))
										{
											client.Out.SendMessage("Template with name '" + args[3] + "' already exists. Use 'replace' flag if you want to overwrite it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
										if (!targetMob.Inventory.SaveIntoDatabase(args[3]))
										{
											client.Out.SendMessage("Error saving template with name " + args[3], eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
										targetMob.EquipmentTemplateID = args[3];
										targetMob.SaveIntoDatabase();
										client.Out.SendMessage("Target mob equipment template is saved as '" + args[3] + "'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
									else
									{
										client.Out.SendMessage("Usage: /mob equiptemplate save <templateID> [replace]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}
								}
								break;

							default:
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								break;
						}

						targetMob.UpdateNPCEquipmentAppearance();
					}
					break;
				case "transparent":
					{
						targetMob.Flags ^= (uint)GameNPC.eFlags.TRANSPARENT;
						targetMob.SaveIntoDatabase();
						client.Out.SendMessage("Mob TRANSPARENT flag is set to " + ((targetMob.Flags & (uint)GameNPC.eFlags.TRANSPARENT) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "peace":
					{
						targetMob.Flags ^= (uint)GameNPC.eFlags.PEACE;
						targetMob.SaveIntoDatabase();
						client.Out.SendMessage("Mob PEACE flag is set to " + ((targetMob.Flags & (uint)GameNPC.eFlags.PEACE) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "fly":
					{
						targetMob.Flags ^= (uint)GameNPC.eFlags.FLYING;
						targetMob.SaveIntoDatabase();
						client.Out.SendMessage("Mob FLYING flag is set to " + ((targetMob.Flags & (uint)GameNPC.eFlags.FLYING) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "noname":
					{
						targetMob.Flags ^= (uint)GameNPC.eFlags.DONTSHOWNAME;
						targetMob.SaveIntoDatabase();
						client.Out.SendMessage("Mob DONTSHOWNAME flag is set to " + ((targetMob.Flags & (uint)GameNPC.eFlags.DONTSHOWNAME) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "notarget":
					{
						targetMob.Flags ^= (uint)GameNPC.eFlags.CANTTARGET;
						targetMob.SaveIntoDatabase();
						client.Out.SendMessage("Mob CANTTARGET flag is set to " + ((targetMob.Flags & (uint)GameNPC.eFlags.CANTTARGET) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "addloot":
					{
						try
						{
							string lootTemplateID = args[2];
							int chance = Convert.ToInt16(args[3]);
							string name = targetMob.Name;

							DataObject[] template = GameServer.Database.SelectObjects(typeof(DBLootTemplate), "TemplateName = '" + GameServer.Database.Escape(name) + "' AND ItemTemplateID = '" + GameServer.Database.Escape(lootTemplateID) + "'");
							if (template != null)
							{
								foreach (DataObject loot in template)
								{
									GameServer.Database.DeleteObject(loot);
								}
								DBLootTemplate lt = new DBLootTemplate();
								lt.Chance = chance;
								lt.TemplateName = name;
								lt.ItemTemplateID = lootTemplateID;

								GameServer.Database.AddNewObject(lt);
							}
							else
							{
								ItemTemplate itemtemplate = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), lootTemplateID);
								if (itemtemplate == null)
								{
									DisplayError(client, "ItemTemplate " + lootTemplateID + " not found!");
									return 0;
								}

								DBLootTemplate lt = new DBLootTemplate();
								lt.Chance = chance;
								lt.TemplateName = name;
								lt.ItemTemplateID = lootTemplateID;

								GameServer.Database.AddNewObject(lt);
							}


							client.Out.SendMessage("LootTemplate " + lootTemplateID + " Successfully Added to " + name + " with a " + chance + "% chance to drop",
								eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch
						{
							client.Out.SendMessage("An Error occured, Please make sure you have a mob targeted.",
								eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					break;
				case "removeloot":
					{
						string lootTemplateID = args[2];
						string name = targetMob.Name;
						if (lootTemplateID.ToLower().ToString() == "all")
						{
							DataObject[] template = GameServer.Database.SelectObjects(typeof(DBLootTemplate), "TemplateName = '" + GameServer.Database.Escape(name) + "'");
							if (template != null)
							{
								foreach (DataObject loot in template)
								{
									GameServer.Database.DeleteObject(loot);
								}
								client.Out.SendMessage("Removed all items from " + targetMob.Name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								client.Out.SendMessage("No items found on " + targetMob.Name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						else
						{
							DataObject[] template = GameServer.Database.SelectObjects(typeof(DBLootTemplate), "TemplateName = '" + GameServer.Database.Escape(name) + "' AND ItemTemplateID = '" + GameServer.Database.Escape(lootTemplateID) + "'");
							if (template != null)
							{
								foreach (DataObject loot in template)
								{
									GameServer.Database.DeleteObject(loot);
								}
								client.Out.SendMessage(lootTemplateID + " removed from " + targetMob.Name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								client.Out.SendMessage(lootTemplateID + " does not exist on " + name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
					}
					break;
				case "viewloot":
					{
						DataObject[] template = GameServer.Database.SelectObjects(typeof(DBLootTemplate), "TemplateName = '" + GameServer.Database.Escape(targetMob.Name) + "'");
						string message = "[ " + targetMob.Name + "'s Loot Table ]\n\n";

						foreach (DBLootTemplate loot in template)
						{
							if (loot.ItemTemplate == null)
								message += loot.ItemTemplateID + " (Template Not Found)";
							else message += loot.ItemTemplate.Name + " (" + loot.ItemTemplate.Id_nb + ")";
							message += " Chance: " + loot.Chance.ToString() + "\n\n";
						}
						client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					break;
				case "copy":
					{
						//Create a new mob
						GameNPC mob = null;

						foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							mob = (GameNPC)assembly.CreateInstance(targetMob.GetType().FullName, true);
							if (mob != null)
								break;
						}

						if (mob == null)
						{
							client.Out.SendMessage("There was an error creating an instance of " + targetMob.GetType().FullName + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;
						}

						//Fill the object variables
						mob.X = client.Player.X;
						mob.Y = client.Player.Y;
						mob.Z = client.Player.Z;
						mob.CurrentRegion = client.Player.CurrentRegion;
						mob.Heading = client.Player.Heading;
						mob.Level = targetMob.Level;
						mob.Realm = targetMob.Realm;
						mob.Name = targetMob.Name;
						mob.Model = targetMob.Model;
						mob.Flags = targetMob.Flags;
						mob.MeleeDamageType = targetMob.MeleeDamageType;
						mob.RespawnInterval = targetMob.RespawnInterval;
						//Fill the living variables
						mob.CurrentSpeed = 0;
						mob.MaxSpeedBase = targetMob.MaxSpeedBase;
						mob.GuildName = targetMob.GuildName;
						mob.Size = targetMob.Size;
						mob.Inventory = targetMob.Inventory;
						mob.NPCTemplate = targetMob.NPCTemplate;
						ABrain brain = null;
						foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							brain = (ABrain)assembly.CreateInstance(targetMob.Brain.GetType().FullName, true);
							if (brain != null)
								break;
						}
						if (brain == null)
						{
							client.Out.SendMessage("Cannot create brain, standard brain being applied", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							mob.SetOwnBrain(new StandardMobBrain());
						}
						else mob.SetOwnBrain(brain);
						mob.AddToWorld();
						mob.SaveIntoDatabase();
						client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
                case "npctemplate":
					{
						if (args.Length < 3)
						{
							DisplayError(client, "Usage: /mob npctemplate <id>", new object[] { });
							break;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							DisplayError(client, args[2] + " does not seem to be a number", new object[] { });
							break;
						}
						INpcTemplate template = NpcTemplateMgr.GetTemplate(id);
						if (template == null)
						{
							DisplayError(client, "no template found for " + id, new object[] { });
							break;
						}
						if (targetMob == null)
						{
							GameNPC mob = new GameNPC(template);
							mob.X = client.Player.X;
							mob.Y = client.Player.Y;
							mob.Z = client.Player.Z;
							mob.Heading = client.Player.Heading;
							mob.CurrentRegion = client.Player.CurrentRegion;
							mob.AddToWorld();
							DisplayMessage(client, "created npc based on template " + id, new object[] { });
						}
						else
						{
							targetMob.LoadTemplate(template);
							targetMob.UpdateNPCEquipmentAppearance();
							targetMob.NPCTemplate = template as NpcTemplate;
							targetMob.SaveIntoDatabase();
							DisplayMessage(client, "updated npc based on template " + id, new object[] { });
						}
						break;
					}
			}
			return 1;
		}

		private string CheckName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}

		private string CheckGuildName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: guild name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}
	}
}
