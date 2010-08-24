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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.ServerProperties;
using log4net;


namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Character Create and Customization handler.  Please maintain all commented debug statements
	/// in order to support future debugging. - Tolakram
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x57 ^ 168, "Handles character creation requests")]
	public class CharacterCreateRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string accountName = packet.ReadString(24);

			//log.Debug("CharacterCreateRequestHandler for account " + accountName + " using version " + client.Version);

			if (!accountName.StartsWith(client.Account.Name))// TODO more correctly check, client send accountName as account-S, -N, -H (if it not fit in 20, then only account)
			{
				if (ServerProperties.Properties.BAN_HACKERS)
				{
					DBBannedAccount b = new DBBannedAccount();
					b.Author = "SERVER";
					b.Ip = client.TcpEndpointAddress;
					b.Account = client.Account.Name;
					b.DateBan = DateTime.Now;
					b.Type = "B";
					b.Reason = String.Format("Autoban wrong Account '{0}'", GameServer.Database.Escape(accountName));
					GameServer.Database.AddObject(b);
					GameServer.Database.SaveObject(b);
					GameServer.Instance.LogCheatAction(b.Reason + ". Account: " + b.Account);
				}

				client.Disconnect();
				return 0;
			}

            if (client.Version >= GameClient.eClientVersion.Version1104)
            {
                packet.ReadIntLowEndian(); //unk - probably indicates customize or create
            }

			int charsCount = client.Version < GameClient.eClientVersion.Version173 ? 8 : 10;
			for (int i = 0; i < charsCount; i++)
			{
				string charName = packet.ReadString(24);

				//log.DebugFormat("Character[{0}] = {1}", i, charName);

				if (charName.Length == 0)
				{
					// 1.104+  if character is not in list but is in DB then delete the character
					if (client.Version >= GameClient.eClientVersion.Version1104)
					{
						CheckForDeletedCharacter(accountName, client, i);
					}

					//If the charname is empty, skip the other bytes
					packet.Skip(160);
					if (client.Version >= GameClient.eClientVersion.Version199)
					{
						// skip 4 bytes added in 1.99
						packet.Skip(4);
					}
				}
				else
				{
					// Graveen: changed the following to allow GMs to have special chars in their names (_,-, etc..)
					Regex nameCheck = new Regex("^[A-Z][a-zA-Z]");
					if (charName.Length < 3 || !nameCheck.IsMatch(charName))
					{
						if (client.Account.PrivLevel == 1)
						{
							if (ServerProperties.Properties.BAN_HACKERS)
							{
								DBBannedAccount b = new DBBannedAccount();
								b.Author = "SERVER";
								b.Ip = client.TcpEndpointAddress;
								b.Account = client.Account.Name;
								b.DateBan = DateTime.Now;
								b.Type = "B";
								b.Reason = String.Format("Autoban bad CharName '{0}'", GameServer.Database.Escape(charName));
								GameServer.Database.AddObject(b);
								GameServer.Database.SaveObject(b);
								GameServer.Instance.LogCheatAction(b.Reason + ". Account: " + b.Account);
							}

							client.Disconnect();
							return 1;
						}
					}

					String select = String.Format("Name = '{0}'", GameServer.Database.Escape(charName));
					DOLCharacters character = GameServer.Database.SelectObject<DOLCharacters>(select);
					if (character != null)
					{
						if (character.AccountName != client.Account.Name)
						{
							if (Properties.BAN_HACKERS == true)
							{
								DBBannedAccount b = new DBBannedAccount();
								b.Author = "SERVER";
								b.Ip = client.TcpEndpointAddress;
								b.Account = client.Account.Name;
								b.DateBan = DateTime.Now;
								b.Type = "B";
								b.Reason = String.Format("Autoban CharName '{0}' on wrong Account '{1}'", GameServer.Database.Escape(charName), GameServer.Database.Escape(client.Account.Name));
								GameServer.Database.AddObject(b);
								GameServer.Database.SaveObject(b);
								GameServer.Instance.LogCheatAction(string.Format(b.Reason + ". Client Account: {0}, DB Account: {1}", client.Account.Name, character.AccountName));
							}

							client.Disconnect();
							return 1;
						}

						byte customizationMode = (byte)packet.ReadByte();

						// log.DebugFormat("CustomizationMode = {0} for charName {1}", customizationMode, charName);

						// check for update to existing character
						CheckCharacterForUpdates(client, packet, character, charName, customizationMode);
					}
					else
					{
						// create new character and return
						return CreateCharacter(client, packet, charName, i);
					}
				}
			}

			return 1;
		}


		#region Create Character

		private int CreateCharacter(GameClient client, GSPacketIn packet, string charName, int accountSlot)
		{
			log.Debug("Create Character");

			Account account = client.Account;
			DOLCharacters ch = new DOLCharacters();
			ch.AccountName = account.Name;
			ch.Name = charName;

			if (packet.ReadByte() == 0x01)
			{
				ch.EyeSize = (byte)packet.ReadByte();
				ch.LipSize = (byte)packet.ReadByte();
				ch.EyeColor = (byte)packet.ReadByte();
				ch.HairColor = (byte)packet.ReadByte();
				ch.FaceType = (byte)packet.ReadByte();
				ch.HairStyle = (byte)packet.ReadByte();
				packet.Skip(3);
				ch.MoodType = (byte)packet.ReadByte();
				ch.CustomisationStep = 2; // disable config button
				packet.Skip(13);
				log.Debug("Disable Config Button");
			}
			else
			{
				packet.Skip(23);
			}

			packet.Skip(24); //Location String
			ch.LastName = "";
			ch.GuildID = "";
			packet.Skip(24); //Skip class name
			packet.Skip(24); //Skip race name
			ch.Level = packet.ReadByte(); //not safe!
			ch.Level = 1;
			ch.Class = packet.ReadByte();
			if (ServerProperties.Properties.START_AS_BASE_CLASS)
			{
				ch.Class = RevertClass(ch);
			}
			ch.Realm = packet.ReadByte();

			if (log.IsDebugEnabled)
				log.Debug("Creation " + client.Version + " character, class:" + ch.Class + ", realm:" + ch.Realm);

			// Is class disabled ?
			int occurences = 0;
			List<string> disabled_classes = new List<string>(Properties.DISABLED_CLASSES.Split(';'));
			occurences = (from j in disabled_classes
						  where j == ch.Class.ToString()
						  select j).Count();

			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				log.Debug("Client " + client.Account.Name + " tried to create a disabled classe: " + (eCharacterClass)ch.Class);
				client.Out.SendCharacterOverview((eRealm)ch.Realm);
				return 1;
			}

			if (client.Version >= GameClient.eClientVersion.Version193)
			{
				ValidateCharacter.init_post193_tables();
			}
			else
			{
				ValidateCharacter.init_pre193_tables();
			}

			if (!Enum.IsDefined(typeof(eCharacterClass), (eCharacterClass)ch.Class))
			{
				log.Error(client.Account.Name + " tried to create a character with wrong class ID: " + ch.Class + ", realm:" + ch.Realm);
				if (ServerProperties.Properties.BAN_HACKERS)
				{
					DBBannedAccount b = new DBBannedAccount();
					b.Author = "SERVER";
					b.Ip = client.TcpEndpointAddress;
					b.Account = client.Account.Name;
					b.DateBan = DateTime.Now;
					b.Type = "B";
					b.Reason = string.Format("Autoban character create class: id:{0} realm:{1} name:{2} account:{3}", ch.Class, ch.Realm, ch.Name, account.Name);
					GameServer.Database.AddObject(b);
					GameServer.Database.SaveObject(b);
					GameServer.Instance.LogCheatAction(b.Reason + ". Account: " + b.Account);
					client.Disconnect();
				}
				return 1;
			}

			ch.AccountSlot = accountSlot + ch.Realm * 100;

			log.Debug("Account Slot " + accountSlot);

			//The following byte contains
			//1bit=start location ... in ShroudedIsles you can choose ...
			//1bit=first race bit
			//1bit=unknown
			//1bit=gender (0=male, 1=female)
			//4bit=race
			byte startRaceGender = (byte)packet.ReadByte();

			ch.Race = (startRaceGender & 0x0F) + ((startRaceGender & 0x40) >> 2);

			List<string> disabled_races = new List<string>(Properties.DISABLED_RACES.Split(';'));
			occurences = (from j in disabled_races
						  where j == ch.Race.ToString()
						  select j).Count();
			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				log.Debug("Client " + client.Account.Name + " tried to create a disabled race: " + (eRace)ch.Race);
				client.Out.SendCharacterOverview((eRealm)ch.Realm);
				return 1;
			}

			ch.Gender = ((startRaceGender >> 4) & 0x01);
			//DOLConsole.WriteLine("startRaceGender="+startRaceGender+"; Race="+ch.Race+"; Gender="+ch.Gender);

			bool siStartLocation = ((startRaceGender >> 7) != 0);

			ch.CreationModel = packet.ReadShortLowEndian();
			ch.CurrentModel = ch.CreationModel;
			ch.Region = packet.ReadByte();
			packet.Skip(1); //TODO second byte of region unused currently
			packet.Skip(4); //TODO Unknown Int / last used?

			ch.Strength = (byte)packet.ReadByte();
			ch.Dexterity = (byte)packet.ReadByte();
			ch.Constitution = (byte)packet.ReadByte();
			ch.Quickness = (byte)packet.ReadByte();
			ch.Intelligence = (byte)packet.ReadByte();
			ch.Piety = (byte)packet.ReadByte();
			ch.Empathy = (byte)packet.ReadByte();
			ch.Charisma = (byte)packet.ReadByte();

			packet.Skip(44); //TODO equipment

			if (client.Version >= GameClient.eClientVersion.Version199)
			{
				// skip 4 bytes added in 1.99
				packet.Skip(4);
			}

			// log.DebugFormat("STR {0}, CON {1}, DEX {2}, QUI {3}, INT {4}, PIE {5}, EMP {6}, CHA {7}", ch.Strength, ch.Constitution, ch.Dexterity, ch.Quickness, ch.Intelligence, ch.Piety, ch.Empathy, ch.Charisma);

			// check if client tried to create invalid char
			if (!ValidateCharacter.IsCharacterValid(ch))
			{
				if (log.IsWarnEnabled)
					log.Warn(ch.AccountName + " tried to create invalid character:" +
							 "\nchar name=" + ch.Name + ", race=" + ch.Race + ", realm=" + ch.Realm + ", class=" + ch.Class + ", region=" + ch.Region +
							 "\nstr=" + ch.Strength + ", con=" + ch.Constitution + ", dex=" + ch.Dexterity + ", qui=" + ch.Quickness + ", int=" + ch.Intelligence + ", pie=" + ch.Piety + ", emp=" + ch.Empathy + ", chr=" + ch.Charisma);

				if (client.Account.Realm == 0) client.Out.SendRealm(eRealm.None);
				else client.Out.SendCharacterOverview((eRealm)client.Account.Realm);
				return 1;
			}

			ch.CreationDate = DateTime.Now;

			ch.Endurance = 100;
			ch.MaxEndurance = 100;
			ch.Concentration = 100;
			ch.MaxSpeed = GamePlayer.PLAYER_BASE_SPEED;

			#region Starting Locations

			//if the server property for disable tutorial is set, we load in the classic starting locations
			if (ch.Region == 27 && ServerProperties.Properties.DISABLE_TUTORIAL)
			{
				switch (ch.Realm)
				{
					case 1: ch.Region = 1; break;
					case 2: ch.Region = 100; break;
					case 3: ch.Region = 200; break;
				}
			}

			ch.Xpos = 505603;
			ch.Ypos = 494709;
			ch.Zpos = 2463;
			ch.Direction = 5947;

			if (ch.Region == 51 && ch.Realm == 1)//Albion ShroudedIsles start point (I hope)
			{
				ch.Xpos = 526252;
				ch.Ypos = 542415;
				ch.Zpos = 3165;
				ch.Direction = 5286;
			}
			if (ch.Region != 51 && ch.Realm == 1)//Albion start point (Church outside Camelot/humberton)
			{
				ch.Xpos = 505603;
				ch.Ypos = 494709;
				ch.Zpos = 2463;
				ch.Direction = 5947;
				//ch.Region = 1;
				//DOLConsole.WriteLine(String.Format("Character ClassName:"+ch.ClassName+" created!"));
				//DOLConsole.WriteLine(String.Format("Character RaceName:"+ch.RaceName+" created!"));
			}
			if (ch.Region == 151 && ch.Realm == 2)//Midgard ShroudedIsles start point
			{
				ch.Xpos = 293720;
				ch.Ypos = 356408;
				ch.Zpos = 3488;
				ch.Direction = 6670;
			}
			if (ch.Region != 151 && ch.Realm == 2)//Midgard start point (Fort Atla)
			{
				ch.Xpos = 749103;
				ch.Ypos = 815835;
				ch.Zpos = 4408;
				ch.Direction = 7915;
				//ch.Region = 100;
				//DOLConsole.WriteLine(String.Format("Character ClassName:"+ch.ClassName+" created!"));
				//DOLConsole.WriteLine(String.Format("Character RaceName:"+ch.RaceName+" created!"));
			}
			if (ch.Region == 181 && ch.Realm == 3)//Hibernia ShroudedIsles start point
			{
				ch.Xpos = 426483;
				ch.Ypos = 440626;
				ch.Zpos = 5952;
				ch.Direction = 2403;
			}
			if (ch.Region != 181 && ch.Realm == 3)//Hibernia start point (Mag Mel)
			{
				ch.Xpos = 345900;
				ch.Ypos = 490867;
				ch.Zpos = 5200;
				ch.Direction = 4826;
				//ch.Region = 200;
				//DOLConsole.WriteLine(String.Format("Character ClassName:"+ch.ClassName+" created!"));
				//DOLConsole.WriteLine(String.Format("Character RaceName:"+ch.RaceName+" created!"));
			}

			// chars are bound on creation
			ch.BindRegion = ch.Region;
			ch.BindHeading = ch.Direction;
			ch.BindXpos = ch.Xpos;
			ch.BindYpos = ch.Ypos;
			ch.BindZpos = ch.Zpos;

			#endregion Starting Locations

			#region starting guilds

			if (account.PrivLevel == 1 && Properties.STARTING_GUILD)
			{
				switch (ch.Realm)
				{
					case 1:
						switch (ServerProperties.Properties.SERV_LANGUAGE)
						{
							case "EN":
								ch.GuildID = GuildMgr.GuildNameToGuildID("Clan Cotswold");
								break;
							case "DE":
								ch.GuildID = GuildMgr.GuildNameToGuildID("Klan Cotswold");
								break;
							default:
								ch.GuildID = GuildMgr.GuildNameToGuildID("Clan Cotswold");
								break;
						}
						break;
					case 2:
						switch (ServerProperties.Properties.SERV_LANGUAGE)
						{
							case "EN":
								ch.GuildID = GuildMgr.GuildNameToGuildID("Mularn Protectors");
								break;
							case "DE":
								ch.GuildID = GuildMgr.GuildNameToGuildID("Beschützer von Mularn");
								break;
							default:
								ch.GuildID = GuildMgr.GuildNameToGuildID("Mularn Protectors");
								break;
						}
						break;
					case 3:
						switch (ServerProperties.Properties.SERV_LANGUAGE)
						{
							case "EN":
								ch.GuildID = GuildMgr.GuildNameToGuildID("Tir na Nog Adventurers");
								break;
							case "DE":
								ch.GuildID = GuildMgr.GuildNameToGuildID("Tir na Nog-Abenteurer");
								break;
							default:
								ch.GuildID = GuildMgr.GuildNameToGuildID("Tir na Nog Adventurers");
								break;
						}
						break;
					default: break;
				}

				if (ch.GuildID != "")
					ch.GuildRank = 8;
			}

			#endregion Starting Guilds

			if (Properties.STARTING_MONEY > 0)
			{
				long value = Properties.STARTING_MONEY;
				ch.Copper = Money.GetCopper(value);
				ch.Silver = Money.GetSilver(value);
				ch.Gold = Money.GetGold(value);
				ch.Platinum = Money.GetPlatinum(value);
			}

			if (Properties.STARTING_REALM_LEVEL > 0)
			{
				int realmLevel = Properties.STARTING_REALM_LEVEL;
				long rpamount = 0;
				if (realmLevel < GamePlayer.REALMPOINTS_FOR_LEVEL.Length)
					rpamount = GamePlayer.REALMPOINTS_FOR_LEVEL[realmLevel];

				// thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
				if (rpamount == 0)
					rpamount = (long)(25.0 / 3.0 * (realmLevel * realmLevel * realmLevel) - 25.0 / 2.0 * (realmLevel * realmLevel) + 25.0 / 6.0 * realmLevel);

				ch.RealmPoints = rpamount;
				ch.RealmLevel = realmLevel;
				ch.RealmSpecialtyPoints = realmLevel;
			}

			ch.RespecAmountRealmSkill += 2;

			SetBasicCraftingForNewCharacter(ch);

			//Save the character in the database
			GameServer.Database.AddObject(ch);
			//Fire the character creation event
			GameEventMgr.Notify(DatabaseEvent.CharacterCreated, null, new CharacterEventArgs(ch, client));
			//add equipment
			StartupEquipment.AddEquipment(ch);
			//write changes
			GameServer.Database.SaveObject(ch);

			// Log creation
			AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.CharacterCreate, "", charName);

			client.Account.Characters = null;

			if (log.IsInfoEnabled)
				log.Info(String.Format("Character {0} created!", charName));

			GameServer.Database.FillObjectRelations(client.Account);
			client.Out.SendCharacterOverview((eRealm)ch.Realm);

			return 1;
		}

		#endregion Create Character


		#region Character Updates

		private int CheckCharacterForUpdates(GameClient client, GSPacketIn packet, DOLCharacters character, string charName, byte customizationMode)
		{
			int newModel = character.CurrentModel;

			if (customizationMode == 1 || customizationMode == 2 || customizationMode == 3)
			{
				bool flagChangedStats = false;
				character.EyeSize = (byte)packet.ReadByte();
				character.LipSize = (byte)packet.ReadByte();
				character.EyeColor = (byte)packet.ReadByte();
				character.HairColor = (byte)packet.ReadByte();
				character.FaceType = (byte)packet.ReadByte();
				character.HairStyle = (byte)packet.ReadByte();
				packet.Skip(3);
				character.MoodType = (byte)packet.ReadByte();
				packet.Skip(89); // Skip location string, race string, classe string, level ,class ,realm and startRaceGender
				newModel = packet.ReadShortLowEndian(); //read new model

				if (customizationMode != 3 && client.Version >= GameClient.eClientVersion.Version189)
				{
					packet.Skip(6); // Region ID + character Internal ID
					int[] stats = new int[8];
					stats[0] = (byte)packet.ReadByte(); // Strength
					stats[2] = (byte)packet.ReadByte(); // Dexterity
					stats[1] = (byte)packet.ReadByte(); // Constitution
					stats[3] = (byte)packet.ReadByte(); // Quickness
					stats[4] = (byte)packet.ReadByte(); // Intelligence
					stats[5] = (byte)packet.ReadByte(); // Piety
					stats[6] = (byte)packet.ReadByte(); // Empathy
					stats[7] = (byte)packet.ReadByte(); // Charisma

					packet.Skip(43);// armor models/armor color/weapon models/active weapon slots/siZone
					if (client.Version >= GameClient.eClientVersion.Version199)
					{
						// skip 4 bytes added in 1.99
						packet.Skip(4);
					}

					// what is this?
					byte newConstitution = (byte)packet.ReadByte();
					if (newConstitution > 0 && newConstitution < 255) // added 255 check, still not sure why this is here - tolakram
						stats[1] = newConstitution;


					flagChangedStats |= stats[0] != character.Strength;
					flagChangedStats |= stats[1] != character.Constitution;
					flagChangedStats |= stats[2] != character.Dexterity;
					flagChangedStats |= stats[3] != character.Quickness;
					flagChangedStats |= stats[4] != character.Intelligence;
					flagChangedStats |= stats[5] != character.Piety;
					flagChangedStats |= stats[6] != character.Empathy;
					flagChangedStats |= stats[7] != character.Charisma;

					//
					// !! Stat changes disabled by Tolakram until someone figures out why this can create invalid stats !!
					//
					flagChangedStats = false;

					if (flagChangedStats)
					{
						ICharacterClass charClass = ScriptMgr.FindCharacterClass(character.Class);

						if (charClass != null)
						{
							int points = 0;
							int[] leveledStats = new int[8];
							int[] raceStats = new int[8];
							bool valid = true;
							for (int j = 0; j < 8; j++)
							{
								eStat stat = (eStat)ValidateCharacter.eStatIndex[j];
								raceStats[j] = ValidateCharacter.STARTING_STATS[character.Race][j];
								for (int level = character.Level; level > 5; level--)
								{
									if (charClass.PrimaryStat != eStat.UNDEFINED && charClass.PrimaryStat == stat)
									{
										leveledStats[j]++;
									}
									if (charClass.SecondaryStat != eStat.UNDEFINED && charClass.SecondaryStat == stat)
									{
										if ((level - 6) % 2 == 0)
											leveledStats[j]++;
									}
									if (charClass.TertiaryStat != eStat.UNDEFINED && charClass.TertiaryStat == stat)
									{
										if ((level - 6) % 3 == 0)
											leveledStats[j]++;
									}
								}

								int result = stats[j] - leveledStats[j] - raceStats[j];

								bool validBeginStat = result >= 0;
								int pointsUsed = result;
								string statCategory = "";

								if (charClass.PrimaryStat != eStat.UNDEFINED && charClass.PrimaryStat == stat)
									statCategory = "1)";
								if (charClass.SecondaryStat != eStat.UNDEFINED && charClass.SecondaryStat == stat)
									statCategory = "2)";
								if (charClass.TertiaryStat != eStat.UNDEFINED && charClass.TertiaryStat == stat)
									statCategory = "3)";

								pointsUsed += Math.Max(0, result - 10); //two points used
								pointsUsed += Math.Max(0, result - 15); //three points used

								log.Info(string.Format("{0,-2} {1,-3}:{2, 3} {3,3} {4,3} {5,3} {6,2} {7} {8}",
													   statCategory,
													   (stat == eStat.STR) ? "STR" : stat.ToString(),
													   stats[j],
													   leveledStats[j],
													   stats[j] - leveledStats[j],
													   raceStats[j],
													   result,
													   pointsUsed,
													   (validBeginStat) ? "" : "Not Valid"));

								points += pointsUsed;

								if (!validBeginStat)
								{
									valid = false;
									if (client.Account.PrivLevel == 1)
									{
										if (ServerProperties.Properties.BAN_HACKERS)
										{
											DBBannedAccount b = new DBBannedAccount();
											b.Author = "SERVER";
											b.Ip = client.TcpEndpointAddress;
											b.Account = client.Account.Name;
											b.DateBan = DateTime.Now;
											b.Type = "B";
											b.Reason = String.Format("Autoban Hack char update : Wrong {0} point:{1}", (stat == eStat.STR) ? "STR" : stat.ToString(), result);
											GameServer.Database.AddObject(b);
											GameServer.Database.SaveObject(b);
											GameServer.Instance.LogCheatAction(b.Reason + ". Account: " + b.Account);
										}

										client.Disconnect();
										return 1;
									}
								}
							}

							if (valid)
							{
								character.Strength = (byte)stats[0];
								character.Constitution = (byte)stats[1];
								character.Dexterity = (byte)stats[2];
								character.Quickness = (byte)stats[3];
								character.Intelligence = (byte)stats[4];
								character.Piety = (byte)stats[5];
								character.Empathy = (byte)stats[6];
								character.Charisma = (byte)stats[7];

								DOLCharacters[] chars = client.Account.Characters;

								for (int z = 0; z < chars.Length; z++)
								{
									if (chars[z].Name != character.Name) continue;

									//Log.Error(string.Format("found activePlayer:[{0}] {1} {2}", client.ActiveCharIndex, client.Player.Name, character.Name));

									if (log.IsInfoEnabled)
										log.Info(String.Format("Character {0} updated in cache!\n", charName));

									if (client.Player != null)
									{
										client.Player.DBCharacter.Strength = (byte)stats[0];
										client.Player.DBCharacter.Constitution = (byte)stats[1];
										client.Player.DBCharacter.Dexterity = (byte)stats[2];
										client.Player.DBCharacter.Quickness = (byte)stats[3];
										client.Player.DBCharacter.Intelligence = (byte)stats[4];
										client.Player.DBCharacter.Piety = (byte)stats[5];
										client.Player.DBCharacter.Empathy = (byte)stats[6];
										client.Player.DBCharacter.Charisma = (byte)stats[7];
									}

									client.Account.Characters[z].Strength = (byte)stats[0];
									client.Account.Characters[z].Constitution = (byte)stats[1];
									client.Account.Characters[z].Dexterity = (byte)stats[2];
									client.Account.Characters[z].Quickness = (byte)stats[3];
									client.Account.Characters[z].Intelligence = (byte)stats[4];
									client.Account.Characters[z].Piety = (byte)stats[5];
									client.Account.Characters[z].Empathy = (byte)stats[6];
									client.Account.Characters[z].Charisma = (byte)stats[7];
								}
							}
						}
						else
						{
							if (log.IsErrorEnabled)
								log.Error("No CharacterClass with ID " + character.Class + " found");
						}
					}
				}
				else
				{
					packet.Skip(58); // skip all other things
					if (client.Version >= GameClient.eClientVersion.Version199)
					{
						// skip 4 bytes added in 1.99
						packet.Skip(4);
					}
				}

				if (customizationMode == 2) // change player customization
				{
					if (client.Account.PrivLevel == 1 && ((newModel >> 11) & 3) == 0) // Player size must be > 0 (from 1 to 3)
					{
						DBBannedAccount b = new DBBannedAccount();
						b.Author = "SERVER";
						b.Ip = client.TcpEndpointAddress;
						b.Account = client.Account.Name;
						b.DateBan = DateTime.Now;
						b.Type = "B";
						b.Reason = String.Format("Autoban Hack char update : zero character size in model:{0}", newModel);
						GameServer.Database.AddObject(b);
						GameServer.Database.SaveObject(b);
						GameServer.Instance.LogCheatAction(b.Reason + ". Account: " + b.Account);
						client.Disconnect();
						return 1;
					}

					if ((ushort)newModel != character.CreationModel)
					{
						character.CurrentModel = newModel;
					}

					character.CustomisationStep = 2; // disable config button

					GameServer.Database.SaveObject(character);

					if (log.IsInfoEnabled)
						log.Info(String.Format("Character {0} face proprieties configured by account {1}!\n", charName, client.Account.Name));
				}
				else if (customizationMode == 3) //auto config -- seems someone thinks this is not possible?
				{
					character.CustomisationStep = 3; // enable config button to player

					GameServer.Database.SaveObject(character);

					//if (log.IsInfoEnabled)
					//	log.Info(String.Format("Character {0} face proprieties auto updated!\n", charName));
				}
				else if (customizationMode == 1 && flagChangedStats) //changed stat only for 1.89+
				{
					GameServer.Database.SaveObject(character);

					if (log.IsInfoEnabled)
						log.Info(String.Format("Character {0} stat updated!\n", charName));
				}
			}

			return 1;
		}

		#endregion Character Updates


		#region Delete Character

		private void CheckForDeletedCharacter(string accountName, GameClient client, int slot)
		{
			int charSlot = 9999;

			if (accountName.EndsWith("-S")) charSlot = 100 + slot;
			else if (accountName.EndsWith("-N")) charSlot = 200 + slot;
			else if (accountName.EndsWith("-H")) charSlot = 300 + slot;

			DOLCharacters[] allChars = client.Account.Characters;

			if (allChars != null)
			{
				foreach (DOLCharacters character in allChars)
				{
					if (character.AccountSlot == charSlot && client.ClientState == GameClient.eClientState.CharScreen)
					{
						log.WarnFormat("DB Character Delete:  Account {0}, Character: {1}, slot position: {2}, client slot {3}", accountName, character.Name, character.AccountSlot, slot);

						GameEventMgr.Notify(DatabaseEvent.CharacterDeleted, null, new CharacterEventArgs(character, client));

						if (Properties.BACKUP_DELETED_CHARACTERS)
						{
							DOLCharactersBackup backupCharacter = new DOLCharactersBackup(character);
							GameServer.Database.AddObject(backupCharacter);
							log.WarnFormat("DB Character {0} backed up to DOLCharactersBackup and no associated content deleted.", character.ObjectId);
						}
						else
						{
							// delete associated data

							try
							{
								var objs = GameServer.Database.SelectObjects<InventoryItem>("OwnerID = '" + GameServer.Database.Escape(character.ObjectId) + "'");
								foreach (InventoryItem item in objs)
								{
									GameServer.Database.DeleteObject(item);
								}
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.Error("Error deleting char items, char OID=" + character.ObjectId, e);
							}

							// delete quests
							try
							{
								var objs = GameServer.Database.SelectObjects<DBQuest>("Character_ID = '" + GameServer.Database.Escape(character.ObjectId) + "'");
								foreach (DBQuest quest in objs)
								{
									GameServer.Database.DeleteObject(quest);
								}
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.Error("Error deleting char quests, char OID=" + character.ObjectId, e);
							}

							// delete ML steps
							try
							{
								var objs = GameServer.Database.SelectObjects<DBCharacterXMasterLevel>("Character_ID = '" + GameServer.Database.Escape(character.ObjectId) + "'");
								foreach (DBCharacterXMasterLevel mlstep in objs)
								{
									GameServer.Database.DeleteObject(mlstep);
								}
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.Error("Error deleting char ml steps, char OID=" + character.ObjectId, e);
							}
						}

						string deletedChar = character.Name;

						GameServer.Database.DeleteObject(character);
						client.Account.Characters = null;
						client.Player = null;
						GameServer.Database.FillObjectRelations(client.Account);

						if (client.Account.Characters == null || client.Account.Characters.Length == 0)
						{
							if (log.IsInfoEnabled)
								log.Info(string.Format("Account {0} has no more chars. Realm reset!", client.Account.Name));

							//Client has no more characters, so the client can choose the realm again!
							client.Account.Realm = 0;
						}

						GameServer.Database.SaveObject(client.Account);

						// Log deletion
						AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterDelete, "", deletedChar);
					}
				}
			}

		}

		#endregion Delete Character


		#region ValidateCharacter

		/// <summary>
		/// Provides methods to handle char creation checks
		/// </summary>
		protected class ValidateCharacter
		{
			protected const int STR = 0;
			protected const int CON = 1;
			protected const int DEX = 2;
			protected const int QUI = 3;
			protected const int INT = 4;
			protected const int PIE = 5;
			protected const int EMP = 6;
			protected const int CHA = 7;

			/// <summary>
			/// Verify whether created character is valid
			/// </summary>
			/// <param name="ch">The character to check</param>
			/// <returns>True if valid</returns>
			public static bool IsCharacterValid(DOLCharacters ch)
			{
				bool valid = true;
				try
				{
					if (ch.Realm > 3)
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong realm: " + ch.Realm);
						valid = false;
					}
					if (ch.Level != 1)
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong level: " + ch.Level);
						valid = false;
					}
					if (Array.IndexOf(STARTING_CLASSES[ch.Realm], ch.Class) == -1)
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong class: " + ch.Class + ", realm:" + ch.Realm);
						valid = false;
					}
					if (Array.IndexOf(RACES_CLASSES[ch.Race], ch.Class) == -1)
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong race: " + ch.Race + ", class:" + ch.Class);
						valid = false;
					}
					int pointsUsed = 0;
					pointsUsed += PointsUsed(ch.Race, STR, ch.Strength);
					pointsUsed += PointsUsed(ch.Race, CON, ch.Constitution);
					pointsUsed += PointsUsed(ch.Race, DEX, ch.Dexterity);
					pointsUsed += PointsUsed(ch.Race, QUI, ch.Quickness);
					pointsUsed += PointsUsed(ch.Race, INT, ch.Intelligence);
					pointsUsed += PointsUsed(ch.Race, PIE, ch.Piety);
					pointsUsed += PointsUsed(ch.Race, EMP, ch.Empathy);
					pointsUsed += PointsUsed(ch.Race, CHA, ch.Charisma);
					if (pointsUsed != 30)
					{
						if (log.IsWarnEnabled)
							log.Warn("Points used: " + pointsUsed);
						valid = false;
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("CharacterCreation", e);
					return false;
				}
				return valid;
			}

			/// <summary>
			/// Calculates amount of starting points spent on one stat
			/// </summary>
			/// <param name="race">race index in starting stats array</param>
			/// <param name="statIndex">index of that stat in starting stats array</param>
			/// <param name="statValue">base+spent points in stat</param>
			/// <returns></returns>
			protected static int PointsUsed(int race, int statIndex, int statValue)
			{
				statValue -= STARTING_STATS[race][statIndex];
				int result = statValue; //one point used
				result += Math.Max(0, statValue - 10); //two points used
				result += Math.Max(0, statValue - 15); //three points used
				return result;
			}

			public static readonly byte[] eStatIndex = new byte[8]
			{
				(byte)eProperty.Strength,
				(byte)eProperty.Constitution,
				(byte)eProperty.Dexterity,
				(byte)eProperty.Quickness,
				(byte)eProperty.Intelligence,
				(byte)eProperty.Piety,
				(byte)eProperty.Empathy,
				(byte)eProperty.Charisma
			};

			/// <summary>
			/// All possible player races
			/// </summary>
			public static readonly int[][] STARTING_STATS = new int[][]
			{
				null, // "Unknown",
				//           STR CON DEX QUI INT PIE EMP CHA
				new int[8] { 60, 60, 60, 60, 60, 60, 60, 60 }, // Briton
				new int[8] { 45, 45, 60, 70, 80, 60, 60, 60 }, // Avalonian
				new int[8] { 70, 70, 50, 50, 60, 60, 60, 60 }, // Highlander
				new int[8] { 50, 50, 80, 60, 60, 60, 60, 60 }, // Saracen
				new int[8] { 70, 70, 50, 50, 60, 60, 60, 60 }, // Norseman
				new int[8] { 100, 70, 35, 35, 60, 60, 60, 60 }, // Troll
				new int[8] { 60, 80, 50, 50, 60, 60, 60, 60 }, // Dwarf
				new int[8] { 50, 50, 70, 70, 60, 60, 60, 60 }, // Kobold
				new int[8] { 60, 60, 60, 60, 60, 60, 60, 60 }, // Celt
				new int[8] { 90, 60, 40, 40, 60, 60, 70, 60 }, // Firbolg
				new int[8] { 40, 40, 75, 75, 70, 60, 60, 60 }, // Elf
				new int[8] { 40, 40, 80, 80, 60, 60, 60, 60 }, // Lurikeen
				new int[8] { 50, 60, 70, 50, 70, 60, 60, 60 }, // Inconnu
				new int[8] { 55, 45, 65, 75, 60, 60, 60, 60 }, // Valkyn
				new int[8] { 70, 60, 55, 45, 70, 60, 60, 60 }, // Sylvan
				new int[8] { 90, 70, 40, 40, 60, 60, 60, 60 }, // Half Ogre
				new int[8] { 55, 55, 55, 60, 60, 75, 60, 60 }, // Frostalf
				new int[8] { 60, 80, 50, 50, 60, 60, 60, 60 }, // Shar
				new int[8] { 80, 70, 50, 40, 60, 60, 60, 60 }, // AlbionMenotaur
				new int[8] { 80, 70, 50, 40, 60, 60, 60, 60 }, // MidgardMenotaur
				new int[8] { 80, 70, 50, 40, 60, 60, 60, 60 }, // HiberniaMenotaur
			};

			/// <summary>
			/// All possible player starting classes
			/// </summary>
			public static int[][] STARTING_CLASSES = null;
			protected static int[][] RACES_CLASSES = null;
			public static int[] ADVANCED_CLASS_TO_BASE_CLASS = null;

			// static methods handling pre and post 1.93 characters creation tables
			public static void init_pre193_tables()
			{
				STARTING_CLASSES = new int[][]
				{
					null, // "Unknown",
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Albion
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Midgard
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Magician,
						(int)eCharacterClass.Forester }, // Hibernia
				};

				RACES_CLASSES = new int[][]
				{
					null, // "Unknown",
					//
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Briton
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist }, // Avalonian
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.AlbionRogue }, // Highlander
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Saracen
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Norseman
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer }, // Troll
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Dwarf
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Kobold
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Magician,
						(int)eCharacterClass.Forester }, // Celt
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Forester }, // Firbolg
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Magician }, // Elf
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Magician }, // Lurikeen
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Inconnu
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.MidgardRogue }, // Valkyn
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Forester }, // Sylvan
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist }, // Half Ogre
					new int[] { (int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Frostalf
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Magician }, // Shar
					new int[] { (int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte },//AlbionMenotaur
					new int[] {	(int)eCharacterClass.Viking,
						(int)eCharacterClass.Seer }, //MidgardMenotaur
					new int[] { (int)eCharacterClass.Guardian,
						(int)eCharacterClass.Naturalist }, //HiberniaMenotaur
				};

				ADVANCED_CLASS_TO_BASE_CLASS = new int[]
				{
					0, // "Unknown",
					(int)eCharacterClass.Fighter, 		// Paladin = 1,
					(int)eCharacterClass.Fighter, 		// Armsman = 2,
					(int)eCharacterClass.AlbionRogue, 	// Scout = 3,
					(int)eCharacterClass.AlbionRogue, 	// Minstrel = 4,
					(int)eCharacterClass.Elementalist, 	// Theurgist = 5,
					(int)eCharacterClass.Acolyte, 		// Cleric = 6,
					(int)eCharacterClass.Elementalist, 	// Wizard = 7,
					(int)eCharacterClass.Mage, 			// Sorcerer = 8,
					(int)eCharacterClass.AlbionRogue, 	// Infiltrator = 9,
					(int)eCharacterClass.Acolyte, 		// Friar = 10,
					(int)eCharacterClass.Fighter, 		// Mercenary = 11,
					(int)eCharacterClass.Disciple, 		// Necromancer = 12,
					(int)eCharacterClass.Mage, 			// Cabalist = 13,
					(int)eCharacterClass.Fighter, 		// Fighter = 14,
					(int)eCharacterClass.Elementalist, 	// Elementalist = 15,
					(int)eCharacterClass.Acolyte, 		// Acolyte = 16,
					(int)eCharacterClass.AlbionRogue, 	// AlbionRogue = 17,
					(int)eCharacterClass.Mage, 			// Mage = 18,
					(int)eCharacterClass.Fighter, 		// Reaver = 19,
					(int)eCharacterClass.Disciple, 		// Disciple = 20,
					(int)eCharacterClass.Viking, 		// Thane = 21,
					(int)eCharacterClass.Viking, 		// Warrior = 22,
					(int)eCharacterClass.MidgardRogue, 	// Shadowblade = 23,
					(int)eCharacterClass.Viking, 		// Skald = 24,
					(int)eCharacterClass.MidgardRogue, 	// Hunter = 25,
					(int)eCharacterClass.Seer, 			// Healer = 26,
					(int)eCharacterClass.Mystic, 		// Spiritmaster = 27,
					(int)eCharacterClass.Seer, 			// Shaman = 28,
					(int)eCharacterClass.Mystic, 		// Runemaster = 29,
					(int)eCharacterClass.Mystic, 		// Bonedancer = 30,
					(int)eCharacterClass.Viking, 		// Berserker = 31,
					(int)eCharacterClass.Viking, 		// Savage = 32,
					(int)eCharacterClass.Acolyte, 		// Heretic = 33,
					(int)eCharacterClass.Viking, 		// Valkyrie = 34,
					(int)eCharacterClass.Viking, 		// Viking = 35,
					(int)eCharacterClass.Mystic, 		// Mystic = 36,
					(int)eCharacterClass.Seer, 			// Seer = 37,
					(int)eCharacterClass.MidgardRogue, 	// MidgardRogue = 38,
					(int)eCharacterClass.Magician, 		// Bainshee = 39,
					(int)eCharacterClass.Magician, 		// Eldritch = 40,
					(int)eCharacterClass.Magician, 		// Enchanter = 41,
					(int)eCharacterClass.Magician, 		// Mentalist = 42,
					(int)eCharacterClass.Guardian, 		// Blademaster = 43,
					(int)eCharacterClass.Guardian, 		// Hero = 44,
					(int)eCharacterClass.Guardian, 		// Champion = 45,
					(int)eCharacterClass.Naturalist, 	// Warden = 46,
					(int)eCharacterClass.Naturalist, 	// Druid = 47,
					(int)eCharacterClass.Naturalist, 	// Bard = 48,
					(int)eCharacterClass.Stalker, 		// Nightshade = 49,
					(int)eCharacterClass.Stalker, 		// Ranger = 50,
					(int)eCharacterClass.Magician, 		// Magician = 51,
					(int)eCharacterClass.Guardian, 		// Guardian = 52,
					(int)eCharacterClass.Naturalist, 	// Naturalist = 53,
					(int)eCharacterClass.Stalker, 		// Stalker = 54,
					(int)eCharacterClass.Forester, 		// Animist = 55,
					(int)eCharacterClass.Forester, 		// Valewalker = 56,
					(int)eCharacterClass.Forester, 		// Forester = 57,
					(int)eCharacterClass.Stalker, 		// Vampiir = 58,
					(int)eCharacterClass.Mystic, 		// Warlock = 59,
					(int)eCharacterClass.Fighter, 		// Mauler_Alb = 60,
					(int)eCharacterClass.Viking, 		// Mauler_Mid = 61,
					(int)eCharacterClass.Guardian, 		// Mauler_Hib = 62,
				};
			}

			public static void init_post193_tables()
			{
				STARTING_CLASSES = new int[][]
				{
					null, // "Unknown",
					new int[]{
						(int)eCharacterClass.Paladin, 		// Paladin = 1,
						(int)eCharacterClass.Armsman, 		// Armsman = 2,
						(int)eCharacterClass.Scout, 	    // Scout = 3,
						(int)eCharacterClass.Minstrel, 	    // Minstrel = 4,
						(int)eCharacterClass.Theurgist, 	// Theurgist = 5,
						(int)eCharacterClass.Cleric, 		// Cleric = 6,
						(int)eCharacterClass.Wizard, 	    // Wizard = 7,
						(int)eCharacterClass.Sorcerer, 		// Sorcerer = 8,
						(int)eCharacterClass.Infiltrator, 	// Infiltrator = 9,
						(int)eCharacterClass.Friar, 		// Friar = 10,
						(int)eCharacterClass.Mercenary, 	// Mercenary = 11,
						(int)eCharacterClass.Necromancer, 	// Necromancer = 12,
						(int)eCharacterClass.Cabalist, 		// Cabalist = 13,
						(int)eCharacterClass.Fighter, 		// Fighter = 14,
						(int)eCharacterClass.Elementalist, 	// Elementalist = 15,
						(int)eCharacterClass.Acolyte, 		// Acolyte = 16,
						(int)eCharacterClass.AlbionRogue, 	// AlbionRogue = 17,
						(int)eCharacterClass.Mage, 			// Mage = 18,
						(int)eCharacterClass.Reaver, 		// Reaver = 19,
						(int)eCharacterClass.Disciple,	   // Disciple = 20,
						(int)eCharacterClass.Heretic, 		// Heretic = 33,
						(int)eCharacterClass.MaulerAlb },	// Mauler_Alb = 60,
					new int[]{
						(int)eCharacterClass.Thane, 		// Thane = 21,
						(int)eCharacterClass.Warrior, 		// Warrior = 22,
						(int)eCharacterClass.Shadowblade, 	// Shadowblade = 23,
						(int)eCharacterClass.Skald, 		// Skald = 24,
						(int)eCharacterClass.Hunter, 	    // Hunter = 25,
						(int)eCharacterClass.Healer, 		// Healer = 26,
						(int)eCharacterClass.Spiritmaster,  // Spiritmaster = 27,
						(int)eCharacterClass.Shaman, 		// Shaman = 28,
						(int)eCharacterClass.Runemaster, 	// Runemaster = 29,
						(int)eCharacterClass.Bonedancer, 	// Bonedancer = 30,
						(int)eCharacterClass.Berserker, 	// Berserker = 31,
						(int)eCharacterClass.Savage, 		// Savage = 32,
						(int)eCharacterClass.Valkyrie, 		// Valkyrie = 34,
						(int)eCharacterClass.Viking, 		// Viking = 35,
						(int)eCharacterClass.Mystic, 		// Mystic = 36,
						(int)eCharacterClass.Seer, 			// Seer = 37,
						(int)eCharacterClass.MidgardRogue,	// MidgardRogue = 38,
						(int)eCharacterClass.Warlock, 		// Warlock = 59,
						(int)eCharacterClass.MaulerMid }, 	// Mauler_Mid = 61,
					new int[]{
						(int)eCharacterClass.Bainshee, 		// Bainshee = 39,
						(int)eCharacterClass.Eldritch, 		// Eldritch = 40,
						(int)eCharacterClass.Enchanter, 	// Enchanter = 41,
						(int)eCharacterClass.Mentalist, 	// Mentalist = 42,
						(int)eCharacterClass.Blademaster, 	// Blademaster = 43,
						(int)eCharacterClass.Hero, 		    // Hero = 44,
						(int)eCharacterClass.Champion, 		// Champion = 45,
						(int)eCharacterClass.Warden, 	    // Warden = 46,
						(int)eCharacterClass.Druid, 	    // Druid = 47,
						(int)eCharacterClass.Bard, 	        // Bard = 48,
						(int)eCharacterClass.Nightshade, 	// Nightshade = 49,
						(int)eCharacterClass.Ranger, 		// Ranger = 50,
						(int)eCharacterClass.Magician, 		// Magician = 51,
						(int)eCharacterClass.Guardian, 		// Guardian = 52,
						(int)eCharacterClass.Naturalist, 	// Naturalist = 53,
						(int)eCharacterClass.Stalker, 		// Stalker = 54,
						(int)eCharacterClass.Animist, 		// Animist = 55,
						(int)eCharacterClass.Valewalker, 	// Valewalker = 56,
						(int)eCharacterClass.Forester, 		// Forester = 57,
						(int)eCharacterClass.Vampiir, 		// Vampiir = 58,
						(int)eCharacterClass.MaulerHib } 	// Mauler_Hib = 62,
				};

				RACES_CLASSES = new int[][]
				{
					null, // "Unknown",
					//
					new int[] {
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Reaver,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Paladin,
						(int)eCharacterClass.Cleric,
						(int)eCharacterClass.Heretic,
						(int)eCharacterClass.Friar,
						(int)eCharacterClass.Sorcerer,
						(int)eCharacterClass.Cabalist,
						(int)eCharacterClass.Theurgist,
						(int)eCharacterClass.Necromancer,
						(int)eCharacterClass.MaulerAlb,
						(int)eCharacterClass.Wizard,
						(int)eCharacterClass.Minstrel,
						(int)eCharacterClass.Infiltrator,
						(int)eCharacterClass.Scout,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Briton
					new int[] {
						(int)eCharacterClass.Paladin,
						(int)eCharacterClass.Cleric,
						(int)eCharacterClass.Wizard,
						(int)eCharacterClass.Theurgist,
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Sorcerer,
						(int)eCharacterClass.Cabalist,
						(int)eCharacterClass.Heretic,
						(int)eCharacterClass.Friar,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist }, // Avalonian
					new int[] {
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Paladin,
						(int)eCharacterClass.Cleric,
						(int)eCharacterClass.Minstrel,
						(int)eCharacterClass.Scout,
						(int)eCharacterClass.Friar,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.AlbionRogue }, // Highlander
					new int[] {
						(int)eCharacterClass.Sorcerer,
						(int)eCharacterClass.Cabalist,
						(int)eCharacterClass.Paladin,
						(int)eCharacterClass.Reaver,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Infiltrator,
						(int)eCharacterClass.Minstrel,
						(int)eCharacterClass.Scout,
						(int)eCharacterClass.Necromancer,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Saracen
					new int[] {
						(int)eCharacterClass.Healer,
						(int)eCharacterClass.Warrior,
						(int)eCharacterClass.Berserker,
						(int)eCharacterClass.Thane,
						(int)eCharacterClass.Warlock,
						(int)eCharacterClass.Skald,
						(int)eCharacterClass.Valkyrie,
						(int)eCharacterClass.Spiritmaster,
						(int)eCharacterClass.Runemaster,
						(int)eCharacterClass.Savage,
						(int)eCharacterClass.MaulerMid,
						(int)eCharacterClass.Shadowblade,
						(int)eCharacterClass.Hunter,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Norseman
					new int[] {
						(int)eCharacterClass.Berserker,
						(int)eCharacterClass.Warrior,
						(int)eCharacterClass.Savage,
						(int)eCharacterClass.Thane,
						(int)eCharacterClass.Skald,
						(int)eCharacterClass.Bonedancer,
						(int)eCharacterClass.Shaman,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer }, // Troll
					new int[] {
						(int)eCharacterClass.Healer,
						(int)eCharacterClass.Thane,
						(int)eCharacterClass.Berserker,
						(int)eCharacterClass.Warrior,
						(int)eCharacterClass.Savage,
						(int)eCharacterClass.Skald,
						(int)eCharacterClass.Valkyrie,
						(int)eCharacterClass.Runemaster,
						(int)eCharacterClass.Hunter,
						(int)eCharacterClass.Shaman,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Dwarf
					new int[] {
						(int)eCharacterClass.Shaman,
						(int)eCharacterClass.Warrior,
						(int)eCharacterClass.Skald,
						(int)eCharacterClass.Savage,
						(int)eCharacterClass.Runemaster,
						(int)eCharacterClass.Spiritmaster,
						(int)eCharacterClass.Bonedancer,
						(int)eCharacterClass.Warlock,
						(int)eCharacterClass.Hunter,
						(int)eCharacterClass.Shadowblade,
						(int)eCharacterClass.MaulerMid,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Kobold
					new int[] {
						(int)eCharacterClass.Bard,
						(int)eCharacterClass.Druid,
						(int)eCharacterClass.Warden,
						(int)eCharacterClass.Blademaster,
						(int)eCharacterClass.Hero,
						(int)eCharacterClass.Vampiir,
						(int)eCharacterClass.Champion,
						(int)eCharacterClass.MaulerHib,
						(int)eCharacterClass.Mentalist,
						(int)eCharacterClass.Bainshee,
						(int)eCharacterClass.Ranger,
						(int)eCharacterClass.Animist,
						(int)eCharacterClass.Valewalker,
						(int)eCharacterClass.Nightshade,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Magician,
						(int)eCharacterClass.Forester }, // Celt
					new int[] {
						(int)eCharacterClass.Bard,
						(int)eCharacterClass.Druid,
						(int)eCharacterClass.Warden,
						(int)eCharacterClass.Hero,
						(int)eCharacterClass.Blademaster,
						(int)eCharacterClass.Animist,
						(int)eCharacterClass.Valewalker,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Forester }, // Firbolg
					new int[] {
						(int)eCharacterClass.Blademaster,
						(int)eCharacterClass.Champion,
						(int)eCharacterClass.Ranger,
						(int)eCharacterClass.Nightshade,
						(int)eCharacterClass.Bainshee,
						(int)eCharacterClass.Enchanter,
						(int)eCharacterClass.Eldritch,
						(int)eCharacterClass.Mentalist,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Magician }, // Elf
					new int[] {
						(int)eCharacterClass.Hero,
						(int)eCharacterClass.Champion,
						(int)eCharacterClass.Vampiir,
						(int)eCharacterClass.Eldritch,
						(int)eCharacterClass.Enchanter,
						(int)eCharacterClass.Mentalist,
						(int)eCharacterClass.Bainshee,
						(int)eCharacterClass.Nightshade,
						(int)eCharacterClass.Ranger,
						(int)eCharacterClass.MaulerHib,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Magician }, // Lurikeen
					new int[] {
						(int)eCharacterClass.Reaver,
						(int)eCharacterClass.Sorcerer,
						(int)eCharacterClass.Cabalist,
						(int)eCharacterClass.Heretic,
						(int)eCharacterClass.Necromancer,
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Infiltrator,
						(int)eCharacterClass.Scout,
						(int)eCharacterClass.MaulerAlb,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.AlbionRogue,
						(int)eCharacterClass.Disciple }, // Inconnu
					new int[] {
						(int)eCharacterClass.Savage,
						(int)eCharacterClass.Berserker,
						(int)eCharacterClass.Bonedancer,
						(int)eCharacterClass.Warrior,
						(int)eCharacterClass.Shadowblade,
						(int)eCharacterClass.Hunter,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.MidgardRogue }, // Valkyn
					new int[] {
						(int)eCharacterClass.Animist,
						(int)eCharacterClass.Druid,
						(int)eCharacterClass.Valewalker,
						(int)eCharacterClass.Hero,
						(int)eCharacterClass.Warden,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Naturalist,
						(int)eCharacterClass.Forester }, // Sylvan
					new int[] {
						(int)eCharacterClass.Wizard,
						(int)eCharacterClass.Theurgist,
						(int)eCharacterClass.Cabalist,
						(int)eCharacterClass.Sorcerer,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Mage,
						(int)eCharacterClass.Elementalist }, // Half Ogre
					new int[] {
						(int)eCharacterClass.Healer,
						(int)eCharacterClass.Shaman,
						(int)eCharacterClass.Thane,
						(int)eCharacterClass.Spiritmaster,
						(int)eCharacterClass.Runemaster,
						(int)eCharacterClass.Warlock,
						(int)eCharacterClass.Valkyrie,
						(int)eCharacterClass.Hunter,
						(int)eCharacterClass.Shadowblade,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Mystic,
						(int)eCharacterClass.Seer,
						(int)eCharacterClass.MidgardRogue }, // Frostalf
					new int[] {
						(int)eCharacterClass.Champion,
						(int)eCharacterClass.Hero,
						(int)eCharacterClass.Blademaster,
						(int)eCharacterClass.Vampiir,
						(int)eCharacterClass.Ranger,
						(int)eCharacterClass.Mentalist,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Stalker,
						(int)eCharacterClass.Magician }, // Shar
					new int[] {
						(int)eCharacterClass.Heretic,
						(int)eCharacterClass.MaulerAlb,
						(int)eCharacterClass.Armsman,
						(int)eCharacterClass.Mercenary,
						(int)eCharacterClass.Fighter,
						(int)eCharacterClass.Acolyte },//AlbMinotaur
					new int[] {
						(int)eCharacterClass.Berserker,
						(int)eCharacterClass.MaulerMid,
						(int)eCharacterClass.Thane,
						(int)eCharacterClass.Viking,
						(int)eCharacterClass.Warrior }, //MidMinotaur
					new int[] {
						(int)eCharacterClass.Hero,
						(int)eCharacterClass.Blademaster,
						(int)eCharacterClass.MaulerHib,
						(int)eCharacterClass.Warden,
						(int)eCharacterClass.Guardian,
						(int)eCharacterClass.Naturalist }, //HibMinotaur
				};
			}
		}

		#endregion Validate Character


		#region Utility

		private static int RevertClass(DOLCharacters ch)
		{
			switch (ch.Class)
			{
				//Alb
				case (int)eCharacterClass.Armsman: return (int)eCharacterClass.Fighter;
				case (int)eCharacterClass.Mercenary: return (int)eCharacterClass.Fighter;
				case (int)eCharacterClass.Paladin: return (int)eCharacterClass.Fighter;
				case (int)eCharacterClass.MaulerAlb: return (int)eCharacterClass.Fighter;
				case (int)eCharacterClass.Reaver: return (int)eCharacterClass.Fighter;
				case (int)eCharacterClass.Cleric: return (int)eCharacterClass.Acolyte;
				case (int)eCharacterClass.Friar: return (int)eCharacterClass.Acolyte;
				case (int)eCharacterClass.Heretic: return (int)eCharacterClass.Acolyte;
				case (int)eCharacterClass.Infiltrator: return (int)eCharacterClass.AlbionRogue;
				case (int)eCharacterClass.Scout: return (int)eCharacterClass.AlbionRogue;
				case (int)eCharacterClass.Minstrel: return (int)eCharacterClass.AlbionRogue;
				case (int)eCharacterClass.Cabalist: return (int)eCharacterClass.Mage;
				case (int)eCharacterClass.Sorcerer: return (int)eCharacterClass.Mage;
				case (int)eCharacterClass.Theurgist: return (int)eCharacterClass.Elementalist;
				case (int)eCharacterClass.Wizard: return (int)eCharacterClass.Elementalist;
				case (int)eCharacterClass.Necromancer: return (int)eCharacterClass.Disciple;
				//Hib
				case (int)eCharacterClass.Hero: return (int)eCharacterClass.Guardian;
				case (int)eCharacterClass.Champion: return (int)eCharacterClass.Guardian;
				case (int)eCharacterClass.Blademaster: return (int)eCharacterClass.Guardian;
				case (int)eCharacterClass.MaulerHib: return (int)eCharacterClass.Guardian;
				case (int)eCharacterClass.Bard: return (int)eCharacterClass.Naturalist;
				case (int)eCharacterClass.Druid: return (int)eCharacterClass.Naturalist;
				case (int)eCharacterClass.Warden: return (int)eCharacterClass.Naturalist;
				case (int)eCharacterClass.Ranger: return (int)eCharacterClass.Stalker;
				case (int)eCharacterClass.Nightshade: return (int)eCharacterClass.Stalker;
				case (int)eCharacterClass.Vampiir: return (int)eCharacterClass.Stalker;
				case (int)eCharacterClass.Bainshee: return (int)eCharacterClass.Magician;
				case (int)eCharacterClass.Eldritch: return (int)eCharacterClass.Magician;
				case (int)eCharacterClass.Enchanter: return (int)eCharacterClass.Magician;
				case (int)eCharacterClass.Mentalist: return (int)eCharacterClass.Magician;
				case (int)eCharacterClass.Animist: return (int)eCharacterClass.Forester;
				case (int)eCharacterClass.Valewalker: return (int)eCharacterClass.Forester;
				//Mid
				case (int)eCharacterClass.Berserker: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.MaulerMid: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.Savage: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.Skald: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.Thane: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.Valkyrie: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.Warrior: return (int)eCharacterClass.Viking;
				case (int)eCharacterClass.Hunter: return (int)eCharacterClass.MidgardRogue;
				case (int)eCharacterClass.Shadowblade: return (int)eCharacterClass.MidgardRogue;
				case (int)eCharacterClass.Healer: return (int)eCharacterClass.Seer;
				case (int)eCharacterClass.Shaman: return (int)eCharacterClass.Seer;
				case (int)eCharacterClass.Bonedancer: return (int)eCharacterClass.Mystic;
				case (int)eCharacterClass.Runemaster: return (int)eCharacterClass.Mystic;
				case (int)eCharacterClass.Warlock: return (int)eCharacterClass.Mystic;
				case (int)eCharacterClass.Spiritmaster: return (int)eCharacterClass.Mystic;
				//older client support
				default: return ch.Class;

			}
		}

		private void SetBasicCraftingForNewCharacter(DOLCharacters ch)
		{
			string serializedAllCraftingSkills = "";
			foreach (int craftingSkillId in Enum.GetValues(typeof(eCraftingSkill)))
			{
				if (craftingSkillId > 0)
				{
					serializedAllCraftingSkills += (int)craftingSkillId + "|1;";
					if (craftingSkillId == (int)eCraftingSkill._Last)
					{
						break;
					}
				}
			}
			if (serializedAllCraftingSkills.Length > 0)
			{
				serializedAllCraftingSkills = serializedAllCraftingSkills.Remove(serializedAllCraftingSkills.Length - 1);
			}
			ch.SerializedCraftingSkills = serializedAllCraftingSkills;
			ch.CraftingPrimarySkill = (int)eCraftingSkill.BasicCrafting;
		}

		#endregion Utility

	}
}