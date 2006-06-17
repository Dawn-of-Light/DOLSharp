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
using System.Reflection;
using System.Collections;
using DOL.Database;
using DOL.Events;
using DOL.GS.Database;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x57^168,"Handles character creation requests")]
	public class CharacterListUpdateRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			packet.Skip(24); //Skip the account name, we don't need it
			int charsCount = client.Version < GameClient.eClientVersion.Version173 ? 8 : 10;
			
			for (byte i = 0; i < charsCount; i++)
			{
				string charname = packet.ReadString(24);
				if(charname.Length == 0)
				{
					//If the charname is empty, skip the other bytes
					packet.Skip(160);
					continue;
				}

				GamePlayer character = client.Account.GetCharacter(charname);

				if(character != null)
				{
					// update old characters
					switch(packet.ReadByte())
					{
						case 0x02: //player config
						{
							if(character.CustomisationStep == 3)
							{
								character.EyeSize = (byte)packet.ReadByte();
								character.LipSize = (byte)packet.ReadByte();
								character.EyeColor = (byte)packet.ReadByte();
								character.HairColor = (byte)packet.ReadByte();
								character.FaceType = (byte)packet.ReadByte();
								character.HairStyle = (byte)packet.ReadByte();
								packet.Skip(3);
								character.MoodType = (byte)packet.ReadByte();
								packet.Skip(89); // Skip location string, race string, classe string, level ,class ,realm and startRaceGender
								character.CreationModel = packet.ReadShortLowEndian(); //read new model
								character.Model = character.CreationModel;
								packet.Skip(58); // skip all other things

								character.CustomisationStep = 2; // disable config button

								GameServer.Database.SaveObject(character);

								if (log.IsInfoEnabled)
									log.Info(String.Format("Character {0} face proprieties configured by account {1}!\n",charname,client.Account.AccountName));
							}
						}
							break;
						case 0x03:  //auto config
						{
							if(character.CustomisationStep == 1)
							{
								character.EyeSize = (byte)packet.ReadByte();
								character.LipSize = (byte)packet.ReadByte();
								character.EyeColor = (byte)packet.ReadByte();
								character.HairColor = (byte)packet.ReadByte();
								character.FaceType = (byte)packet.ReadByte();
								character.HairStyle = (byte)packet.ReadByte();
								packet.Skip(3);
								character.MoodType = (byte)packet.ReadByte();
								packet.Skip(149); // skip all other things

								character.CustomisationStep = 3; // enable config button to player

								GameServer.Database.SaveObject(character);

								if (log.IsInfoEnabled)
									log.Info(String.Format("Character {0} face proprieties auto updated!\n",charname));
						
							}
						}
							break;
						default :  //do nothing
						{
							packet.Skip(159);
						}
							break;
					}

				}
				else
				{
					// create new character
					GamePlayer ch = new GamePlayer();
					ch.Name = charname;

					if(packet.ReadByte() == 0x01)
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
					}
					else
					{
						packet.Skip(23);
					}

					packet.Skip(24); //Location String
					ch.LastName = "";
					packet.Skip(24); //Skip class name
					packet.Skip(24); //Skip race name
					ch.Level = (byte)packet.ReadByte(); //not safe!
					ch.Level = 1;
					ch.CharacterClassID = packet.ReadByte();
					ch.Realm = (byte)packet.ReadByte();
					ch.SlotPosition = i;

					ch.MaxSpeedBase = GamePlayer.PLAYER_BASE_SPEED;
					
					//The following byte contains
					//1bit=start location ... in SI you can choose ...
					//1bit=first race bit
					//1bit=unknown
					//1bit=gender (0=male, 1=female)
					//4bit=race
					byte startRaceGender = (byte)packet.ReadByte();

					ch.Race = (startRaceGender&0x0F) + ((startRaceGender&0x40)>>2);
					ch.Gender = ((startRaceGender>>4)&0x01);
					//DOLConsole.WriteLine("startRaceGender="+startRaceGender+"; Race="+ch.Race+"; Gender="+ch.Gender);

					bool siStartLocation = ((startRaceGender>>7)!=0);

					ch.CreationModel = packet.ReadShortLowEndian();
					ch.Model = ch.CreationModel;
					/*ch.Region =*/ packet.ReadByte();
					packet.Skip(1); //TODO second byte of region unused currently
					packet.Skip(4); //TODO Unknown Int / last used?
					ch.BaseStrength = packet.ReadByte();
					ch.BaseDexterity = packet.ReadByte();
					ch.BaseConstitution = packet.ReadByte();
					ch.BaseQuickness = packet.ReadByte();
					ch.BaseIntelligence = packet.ReadByte();
					ch.BasePiety = packet.ReadByte();
					ch.BaseEmpathy = packet.ReadByte();
					ch.BaseCharisma = packet.ReadByte();
					packet.Skip(44); //TODO equipment

					ch.CreationDate = DateTime.Now;

					ch.Position = new Point(505603, 494709, 2463);
					ch.Heading = 5947;

					switch(ch.Realm)
					{
						case (byte)eRealm.Albion :
						{
							if(siStartLocation)//Albion SI start point (I hope)
							{
								ch.Position = new Point(526252, 542415, 3165);
								ch.Heading = 5286;
								ch.Region = WorldMgr.GetRegion(51);
							}
							else//Albion start point (Church outside Camelot/humberton)
							{
								ch.Position = new Point(505603, 494709, 2463);
								ch.Heading = 5947;
								ch.Region = WorldMgr.GetRegion(1);
							}
							break;
						}
						case (byte)eRealm.Midgard :
						{
							if(siStartLocation)//Midgard SI start point
							{
								ch.Position = new Point(293720, 356408, 3488);
								ch.Heading = 6670;
								ch.Region = WorldMgr.GetRegion(151);
							}
							else//Midgard start point (Fort Atla)
							{
								ch.Position = new Point(749103, 815835, 4408);
								ch.Heading = 7915;
								ch.Region = WorldMgr.GetRegion(100);
							}
							break;
						}
						case (byte)eRealm.Hibernia :
						{
							if(siStartLocation)//Hibernia SI start point
							{
								ch.Position = new Point(426483, 440626, 5952);
								ch.Heading = 2403;
								ch.Region = WorldMgr.GetRegion(181);
							}
							else//Hibernia start point (Mag Mel)
							{
								ch.Position = new Point(345900, 490867, 5200);
								ch.Heading = 4826;
								ch.Region = WorldMgr.GetRegion(200);
							}
							break;
						}
					}

                    // chars are bound on creation
					ch.BindRegion = ch.Region;
					ch.BindHeading = ch.Heading;
					ch.BindPosition = ch.Position;

					ch.IsLevelRespecUsed = true;
					ch.SafetyFlag = true;

					ch.Inventory = new GamePlayerInventory();
					((GamePlayerInventory)ch.Inventory).Owner = ch;

					// check if client tried to create invalid char
					if(CheckCharacter.IsCharacterValid(ch))
					{
						//Fire the character creation event
						GameEventMgr.Notify(DatabaseEvent.CharacterCreated, null, new CharacterEventArgs(ch));
						//Save the character in the database
						ch.AccountId = client.Account.AccountId;
						GameServer.Database.AddNewObject(ch);
						
						client.Account.AddCharacter(ch);

						if (log.IsInfoEnabled)
							log.Info(String.Format("Character {0} created!\n",charname));
					}
					else
					{
						if (log.IsWarnEnabled)
							log.Warn(client.Account.AccountName + " tried to create invalid character:" +
								"\nchar name="+ch.Name+", race="+ch.Race+", realm="+ch.Realm+", class="+ch.CharacterClassID+", region="+ch.Region+
								"\nstr="+ch.BaseStrength+", con="+ch.BaseConstitution+", dex="+ch.BaseDexterity+", qui="+ch.BaseQuickness+", int="+ch.BaseIntelligence+", pie="+ch.BasePiety+", emp="+ch.BaseEmpathy+", chr="+ch.BaseCharisma);

						if (client.Account.Realm == 0) client.Out.SendRealm(eRealm.None);
						else client.Out.SendCharacterOverview((eRealm)client.Account.Realm);
					}

					break;
				}
			}
			
			return 1;
		}

		#region CheckCharacter

		/// <summary>
		/// Provides methods to handle char creation checks
		/// </summary>
		protected class CheckCharacter
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
			public static bool IsCharacterValid(GamePlayer ch)
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
					if (Array.IndexOf(CLASSES_BY_REALM[ch.Realm], ch.CharacterClassID) == -1)
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong class: " + ch.CharacterClassID + ", realm:" + ch.Realm);
						valid = false;
					}
					if (Array.IndexOf(CLASSES_BY_RACE[ch.Race], ch.CharacterClassID) == -1)
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong race: " + ch.Race + ", class:" + ch.CharacterClassID);
						valid = false;
					}
					int pointsUsed = 0;
					pointsUsed += PointsUsed(ch.Race, STR, ch.BaseStrength);
					pointsUsed += PointsUsed(ch.Race, CON, ch.BaseConstitution);
					pointsUsed += PointsUsed(ch.Race, DEX, ch.BaseDexterity);
					pointsUsed += PointsUsed(ch.Race, QUI, ch.BaseQuickness);
					pointsUsed += PointsUsed(ch.Race, INT, ch.BaseIntelligence);
					pointsUsed += PointsUsed(ch.Race, PIE, ch.BasePiety);
					pointsUsed += PointsUsed(ch.Race, EMP, ch.BaseEmpathy);
					pointsUsed += PointsUsed(ch.Race, CHA, ch.BaseCharisma);

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

			/// <summary>
			/// All possible player races
			/// </summary>
			protected static readonly int[][] STARTING_STATS = new int[][]
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
			};

			/// <summary>
			/// All possible player starting classes
			/// </summary>
			protected static readonly int[][] CLASSES_BY_REALM = new int[][]
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

			/// <summary>
			/// All the possible classes by race
			/// </summary>
			protected static readonly int[][] CLASSES_BY_RACE = new int[][]
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
			};
		}

		#endregion

	}
}