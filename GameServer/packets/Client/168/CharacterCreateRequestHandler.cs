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
		public const int MaxStartingBonusPoints = 30;

		/// <summary>
		/// Client Operation Value.
		/// </summary>
		public enum eOperation : uint
		{
			Delete = 0x12345678,
			Create = 0x23456789,
			Customize = 0x3456789A,
			Unknown = 0x456789AB,
		}


		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Version >= GameClient.eClientVersion.Version1124) // 1124 support
			{
				_HandlePacket1124(client, packet);
				return;
			}

			var accountName = packet.ReadString(24);
			if (log.IsDebugEnabled)
				log.DebugFormat("CharacterCreateRequestHandler for account {0} using version {1}", accountName, client.Version);

			// TODO more correctly check, client send accountName as account-S, -N, -H (if it not fit in 20, then only account)
			if (!accountName.StartsWith(client.Account.Name))
			{
				if (Properties.BAN_HACKERS)
					client.BanAccount(string.Format("Autoban wrong Account '{0}'", accountName));

				client.Disconnect();
				return;
			}

			// Realm
			eRealm currentRealm = eRealm.None;
			if (accountName.EndsWith("-S")) currentRealm = eRealm.Albion;
			else if (accountName.EndsWith("-N")) currentRealm = eRealm.Midgard;
			else if (accountName.EndsWith("-H")) currentRealm = eRealm.Hibernia;


			// Client character count support
			int charsCount = client.Version < GameClient.eClientVersion.Version173 ? 8 : 10;
			bool needRefresh = false;

			for (int i = 0; i < charsCount; i++)
			{
				var pakdata = new CreationCharacterData(packet, client);

				// Graveen: changed the following to allow GMs to have special chars in their names (_,-, etc..)
				var nameCheck = new Regex("^[A-Z][a-zA-Z]");
				if (!string.IsNullOrEmpty(pakdata.CharName) && (pakdata.CharName.Length < 3 || !nameCheck.IsMatch(pakdata.CharName)))
				{
					if ((ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
					{
						if (ServerProperties.Properties.BAN_HACKERS)
							client.BanAccount(string.Format("Autoban bad CharName '{0}'", pakdata.CharName));

						client.Disconnect();
						return;
					}
				}

				switch ((eOperation)pakdata.Operation)
				{
					case eOperation.Delete:
						if (string.IsNullOrEmpty(pakdata.CharName))
						{
							// Deletion in 1.104+ check for removed character.
							needRefresh |= CheckForDeletedCharacter(accountName, client, i);
						}
						break;
					case eOperation.Customize:
						if (!string.IsNullOrEmpty(pakdata.CharName))
						{
							// Candidate for Customizing ?
							var character = client.Account.Characters != null ? client.Account.Characters.FirstOrDefault(ch => ch.Name.Equals(pakdata.CharName, StringComparison.OrdinalIgnoreCase)) : null;
							if (character != null)
								needRefresh |= CheckCharacterForUpdates(pakdata, client, character);
						}
						break;
					case eOperation.Create:
						if (!string.IsNullOrEmpty(pakdata.CharName))
						{
							// Candidate for Creation ?
							var character = client.Account.Characters != null ? client.Account.Characters.FirstOrDefault(ch => ch.Name.Equals(pakdata.CharName, StringComparison.OrdinalIgnoreCase)) : null;
							if (character == null)
								needRefresh |= CreateCharacter(pakdata, client, i);
						}
						break;
					default:
						break;
				}
			}

			if (needRefresh)
			{
				client.Out.SendCharacterOverview(currentRealm);
			}
		}

		#region caracter creation data
		struct CreationCharacterData
		{
			public readonly string CharName;
			public readonly int CustomMode;
			public readonly int EyeSize;
			public readonly int LipSize;
			public readonly int EyeColor;
			public readonly int HairColor;
			public readonly int FaceType;
			public readonly int HairStyle;
			public readonly int MoodType;
			public readonly uint Operation;
			public readonly int Class;
			public readonly int Realm;
			public readonly int Race;
			public readonly int Gender;
			public readonly bool SIStartLocation;
			public readonly ushort CreationModel;
			public readonly int Region;

			public readonly int Strength;
			public readonly int Dexterity;
			public readonly int Constitution;
			public readonly int Quickness;
			public readonly int Intelligence;
			public readonly int Piety;
			public readonly int Empathy;
			public readonly int Charisma;
			public readonly int NewConstitution;

			public int ConstitutionDiff { get { return NewConstitution - Constitution; } }

			/// <summary>
			/// Reads up ONE character iteration on the packet stream
			/// </summary>
			/// <param name="packet"></param>
			/// <param name="client"></param>
			public CreationCharacterData(GSPacketIn packet, GameClient client)
			{
				//unk - probably indicates customize or create (these are moved from 1.99 4 added bytes)
				if (client.Version >= GameClient.eClientVersion.Version1104)
					packet.ReadIntLowEndian();

				CharName = packet.ReadString(24);
				CustomMode = packet.ReadByte();
				EyeSize = packet.ReadByte();
				LipSize = packet.ReadByte();
				EyeColor = packet.ReadByte();
				HairColor = packet.ReadByte();
				FaceType = packet.ReadByte();
				HairStyle = packet.ReadByte();
				packet.Skip(3);
				MoodType = packet.ReadByte();
				packet.Skip(8);

				Operation = packet.ReadInt();
				var unk = packet.ReadByte();

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
		private bool CreateCharacter(CreationCharacterData pdata, GameClient client, int accountSlot)
		{
			Account account = client.Account;
			var ch = new DOLCharacters();
			ch.AccountName = account.Name;
			ch.Name = pdata.CharName;

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
			List<string> disabled_classes = Util.SplitCSV(Properties.DISABLED_CLASSES, true);
			occurences = (from j in disabled_classes
						  where j == ch.Class.ToString()
						  select j).Count();

			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0} tried to create a disabled classe: {1}", client.Account.Name, (eCharacterClass)ch.Class);

				return true;
			}

			// check if race disabled
			List<string> disabled_races = Util.SplitCSV(Properties.DISABLED_RACES, true);
			occurences = (from j in disabled_races
						  where j == ch.Race.ToString()
						  select j).Count();

			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0} tried to create a disabled race: {1}", client.Account.Name, (eRace)ch.Race);

				return true;
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
					return false;
				}
				return true;
			}

			// check if client tried to create invalid char
			if (!IsCharacterValid(ch))
			{
				if (log.IsWarnEnabled)
				{
					log.WarnFormat("{0} tried to create invalid character:\nchar name={1}, gender={2}, race={3}, realm={4}, class={5}, region={6}" +
								   "\nstr={7}, con={8}, dex={9}, qui={10}, int={11}, pie={12}, emp={13}, chr={14}", ch.AccountName, ch.Name, ch.Gender,
								  ch.Race, ch.Realm, ch.Class, ch.Region, ch.Strength, ch.Constitution, ch.Dexterity, ch.Quickness, ch.Intelligence, ch.Piety, ch.Empathy, ch.Charisma);
				}
				return true;
			}

			//Save the character in the database
			GameServer.Database.AddObject(ch);

			// Fire the character creation event
			// This is Where Most Creation Script should take over to update any data they would like !
			GameEventMgr.Notify(DatabaseEvent.CharacterCreated, null, new CharacterEventArgs(ch, client));

			//write changes
			GameServer.Database.SaveObject(ch);

			// Log creation
			AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.CharacterCreate, "", pdata.CharName);

			client.Account.Characters = null;

			if (log.IsInfoEnabled)
				log.InfoFormat("Character {0} created on Account {1}!", pdata.CharName, account);

			// Reload Account Relations
			GameServer.Database.FillObjectRelations(client.Account);

			return true;
		}

		#endregion Create Character


		#region Character Updates
		/// <summary>
		/// Check if a Character Needs update based to packet data
		/// </summary>
		/// <param name="pdata">packet data</param>
		/// <param name="client">client</param>
		/// <param name="character">db character</param>
		/// <returns>True if character need refreshment false if no refresh needed.</returns>
		private bool CheckCharacterForUpdates(CreationCharacterData pdata, GameClient client, DOLCharacters character)
		{
			int newModel = character.CurrentModel;

			if (pdata.CustomMode == 1 || pdata.CustomMode == 2 || pdata.CustomMode == 3)
			{
				bool flagChangedStats = false;

				if (Properties.ALLOW_CUSTOMIZE_FACE_AFTER_CREATION)
				{
					character.EyeSize = (byte)pdata.EyeSize;
					character.LipSize = (byte)pdata.LipSize;
					character.EyeColor = (byte)pdata.EyeColor;
					character.HairColor = (byte)pdata.HairColor;
					character.FaceType = (byte)pdata.FaceType;
					character.HairStyle = (byte)pdata.HairStyle;
					character.MoodType = (byte)pdata.MoodType;
				}

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
							bool valid = IsCustomPointsDistributionValid(character, stats, out points);

							// Hacking attemp ?
							if (points > MaxStartingBonusPoints)
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
								return true;
							}

							if (Properties.ALLOW_CUSTOMIZE_STATS_AFTER_CREATION)
							{
								// Set Stats, valid is ok.
								character.Strength = stats[eStat.STR];
								character.Constitution = stats[eStat.CON];
								character.Dexterity = stats[eStat.DEX];
								character.Quickness = stats[eStat.QUI];
								character.Intelligence = stats[eStat.INT];
								character.Piety = stats[eStat.PIE];
								character.Empathy = stats[eStat.EMP];
								character.Charisma = stats[eStat.CHR];

								if (log.IsInfoEnabled)
									log.InfoFormat("Character {0} Stats updated in cache!", character.Name);

								if (client.Player != null)
								{
									foreach (var stat in stats.Keys)
										client.Player.ChangeBaseStat(stat, (short)(stats[stat] - client.Player.GetBaseStat(stat)));

									if (log.IsInfoEnabled)
										log.InfoFormat("Character {0} Player Stats updated in cache!", character.Name);
								}
							}
						}
						else if (log.IsErrorEnabled)
							log.ErrorFormat("No CharacterClass with ID {0} found", character.Class);
					}
				}


				if (pdata.CustomMode == 2) // change player customization
				{
					if (client.Account.PrivLevel == 1 && ((pdata.CreationModel >> 11) & 3) == 0)
					{
						if (ServerProperties.Properties.BAN_HACKERS) // Player size must be > 0 (from 1 to 3)
						{
							client.BanAccount(string.Format("Autoban Hack char update : zero character size in model:{0}", newModel));
							client.Disconnect();
							return false;
						}
						return true;
					}

					character.CustomisationStep = 2; // disable config button

					if (Properties.ALLOW_CUSTOMIZE_FACE_AFTER_CREATION)
					{
						if (pdata.CreationModel != character.CreationModel)
							character.CurrentModel = newModel;

						if (log.IsInfoEnabled)
							log.InfoFormat("Character {0} face properties configured by account {1}!", character.Name, client.Account.Name);
					}
				}
				else if (pdata.CustomMode == 3) //auto config -- seems someone thinks this is not possible?
				{
					character.CustomisationStep = 3; // enable config button to player
				}

				//Save the character in the database
				GameServer.Database.SaveObject(character);
			}

			return false;
		}
		#endregion Character Updates

		#region Delete Character
		public static bool CheckForDeletedCharacter(string accountName, GameClient client, int slot)
		{
			int charSlot = slot;

			if (accountName.EndsWith("-S")) charSlot = 100 + slot;
			else if (accountName.EndsWith("-N")) charSlot = 200 + slot;
			else if (accountName.EndsWith("-H")) charSlot = 300 + slot;

			DOLCharacters[] allChars = client.Account.Characters;

			if (allChars != null)
			{
				foreach (DOLCharacters character in allChars.ToArray())
				{
					if (character.AccountSlot == charSlot && client.ClientState == GameClient.eClientState.CharScreen)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("DB Character Delete:  Account {0}, Character: {1}, slot position: {2}, client slot {3}", accountName, character.Name, character.AccountSlot, slot);

						var characterRealm = (eRealm)character.Realm;

						if (allChars.Length < client.ActiveCharIndex && client.ActiveCharIndex > -1 && allChars[client.ActiveCharIndex] == character)
							client.ActiveCharIndex = -1;


						GameEventMgr.Notify(DatabaseEvent.CharacterDeleted, null, new CharacterEventArgs(character, client));

						if (Properties.BACKUP_DELETED_CHARACTERS)
						{
							var backupCharacter = new DOLCharactersBackup(character);
							Util.ForEach(backupCharacter.CustomParams, param => GameServer.Database.AddObject(param));
							GameServer.Database.AddObject(backupCharacter);

							if (log.IsWarnEnabled)
								log.WarnFormat("DB Character {0} backed up to DOLCharactersBackup and no associated content deleted.", character.ObjectId);
						}
						else
						{
							// delete associated data

							try
							{
								var objs = GameServer.Database.SelectObjects<InventoryItem>("`OwnerID` = @OwnerID", new QueryParameter("@OwnerID", character.ObjectId));
								GameServer.Database.DeleteObject(objs);
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.ErrorFormat("Error deleting char items, char OID={0}, Exception:{1}", character.ObjectId, e);
							}

							// delete quests
							try
							{
								var objs = GameServer.Database.SelectObjects<DBQuest>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", character.ObjectId));
								GameServer.Database.DeleteObject(objs);
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.ErrorFormat("Error deleting char quests, char OID={0}, Exception:{1}", character.ObjectId, e);
							}

							// delete ML steps
							try
							{
								var objs = GameServer.Database.SelectObjects<DBCharacterXMasterLevel>("`Character_ID` = @Character_ID", new QueryParameter("@Character_ID", character.ObjectId));
								GameServer.Database.DeleteObject(objs);
							}
							catch (Exception e)
							{
								if (log.IsErrorEnabled)
									log.ErrorFormat("Error deleting char ml steps, char OID={0}, Exception:{1}", character.ObjectId, e);
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
								log.InfoFormat("Account {0} has no more chars. Realm reset!", client.Account.Name);

							//Client has no more characters, so the client can choose the realm again!
							client.Account.Realm = 0;
						}

						GameServer.Database.SaveObject(client.Account);

						// Log deletion
						AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterDelete, "", deletedChar);

						return true;
					}
				}
			}
			return false;
		}
		#endregion Delete Character

		#region ValidateCharacter
		/// <summary>
		/// Check if Custom Creation Points Distribution is Valid.
		/// </summary>
		/// <param name="character"></param>
		/// <param name="stats"></param>
		/// <param name="points"></param>
		/// <returns></returns>
		public static bool IsCustomPointsDistributionValid(DOLCharacters character, IDictionary<eStat, int> stats, out int points)
		{
			ICharacterClass charClass = ScriptMgr.FindCharacterClass(character.Class);

			if (charClass != null)
			{
				points = 0;

				// check if each stat is valid.
				foreach (var stat in stats.Keys)
				{
					int raceAmount = GlobalConstants.STARTING_STATS_DICT[(eRace)character.Race][stat];

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

				return points == MaxStartingBonusPoints;
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
				if ((eRealm)ch.Realm < eRealm._FirstPlayerRealm || (eRealm)ch.Realm > eRealm._LastPlayerRealm)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Wrong realm: {0} on character creation from Account: {1}", ch.Realm, ch.AccountName);
					valid = false;
				}
				if (ch.Level != 1)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Wrong level: {0} on character creation from Account: {1}", ch.Level, ch.AccountName);
					valid = false;
				}
				if (!GlobalConstants.STARTING_CLASSES_DICT.ContainsKey((eRealm)ch.Realm) || !GlobalConstants.STARTING_CLASSES_DICT[(eRealm)ch.Realm].Contains((eCharacterClass)ch.Class))
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Wrong class: {0}, realm:{1} on character creation from Account: {2}", ch.Class, ch.Realm, ch.AccountName);
					valid = false;
				}
				if (!GlobalConstants.RACES_CLASSES_DICT.ContainsKey((eRace)ch.Race) || !GlobalConstants.RACES_CLASSES_DICT[(eRace)ch.Race].Contains((eCharacterClass)ch.Class))
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Wrong race: {0}, class:{1} on character creation from Account: {2}", ch.Race, ch.Class, ch.AccountName);
					valid = false;
				}
				int pointsUsed;
				var stats = new Dictionary<eStat, int>{{eStat.STR, ch.Strength},{eStat.CON, ch.Constitution},{eStat.DEX, ch.Dexterity},{eStat.QUI, ch.Quickness},
					{eStat.INT, ch.Intelligence},{eStat.PIE, ch.Piety},{eStat.EMP, ch.Empathy},{eStat.CHR, ch.Charisma},};

				valid &= IsCustomPointsDistributionValid(ch, stats, out pointsUsed);

				if (pointsUsed != MaxStartingBonusPoints)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Points used: {0} on character creation from Account: {1}", pointsUsed, ch.AccountName);
					valid = false;
				}

				eGender gender = ch.Gender == 0 ? eGender.Male : eGender.Female;

				if (GlobalConstants.RACE_GENDER_CONSTRAINTS_DICT.ContainsKey((eRace)ch.Race) && GlobalConstants.RACE_GENDER_CONSTRAINTS_DICT[(eRace)ch.Race] != gender)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Wrong Race gender: {0}, race: {1} on character creation from Account: {2}", ch.Gender, ch.Race, ch.AccountName);
					valid = false;
				}

				if (GlobalConstants.CLASS_GENDER_CONSTRAINTS_DICT.ContainsKey((eCharacterClass)ch.Class) && GlobalConstants.CLASS_GENDER_CONSTRAINTS_DICT[(eCharacterClass)ch.Class] != gender)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Wrong class gender: {0}, class:{1} on character creation from Account: {2}", ch.Gender, ch.Class, ch.AccountName);
					valid = false;
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("CharacterCreation error on account {0}, slot {1}. Exception:{2}", ch.AccountName, ch.AccountSlot, e);

				valid = false;
			}

			return valid;
		}
		#endregion Validate Character

		public struct CreationCharacterData1124
		{
			public readonly string CharName;
			public readonly int CustomMode;
			public readonly int EyeSize;
			public readonly int LipSize;
			public readonly int EyeColor;
			public readonly int HairColor;
			public readonly int FaceType;
			public readonly int HairStyle;
			public readonly int MoodType;
			public readonly uint Operation;
			public readonly int Class;
			public readonly int Realm;
			public readonly int Race;
			public readonly int Gender;
			public readonly ushort CreationModel;
			public readonly int Region;

			public readonly int Strength;
			public readonly int Dexterity;
			public readonly int Constitution;
			public readonly int Quickness;
			public readonly int Intelligence;
			public readonly int Piety;
			public readonly int Empathy;
			public readonly int Charisma;
			public readonly int NewConstitution;
			public int CharacterSlot; // 1125 support
			public int CustomizeType; // 1125 support

			public CreationCharacterData1124(GameClient client, GSPacketIn packet)
			{
				CharacterSlot = packet.ReadByte();
				CharName = packet.ReadIntPascalStringLowEndian();
				packet.Skip(4); // 0x18 0x00 0x00 0x00
				CustomMode = packet.ReadByte();
				EyeSize = packet.ReadByte();
				LipSize = packet.ReadByte();
				EyeColor = packet.ReadByte();
				HairColor = packet.ReadByte();
				FaceType = packet.ReadByte();
				HairStyle = packet.ReadByte();
				packet.Skip(3);
				MoodType = packet.ReadByte();
				packet.Skip(9); // one extra byte skipped?
				Operation = (uint)packet.ReadByte(); // probably low end int, but im just gonna read the first byte
				CustomizeType = packet.ReadByte(); // 1 = face 2 = attributes 3 = both
				packet.Skip(2); // last two bytes in the supposed int
								// the following are now low endian int pascal strings
				packet.Skip(5); //Location string
				packet.Skip(5); //Skip class name
				packet.Skip(5); //Skip race name
				packet.Skip(1); // not sure what this is? previous pak says level, but its unused anyway
				Class = packet.ReadByte();
				Realm = packet.ReadByte();
				if (Realm > 0) // put inside this, as when a char is deleted, there is no realm sent TODO redo how account slot is stored in DB perhaps
				{
					CharacterSlot -= (Realm - 1) * 10; // calc to get character slot into same format used in database.
				}
				//The following byte contains
				//1bit=start location ... in ShroudedIsles you can choose ...
				//1bit=first race bit
				//1bit=unknown
				//1bit=gender (0=male, 1=female)
				//4bit=race
				byte startRaceGender1 = (byte)packet.ReadByte();
				Race = startRaceGender1 & 0x1F;
				Gender = ((startRaceGender1 >> 7) & 0x01);
				//SIStartLocation = ((startRaceGender1 >> 7) != 0);
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
				packet.Skip(3); // explained above
								// New constitution must be read before skipping 4 bytes
				NewConstitution = packet.ReadByte(); // 0x9F
													 // trailing 0x00
			}
		}

		private void _HandlePacket1124(GameClient client, GSPacketIn packet)
		{
			var pakdata = new CreationCharacterData1124(client, packet);

			// Graveen: changed the following to allow GMs to have special chars in their names (_,-, etc..)
			var nameCheck = new Regex("^[A-Z][a-zA-Z]");
			if (!string.IsNullOrEmpty(pakdata.CharName) && (pakdata.CharName.Length < 3 || !nameCheck.IsMatch(pakdata.CharName)))
			{
				if ((ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
				{
					if (Properties.BAN_HACKERS)
					{
						client.BanAccount(string.Format("Autoban bad CharName '{0}'", pakdata.CharName));
					}

					client.Disconnect();
					return;
				}
			}

			var needRefresh = false;
			switch (pakdata.Operation)
			{
				case 3: // delete request
					if (string.IsNullOrEmpty(pakdata.CharName))
					{
						// Deletion in 1.104+ check for removed character.
						needRefresh |= CheckForDeletedCharacter(client.Account.Name, client, pakdata.CharacterSlot);
					}
					break;
				case 2: // Customize face or stats
					if (!string.IsNullOrEmpty(pakdata.CharName))
					{
						// Candidate for Customizing ?
						var character = client.Account.Characters?.FirstOrDefault(ch => ch.Name.Equals(pakdata.CharName, StringComparison.OrdinalIgnoreCase));
						if (character != null)
							needRefresh |= CheckCharacterForUpdates1124(pakdata, client, character, pakdata.CustomizeType);
					}
					break;
				case 1: // create request
					if (!string.IsNullOrEmpty(pakdata.CharName))
					{
						// Candidate for Creation ?
						var character = client.Account.Characters?.FirstOrDefault(ch => ch.Name.Equals(pakdata.CharName, StringComparison.OrdinalIgnoreCase));
						if (character == null)
							needRefresh |= _CreateCharacter1124(pakdata, client, pakdata.CharacterSlot);
					}
					break;
				default:
					break;
			}

			// live actually just sends server login request response... i think this sets the realm select button again too
			if (needRefresh)
				client.Out.SendLoginGranted();
		}

		private bool _CreateCharacter1124(CreationCharacterData1124 pdata, GameClient client, int accountSlot)
		{
			Account account = client.Account;
			var ch = new DOLCharacters
			{
				AccountName = account.Name,
				Name = pdata.CharName
			};

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

			log.Debug($"Creation {client.Version} character, class:{ch.Class}, realm:{ch.Realm}");

			// Is class disabled ?
			List<string> disabledClasses = Util.SplitCSV(Properties.DISABLED_CLASSES, true);
			var occurences = disabledClasses.Count(j => j == ch.Class.ToString());
			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				log.Debug($"Client {client.Account.Name} tried to create a disabled classe: {(eCharacterClass)ch.Class}");
				return true;
			}

			// check if race disabled
			List<string> disabledRaces = Util.SplitCSV(Properties.DISABLED_RACES, true);
			occurences = disabledRaces.Count(j => j == ch.Race.ToString());
			if (occurences > 0 && (ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
			{
				log.Debug($"Client {client.Account.Name} tried to create a disabled race: {(eRace)ch.Race}");
				return true;
			}

			// If sending invalid Class ID
			if (!Enum.IsDefined(typeof(eCharacterClass), (eCharacterClass)ch.Class))
			{
				log.Error($"{client.Account.Name} tried to create a character with wrong class ID: {ch.Class}, realm:{ch.Realm}");

				if (Properties.BAN_HACKERS)
				{
					client.BanAccount($"Autoban character create class: id:{ch.Class} realm:{ch.Realm} name:{ch.Name} account:{account.Name}");
					client.Disconnect();
					return false;
				}

				return true;
			}

			// check if client tried to create invalid char
			if (!IsCharacterValid(ch))
			{
				log.Warn($"{ch.AccountName} tried to create invalid character:\nchar name={ch.Name}, gender={ch.Gender}, race={ch.Race}, realm={ch.Realm}, class={ch.Class}, region={ch.Region}\nstr={ch.Strength}, con={ch.Constitution}, dex={ch.Dexterity}, qui={ch.Quickness}, int={ch.Intelligence}, pie={ch.Piety}, emp={ch.Empathy}, chr={ch.Charisma}");
				return true;
			}

			// Save the character in the database
			GameServer.Database.AddObject(ch);

			// Fire the character creation event
			// This is Where Most Creation Script should take over to update any data they would like !
			GameEventMgr.Notify(DatabaseEvent.CharacterCreated, null, new CharacterEventArgs(ch, client));

			// write changes
			GameServer.Database.SaveObject(ch);

			// Log creation
			AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.CharacterCreate, string.Empty, pdata.CharName);

			client.Account.Characters = null;

			log.Info($"Character {pdata.CharName} created on Account {account}!");

			// Reload Account Relations
			GameServer.Database.FillObjectRelations(client.Account);
			return true;
		}

		// 1125 support
		private bool CheckCharacterForUpdates1124(CreationCharacterData1124 pdata, GameClient client, DOLCharacters character, int type)
		{
			int newModel = character.CurrentModel;

			if (pdata.CustomMode == 1 || pdata.CustomMode == 2 || pdata.CustomMode == 3)
			{
				bool flagChangedStats = false;

				if (type == 1 || type == 3) // face changes
				{
					if (Properties.ALLOW_CUSTOMIZE_FACE_AFTER_CREATION)
					{
						character.EyeSize = (byte)pdata.EyeSize;
						character.LipSize = (byte)pdata.LipSize;
						character.EyeColor = (byte)pdata.EyeColor;
						character.HairColor = (byte)pdata.HairColor;
						character.FaceType = (byte)pdata.FaceType;
						character.HairStyle = (byte)pdata.HairStyle;
						character.MoodType = (byte)pdata.MoodType;
					}
				}
				if (type == 2 || type == 3) // attributes changes
				{
					if (pdata.CustomMode != 3)//patch 0042 // TODO check out these different custommodes
					{
						var stats = new Dictionary<eStat, int>
						{
							[eStat.STR] = pdata.Strength, // Strength
							[eStat.DEX] = pdata.Dexterity, // Dexterity
							[eStat.CON] = pdata.NewConstitution, // New Constitution
							[eStat.QUI] = pdata.Quickness, // Quickness
							[eStat.INT] = pdata.Intelligence, // Intelligence
							[eStat.PIE] = pdata.Piety, // Piety
							[eStat.EMP] = pdata.Empathy, // Empathy
							[eStat.CHR] = pdata.Charisma // Charisma
						};

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
								bool valid = IsCustomPointsDistributionValid(character, stats, out int points);

								// Hacking attemp ?
								if (points > MaxStartingBonusPoints)
								{
									if ((ePrivLevel)client.Account.PrivLevel == ePrivLevel.Player)
									{
										if (Properties.BAN_HACKERS)
											client.BanAccount(string.Format("Autoban Hack char update : Wrong allowed points:{0}", points));

										client.Disconnect();
										return false;
									}
								}

								// Error in setting points
								if (!valid)
									return true;

								if (Properties.ALLOW_CUSTOMIZE_STATS_AFTER_CREATION)
								{
									// Set Stats, valid is ok.
									character.Strength = stats[eStat.STR];
									character.Constitution = stats[eStat.CON];
									character.Dexterity = stats[eStat.DEX];
									character.Quickness = stats[eStat.QUI];
									character.Intelligence = stats[eStat.INT];
									character.Piety = stats[eStat.PIE];
									character.Empathy = stats[eStat.EMP];
									character.Charisma = stats[eStat.CHR];

									log.InfoFormat("Character {0} Stats updated in cache!", character.Name);
									if (client.Player != null)
									{
										foreach (var stat in stats.Keys)
											client.Player.ChangeBaseStat(stat, (short)(stats[stat] - client.Player.GetBaseStat(stat)));

										log.InfoFormat("Character {0} Player Stats updated in cache!", character.Name);
									}
								}
							}
							else if (log.IsErrorEnabled)
							{
								log.ErrorFormat("No CharacterClass with ID {0} found", character.Class);
							}
						}
					}
				}

				if (pdata.CustomMode == 2) // change player customization // is this changing starting race? im not sure TODO check
				{
					if (client.Account.PrivLevel == 1 && ((pdata.CreationModel >> 11) & 3) == 0)
					{
						if (Properties.BAN_HACKERS) // Player size must be > 0 (from 1 to 3)
						{
							client.BanAccount(string.Format("Autoban Hack char update : zero character size in model:{0}", newModel));
							client.Disconnect();
							return false;
						}
						return true;
					}

					character.CustomisationStep = 2; // disable config button

					if (Properties.ALLOW_CUSTOMIZE_FACE_AFTER_CREATION)
					{
						if (pdata.CreationModel != character.CreationModel)
							character.CurrentModel = newModel;

						log.InfoFormat("Character {0} face properties configured by account {1}!", character.Name, client.Account.Name);
					}
				}
				else if (pdata.CustomMode == 3) //auto config -- seems someone thinks this is not possible?
				{
					character.CustomisationStep = 3; // enable config button to player
				}

				//Save the character in the database
				GameServer.Database.SaveObject(character);
			}

			return false;
		}
	}
}