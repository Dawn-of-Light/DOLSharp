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
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.CharacterCreateRequest, "Handles character creation requests", eClientStatus.LoggedIn)]
	public class CharacterCreateRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// Max Points to allow on player creation
		/// </summary>
		public const int MAX_STARTING_BONUS_POINTS = 30;
		
		/// <summary>
		/// All possible player races
		/// </summary>
		public static readonly Dictionary<eRace, Dictionary<eStat, int>> STARTING_STATS_DICT = new Dictionary<eRace, Dictionary<eStat, int>>()
		{ 
			{ eRace.Unknown, new Dictionary<eStat, int>()			{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Briton, new Dictionary<eStat, int>()			{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Avalonian, new Dictionary<eStat, int>()			{{eStat.STR, 45}, {eStat.CON, 45}, {eStat.DEX, 60}, {eStat.QUI, 70}, {eStat.INT, 80}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Highlander, new Dictionary<eStat, int>()		{{eStat.STR, 70}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Saracen, new Dictionary<eStat, int>()			{{eStat.STR, 50}, {eStat.CON, 50}, {eStat.DEX, 80}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Norseman, new Dictionary<eStat, int>()			{{eStat.STR, 70}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Troll, new Dictionary<eStat, int>()				{{eStat.STR, 100}, {eStat.CON, 70}, {eStat.DEX, 35}, {eStat.QUI, 35}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Dwarf, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 80}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Kobold, new Dictionary<eStat, int>()			{{eStat.STR, 50}, {eStat.CON, 50}, {eStat.DEX, 70}, {eStat.QUI, 70}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Celt, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Firbolg, new Dictionary<eStat, int>()			{{eStat.STR, 90}, {eStat.CON, 60}, {eStat.DEX, 40}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 70}, {eStat.CHR, 60}, }},
			{ eRace.Elf, new Dictionary<eStat, int>()				{{eStat.STR, 40}, {eStat.CON, 40}, {eStat.DEX, 75}, {eStat.QUI, 75}, {eStat.INT, 70}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Lurikeen, new Dictionary<eStat, int>()			{{eStat.STR, 40}, {eStat.CON, 40}, {eStat.DEX, 80}, {eStat.QUI, 80}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Inconnu, new Dictionary<eStat, int>()			{{eStat.STR, 50}, {eStat.CON, 60}, {eStat.DEX, 70}, {eStat.QUI, 50}, {eStat.INT, 70}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Valkyn, new Dictionary<eStat, int>()			{{eStat.STR, 55}, {eStat.CON, 45}, {eStat.DEX, 65}, {eStat.QUI, 75}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Sylvan, new Dictionary<eStat, int>()			{{eStat.STR, 70}, {eStat.CON, 60}, {eStat.DEX, 55}, {eStat.QUI, 45}, {eStat.INT, 70}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.HalfOgre, new Dictionary<eStat, int>()			{{eStat.STR, 90}, {eStat.CON, 70}, {eStat.DEX, 40}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Frostalf, new Dictionary<eStat, int>()			{{eStat.STR, 55}, {eStat.CON, 55}, {eStat.DEX, 55}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 75}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Shar, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 80}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.AlbionMinotaur, new Dictionary<eStat, int>()	{{eStat.STR, 80}, {eStat.CON, 50}, {eStat.DEX, 40}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.MidgardMinotaur, new Dictionary<eStat, int>()	{{eStat.STR, 80}, {eStat.CON, 50}, {eStat.DEX, 40}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.HiberniaMinotaur, new Dictionary<eStat, int>()	{{eStat.STR, 80}, {eStat.CON, 50}, {eStat.DEX, 40}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
		};

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			string accountName = packet.ReadString(24);

			if (log.IsDebugEnabled)
				log.DebugFormat("CharacterCreateRequestHandler for account {0} using version {1}", accountName, client.Version);

			if (!accountName.StartsWith(client.Account.Name))// TODO more correctly check, client send accountName as account-S, -N, -H (if it not fit in 20, then only account)
			{
				if (ServerProperties.Properties.BAN_HACKERS)
					client.BanAccount(string.Format("Autoban wrong Account '{0}'", accountName));

				client.Disconnect();
				return;
			}
			
			// Client character count support
			int charsCount = client.Version < GameClient.eClientVersion.Version173 ? 8 : 10;
			
			for (int i = 0; i < charsCount; i++)
			{
				//unk - probably indicates customize or create
				if (client.Version >= GameClient.eClientVersion.Version1104)
					packet.ReadIntLowEndian();

				string charName = packet.ReadString(24);

				//log.DebugFormat("Character[{0}] = {1}", i, charName);

				if (charName.Length == 0)
				{
					// 1.104+  if character is not in list but is in DB then delete the character
					if (client.Version >= GameClient.eClientVersion.Version1104)
						CheckForDeletedCharacter(accountName, client, i);
					
					var consume = new CreationCharacterData(packet, client);
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
								client.BanAccount(string.Format("Autoban bad CharName '{0}'", charName));

							client.Disconnect();
							return;
						}
					}

					var character = client.Account.Characters.FirstOrDefault(ch => ch.Name.Equals(charName));

					// check for update to existing character
					if (character != null)
						CheckCharacterForUpdates(new CreationCharacterData(packet, client), client, character);
					// create new character and return
					else
						CreateCharacter(new CreationCharacterData(packet, client), client, charName, i);
				}
			}
		}
		
		#region caracter creation data
		class CreationCharacterData
		{
			public int CustomMode { get; set; }
			public int EyeSize { get; set; }
			public int LipSize { get; set; }
			public int EyeColor { get; set; }
			public int HairColor { get; set; }
			public int FaceType { get; set; }
			public int HairStyle { get; set; }
			public int MoodType { get; set; }
			public int Class { get; set; }
			public int Realm { get; set; }
			public int Race { get; set; }
			public int Gender { get; set; }
			public bool SIStartLocation { get; set; }
			public ushort CreationModel { get; set; }
			public int Region { get; set; }
			
			public int Strength { get; set; }
			public int Dexterity { get; set; }
			public int Constitution { get; set; }
			public int Quickness { get; set; }
			public int Intelligence { get; set; }
			public int Piety { get; set; }
			public int Empathy { get; set; }
			public int Charisma { get; set; }
			public int NewConstitution { get; set; }
			
			public int ConstitutionDiff { get { return NewConstitution - Constitution; }}
			
			/// <summary>
			/// Reads up ONE character iteration on the packet stream
			/// </summary>
			/// <param name="packet"></param>
			/// <param name="client"></param>
			public CreationCharacterData(GSPacketIn packet, GameClient client)
			{
				CustomMode = packet.ReadByte();
				EyeSize = packet.ReadByte();
				LipSize = packet.ReadByte();
				EyeColor = packet.ReadByte();
				HairColor = packet.ReadByte();
				FaceType = packet.ReadByte();
				HairStyle = packet.ReadByte();
				packet.Skip(3);
				MoodType = packet.ReadByte();
				packet.Skip(13);
				
				packet.Skip(24); //Location String
				packet.Skip(24); //Skip class name
				packet.Skip(24); //Skip race name
				
				var level = packet.ReadByte(); //not safe!
				Class = packet.ReadByte();
				Realm = packet.ReadByte();
				
				//The following byte contains
				//1bit=start location ... in ShroudedIsles you can choose ...
				//1bit=first race bit
				//1bit=unknown
				//1bit=gender (0=male, 1=female)
				//4bit=race
				byte startRaceGender = (byte)packet.ReadByte();
				Race = (startRaceGender & 0x0F) + ((startRaceGender & 0x40) >> 2);
				Gender = ((startRaceGender >> 4) & 0x01);
				SIStartLocation = ((startRaceGender >> 7) != 0);

				CreationModel = packet.ReadShortLowEndian();
				Region = packet.ReadByte();
				packet.Skip(1); //TODO second byte of region unused currently
				packet.Skip(4); //TODO Unknown Int / last used?

				Strength = packet.ReadByte();
				Dexterity = packet.ReadByte();
				Constitution = packet.ReadByte();
				Quickness = packet.ReadByte();
				Intelligence = packet.ReadByte();
				Piety = packet.ReadByte();
				Empathy = packet.ReadByte();
				Charisma = packet.ReadByte();

				packet.Skip(40); //TODO equipment
				
				var activeRightSlot = packet.ReadByte(); // 0x9C
				var activeLeftSlot = packet.ReadByte(); // 0x9D
				var siZone = packet.ReadByte(); // 0x9E
				
				// skip 4 bytes added in 1.99
				if (client.Version >= GameClient.eClientVersion.Version199 && client.Version < GameClient.eClientVersion.Version1104)
					packet.Skip(4);
				
				// New constitution must be read before skipping 4 bytes
				NewConstitution = packet.ReadByte(); // 0x9F


			}
		}
		#endregion

		#region Create Character
		private void CreateCharacter(CreationCharacterData pdata, GameClient client, string charName, int accountSlot)
		{
			Account account = client.Account;
			var ch = new DOLCharacters();
			ch.AccountName = account.Name;
			ch.Name = charName;
			
			if (pdata.CustomMode == 0x01)
			{
				ch.EyeSize = (byte)pdata.EyeSize;
				ch.LipSize = (byte)pdata.LipSize;
				ch.EyeColor = (byte)pdata.EyeColor;
				ch.HairColor = (byte)pdata.HairColor;
				ch.FaceType = (byte)pdata.FaceType;
				ch.HairStyle = (byte)pdata.HairStyle;
				ch.MoodType = (byte)pdata.MoodType;
				ch.CustomisationStep = 2; // disable config button
				
				if (log.IsDebugEnabled)
					log.Debug("Disable Config Button");
			}
			
			ch.Level = 1;
			// Set Realm and Class
			ch.Realm = pdata.Realm;
			ch.Class = pdata.Class;
			
			// Set Account Slot, Gender
			ch.AccountSlot = accountSlot + ch.Realm * 100;
			ch.Gender = pdata.Gender;

			// Set Race
			ch.Race = pdata.Race;
			
			ch.CreationModel = pdata.CreationModel;
			ch.CurrentModel = ch.CreationModel;
			ch.Region = pdata.Region;

			ch.Strength = pdata.Strength;
			ch.Dexterity = pdata.Dexterity;
			ch.Constitution = pdata.Constitution;
			ch.Quickness = pdata.Quickness;
			ch.Intelligence = pdata.Intelligence;
			ch.Piety = pdata.Piety;
			ch.Empathy = pdata.Empathy;
			ch.Charisma = pdata.Charisma;
			
			// defaults
			ch.CreationDate = DateTime.Now;

			ch.Endurance = 100;
			ch.MaxEndurance = 100;
			ch.Concentration = 100;
			ch.MaxSpeed = GamePlayer.PLAYER_BASE_SPEED;
			
			if (log.IsDebugEnabled)
				log.DebugFormat("Creation {0} character, class:{1}, realm:{2}", client.Version, ch.Class, ch.Realm);
			
			// Is class disabled ?
			int occurences = 0;
			List<string> disabled_classes = Properties.DISABLED_CLASSES.SplitCSV(true);
			occurences = (from j in disabled_classes
			              where j == ch.Class.ToString()
			              select j).Count();

			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0} tried to create a disabled classe: {1}", client.Account.Name, (eCharacterClass)ch.Class);
				
				// Reset to character view...
				client.Out.SendCharacterOverview((eRealm)ch.Realm);
			    return;
			}
			
			// check if race disabled
			List<string> disabled_races = Properties.DISABLED_RACES.SplitCSV(true);
			occurences = (from j in disabled_races
			              where j == ch.Race.ToString()
			              select j).Count();
			
			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0} tried to create a disabled race: {1}", client.Account.Name, (eRace)ch.Race);
				// Reset to character view...
				client.Out.SendCharacterOverview((eRealm)ch.Realm);
			    return;
			}

			
			// If sending invalid Class ID
			if (!Enum.IsDefined(typeof(eCharacterClass), (eCharacterClass)ch.Class))
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("{0} tried to create a character with wrong class ID: {1}, realm:{2}", client.Account.Name, ch.Class, ch.Realm);
				
				if (ServerProperties.Properties.BAN_HACKERS)
				{
					client.BanAccount(string.Format("Autoban character create class: id:{0} realm:{1} name:{2} account:{3}", ch.Class, ch.Realm, ch.Name, account.Name));
					client.Disconnect();
				}
			    return;
			}

			// check if client tried to create invalid char
			if (!ValidateCharacter.IsCharacterValid(ch))
			{
				if (log.IsWarnEnabled)
				{
					log.WarnFormat("{0} tried to create invalid character:\nchar name={1}, gender={2}, race={2}, realm={3}, class={4}, region={5}" +
					               "\nstr={6}, con={7}, dex={8}, qui={9}, int={10}, pie={11}, emp={12}, chr={13}", ch.AccountName, ch.Name, ch.Gender,
					              ch.Race, ch.Realm, ch.Class, ch.Region, ch.Strength, ch.Constitution, ch.Dexterity, ch.Quickness, ch.Intelligence, ch.Piety, ch.Empathy, ch.Charisma);
				}
				// This is not live like but unfortunately we are missing code / packet support to stay on character create screen if something is invalid
				client.Out.SendCharacterOverview((eRealm)ch.Realm);
			    return;
			}


			SetBasicCraftingForNewCharacter(ch);

			//Save the character in the database
			GameServer.Database.AddObject(ch);
			
			// Fire the character creation event
			// This is Where Most Creation Script should take over to update any data they would like !
			GameEventMgr.Notify(DatabaseEvent.CharacterCreated, null, new CharacterEventArgs(ch, client));
			
			//write changes
			GameServer.Database.SaveObject(ch);

			// Log creation
			AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.CharacterCreate, "", charName);

			client.Account.Characters = null;

			if (log.IsInfoEnabled)
				log.InfoFormat("Character {0} created!", charName);

			// Reload Account Relations
			GameServer.Database.FillObjectRelations(client.Account);
			client.Out.SendCharacterOverview((eRealm)ch.Realm);

		    return;
		}
		
		#endregion Create Character


		#region Character Updates
		/// <summary>
		/// Check if a Character Needs update based to packet data
		/// </summary>
		/// <param name="pdata">packet data</param>
		/// <param name="client">client</param>
		/// <param name="character">db character</param>
		/// <returns>went ok</returns>
		private bool CheckCharacterForUpdates(CreationCharacterData pdata, GameClient client, DOLCharacters character)
		{
			int newModel = character.CurrentModel;

			if (pdata.CustomMode == 1 || pdata.CustomMode == 2 || pdata.CustomMode == 3)
			{
				bool flagChangedStats = false;
				character.EyeSize = (byte)pdata.EyeSize;
				character.LipSize = (byte)pdata.LipSize;
				character.EyeColor = (byte)pdata.EyeColor;
				character.HairColor = (byte)pdata.HairColor;
				character.FaceType = (byte)pdata.FaceType;
				character.HairStyle = (byte)pdata.HairStyle;
				character.MoodType = (byte)pdata.MoodType;
				
				if (pdata.CustomMode != 3 && client.Version >= GameClient.eClientVersion.Version189)
				{
					var stats = new Dictionary<eStat, int>();
					stats[eStat.STR] = pdata.Strength; // Strength
					stats[eStat.DEX] = pdata.Dexterity; // Dexterity
					stats[eStat.CON] = pdata.NewConstitution; // New Constitution
					stats[eStat.QUI] = pdata.Quickness; // Quickness
					stats[eStat.INT] = pdata.Intelligence; // Intelligence
					stats[eStat.PIE] = pdata.Piety; // Piety
					stats[eStat.EMP] = pdata.Empathy; // Empathy
					stats[eStat.CHR] = pdata.Charisma; // Charisma

					// check for changed stats.
					flagChangedStats |= stats[eStat.STR] != character.Strength;
					flagChangedStats |= stats[eStat.CON] != character.Constitution;
					flagChangedStats |= stats[eStat.DEX] != character.Dexterity;
					flagChangedStats |= stats[eStat.QUI] != character.Quickness;
					flagChangedStats |= stats[eStat.INT] != character.Intelligence;
					flagChangedStats |= stats[eStat.PIE] != character.Piety;
					flagChangedStats |= stats[eStat.EMP] != character.Empathy;
					flagChangedStats |= stats[eStat.CHR] != character.Charisma;

					if (flagChangedStats)
					{
						ICharacterClass charClass = ScriptMgr.FindCharacterClass(character.Class);

						if (charClass != null)
						{
							int points;							
							bool valid = ValidateCharacter.IsCustomPointsDistributionValid(character, stats, out points);

							// Hacking attemp ?
							if (points > MAX_STARTING_BONUS_POINTS)
							{
								if ((ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
								{
									if (ServerProperties.Properties.BAN_HACKERS)
										client.BanAccount(string.Format("Autoban Hack char update : Wrong allowed points:{0}", points));

									client.Disconnect();
									return false;
								}
							}
							
							// Error in setting points
							if (!valid)
							{
								// This is not live like but unfortunately we are missing code / packet support to stay on character create screen if something is invalid
								client.Out.SendCharacterOverview((eRealm)character.Realm);
							    return false;
							}
							
							// Set Stats, valid is ok.
							character.Strength = (byte)stats[eStat.STR];
							character.Constitution = (byte)stats[eStat.CON];
							character.Dexterity = (byte)stats[eStat.DEX];
							character.Quickness = (byte)stats[eStat.QUI];
							character.Intelligence = (byte)stats[eStat.INT];
							character.Piety = (byte)stats[eStat.PIE];
							character.Empathy = (byte)stats[eStat.EMP];
							character.Charisma = (byte)stats[eStat.CHR];

							if (log.IsInfoEnabled)
								log.InfoFormat("Character {0} updated in cache!\n", character.Name);

							if (client.Player != null)
							{
								client.Player.DBCharacter.Strength = (byte)stats[eStat.STR];
								client.Player.DBCharacter.Constitution = (byte)stats[eStat.CON];
								client.Player.DBCharacter.Dexterity = (byte)stats[eStat.DEX];
								client.Player.DBCharacter.Quickness = (byte)stats[eStat.QUI];
								client.Player.DBCharacter.Intelligence = (byte)stats[eStat.INT];
								client.Player.DBCharacter.Piety = (byte)stats[eStat.PIE];
								client.Player.DBCharacter.Empathy = (byte)stats[eStat.EMP];
								client.Player.DBCharacter.Charisma = (byte)stats[eStat.CHR];
							}
						}
						else if (log.IsErrorEnabled)
							log.ErrorFormat("No CharacterClass with ID {0} found", character.Class);
					}
				}


				if (pdata.CustomMode == 2) // change player customization
				{
					if (ServerProperties.Properties.BAN_HACKERS && client.Account.PrivLevel == 1 && ((pdata.CreationModel >> 11) & 3) == 0) // Player size must be > 0 (from 1 to 3)
					{
						client.BanAccount(string.Format("Autoban Hack char update : zero character size in model:{0}", newModel));
						client.Disconnect();
						return false;
					}

					if (pdata.CreationModel != character.CreationModel)
						character.CurrentModel = newModel;

					character.CustomisationStep = 2; // disable config button

					if (log.IsInfoEnabled)
						log.InfoFormat("Character {0} face proprieties configured by account {1}!\n", character.Name, client.Account.Name);
				}
				else if (pdata.CustomMode == 3) //auto config -- seems someone thinks this is not possible?
				{
					character.CustomisationStep = 3; // enable config button to player
					return true;
				}
				else if (log.IsInfoEnabled && pdata.CustomMode == 1 && flagChangedStats) //changed stat only for 1.89+
					log.InfoFormat("Character {0} stat updated!\n", character.Name);
			}
			
			//Save the character in the database
			GameServer.Database.SaveObject(character);

			return true;
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
			public static bool IsCustomPointsDistributionValid(DOLCharacters character, IDictionary<eStat, int> stats, out int points)
			{
				ICharacterClass charClass = ScriptMgr.FindCharacterClass(character.Class);

				if (charClass != null)
				{
					points = 0;							
					
					// check if each stat is valid.
					foreach(var stat in stats.Keys)
					{
						int raceAmount = STARTING_STATS_DICT[(eRace)character.Race][stat];
						
						int classAmount = 0;
						
						for (int level = character.Level; level > 5; level--)
						{
							if (charClass.PrimaryStat != eStat.UNDEFINED && charClass.PrimaryStat == stat)
								classAmount++;
							if (charClass.SecondaryStat != eStat.UNDEFINED && charClass.SecondaryStat == stat && (level - 6) % 2 == 0)
								classAmount++;
							if (charClass.TertiaryStat != eStat.UNDEFINED && charClass.TertiaryStat == stat && (level - 6) % 3 == 0)
								classAmount++;
						}
						
						int above = stats[stat] - raceAmount - classAmount;
						
						// Miss Some points...
						if (above < 0)
							return false;
						
						points += above;
						points += Math.Max(0, above - 10); //two points used
						points += Math.Max(0, above - 15); //three points used
					}
					
					return points != MAX_STARTING_BONUS_POINTS;
				}
				
				points = -1;
				return false;
			}

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
					if (!STARTING_CLASSES_DICT.ContainsKey((eRealm)ch.Realm) || !STARTING_CLASSES_DICT[(eRealm)ch.Realm].Contains((eCharacterClass)ch.Class))
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong class: " + ch.Class + ", realm:" + ch.Realm);
						valid = false;
					}
					if (!RACES_CLASSES_DICT.ContainsKey((eRace)ch.Race) || !RACES_CLASSES_DICT[(eRace)ch.Race].Contains((eCharacterClass)ch.Class))
					{
						if (log.IsWarnEnabled)
							log.Warn("Wrong race: " + ch.Race + ", class:" + ch.Class);
						valid = false;
					}
					int pointsUsed;
					var stats = new Dictionary<eStat, int>{{eStat.STR, ch.Strength},{eStat.CON, ch.Constitution},{eStat.DEX, ch.Dexterity},{eStat.QUI, ch.Quickness},
						{eStat.INT, ch.Intelligence},{eStat.PIE, ch.Piety},{eStat.EMP, ch.Empathy},{eStat.CHR, ch.Charisma},};
					
					valid = IsCustomPointsDistributionValid(ch, stats, out pointsUsed);
					
					if (pointsUsed != MAX_STARTING_BONUS_POINTS)
					{
						if (log.IsWarnEnabled)
						{
							log.Warn("Points used: " + pointsUsed);
						}

						valid = false;
					}

					if (ch.Gender > 0 && (ch.Race == (byte)eRace.Korazh || ch.Race == (byte)eRace.Deifrang || ch.Race == (byte)eRace.Graoch))
					{
						log.Warn("Wrong minotaur gender: " + ch.Gender + ", race: " + ch.Race);
						valid = false;
					}

					if (ch.Gender == 0 && (ch.Class == (int)eCharacterClass.Bainshee || ch.Class == (int)eCharacterClass.Valkyrie))
					{
						log.Warn("Wrong class gender: " + ch.Gender + ", class:" + ch.Class);
						valid = false;
					}

				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
					{
						log.Error(string.Format("CharacterCreation error on account {0}, slot {1}.", ch.AccountName, ch.AccountSlot), e);
					}

					valid = false;
				}

				return valid;
			}


			/// <summary>
			/// All possible player starting classes
			/// </summary>
			public static Dictionary<eRealm, List<eCharacterClass>> STARTING_CLASSES_DICT = new Dictionary<eRealm, List<eCharacterClass>>()
			{
				// pre 1.93
				{eRealm.Albion, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
					// post 1.93
					eCharacterClass.Paladin, 		// Paladin = 1,
					eCharacterClass.Armsman, 		// Armsman = 2,
					eCharacterClass.Scout, 	    // Scout = 3,
					eCharacterClass.Minstrel, 	    // Minstrel = 4,
					eCharacterClass.Theurgist, 	// Theurgist = 5,
					eCharacterClass.Cleric, 		// Cleric = 6,
					eCharacterClass.Wizard, 	    // Wizard = 7,
					eCharacterClass.Sorcerer, 		// Sorcerer = 8,
					eCharacterClass.Infiltrator, 	// Infiltrator = 9,
					eCharacterClass.Friar, 		// Friar = 10,
					eCharacterClass.Mercenary, 	// Mercenary = 11,
					eCharacterClass.Necromancer, 	// Necromancer = 12,
					eCharacterClass.Cabalist, 		// Cabalist = 13,
					eCharacterClass.Fighter, 		// Fighter = 14,
					eCharacterClass.Elementalist, 	// Elementalist = 15,
					eCharacterClass.Acolyte, 		// Acolyte = 16,
					eCharacterClass.AlbionRogue, 	// AlbionRogue = 17,
					eCharacterClass.Mage, 			// Mage = 18,
					eCharacterClass.Reaver, 		// Reaver = 19,
					eCharacterClass.Disciple,		// Disciple = 20,
					eCharacterClass.Heretic, 		// Heretic = 33,
					eCharacterClass.MaulerAlb		// Mauler_Alb = 60,
				}},
				{eRealm.Midgard, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Thane, 		// Thane = 21,
					eCharacterClass.Warrior, 		// Warrior = 22,
					eCharacterClass.Shadowblade, 	// Shadowblade = 23,
					eCharacterClass.Skald, 		// Skald = 24,
					eCharacterClass.Hunter, 	    // Hunter = 25,
					eCharacterClass.Healer, 		// Healer = 26,
					eCharacterClass.Spiritmaster,  // Spiritmaster = 27,
					eCharacterClass.Shaman, 		// Shaman = 28,
					eCharacterClass.Runemaster, 	// Runemaster = 29,
					eCharacterClass.Bonedancer, 	// Bonedancer = 30,
					eCharacterClass.Berserker, 	// Berserker = 31,
					eCharacterClass.Savage, 		// Savage = 32,
					eCharacterClass.Valkyrie, 		// Valkyrie = 34,
					eCharacterClass.Viking, 		// Viking = 35,
					eCharacterClass.Mystic, 		// Mystic = 36,
					eCharacterClass.Seer, 			// Seer = 37,
					eCharacterClass.MidgardRogue,	// MidgardRogue = 38,
					eCharacterClass.Warlock, 		// Warlock = 59,
					eCharacterClass.MaulerMid		// Mauler_Mid = 61,
				}},
				{eRealm.Hibernia, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Naturalist, eCharacterClass.Magician, eCharacterClass.Forester,
					// post 1.93
					eCharacterClass.Bainshee, 		// Bainshee = 39,
					eCharacterClass.Eldritch, 		// Eldritch = 40,
					eCharacterClass.Enchanter, 	// Enchanter = 41,
					eCharacterClass.Mentalist, 	// Mentalist = 42,
					eCharacterClass.Blademaster, 	// Blademaster = 43,
					eCharacterClass.Hero, 		    // Hero = 44,
					eCharacterClass.Champion, 		// Champion = 45,
					eCharacterClass.Warden, 	    // Warden = 46,
					eCharacterClass.Druid, 	    // Druid = 47,
					eCharacterClass.Bard, 	        // Bard = 48,
					eCharacterClass.Nightshade, 	// Nightshade = 49,
					eCharacterClass.Ranger, 		// Ranger = 50,
					eCharacterClass.Magician, 		// Magician = 51,
					eCharacterClass.Guardian, 		// Guardian = 52,
					eCharacterClass.Naturalist, 	// Naturalist = 53,
					eCharacterClass.Stalker, 		// Stalker = 54,
					eCharacterClass.Animist, 		// Animist = 55,
					eCharacterClass.Valewalker, 	// Valewalker = 56,
					eCharacterClass.Forester, 		// Forester = 57,
					eCharacterClass.Vampiir, 		// Vampiir = 58,
					eCharacterClass.MaulerHib	 	// Mauler_Hib = 62,
				}},
			};
			
			protected static Dictionary<eRace, List<eCharacterClass>> RACES_CLASSES_DICT = new Dictionary<eRace, List<eCharacterClass>>()
			{
				{eRace.Unknown, new List<eCharacterClass>()},
				// pre 1.93
				{eRace.Briton, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
					// post 1.93
					eCharacterClass.Armsman,
					eCharacterClass.Reaver,
					eCharacterClass.Mercenary,
					eCharacterClass.Paladin,
					eCharacterClass.Cleric,
					eCharacterClass.Heretic,
					eCharacterClass.Friar,
					eCharacterClass.Sorcerer,
					eCharacterClass.Cabalist,
					eCharacterClass.Theurgist,
					eCharacterClass.Necromancer,
					eCharacterClass.MaulerAlb,
					eCharacterClass.Wizard,
					eCharacterClass.Minstrel,
					eCharacterClass.Infiltrator,
					eCharacterClass.Scout,
					eCharacterClass.Fighter,
					eCharacterClass.Acolyte,
					eCharacterClass.Mage,
					eCharacterClass.Elementalist,
					eCharacterClass.AlbionRogue,
					eCharacterClass.Disciple
					}},
				{eRace.Avalonian, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist,
					// post 1.93
					eCharacterClass.Paladin,
					eCharacterClass.Cleric,
					eCharacterClass.Wizard,
					eCharacterClass.Theurgist,
					eCharacterClass.Armsman,
					eCharacterClass.Mercenary,
					eCharacterClass.Sorcerer,
					eCharacterClass.Cabalist,
					eCharacterClass.Heretic,
					eCharacterClass.Friar,
					eCharacterClass.Fighter,
					eCharacterClass.Acolyte,
					eCharacterClass.Mage,
					eCharacterClass.Elementalist
					}},
				{eRace.Highlander, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.AlbionRogue,
					// post 1.93
					eCharacterClass.Armsman,
					eCharacterClass.Mercenary,
					eCharacterClass.Paladin,
					eCharacterClass.Cleric,
					eCharacterClass.Minstrel,
					eCharacterClass.Scout,
					eCharacterClass.Friar,
					eCharacterClass.Fighter,
					eCharacterClass.Acolyte,
					eCharacterClass.AlbionRogue
					}},
				{eRace.Saracen, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Mage, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
					// post 1.93
					eCharacterClass.Sorcerer,
					eCharacterClass.Cabalist,
					eCharacterClass.Paladin,
					eCharacterClass.Reaver,
					eCharacterClass.Mercenary,
					eCharacterClass.Armsman,
					eCharacterClass.Infiltrator,
					eCharacterClass.Minstrel,
					eCharacterClass.Scout,
					eCharacterClass.Necromancer,
					eCharacterClass.Fighter,
					eCharacterClass.Mage,
					eCharacterClass.AlbionRogue,
					eCharacterClass.Disciple
					}},
				
				{eRace.Norseman, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Healer,
					eCharacterClass.Warrior,
					eCharacterClass.Berserker,
					eCharacterClass.Thane,
					eCharacterClass.Warlock,
					eCharacterClass.Skald,
					eCharacterClass.Valkyrie,
					eCharacterClass.Spiritmaster,
					eCharacterClass.Runemaster,
					eCharacterClass.Savage,
					eCharacterClass.MaulerMid,
					eCharacterClass.Shadowblade,
					eCharacterClass.Hunter,
					eCharacterClass.Viking,
					eCharacterClass.Mystic,
					eCharacterClass.Seer,
					eCharacterClass.MidgardRogue
					}},
				{eRace.Troll, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer,
					// post 1.93
					eCharacterClass.Berserker,
					eCharacterClass.Warrior,
					eCharacterClass.Savage,
					eCharacterClass.Thane,
					eCharacterClass.Skald,
					eCharacterClass.Bonedancer,
					eCharacterClass.Shaman,
					eCharacterClass.Viking,
					eCharacterClass.Mystic,
					eCharacterClass.Seer
					}},
				{eRace.Dwarf, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Healer,
					eCharacterClass.Thane,
					eCharacterClass.Berserker,
					eCharacterClass.Warrior,
					eCharacterClass.Savage,
					eCharacterClass.Skald,
					eCharacterClass.Valkyrie,
					eCharacterClass.Runemaster,
					eCharacterClass.Hunter,
					eCharacterClass.Shaman,
					eCharacterClass.Viking,
					eCharacterClass.Mystic,
					eCharacterClass.Seer,
					eCharacterClass.MidgardRogue
					}},
				{eRace.Kobold, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Shaman,
					eCharacterClass.Warrior,
					eCharacterClass.Skald,
					eCharacterClass.Savage,
					eCharacterClass.Runemaster,
					eCharacterClass.Spiritmaster,
					eCharacterClass.Bonedancer,
					eCharacterClass.Warlock,
					eCharacterClass.Hunter,
					eCharacterClass.Shadowblade,
					eCharacterClass.MaulerMid,
					eCharacterClass.Viking,
					eCharacterClass.Mystic,
					eCharacterClass.Seer,
					eCharacterClass.MidgardRogue
					}},
					
				{eRace.Celt, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Naturalist, eCharacterClass.Magician, eCharacterClass.Forester,
					// post 1.93
					eCharacterClass.Bard,
					eCharacterClass.Druid,
					eCharacterClass.Warden,
					eCharacterClass.Blademaster,
					eCharacterClass.Hero,
					eCharacterClass.Vampiir,
					eCharacterClass.Champion,
					eCharacterClass.MaulerHib,
					eCharacterClass.Mentalist,
					eCharacterClass.Bainshee,
					eCharacterClass.Ranger,
					eCharacterClass.Animist,
					eCharacterClass.Valewalker,
					eCharacterClass.Nightshade,
					eCharacterClass.Guardian,
					eCharacterClass.Stalker,
					eCharacterClass.Naturalist,
					eCharacterClass.Magician,
					eCharacterClass.Forester
					}},
				{eRace.Firbolg, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Naturalist, eCharacterClass.Forester,
					// post 1.93
					eCharacterClass.Bard,
					eCharacterClass.Druid,
					eCharacterClass.Warden,
					eCharacterClass.Hero,
					eCharacterClass.Blademaster,
					eCharacterClass.Animist,
					eCharacterClass.Valewalker,
					eCharacterClass.Guardian,
					eCharacterClass.Naturalist,
					eCharacterClass.Forester
					}},
				{eRace.Elf, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Magician,
					// post 1.93
					eCharacterClass.Blademaster,
					eCharacterClass.Champion,
					eCharacterClass.Ranger,
					eCharacterClass.Nightshade,
					eCharacterClass.Bainshee,
					eCharacterClass.Enchanter,
					eCharacterClass.Eldritch,
					eCharacterClass.Mentalist,
					eCharacterClass.Guardian,
					eCharacterClass.Stalker,
					eCharacterClass.Magician
					}},
				{eRace.Lurikeen, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Magician,
					// post 1.93
					eCharacterClass.Hero,
					eCharacterClass.Champion,
					eCharacterClass.Vampiir,
					eCharacterClass.Eldritch,
					eCharacterClass.Enchanter,
					eCharacterClass.Mentalist,
					eCharacterClass.Bainshee,
					eCharacterClass.Nightshade,
					eCharacterClass.Ranger,
					eCharacterClass.MaulerHib,
					eCharacterClass.Guardian,
					eCharacterClass.Stalker,
					eCharacterClass.Magician
					}},
				
				{eRace.Inconnu, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
					// post 1.93
					eCharacterClass.Reaver,
					eCharacterClass.Sorcerer,
					eCharacterClass.Cabalist,
					eCharacterClass.Heretic,
					eCharacterClass.Necromancer,
					eCharacterClass.Armsman,
					eCharacterClass.Mercenary,
					eCharacterClass.Infiltrator,
					eCharacterClass.Scout,
					eCharacterClass.MaulerAlb,
					eCharacterClass.Fighter,
					eCharacterClass.Acolyte,
					eCharacterClass.Mage,
					eCharacterClass.AlbionRogue,
					eCharacterClass.Disciple
					}},
				
				{eRace.Valkyn, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Savage,
					eCharacterClass.Berserker,
					eCharacterClass.Bonedancer,
					eCharacterClass.Warrior,
					eCharacterClass.Shadowblade,
					eCharacterClass.Hunter,
					eCharacterClass.Viking,
					eCharacterClass.Mystic,
					eCharacterClass.MidgardRogue
					}},
				
				{eRace.Sylvan, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Naturalist, eCharacterClass.Forester,
					// post 1.93
					eCharacterClass.Animist,
					eCharacterClass.Druid,
					eCharacterClass.Valewalker,
					eCharacterClass.Hero,
					eCharacterClass.Warden,
					eCharacterClass.Guardian,
					eCharacterClass.Naturalist,
					eCharacterClass.Forester
					}},
				
				{eRace.HalfOgre, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Mage, eCharacterClass.Elementalist,
					// post 1.93
					eCharacterClass.Wizard,
					eCharacterClass.Theurgist,
					eCharacterClass.Cabalist,
					eCharacterClass.Sorcerer,
					eCharacterClass.Mercenary,
					eCharacterClass.Armsman,
					eCharacterClass.Fighter,
					eCharacterClass.Mage,
					eCharacterClass.Elementalist
					}},
				
				{eRace.Frostalf, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Healer,
					eCharacterClass.Shaman,
					eCharacterClass.Thane,
					eCharacterClass.Spiritmaster,
					eCharacterClass.Runemaster,
					eCharacterClass.Warlock,
					eCharacterClass.Valkyrie,
					eCharacterClass.Hunter,
					eCharacterClass.Shadowblade,
					eCharacterClass.Viking,
					eCharacterClass.Mystic,
					eCharacterClass.Seer,
					eCharacterClass.MidgardRogue
					}},
				
				{eRace.Shar, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Magician,
					// post 1.93
					eCharacterClass.Champion,
					eCharacterClass.Hero,
					eCharacterClass.Blademaster,
					eCharacterClass.Vampiir,
					eCharacterClass.Ranger,
					eCharacterClass.Mentalist,
					eCharacterClass.Guardian,
					eCharacterClass.Stalker,
					eCharacterClass.Magician
					}},
				
				{eRace.AlbionMinotaur, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
					// post 1.93
					eCharacterClass.Heretic,
					eCharacterClass.MaulerAlb,
					eCharacterClass.Armsman,
					eCharacterClass.Mercenary,
					eCharacterClass.Fighter,
					eCharacterClass.Acolyte
					}},
				
				{eRace.MidgardMinotaur, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
					// post 1.93
					eCharacterClass.Berserker,
					eCharacterClass.MaulerMid,
					eCharacterClass.Thane,
					eCharacterClass.Viking,
					eCharacterClass.Warrior
					}},
				
				{eRace.HiberniaMinotaur, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Naturalist, eCharacterClass.Magician, eCharacterClass.Forester,
					// post 1.93
					eCharacterClass.Hero,
					eCharacterClass.Blademaster,
					eCharacterClass.MaulerHib,
					eCharacterClass.Warden,
					eCharacterClass.Guardian,
					eCharacterClass.Naturalist
					}},
			};

		}

		#endregion Validate Character


		#region Utility
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