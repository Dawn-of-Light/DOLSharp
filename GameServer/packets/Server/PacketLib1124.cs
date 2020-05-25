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
#define NOENCRYPTION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Reflection;

using DOL.Database;
using DOL.Language;
using DOL.AI.Brain;
using DOL.GS.Behaviour;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PlayerTitles;
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.GS.Styles;

using log4net;


namespace DOL.GS.PacketHandler
{
	[PacketLib(1124, GameClient.eClientVersion.Version1124)]
	public class PacketLib1124 : AbstractPacketLib, IPacketLib
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		protected static int MAX_NAME_LENGTH = 55;
		private const int MaxPacketLength = 2048;
		private const ushort MAX_STORY_LENGTH = 1000;   // Via trial and error, 1.108 client.
														// Often will cut off text around 990 but longer strings do not result in any errors. -Tolakram
		protected byte icons;
		public byte Icons
		{
			get { return icons; }
		}
		private byte WarlockChamberEffectId(GameSpellEffect effect)
		{
			return 0; // ??
		}

		/// <summary>
		/// Property to enable "forced" Tooltip send when Update are made to player skills, or player effects.
		/// This can be controlled through server propertiers !
		/// </summary>
		public virtual bool ForceTooltipUpdate
		{
			get { return ServerProperties.Properties.USE_NEW_TOOLTIP_FORCEDUPDATE; }
		}

		protected string BuildTaskString()
		{
			if (m_gameClient.Player == null)
				return "";

			AbstractTask task = m_gameClient.Player.Task;
			AbstractMission pMission = m_gameClient.Player.Mission;

			AbstractMission gMission = null;
			if (m_gameClient.Player.Group != null)
				gMission = m_gameClient.Player.Group.Mission;

			AbstractMission rMission = null;

			//all the task info is sent in name field

			string taskStr = "";
			if (task == null)
				taskStr = "You have no current personal task.\n";
			else taskStr = "[" + task.Name + "] " + task.Description + ".\n";

			string personalMission = "";
			if (pMission != null)
				personalMission = "[" + pMission.Name + "] " + pMission.Description + ".\n";

			string groupMission = "";
			if (gMission != null)
				groupMission = "[" + gMission.Name + "] " + gMission.Description + ".\n";

			string realmMission = "";
			if (rMission != null)
				realmMission = "[" + rMission.Name + "]" + " " + rMission.Description + ".\n";

			string name = taskStr + personalMission + groupMission + realmMission;

			if (name.Length > ushort.MaxValue)
			{
				if (log.IsWarnEnabled)
					log.Warn("Task packet name is too long for 1.71 clients (" + name.Length + ") '" + name + "'");
				name = name.Substring(0, ushort.MaxValue);
			}
			if (name.Length > 2048 - 10)
			{
				name = name.Substring(0, 2048 - 10 - name.Length);
			}

			return name;
		}
		/// <summary>
		/// The bow prepare animation
		/// </summary>
		public virtual int BowPrepare
		{
			get { return 0x3E80; }
		}

		/// <summary>
		/// one dual weapon hit animation
		/// </summary>
		public virtual int OneDualWeaponHit
		{
			get { return 0x3E81; }
		}

		/// <summary>
		/// both dual weapons hit animation
		/// </summary>
		public virtual int BothDualWeaponHit
		{
			get { return 0x3E82; }
		}

		/// <summary>
		/// The bow shoot animation
		/// </summary>
		public virtual int BowShoot
		{
			get { return 0x3E83; }
		}
		/// <summary>
		/// Constructs a new PacketLib for Client Version 1.124
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib1124(GameClient client) : base(client)
		{
			icons = 1;
		}

		public virtual void CheckLengthHybridSkillsPacket(ref GSTCPPacketOut pak, ref int maxSkills, ref int first)
		{
			if (pak.Length > 1500)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - first));
				pak.WriteByte((byte)(first == 0 ? 99 : 0x03)); //subtype
				pak.WriteByte((byte)first);
				SendTCP(pak);
				pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
				pak.WriteByte(0x01); //subcode
				pak.WriteByte((byte)maxSkills); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)first);
				first = maxSkills;
			}
			maxSkills++;
		}
		public virtual void SendAttackMode(bool attackState)
		{
			if (m_gameClient.Player == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.AttackMode)))
			{
				pak.WriteByte((byte)(attackState ? 0x01 : 0x00));
				pak.Fill(0x00, 3);

				SendTCP(pak);
			}
		}
		public virtual void SendBadNameCheckReply(string name, bool bad)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.BadNameCheckReply)))
			{
				pak.FillString(name, 30);
				pak.FillString(m_gameClient.Account.Name, 20);
				pak.WriteByte((byte)(bad ? 0x0 : 0x1));
				pak.Fill(0x0, 3);
				SendTCP(pak);
			}
		}
		public virtual void SendBlinkPanel(byte flag)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{
				GamePlayer player = base.m_gameClient.Player;

				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte((byte)8);
				pak.WriteByte((byte)flag);
				pak.WriteByte((byte)0);

				SendTCP(pak);
			}
		}
		public virtual void SendCharacterOverview(eRealm realm)
		{
			if (realm < eRealm._FirstPlayerRealm || realm > eRealm._LastPlayerRealm)
			{
				throw new Exception("CharacterOverview requested for unknown realm " + realm);
			}

			int firstSlot = (byte)realm * 100;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterOverview)))
			{
				pak.FillString(m_gameClient.Account.Name, 24);

				if (m_gameClient.Account.Characters == null)
				{
					pak.Fill(0x0, 1880);
				}
				else
				{
					Dictionary<int, DOLCharacters> charsBySlot = new Dictionary<int, DOLCharacters>();
					foreach (DOLCharacters c in m_gameClient.Account.Characters)
					{
						try
						{
							charsBySlot.Add(c.AccountSlot, c);
						}
						catch (Exception ex)
						{
							log.Error("SendCharacterOverview - Duplicate char in slot? Slot: " + c.AccountSlot + ", Account: " + c.AccountName, ex);
						}
					}
					var itemsByOwnerID = new Dictionary<string, Dictionary<eInventorySlot, InventoryItem>>();

					if (charsBySlot.Any())
					{
						var allItems = GameServer.Database.SelectObjects<InventoryItem>("`OwnerID` = @OwnerID AND `SlotPosition` >= @MinEquipable AND `SlotPosition` <= @MaxEquipable",
																						charsBySlot.Select(kv => new[] { new QueryParameter("@OwnerID", kv.Value.ObjectId), new QueryParameter("@MinEquipable", (int)eInventorySlot.MinEquipable), new QueryParameter("@MaxEquipable", (int)eInventorySlot.MaxEquipable) }))
							.SelectMany(objs => objs);

						foreach (InventoryItem item in allItems)
						{
							try
							{
								if (!itemsByOwnerID.ContainsKey(item.OwnerID))
									itemsByOwnerID.Add(item.OwnerID, new Dictionary<eInventorySlot, InventoryItem>());

								itemsByOwnerID[item.OwnerID].Add((eInventorySlot)item.SlotPosition, item);
							}
							catch (Exception ex)
							{
								log.Error("SendCharacterOverview - Duplicate item on character? OwnerID: " + item.OwnerID + ", SlotPosition: " + item.SlotPosition + ", Account: " + m_gameClient.Account.Name, ex);
							}
						}
					}

					for (int i = firstSlot; i < (firstSlot + 10); i++)
					{
						DOLCharacters c = null;
						if (!charsBySlot.TryGetValue(i, out c))
						{
							pak.Fill(0x0, 188);
						}
						else
						{
							Dictionary<eInventorySlot, InventoryItem> charItems = null;

							if (!itemsByOwnerID.TryGetValue(c.ObjectId, out charItems))
								charItems = new Dictionary<eInventorySlot, InventoryItem>();

							byte extensionTorso = 0;
							byte extensionGloves = 0;
							byte extensionBoots = 0;

							InventoryItem item = null;

							if (charItems.TryGetValue(eInventorySlot.TorsoArmor, out item))
								extensionTorso = item.Extension;

							if (charItems.TryGetValue(eInventorySlot.HandsArmor, out item))
								extensionGloves = item.Extension;

							if (charItems.TryGetValue(eInventorySlot.FeetArmor, out item))
								extensionBoots = item.Extension;

							pak.Fill(0x00, 4);//new heading bytes in from 1.99 relocated in 1.104
							pak.FillString(c.Name, 24);
							pak.WriteByte(0x01);
							pak.WriteByte((byte)c.EyeSize);
							pak.WriteByte((byte)c.LipSize);
							pak.WriteByte((byte)c.EyeColor);
							pak.WriteByte((byte)c.HairColor);
							pak.WriteByte((byte)c.FaceType);
							pak.WriteByte((byte)c.HairStyle);
							pak.WriteByte((byte)((extensionBoots << 4) | extensionGloves));
							pak.WriteByte((byte)((extensionTorso << 4) | (c.IsCloakHoodUp ? 0x1 : 0x0)));
							pak.WriteByte((byte)c.CustomisationStep); //1 = auto generate config, 2= config ended by player, 3= enable config to player
							pak.WriteByte((byte)c.MoodType);
							pak.Fill(0x0, 13); //0 String

							string locationDescription = string.Empty;
							Region region = WorldMgr.GetRegion((ushort)c.Region);
							if (region != null)
							{
								locationDescription = m_gameClient.GetTranslatedSpotDescription(region, c.Xpos, c.Ypos, c.Zpos);
							}
							pak.FillString(locationDescription, 24);

							string classname = "";
							if (c.Class != 0)
								classname = ((eCharacterClass)c.Class).ToString();
							pak.FillString(classname, 24);

							string racename = m_gameClient.RaceToTranslatedName(c.Race, c.Gender);
							pak.FillString(racename, 24);

							pak.WriteByte((byte)c.Level);
							pak.WriteByte((byte)c.Class);
							pak.WriteByte((byte)c.Realm);
							pak.WriteByte((byte)((((c.Race & 0x10) << 2) + (c.Race & 0x0F)) | (c.Gender << 4))); // race max value can be 0x1F
							pak.WriteShortLowEndian((ushort)c.CurrentModel);
							pak.WriteByte((byte)c.Region);
							if (region == null || (int)m_gameClient.ClientType > region.Expansion)
								pak.WriteByte(0x00);
							else
								pak.WriteByte((byte)(region.Expansion + 1)); //0x04-Cata zone, 0x05 - DR zone
							pak.WriteInt(0x0); // Internal database ID
							pak.WriteByte((byte)c.Strength);
							pak.WriteByte((byte)c.Dexterity);
							pak.WriteByte((byte)c.Constitution);
							pak.WriteByte((byte)c.Quickness);
							pak.WriteByte((byte)c.Intelligence);
							pak.WriteByte((byte)c.Piety);
							pak.WriteByte((byte)c.Empathy);
							pak.WriteByte((byte)c.Charisma);

							InventoryItem rightHandWeapon = null;
							charItems.TryGetValue(eInventorySlot.RightHandWeapon, out rightHandWeapon);
							InventoryItem leftHandWeapon = null;
							charItems.TryGetValue(eInventorySlot.LeftHandWeapon, out leftHandWeapon);
							InventoryItem twoHandWeapon = null;
							charItems.TryGetValue(eInventorySlot.TwoHandWeapon, out twoHandWeapon);
							InventoryItem distanceWeapon = null;
							charItems.TryGetValue(eInventorySlot.DistanceWeapon, out distanceWeapon);

							InventoryItem helmet = null;
							charItems.TryGetValue(eInventorySlot.HeadArmor, out helmet);
							InventoryItem gloves = null;
							charItems.TryGetValue(eInventorySlot.HandsArmor, out gloves);
							InventoryItem boots = null;
							charItems.TryGetValue(eInventorySlot.FeetArmor, out boots);
							InventoryItem torso = null;
							charItems.TryGetValue(eInventorySlot.TorsoArmor, out torso);
							InventoryItem cloak = null;
							charItems.TryGetValue(eInventorySlot.Cloak, out cloak);
							InventoryItem legs = null;
							charItems.TryGetValue(eInventorySlot.LegsArmor, out legs);
							InventoryItem arms = null;
							charItems.TryGetValue(eInventorySlot.ArmsArmor, out arms);

							pak.WriteShortLowEndian((ushort)(helmet != null ? helmet.Model : 0));
							pak.WriteShortLowEndian((ushort)(gloves != null ? gloves.Model : 0));
							pak.WriteShortLowEndian((ushort)(boots != null ? boots.Model : 0));

							ushort rightHandColor = 0;
							if (rightHandWeapon != null)
							{
								rightHandColor = (ushort)(rightHandWeapon.Emblem != 0 ? rightHandWeapon.Emblem : rightHandWeapon.Color);
							}
							pak.WriteShortLowEndian(rightHandColor);

							pak.WriteShortLowEndian((ushort)(torso != null ? torso.Model : 0));
							pak.WriteShortLowEndian((ushort)(cloak != null ? cloak.Model : 0));
							pak.WriteShortLowEndian((ushort)(legs != null ? legs.Model : 0));
							pak.WriteShortLowEndian((ushort)(arms != null ? arms.Model : 0));

							ushort helmetColor = 0;
							if (helmet != null)
							{
								helmetColor = (ushort)(helmet.Emblem != 0 ? helmet.Emblem : helmet.Color);
							}
							pak.WriteShortLowEndian(helmetColor);

							ushort glovesColor = 0;
							if (gloves != null)
							{
								glovesColor = (ushort)(gloves.Emblem != 0 ? gloves.Emblem : gloves.Color);
							}
							pak.WriteShortLowEndian(glovesColor);

							ushort bootsColor = 0;
							if (boots != null)
							{
								bootsColor = (ushort)(boots.Emblem != 0 ? boots.Emblem : boots.Color);
							}
							pak.WriteShortLowEndian(bootsColor);

							ushort leftHandWeaponColor = 0;
							if (leftHandWeapon != null)
							{
								leftHandWeaponColor = (ushort)(leftHandWeapon.Emblem != 0 ? leftHandWeapon.Emblem : leftHandWeapon.Color);
							}
							pak.WriteShortLowEndian(leftHandWeaponColor);

							ushort torsoColor = 0;
							if (torso != null)
							{
								torsoColor = (ushort)(torso.Emblem != 0 ? torso.Emblem : torso.Color);
							}
							pak.WriteShortLowEndian(torsoColor);

							ushort cloakColor = 0;
							if (cloak != null)
							{
								cloakColor = (ushort)(cloak.Emblem != 0 ? cloak.Emblem : cloak.Color);
							}
							pak.WriteShortLowEndian(cloakColor);

							ushort legsColor = 0;
							if (legs != null)
							{
								legsColor = (ushort)(legs.Emblem != 0 ? legs.Emblem : legs.Color);
							}
							pak.WriteShortLowEndian(legsColor);

							ushort armsColor = 0;
							if (arms != null)
							{
								armsColor = (ushort)(arms.Emblem != 0 ? arms.Emblem : arms.Color);
							}
							pak.WriteShortLowEndian(armsColor);

							//weapon models

							pak.WriteShortLowEndian((ushort)(rightHandWeapon != null ? rightHandWeapon.Model : 0));
							pak.WriteShortLowEndian((ushort)(leftHandWeapon != null ? leftHandWeapon.Model : 0));
							pak.WriteShortLowEndian((ushort)(twoHandWeapon != null ? twoHandWeapon.Model : 0));
							pak.WriteShortLowEndian((ushort)(distanceWeapon != null ? distanceWeapon.Model : 0));

							if (c.ActiveWeaponSlot == (byte)DOL.GS.GameLiving.eActiveWeaponSlot.TwoHanded)
							{
								pak.WriteByte(0x02);
								pak.WriteByte(0x02);
							}
							else if (c.ActiveWeaponSlot == (byte)DOL.GS.GameLiving.eActiveWeaponSlot.Distance)
							{
								pak.WriteByte(0x03);
								pak.WriteByte(0x03);
							}
							else
							{
								byte righthand = 0xFF;
								byte lefthand = 0xFF;

								if (rightHandWeapon != null)
									righthand = 0x00;

								if (leftHandWeapon != null)
									lefthand = 0x01;

								pak.WriteByte(righthand);
								pak.WriteByte(lefthand);
							}

							if (region == null || region.Expansion != 1)
								pak.WriteByte(0x00);
							else
								pak.WriteByte(0x01); //0x01=char in SI zone, classic client can't "play"

							pak.WriteByte((byte)c.Constitution);
						}

					}
				}

				pak.Fill(0x0, 94);
				SendTCP(pak);
			}
		}
		public virtual void SendCharCreateReply(string name)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterCreateReply)))
			{
				pak.FillString(name, 24);
				SendTCP(pak);
			}
		}
		public virtual void SendCharResistsUpdate()
		{
			if (m_gameClient.Player == null)
				return;

			eResist[] updateResists =
			{
				eResist.Crush,
				eResist.Slash,
				eResist.Thrust,
				eResist.Heat,
				eResist.Cold,
				eResist.Matter,
				eResist.Body,
				eResist.Spirit,
				eResist.Energy,
			};

			int[] racial = new int[updateResists.Length];
			int[] caps = new int[updateResists.Length];

			int cap = (m_gameClient.Player.Level >> 1) + 1;
			for (int i = 0; i < updateResists.Length; i++)
			{
				caps[i] = cap;
			}


			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.StatsUpdate)))
			{

				// racial resists
				for (int i = 0; i < updateResists.Length; i++)
				{
					racial[i] = SkillBase.GetRaceResist(m_gameClient.Player.Race, updateResists[i]);
					pak.WriteShort((ushort)racial[i]);
				}

				// buffs/debuffs only; remove base, item bonus, RA bonus, race bonus
				for (int i = 0; i < updateResists.Length; i++)
				{
					int mod = m_gameClient.Player.GetModified((eProperty)updateResists[i]);
					int buff = mod - racial[i] - m_gameClient.Player.AbilityBonus[(int)updateResists[i]] - Math.Min(caps[i], m_gameClient.Player.ItemBonus[(int)updateResists[i]]);
					pak.WriteShort((ushort)buff);
				}

				// item bonuses
				for (int i = 0; i < updateResists.Length; i++)
				{
					pak.WriteShort((ushort)(m_gameClient.Player.ItemBonus[(int)updateResists[i]]));
				}

				// item caps
				for (int i = 0; i < updateResists.Length; i++)
				{
					pak.WriteByte((byte)caps[i]);
				}

				// RA bonuses
				for (int i = 0; i < updateResists.Length; i++)
				{
					pak.WriteByte((byte)(m_gameClient.Player.AbilityBonus[(int)updateResists[i]]));
				}

				pak.WriteByte(0xFF); // FF if resists packet
				pak.WriteByte(0);
				pak.WriteShort(0);
				pak.WriteShort(0);

				SendTCP(pak);
			}
		}
		public virtual void SendCharStatsUpdate()
		{
			if (m_gameClient.Player == null)
				return;

			eStat[] updateStats =
			{
				eStat.STR,
				eStat.DEX,
				eStat.CON,
				eStat.QUI,
				eStat.INT,
				eStat.PIE,
				eStat.EMP,
				eStat.CHR,
			};

			int[] baseStats = new int[updateStats.Length];
			int[] modStats = new int[updateStats.Length];
			int[] itemCaps = new int[updateStats.Length];

			int itemCap = (int)(m_gameClient.Player.Level * 1.5);
			int bonusCap = (int)(m_gameClient.Player.Level / 2 + 1);
			for (int i = 0; i < updateStats.Length; i++)
			{
				int cap = itemCap;
				switch ((eProperty)updateStats[i])
				{
					case eProperty.Strength:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.StrCapBonus];
						break;
					case eProperty.Dexterity:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.DexCapBonus];
						break;
					case eProperty.Constitution:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.ConCapBonus];
						break;
					case eProperty.Quickness:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.QuiCapBonus];
						break;
					case eProperty.Intelligence:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.IntCapBonus];
						break;
					case eProperty.Piety:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.PieCapBonus];
						break;
					case eProperty.Charisma:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.ChaCapBonus];
						break;
					case eProperty.Empathy:
						cap += m_gameClient.Player.ItemBonus[(int)eProperty.EmpCapBonus];
						break;
					default: break;
				}

				if (updateStats[i] == m_gameClient.Player.CharacterClass.ManaStat)
					cap += m_gameClient.Player.ItemBonus[(int)eProperty.AcuCapBonus];

				itemCaps[i] = Math.Min(cap, itemCap + bonusCap);
			}


			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.StatsUpdate)))
			{

				// base
				for (int i = 0; i < updateStats.Length; i++)
				{
					baseStats[i] = m_gameClient.Player.GetBaseStat(updateStats[i]);

					if (updateStats[i] == eStat.CON)
						baseStats[i] -= m_gameClient.Player.TotalConstitutionLostAtDeath;

					pak.WriteShort((ushort)baseStats[i]);
				}

				pak.WriteShort(0);

				// buffs/debuffs only; remove base, item bonus, RA bonus, class bonus
				for (int i = 0; i < updateStats.Length; i++)
				{
					modStats[i] = m_gameClient.Player.GetModified((eProperty)updateStats[i]);

					int abilityBonus = m_gameClient.Player.AbilityBonus[(int)updateStats[i]];

					int acuityItemBonus = 0;
					if (updateStats[i] == m_gameClient.Player.CharacterClass.ManaStat)
					{
						if (m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Scout && m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Hunter && m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Ranger)
						{
							abilityBonus += m_gameClient.Player.AbilityBonus[(int)eProperty.Acuity];

							if (m_gameClient.Player.CharacterClass.ClassType != eClassType.PureTank)
								acuityItemBonus = m_gameClient.Player.ItemBonus[(int)eProperty.Acuity];
						}
					}

					int buff = modStats[i] - baseStats[i];
					buff -= abilityBonus;
					buff -= Math.Min(itemCaps[i], m_gameClient.Player.ItemBonus[(int)updateStats[i]] + acuityItemBonus);

					pak.WriteShort((ushort)buff);
				}

				pak.WriteShort(0);

				// item bonuses
				for (int i = 0; i < updateStats.Length; i++)
				{
					int acuityItemBonus = 0;

					if (updateStats[i] == m_gameClient.Player.CharacterClass.ManaStat)
					{
						if (m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Scout && m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Hunter && m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Ranger)
						{

							if (m_gameClient.Player.CharacterClass.ClassType != eClassType.PureTank)
								acuityItemBonus = m_gameClient.Player.ItemBonus[(int)eProperty.Acuity];
						}
					}

					pak.WriteShort((ushort)(m_gameClient.Player.ItemBonus[(int)updateStats[i]] + acuityItemBonus));
				}

				pak.WriteShort(0);

				// item caps
				for (int i = 0; i < updateStats.Length; i++)
				{
					pak.WriteByte((byte)itemCaps[i]);
				}

				pak.WriteByte(0);

				// RA bonuses
				for (int i = 0; i < updateStats.Length; i++)
				{
					int acuityItemBonus = 0;
					if (m_gameClient.Player.CharacterClass.ClassType != eClassType.PureTank && (int)updateStats[i] == (int)m_gameClient.Player.CharacterClass.ManaStat)
					{
						if (m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Scout && m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Hunter && m_gameClient.Player.CharacterClass.ID != (int)eCharacterClass.Ranger)
						{
							acuityItemBonus = m_gameClient.Player.AbilityBonus[(int)eProperty.Acuity];
						}
					}
					pak.WriteByte((byte)(m_gameClient.Player.AbilityBonus[(int)updateStats[i]] + acuityItemBonus));
				}

				pak.WriteByte(0);

				//Why don't we and mythic use this class bonus byte?
				//pak.Fill(0, 9);
				//if (_gameClient.Player.CharacterClass.ID == (int)eCharacterClass.Vampiir)
				//	pak.WriteByte((byte)(_gameClient.Player.Level - 5)); // Vampire bonuses
				//else
				pak.WriteByte(0x00); // FF if resists packet
				pak.WriteByte((byte)m_gameClient.Player.TotalConstitutionLostAtDeath);
				pak.WriteShort((ushort)m_gameClient.Player.MaxHealth);
				pak.WriteShort(0);

				SendTCP(pak);
			}
		}
		public virtual void SendConcentrationList()
		{
			if (m_gameClient.Player == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ConcentrationList)))
			{
				lock (m_gameClient.Player.ConcentrationEffects)
				{
					pak.WriteByte((byte)(m_gameClient.Player.ConcentrationEffects.Count));
					pak.WriteByte(0); // unknown
					pak.WriteByte(0); // unknown
					pak.WriteByte(0); // unknown

					for (int i = 0; i < m_gameClient.Player.ConcentrationEffects.Count; i++)
					{
						IConcentrationEffect effect = m_gameClient.Player.ConcentrationEffects[i];
						pak.WriteByte((byte)i);
						pak.WriteByte(0); // unknown
						pak.WriteByte(effect.Concentration);
						pak.WriteShort(effect.Icon);
						if (effect.Name.Length > 14)
							pak.WritePascalString(effect.Name.Substring(0, 12) + "..");
						else
							pak.WritePascalString(effect.Name);
						if (effect.OwnerName.Length > 14)
							pak.WritePascalString(effect.OwnerName.Substring(0, 12) + "..");
						else
							pak.WritePascalString(effect.OwnerName);
					}
				}
				SendTCP(pak);
			}

			SendStatusUpdate(); // send status update for convinience, mostly the conc has changed
		}
		public virtual void SendConsignmentMerchantMoney(long money)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ConsignmentMerchantMoney)))
			{
				pak.WriteByte((byte)Money.GetCopper(money));
				pak.WriteByte((byte)Money.GetSilver(money));
				pak.WriteShort((ushort)Money.GetGold(money));

				// Yes, these are sent in reverse order! - tolakram confirmed 1.98 - 1.109
				pak.WriteShort((ushort)Money.GetMithril(money));
				pak.WriteShort((ushort)Money.GetPlatinum(money));

				SendTCP(pak);
			}
		}
		public virtual void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID, ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CombatAnimation)))
			{
				if (attacker != null)
					pak.WriteShort((ushort)attacker.ObjectID);
				else
					pak.WriteShort(0x00);

				if (defender != null)
					pak.WriteShort((ushort)defender.ObjectID);
				else
					pak.WriteShort(0x00);

				pak.WriteShort(weaponID);
				pak.WriteShort(shieldID);
				pak.WriteShortLowEndian((ushort)style);
				pak.WriteByte(stance);
				pak.WriteByte(result);

				// If Health Percent is invalid get the living Health.
				if (defender is GameLiving && targetHealthPercent > 100)
				{
					targetHealthPercent = (defender as GameLiving).HealthPercent;
				}

				pak.WriteByte(targetHealthPercent);
				pak.WriteByte(0);//unk
				SendTCP(pak);
			}
		}
		public virtual void SendControlledHorse(GamePlayer player, bool flag)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ControlledHorse)))
			{
				if (!flag || !player.HasHorse)
				{
					pak.WriteShort((ushort)player.ObjectID);
					pak.Fill(0x00, 6);
				}
				else
				{
					pak.WriteShort((ushort)player.ObjectID);
					pak.WriteByte(player.ActiveHorse.ID);
					if (player.ActiveHorse.BardingColor == 0 && player.ActiveHorse.Barding != 0 && player.Guild != null)
					{
						int newGuildBitMask = (player.Guild.Emblem & 0x010000) >> 9;
						pak.WriteByte((byte)(player.ActiveHorse.Barding | newGuildBitMask));
						pak.WriteShort((ushort)player.Guild.Emblem);
					}
					else
					{
						pak.WriteByte(player.ActiveHorse.Barding);
						pak.WriteShort(player.ActiveHorse.BardingColor);
					}

					pak.WriteByte(player.ActiveHorse.Saddle);
					pak.WriteByte(player.ActiveHorse.SaddleColor);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendCrash(string str)
		{
			using (var pak = new GSTCPPacketOut(0x86))
			{
				pak.WriteByte(0xFF);
				pak.WritePascalString(str);
				SendTCP(pak);
			}
		}
		public virtual void SendCustomTextWindow(string caption, IList<string> text)
		{
			if (text == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DetailWindow)))
			{
				pak.WriteByte(0); // new in 1.75
				pak.WriteByte(0); // new in 1.81
				if (caption == null)
					caption = "";
				if (caption.Length > byte.MaxValue)
					caption = caption.Substring(0, byte.MaxValue);
				pak.WritePascalString(caption); //window caption

				WriteCustomTextWindowData(pak, text);

				//Trailing Zero!
				pak.WriteByte(0);
				SendTCP(pak);
			}
		}

		/// <summary>
		/// New system in v1.110+ for delve info. delve is cached by client in extra file, stored locally.
		/// </summary>
		/// <param name="info"></param>
		public virtual void SendDelveInfo(string info)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DelveInfo)))
			{
				pak.WriteString(info, 2048);
				pak.WriteByte(0); // 0-terminated
				SendTCP(pak);
			}
		}

		public virtual void SendDupNameCheckReply(string name, byte result)
		{
			if (m_gameClient == null || m_gameClient.Account == null)
				return;

			// This presents the user with Name Not Allowed which may not be correct but at least it prevents duplicate char creation
			// - tolakram
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DupNameCheckReply)))
			{
				pak.FillString(name, 30);
				pak.FillString(m_gameClient.Account.Name, 24);
				pak.WriteByte(result);
				pak.Fill(0x0, 3);
				SendTCP(pak);
			}
		}
		public virtual void SendEnterHouse(House house)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseEnter)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteShort((ushort)25000);         //constant!
				pak.WriteInt((uint)house.X);
				pak.WriteInt((uint)house.Y);
				pak.WriteShort((ushort)house.Heading); //useless/ignored by client.
				pak.WriteByte(0x00);
				pak.WriteByte((byte)(house.GetGuildEmblemFlags() | (house.Emblem & 0x010000) >> 14));//new Guild Emblem
				pak.WriteShort((ushort)house.Emblem);   //emblem
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				pak.WriteByte((byte)house.Model);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				pak.WriteByte((byte)house.Rug1Color);
				pak.WriteByte((byte)house.Rug2Color);
				pak.WriteByte((byte)house.Rug3Color);
				pak.WriteByte((byte)house.Rug4Color);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00); // houses codemned ?
				pak.WriteShort(0); // 0xFFBF = condemned door model
				pak.WriteByte(0x00);

				SendTCP(pak);
			}
		}
		public virtual void SendFindGroupWindowUpdate(GamePlayer[] list)
		{
			if (m_gameClient.Player == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.FindGroupUpdate)))
			{
				if (list != null)
				{
					pak.WriteByte((byte)list.Length);
					byte nbleader = 0;
					byte nbsolo = 0x1E;
					foreach (GamePlayer player in list)
					{
						if (player.Group != null)
						{
							pak.WriteByte(nbleader++);
						}
						else
						{
							pak.WriteByte(nbsolo++);
						}
						pak.WriteByte(player.Level);
						pak.WritePascalString(player.Name);
						pak.WriteString(player.CharacterClass.Name, 4);
						//Dinberg:Instances - We use ZoneSkinID to bluff our way to victory and
						//trick the client for positioning objects (as IDs are hard coded).
						if (player.CurrentZone != null)
							pak.WriteShort(player.CurrentZone.ZoneSkinID);
						else
							pak.WriteShort(0); // ?
						pak.WriteByte(0); // duration
						pak.WriteByte(0); // objective
						pak.WriteByte(0);
						pak.WriteByte(0);
						pak.WriteByte((byte)(player.Group != null ? 1 : 0));
						pak.WriteByte(0);
					}
				}
				else
				{
					pak.WriteByte(0);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendGameOpenReply()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.GameOpenReply)))
			{
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}
		public virtual void SendGarden(House house)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseChangeGarden)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteShort(0); // sheduled for repossession (in hours) new in 1.89b+
				pak.WriteByte((byte)house.OutdoorItems.Count);
				pak.WriteByte(0x80);

				foreach (var entry in house.OutdoorItems.OrderBy(entry => entry.Key))
				{
					OutdoorItem item = entry.Value;
					pak.WriteByte((byte)entry.Key);
					pak.WriteShort((ushort)item.Model);
					pak.WriteByte((byte)item.Position);
					pak.WriteByte((byte)item.Rotation);
				}

				SendTCP(pak);
			}

			// Update cache
			m_gameClient.HouseUpdateArray.UpdateIfExists(new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber), GameTimer.GetTickCount());
		}
		public virtual void SendGarden(House house, int i)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseChangeGarden)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteShort(0); // sheduled for repossession (in hours) new in 1.89b+
				pak.WriteByte(0x01);
				pak.WriteByte(0x00); // update
				OutdoorItem item = (OutdoorItem)house.OutdoorItems[i];
				pak.WriteByte((byte)i);
				pak.WriteShort((ushort)item.Model);
				pak.WriteByte((byte)item.Position);
				pak.WriteByte((byte)item.Rotation);
				SendTCP(pak);
			}

			// Update cache
			m_gameClient.HouseUpdateArray.UpdateIfExists(new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber), GameTimer.GetTickCount());
		}
		public virtual void SendGroupWindowUpdate()
		{
			if (m_gameClient.Player == null) return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak.WriteByte(0x06);

				Group group = m_gameClient.Player.Group;
				if (group == null)
				{
					pak.WriteByte(0x00);
				}
				else
				{
					pak.WriteByte(group.MemberCount);
				}

				pak.WriteByte(0x01);
				pak.WriteByte(0x00);

				if (group != null)
				{
					foreach (GameLiving living in group.GetMembersInTheGroup())
					{
						bool sameRegion = living.CurrentRegion == m_gameClient.Player.CurrentRegion;

						pak.WriteByte(living.Level);
						if (sameRegion)
						{
							pak.WriteByte(living.HealthPercentGroupWindow);
							pak.WriteByte(living.ManaPercent);
							pak.WriteByte(living.EndurancePercent); //new in 1.69

							byte playerStatus = 0;
							if (!living.IsAlive)
								playerStatus |= 0x01;
							if (living.IsMezzed)
								playerStatus |= 0x02;
							if (living.IsDiseased)
								playerStatus |= 0x04;
							if (SpellHelper.FindEffectOnTarget(living, "DamageOverTime") != null)
								playerStatus |= 0x08;
							if (living is GamePlayer && ((GamePlayer)living).Client.ClientState == GameClient.eClientState.Linkdead)
								playerStatus |= 0x10;
							if (living.CurrentRegion != m_gameClient.Player.CurrentRegion)
								playerStatus |= 0x20;

							pak.WriteByte(playerStatus);
							// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased , 
							// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region

							pak.WriteShort((ushort)living.ObjectID);//or session id?
						}
						else
						{
							pak.WriteInt(0x20);
							pak.WriteShort(0);
						}
						pak.WritePascalString(living.Name);
						pak.WritePascalString(living is GamePlayer ? ((GamePlayer)living).CharacterClass.Name : "NPC");//classname
					}
				}
				SendTCP(pak);
			}
		}
		public virtual void SendHookPointStore(GameKeepHookPoint hookPoint)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentHookpointStore)))
			{

				pak.WriteShort((ushort)hookPoint.Component.AbstractKeep.KeepID);
				pak.WriteShort((ushort)hookPoint.Component.ID);
				pak.WriteShort((ushort)hookPoint.ID);
				pak.Fill(0x01, 3);
				HookPointInventory inventory;
				if (hookPoint.ID > 0x80) inventory = HookPointInventory.YellowHPInventory; //oil
				else if (hookPoint.ID > 0x60) inventory = HookPointInventory.GreenHPInventory;//big siege
				else if (hookPoint.ID > 0x40) inventory = HookPointInventory.LightGreenHPInventory; //small siege
				else if (hookPoint.ID > 0x20) inventory = HookPointInventory.BlueHPInventory;//npc
				else inventory = HookPointInventory.RedHPInventory;//guard

				pak.WriteByte((byte)inventory.GetAllItems().Count);//count
				pak.WriteShort(0);
				int i = 0;
				foreach (HookPointItem item in inventory.GetAllItems())
				{
					//TODO : must be quite like the merchant item.
					//the problem is to change how it is done maybe make the hookpoint item inherit from an interface in common with itemtemplate. have to think to that.
					pak.WriteByte((byte)i);
					i++;
					if (item.GameObjectType == "GameKeepGuard")//TODO: hack wrong must think how design thing to have merchante of gameobject(living or item)
						pak.WriteShort(0);
					else
						pak.WriteShort(item.Flag);
					pak.WriteShort(0);
					pak.WriteShort(0);
					pak.WriteShort(0);
					pak.WriteInt((uint)item.Gold);
					pak.WriteShort(item.Icon);
					pak.WritePascalString(item.Name);//item sell
				}
				SendTCP(pak);
			}
		}
		public virtual void SendHouse(House house)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseCreate)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteShort((ushort)house.Z);
				pak.WriteInt((uint)house.X);
				pak.WriteInt((uint)house.Y);
				pak.WriteShort((ushort)house.Heading);
				pak.WriteShort((ushort)house.PorchRoofColor);
				int flagPorchAndGuildEmblem = (house.Emblem & 0x010000) >> 13;//new Guild Emblem
				if (house.Porch)
					flagPorchAndGuildEmblem |= 1;
				if (house.OutdoorGuildBanner)
					flagPorchAndGuildEmblem |= 2;
				if (house.OutdoorGuildShield)
					flagPorchAndGuildEmblem |= 4;
				pak.WriteShort((ushort)flagPorchAndGuildEmblem);
				pak.WriteShort((ushort)house.Emblem);
				pak.WriteShort(0); // new in 1.89b+ (scheduled for resposession XXX hourses ago)
				pak.WriteByte((byte)house.Model);
				pak.WriteByte((byte)house.RoofMaterial);
				pak.WriteByte((byte)house.WallMaterial);
				pak.WriteByte((byte)house.DoorMaterial);
				pak.WriteByte((byte)house.TrussMaterial);
				pak.WriteByte((byte)house.PorchMaterial);
				pak.WriteByte((byte)house.WindowMaterial);
				pak.WriteByte(0);
				pak.WriteShort(0); // new in 1.89b+
				pak.WritePascalString(house.Name);

				SendTCP(pak);
			}

			// Update cache
			m_gameClient.HouseUpdateArray[new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber)] = GameTimer.GetTickCount();
		}
		public virtual void SendHouseOccupied(House house, bool flagHouseOccuped)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseChangeGarden)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteShort(0); // sheduled for repossession (in hours) new in 1.89b+
				pak.WriteByte(0x00);
				pak.WriteByte((byte)(flagHouseOccuped ? 1 : 0));

				SendTCP(pak);
			}

			// Update cache
			m_gameClient.HouseUpdateArray.UpdateIfExists(new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber), GameTimer.GetTickCount());
		}
		public virtual void SendHexEffect(GamePlayer player, byte effect1, byte effect2, byte effect3, byte effect4, byte effect5)
		{
			if (player == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{
				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte(0x3); // show Hex
				pak.WriteByte(effect1);
				pak.WriteByte(effect2);
				pak.WriteByte(effect3);
				pak.WriteByte(effect4);
				pak.WriteByte(effect5);

				SendTCP(pak);
			}
		}
		/// <summary>
		/// New inventory update handler. This handler takes into account that
		/// a slot on the client isn't necessarily the same as a slot on the
		/// server, e.g. house vaults.
		/// </summary>
		/// <param name="updateItems"></param>
		/// <param name="windowType"></param>
		public virtual void SendInventoryItemsUpdate(IDictionary<int, InventoryItem> updateItems, eInventoryWindowType windowType)
		{
			if (m_gameClient.Player == null)
				return;
			if (updateItems == null)
				updateItems = new Dictionary<int, InventoryItem>();
			if (updateItems.Count <= ServerProperties.Properties.MAX_ITEMS_PER_PACKET)
			{
				SendInventoryItemsPartialUpdate(updateItems, windowType);
				return;
			}

			var items = new Dictionary<int, InventoryItem>(ServerProperties.Properties.MAX_ITEMS_PER_PACKET);
			foreach (var item in updateItems)
			{
				items.Add(item.Key, item.Value);
				if (items.Count >= ServerProperties.Properties.MAX_ITEMS_PER_PACKET)
				{
					SendInventoryItemsPartialUpdate(items, windowType);
					items.Clear();
					windowType = eInventoryWindowType.Update;
				}
			}

			if (items.Count > 0)
				SendInventoryItemsPartialUpdate(items, windowType);
		}
		/// <summary>
		/// Sends inventory items to the client.  If windowType is one of the client inventory windows then the client
		/// will display the window.  Once the window is displayed to the client all handling of items in the window
		/// is done in the move item request handlers
		/// </summary>
		/// <param name="items"></param>
		/// <param name="windowType"></param>
		protected virtual void SendInventoryItemsPartialUpdate(IDictionary<int, InventoryItem> items, eInventoryWindowType windowType)
		{
			//ChatUtil.SendDebugMessage(_gameClient, string.Format("SendItemsPartialUpdate: windowType: {0}, {1}", windowType, items == null ? "nothing" : items[0].Name));

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.InventoryUpdate)))
			{
				GameVault houseVault = m_gameClient.Player.ActiveInventoryObject as GameVault;
				pak.WriteByte((byte)(items.Count));
				pak.WriteByte(0x00); // new in 189b+, show shield in left hand
				pak.WriteByte((byte)((m_gameClient.Player.IsCloakInvisible ? 0x01 : 0x00) | (m_gameClient.Player.IsHelmInvisible ? 0x02 : 0x00))); // new in 189b+, cloack/helm visibility
				if (windowType == eInventoryWindowType.HouseVault && houseVault != null)
					pak.WriteByte((byte)(houseVault.Index + 1));    // Add the vault number to the window caption
				else
					pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
																																	  // ^ in 1.89b+, 0 bit - showing hooded cloack, if not hooded not show cloack at all ?
				pak.WriteByte(m_gameClient.Player.VisibleActiveWeaponSlots);
				pak.WriteByte((byte)windowType);
				foreach (var entry in items)
				{
					pak.WriteByte((byte)(entry.Key));
					WriteItemData(pak, entry.Value);
				}
				SendTCP(pak);
			}
		}
		/// <summary>
		/// Legacy inventory update. This handler silently
		/// assumes that a slot on the client matches a slot on the server.
		/// </summary>
		/// <param name="slots"></param>
		/// <param name="preAction"></param>
		protected virtual void SendInventorySlotsUpdateRange(ICollection<int> slots, eInventoryWindowType windowType)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.InventoryUpdate)))
			{
				GameVault houseVault = m_gameClient.Player.ActiveInventoryObject as GameVault;

				pak.WriteByte((byte)(slots == null ? 0 : slots.Count));
				pak.WriteByte(0); // CurrentSpeed & 0xFF (not used for player, only for NPC)
				pak.WriteByte((byte)((m_gameClient.Player.IsCloakInvisible ? 0x01 : 0x00) | (m_gameClient.Player.IsHelmInvisible ? 0x02 : 0x00))); // new in 189b+, cloack/helm visibility

				if (windowType == eInventoryWindowType.HouseVault && houseVault != null)
				{
					pak.WriteByte((byte)(houseVault.Index + 1));    // Add the vault number to the window caption
				}
				else
				{
					pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
				}

				pak.WriteByte((byte)m_gameClient.Player.VisibleActiveWeaponSlots);
				pak.WriteByte((byte)windowType);

				if (slots != null)
				{
					foreach (int updatedSlot in slots)
					{
						if (updatedSlot >= (int)eInventorySlot.Consignment_First && updatedSlot <= (int)eInventorySlot.Consignment_Last)
						{
							log.Error("PacketLib198:SendInventorySlotsUpdateBase - GameConsignmentMerchant inventory is no longer cached with player.  Use a Dictionary<int, InventoryItem> instead.");
							pak.WriteByte((byte)(updatedSlot - (int)eInventorySlot.Consignment_First + (int)eInventorySlot.HousingInventory_First));
						}
						else
						{
							pak.WriteByte((byte)(updatedSlot));
						}

						WriteItemData(pak, m_gameClient.Player.Inventory.GetItem((eInventorySlot)(updatedSlot)));

					}
				}

				SendTCP(pak);
			}
		}
		public virtual void SendKeepClaim(IGameKeep keep, byte flag)
		{
			if (m_gameClient.Player == null || keep == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepClaim)))
			{

				pak.WriteShort((ushort)keep.KeepID);
				pak.WriteByte(flag);//0-Info,1-KeepTargetLevel,2-KeepLordType,4-Release
				pak.WriteByte((byte)1); //Keep Lord Type: always melee, type is no longer used
				pak.WriteByte((byte)ServerProperties.Properties.MAX_KEEP_LEVEL);
				pak.WriteByte((byte)keep.Level);
				SendTCP(pak);
			}
		}
		/// <summary>
		/// Default Keep Model changed for 1.1115
		/// </summary>
		/// <param name="keep"></param>
		public virtual void SendKeepInfo(IGameKeep keep)
		{
			if (m_gameClient.Player == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepInfo)))
			{
				pak.WriteShort((ushort)keep.KeepID);
				pak.WriteShort(0);
				pak.WriteInt((uint)keep.X);
				pak.WriteInt((uint)keep.Y);
				pak.WriteShort((ushort)keep.Heading);
				pak.WriteByte((byte)keep.Realm);
				pak.WriteByte((byte)keep.Level);//level
				pak.WriteShort(0);//unk
				pak.WriteByte(0);//model // patch 0072
				pak.WriteByte(0);//unk

				SendTCP(pak);
			}
		}
		public virtual void SendKeepRealmUpdate(IGameKeep keep)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepRealmUpdate)))
			{

				pak.WriteShort((ushort)keep.KeepID);
				pak.WriteByte((byte)keep.Realm);
				pak.WriteByte((byte)keep.Level);
				SendTCP(pak);
			}
		}
		public virtual void SendClearKeepComponentHookPoint(IGameKeepComponent component, int selectedHookPointIndex)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentHookpointUpdate)))
			{
				pak.WriteShort((ushort)component.Keep.KeepID);
				pak.WriteShort((ushort)component.ID);
				pak.WriteByte((byte)0);
				pak.WriteByte((byte)selectedHookPointIndex);
				SendTCP(pak);
			}
		}
		public virtual void SendKeepComponentInfo(IGameKeepComponent keepComponent)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentInfo)))
			{

				pak.WriteShort((ushort)keepComponent.Keep.KeepID);
				pak.WriteShort((ushort)keepComponent.ID);
				pak.WriteInt((uint)keepComponent.ObjectID);
				pak.WriteByte((byte)keepComponent.Skin);
				pak.WriteByte((byte)(keepComponent.ComponentX));//relative to keep
				pak.WriteByte((byte)(keepComponent.ComponentY));//relative to keep
				pak.WriteByte((byte)keepComponent.ComponentHeading);
				pak.WriteByte((byte)keepComponent.Height);
				pak.WriteByte(keepComponent.HealthPercent);
				byte flag = keepComponent.Status;
				if (keepComponent.IsRaized) // Only for towers
					flag |= 0x04;
				if (flag == 0x00 && keepComponent.Climbing)
					flag = 0x02;
				pak.WriteByte(flag);
				pak.WriteByte(0x00); //unk
				SendTCP(pak);
			}
		}
		public virtual void SendKeepComponentDetailUpdate(IGameKeepComponent keepComponent)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentDetailUpdate)))
			{
				pak.WriteShort((ushort)keepComponent.Keep.KeepID);
				pak.WriteShort((ushort)keepComponent.ID);
				pak.WriteByte((byte)keepComponent.Height);
				pak.WriteByte(keepComponent.HealthPercent);
				byte flag = keepComponent.Status;

				if (keepComponent.IsRaized) // Only for towers
					flag |= 0x04;

				if (flag == 0x00 && keepComponent.Climbing)
					flag = 0x02;

				pak.WriteByte(flag);
				pak.WriteByte(0x00);//unk
				SendTCP(pak);
			}
		}
		public virtual void SendKeepComponentRemove(IGameKeepComponent keepComponent)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentRemove)))
			{
				pak.WriteShort((ushort)keepComponent.Keep.KeepID);
				pak.WriteShort((ushort)keepComponent.ID);
				SendTCP(pak);
			}
		}
		public virtual void SendKeepComponentUpdate(IGameKeep keep, bool LevelUp)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentUpdate)))
			{

				pak.WriteShort((ushort)keep.KeepID);
				pak.WriteByte((byte)keep.Realm);
				pak.WriteByte((byte)keep.Level);
				pak.WriteByte((byte)keep.SentKeepComponents.Count);
				foreach (IGameKeepComponent component in keep.SentKeepComponents)
				{
					byte m_flag = (byte)component.Height;
					if (component.Status == 0 && component.Climbing)
						m_flag |= 0x80;
					if (component.IsRaized) // Only for towers
						m_flag |= 0x10;
					if (LevelUp)
						m_flag |= 0x20;
					if (!component.IsAlive)
						m_flag |= 0x40;
					pak.WriteByte(m_flag);
				}
				pak.WriteByte((byte)0);//unk
				SendTCP(pak);
			}
		}
		public virtual void SendKeepComponentInteract(IGameKeepComponent component)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentInteractResponse)))
			{
				pak.WriteShort((ushort)component.Keep.KeepID);
				pak.WriteByte((byte)component.Keep.Realm);
				pak.WriteByte(component.HealthPercent);

				pak.WriteByte(component.Keep.EffectiveLevel(component.Keep.Level));
				pak.WriteByte(component.Keep.EffectiveLevel((byte)ServerProperties.Properties.MAX_KEEP_LEVEL));
				//guild
				pak.WriteByte((byte)1); //Keep Type: always melee here, type is no longer used

				if (component.Keep.Guild != null)
				{
					pak.WriteString(component.Keep.Guild.Name);
				}
				pak.WriteByte(0);
				SendTCP(pak);
			}
		}

		public virtual void SendKeepComponentHookPoint(IGameKeepComponent component, int selectedHookPointIndex)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepComponentHookpointUpdate)))
			{
				pak.WriteShort((ushort)component.Keep.KeepID);
				pak.WriteShort((ushort)component.ID);
				ArrayList freeHookpoints = new ArrayList();
				foreach (GameKeepHookPoint hookPt in component.HookPoints.Values)
				{
					if (hookPt.IsFree) freeHookpoints.Add(hookPt);
				}
				pak.WriteByte((byte)freeHookpoints.Count);
				pak.WriteByte((byte)selectedHookPointIndex);
				foreach (GameKeepHookPoint hookPt in freeHookpoints)//have to sort by index?
				{
					pak.WriteByte((byte)hookPt.ID);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendKeepRemove(IGameKeep keep)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.KeepRemove)))
			{

				pak.WriteShort((ushort)keep.KeepID);
				SendTCP(pak);
			}
		}
		public virtual void SendLivingEquipmentUpdate(GameLiving living)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.EquipmentUpdate)))
			{

				ICollection<InventoryItem> items = null;
				if (living.Inventory != null)
					items = living.Inventory.VisibleItems;

				pak.WriteShort((ushort)living.ObjectID);
				pak.WriteByte((byte)living.VisibleActiveWeaponSlots);
				pak.WriteByte((byte)living.CurrentSpeed); // new in 189b+, speed
				pak.WriteByte((byte)((living.IsCloakInvisible ? 0x01 : 0x00) | (living.IsHelmInvisible ? 0x02 : 0x00))); // new in 189b+, cloack/helm visibility
				pak.WriteByte((byte)((living.IsCloakHoodUp ? 0x01 : 0x00) | (int)living.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver

				if (items != null)
				{
					pak.WriteByte((byte)items.Count);
					foreach (InventoryItem item in items)
					{
						ushort model = (ushort)(item.Model & 0x1FFF);
						int slot = item.SlotPosition;
						//model = GetModifiedModel(model);
						int texture = item.Emblem != 0 ? item.Emblem : item.Color;
						if (item.SlotPosition == Slot.LEFTHAND || item.SlotPosition == Slot.CLOAK) // for test only cloack and shield
							slot = slot | ((texture & 0x010000) >> 9); // slot & 0x80 if new emblem
						pak.WriteByte((byte)slot);
						if ((texture & ~0xFF) != 0)
							model |= 0x8000;
						else if ((texture & 0xFF) != 0)
							model |= 0x4000;
						if (item.Effect != 0)
							model |= 0x2000;

						pak.WriteShort(model);

						if (item.SlotPosition > Slot.RANGED || item.SlotPosition < Slot.RIGHTHAND)
							pak.WriteByte((byte)item.Extension);

						if ((texture & ~0xFF) != 0)
							pak.WriteShort((ushort)texture);
						else if ((texture & 0xFF) != 0)
							pak.WriteByte((byte)texture);
						if (item.Effect != 0)
							pak.WriteShort((byte)item.Effect); // effect changed to short
					}
				}
				else
				{
					pak.WriteByte(0x00);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendLivingDataUpdate(GameLiving living, bool updateStrings)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ObjectDataUpdate)))
			{
				pak.WriteShort((ushort)living.ObjectID);
				pak.WriteByte(0);
				pak.WriteByte(living.Level);
				GamePlayer player = living as GamePlayer;
				if (player != null)
				{
					pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, player));
					pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, player));
				}
				else if (!updateStrings)
				{
					pak.WriteByte(0xFF);
				}
				else
				{
					pak.WritePascalString(living.GuildName);
					pak.WritePascalString(living.Name);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendLoginDenied(eLoginError et)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.LoginDenied)))
			{
				pak.WriteByte((byte)et); // Error Code
										 /*
									 if(is_si)
										 pak.WriteByte(0x32);
									 else
										 pak.WriteByte(0x31);
										  */
				pak.WriteByte(0x01);
				pak.WriteByte(ParseVersion((int)m_gameClient.Version, true));
				pak.WriteByte(ParseVersion((int)m_gameClient.Version, false));
				//pak.WriteByte(build);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}
		public virtual void SendLoginGranted()
		{
			//[Freya] Nidel: Can use realm button in character selection screen

			if (ServerProperties.Properties.ALLOW_ALL_REALMS || m_gameClient.Account.PrivLevel > (int)ePrivLevel.Player)
			{
				SendLoginGranted(1);
			}
			else
			{
				SendLoginGranted(GameServer.ServerRules.GetColorHandling(m_gameClient));
			}
		}
		public virtual void SendLoginGranted(byte color)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.LoginGranted)))
			{
				pak.WritePascalString(m_gameClient.Account.Name);
				pak.WritePascalString(GameServer.Instance.Configuration.ServerNameShort); //server name
				pak.WriteByte(0x0C); //Server ID
				pak.WriteByte(color);
				pak.WriteByte(0x00);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}
		public virtual void SendMarketExplorerWindow()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MarketExplorerWindow)))
			{
				pak.WriteByte(255);
				pak.Fill(0, 3);
				SendTCP(pak);
			}
		}

		public virtual void SendMarketExplorerWindow(IList<InventoryItem> items, byte page, byte maxpage)
		{
			if (m_gameClient == null || m_gameClient.Player == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MarketExplorerWindow)))
			{
				pak.WriteByte((byte)items.Count);
				pak.WriteByte(page);
				pak.WriteByte(maxpage);
				pak.WriteByte(0);
				foreach (InventoryItem item in items)
				{
					pak.WriteByte((byte)items.IndexOf(item));
					pak.WriteByte((byte)item.Level);
					int value1; // some object types use this field to display count
					int value2; // some object types use this field to display count
					switch (item.Object_Type)
					{
						case (int)eObjectType.Arrow:
						case (int)eObjectType.Bolt:
						case (int)eObjectType.Poison:
						case (int)eObjectType.GenericItem:
							value1 = item.PackSize;
							value2 = item.SPD_ABS; break;
						case (int)eObjectType.Thrown:
							value1 = item.DPS_AF;
							value2 = item.PackSize; break;
						case (int)eObjectType.Instrument:
							value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF); // 0x00 = Lute ; 0x01 = Drum ; 0x03 = Flute
							value2 = 0; break; // unused
						case (int)eObjectType.Shield:
							value1 = item.Type_Damage;
							value2 = item.DPS_AF; break;
						case (int)eObjectType.GardenObject:
						case (int)eObjectType.HouseWallObject:
						case (int)eObjectType.HouseFloorObject:
							value1 = 0;
							value2 = item.SPD_ABS; break;
						default:
							value1 = item.DPS_AF;
							value2 = item.SPD_ABS; break;
					}
					pak.WriteByte((byte)value1);
					pak.WriteByte((byte)value2);
					if (item.Object_Type == (int)eObjectType.GardenObject)
						pak.WriteByte((byte)(item.DPS_AF));
					else
						pak.WriteByte((byte)(item.Hand << 6));
					pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
					pak.WriteByte((byte)(m_gameClient.Player.HasAbilityToUseItem(item.Template) ? 0 : 1));
					pak.WriteShort((ushort)(item.PackSize > 1 ? item.Weight * item.PackSize : item.Weight));
					pak.WriteByte((byte)item.ConditionPercent);
					pak.WriteByte((byte)item.DurabilityPercent);
					pak.WriteByte((byte)item.Quality);
					pak.WriteByte((byte)item.Bonus);
					pak.WriteShort((ushort)item.Model);
					if (item.Emblem != 0)
						pak.WriteShort((ushort)item.Emblem);
					else
						pak.WriteShort((ushort)item.Color);
					pak.WriteShort((byte)item.Effect);
					pak.WriteShort(item.OwnerLot);//lot
					pak.WriteInt((uint)item.SellPrice);

					if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					{
						string bpPrice = "";
						if (item.SellPrice > 0)
							bpPrice = "[" + item.SellPrice.ToString() + " BP";

						if (item.Count > 1)
							pak.WritePascalString(item.Count + " " + item.Name);
						else if (item.PackSize > 1)
							pak.WritePascalString(item.PackSize + " " + item.Name + bpPrice);
						else
							pak.WritePascalString(item.Name + bpPrice);
					}
					else
					{
						if (item.Count > 1)
							pak.WritePascalString(item.Count + " " + item.Name);
						else if (item.PackSize > 1)
							pak.WritePascalString(item.PackSize + " " + item.Name);
						else
							pak.WritePascalString(item.Name);
					}
				}

				SendTCP(pak);
			}
		}
		public virtual void SendMasterLevelWindow(byte ml)
		{
			if (m_gameClient == null || m_gameClient.Player == null)
				return;

			// If required ML=0 then send current player ML data
			byte mlToSend = (byte)(ml == 0 ? (m_gameClient.Player.MLLevel == 0 ? 1 : m_gameClient.Player.MLLevel) : ml);

			if (mlToSend > GamePlayer.ML_MAX_LEVEL)
				mlToSend = GamePlayer.ML_MAX_LEVEL;

			double mlXPPercent = 0;
			double mlStepPercent = 0;

			if (m_gameClient.Player.MLLevel < 10)
			{
				mlXPPercent = 100.0 * m_gameClient.Player.MLExperience / m_gameClient.Player.GetMLExperienceForLevel(m_gameClient.Player.MLLevel + 1);
				if (m_gameClient.Player.GetStepCountForML((byte)(m_gameClient.Player.MLLevel + 1)) > 0)
				{
					mlStepPercent = 100.0 * m_gameClient.Player.GetCountMLStepsCompleted((byte)(m_gameClient.Player.MLLevel + 1)) / m_gameClient.Player.GetStepCountForML((byte)(m_gameClient.Player.MLLevel + 1));
				}
			}
			else
			{
				mlXPPercent = 100.0; // ML10 has no MLXP, so always 100%
			}

			using (GSTCPPacketOut pak = new GSTCPPacketOut((byte)eServerPackets.MasterLevelWindow))
			{
				pak.WriteByte((byte)mlXPPercent); // MLXP (blue bar)
				pak.WriteByte((byte)Math.Min(mlStepPercent, 100)); // Step percent (red bar)
				pak.WriteByte((byte)(m_gameClient.Player.MLLevel + 1)); // ML level + 1
				pak.WriteByte(0);
				pak.WriteShort((ushort)0); // exp1 ? new in 1.90
				pak.WriteShort((ushort)0); // exp2 ? new in 1.90
				pak.WriteByte(ml);

				// ML level completion is displayed client side for Step 11
				for (int i = 1; i < 11; i++)
				{
					string description = m_gameClient.Player.GetMLStepDescription(mlToSend, i);
					pak.WritePascalString(description);
				}

				pak.WriteByte(0);
				SendTCP(pak);
			}
		}
		public virtual void SendMessage(string msg, eChatType type, eChatLoc loc)
		{
			if (m_gameClient.ClientState == GameClient.eClientState.CharScreen)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Message)))
			{
				pak.WriteShort(0xFFFF);
				pak.WriteShort((ushort)m_gameClient.SessionID);
				pak.WriteByte((byte)type);
				pak.Fill(0x0, 3);

				string str;
				if (loc == eChatLoc.CL_ChatWindow)
					str = "@@";
				else if (loc == eChatLoc.CL_PopupWindow)
					str = "##";
				else
					str = "";

				pak.WriteString(str + msg);
				SendTCP(pak);
			}
		}
		public virtual void SendMinotaurRelicMapRemove(byte id)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MinotaurRelicMapRemove)))
			{
				pak.WriteIntLowEndian((uint)id);
				SendTCP(pak);
			}
		}

		public virtual void SendMinotaurRelicMapUpdate(byte id, ushort region, int x, int y, int z)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MinotaurRelicMapUpdate)))
			{

				pak.WriteIntLowEndian((uint)id);
				pak.WriteIntLowEndian((uint)region);
				pak.WriteIntLowEndian((uint)x);
				pak.WriteIntLowEndian((uint)y);
				pak.WriteIntLowEndian((uint)z);

				SendTCP(pak);
			}
		}

		public virtual void SendMinotaurRelicWindow(GamePlayer player, int effect, bool flag)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{

				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte((byte)13);
				if (flag)
				{
					pak.WriteByte(0);
					pak.WriteInt((uint)effect);
				}
				else
				{
					pak.WriteByte(1);
					pak.WriteInt((uint)effect);
				}

				SendTCP(pak);
			}
		}

		public virtual void SendMinotaurRelicBarUpdate(GamePlayer player, int xp)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{

				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte((byte)14);
				pak.WriteByte(0);
				//4k maximum
				if (xp > 4000) xp = 4000;
				if (xp < 0) xp = 0;

				pak.WriteInt((uint)xp);

				SendTCP(pak);
			}
		}
		/// <summary>
		/// Send non hybrid and advanced spell lines
		/// </summary>
		public virtual void SendNonHybridSpellLines()
		{
			GamePlayer player = m_gameClient.Player;
			if (player == null)
				return;

			List<Tuple<SpellLine, List<Skill>>> spellsXLines = player.GetAllUsableListSpells(true);

			int lineIndex = 0;
			foreach (var spXsl in spellsXLines)
			{
				// Prepare packet
				using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
				{
					// Add Line Header
					pak.WriteByte(0x02); //subcode
					pak.WriteByte((byte)(spXsl.Item2.Count + 1)); //number of entry
					pak.WriteByte(0x02); //subtype
					pak.WriteByte((byte)lineIndex); //number of line

					pak.WriteShortLowEndian(0); // level, not used when spell line
					pak.WriteShort((ushort)spXsl.Item1.InternalID); //new 1.112
					pak.WriteShort(0); // icon, not used when spell line
					pak.WritePascalString(spXsl.Item1.Name);

					// Add All Spells...
					foreach (Skill sk in spXsl.Item2)
					{
						if (sk is Spell)
						{
							Spell sp = (Spell)sk;
							pak.WriteShortLowEndian((byte)sp.Level);
							pak.WriteShort((ushort)sp.InternalID); //new 1.112
							pak.WriteShort(sp.Icon);
							pak.WritePascalString(sp.Name);
						}
						else
						{
							int reqLevel = 1;
							if (sk is Style)
								reqLevel = ((Style)sk).SpecLevelRequirement;
							else if (sk is Ability)
								reqLevel = ((Ability)sk).SpecLevelRequirement;

							pak.WriteShortLowEndian((ushort)((byte)reqLevel + (sk is Style ? 512 : 256)));
							pak.WriteShort((ushort)sk.InternalID); //new 1.112
							pak.WriteShort(sk.Icon);
							pak.WritePascalString(sk.Name);
						}
					}

					// Send
					SendTCP(pak);
				}

				lineIndex++;
			}

			// Footer packet
			using (GSTCPPacketOut pak3 = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak3.WriteByte(0x02); //subcode
				pak3.WriteByte(0x00);
				pak3.WriteByte(99); //subtype (new subtype 99 in 1.80e)
				pak3.WriteByte(0x00);
				SendTCP(pak3);
			}

			if (ForceTooltipUpdate)
				SendForceTooltipUpdate(spellsXLines.SelectMany(e => e.Item2));
		}
		public virtual void SendNPCCreate(GameNPC npc)
		{

			if (m_gameClient.Player == null || npc.IsVisibleTo(m_gameClient.Player) == false)
				return;

			//Added by Suncheck - Mines are not shown to enemy players
			if (npc is GameMine)
			{
				if (GameServer.ServerRules.IsAllowedToAttack((npc as GameMine).Owner, m_gameClient.Player, true))
				{
					return;
				}
			}

			if (npc is GameMovingObject)
			{
				SendMovingObjectCreate(npc as GameMovingObject);
				return;
			}

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.NPCCreate)))
			{
				int speed = 0;
				ushort speedZ = 0;
				if (npc == null)
					return;
				if (!npc.IsAtTargetPosition)
				{
					speed = npc.CurrentSpeed;
					speedZ = (ushort)npc.TickSpeedZ;
				}
				pak.WriteShort((ushort)npc.ObjectID);
				pak.WriteShort((ushort)(speed));
				pak.WriteShort(npc.Heading);
				pak.WriteShort((ushort)npc.Z);
				pak.WriteInt((uint)npc.X);
				pak.WriteInt((uint)npc.Y);
				pak.WriteShort(speedZ);
				pak.WriteShort(npc.Model);
				pak.WriteByte(npc.Size);
				byte level = npc.GetDisplayLevel(m_gameClient.Player);
				if ((npc.Flags & GameNPC.eFlags.STATUE) != 0)
				{
					level |= 0x80;
				}
				pak.WriteByte(level);

				byte flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);
				if ((npc.Flags & GameNPC.eFlags.GHOST) != 0) flags |= 0x01;
				if (npc.Inventory != null) flags |= 0x02; //If mob has equipment, then only show it after the client gets the 0xBD packet
				if ((npc.Flags & GameNPC.eFlags.PEACE) != 0) flags |= 0x10;
				if ((npc.Flags & GameNPC.eFlags.FLYING) != 0) flags |= 0x20;
				if ((npc.Flags & GameNPC.eFlags.TORCH) != 0) flags |= 0x04;

				pak.WriteByte(flags);
				pak.WriteByte(0x20); //TODO this is the default maxstick distance

				string add = "";
				byte flags2 = 0x00;
				IControlledBrain brain = npc.Brain as IControlledBrain;

				if (brain != null)
				{
					flags2 |= 0x80; // have Owner
				}

				if ((npc.Flags & GameNPC.eFlags.CANTTARGET) != 0)
					if (m_gameClient.Account.PrivLevel > 1) add += "-DOR"; // indicates DOR flag for GMs
					else flags2 |= 0x01;
				if ((npc.Flags & GameNPC.eFlags.DONTSHOWNAME) != 0)
					if (m_gameClient.Account.PrivLevel > 1) add += "-NON"; // indicates NON flag for GMs
					else flags2 |= 0x02;

				if ((npc.Flags & GameNPC.eFlags.STEALTH) > 0)
					flags2 |= 0x04;

				eQuestIndicator questIndicator = npc.GetQuestIndicator(m_gameClient.Player);

				if (questIndicator == eQuestIndicator.Available)
					flags2 |= 0x08;//hex 8 - quest available
				if (questIndicator == eQuestIndicator.Finish)
					flags2 |= 0x10;//hex 16 - quest finish
								   //flags2 |= 0x20;//hex 32 - water mob?
								   //flags2 |= 0x40;//hex 64 - unknown
								   //flags2 |= 0x80;//hex 128 - has owner


				pak.WriteByte(flags2); // flags 2

				byte flags3 = 0x00;
				if (questIndicator == eQuestIndicator.Lesson)
					flags3 |= 0x01;
				if (questIndicator == eQuestIndicator.Lore)
					flags3 |= 0x02;
				if (questIndicator == eQuestIndicator.Pending) // new? patch 0031
					flags3 |= 0x20;
				pak.WriteByte(flags3); // new in 1.71 (region instance ID from StoC_0x20) OR flags 3?
				pak.WriteShort(0x00); // new in 1.71 unknown

				string name = npc.Name;
				string guildName = npc.GuildName;

				LanguageDataObject translation = LanguageMgr.GetTranslation(m_gameClient, npc);
				if (translation != null)
				{
					if (!Util.IsEmpty(((DBLanguageNPC)translation).Name))
						name = ((DBLanguageNPC)translation).Name;

					if (!Util.IsEmpty(((DBLanguageNPC)translation).GuildName))
						guildName = ((DBLanguageNPC)translation).GuildName;
				}

				if (name.Length + add.Length + 2 > 47) // clients crash with too long names
					name = name.Substring(0, 47 - add.Length - 2);
				if (add.Length > 0)
					name = string.Format("[{0}]{1}", name, add);

				pak.WritePascalString(name);

				if (guildName.Length > 47)
					pak.WritePascalString(guildName.Substring(0, 47));
				else pak.WritePascalString(guildName);

				pak.WriteByte(0x00);
				SendTCP(pak);
			}
			/* removed, hack fix for client spamming requests for npcupdates/ creates
            if (_gameClient.Player.Client.Version < _gameClient.eClientVersion.Version1124) 
            {   // Update Cache
                _gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(npc.CurrentRegionID, (ushort)npc.ObjectID)] = 0;
            }*/
		}
		public virtual void SendNPCsQuestEffect(GameNPC npc, eQuestIndicator indicator)
		{
			if (m_gameClient.Player == null || npc == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{

				pak.WriteShort((ushort)npc.ObjectID);
				pak.WriteByte(0x7); // Quest visual effect
				pak.WriteByte((byte)indicator);
				pak.WriteInt(0);

				SendTCP(pak);
			}
		}
		public virtual void SendObjectCreate(GameObject obj)
		{
			if (obj == null)
				return;

			if (obj.IsVisibleTo(m_gameClient.Player) == false)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ObjectCreate)))
			{
				pak.WriteShort((ushort)obj.ObjectID);

				if (obj is GameStaticItem)
					pak.WriteShort((ushort)(obj as GameStaticItem).Emblem);
				else
					pak.WriteShort(0);

				pak.WriteShort(obj.Heading);
				pak.WriteShort((ushort)obj.Z);
				pak.WriteInt((uint)obj.X);
				pak.WriteInt((uint)obj.Y);
				int flag = ((byte)obj.Realm & 3) << 4;
				ushort model = obj.Model;
				if (obj.IsUnderwater)
				{
					if (obj is GameNPC)
						model |= 0x8000;
					else
						flag |= 0x01; // Underwater
				}
				pak.WriteShort(model);
				if (obj is Keeps.GameKeepBanner)
					flag |= 0x08;
				if (obj is GameStaticItemTimed && m_gameClient.Player != null && ((GameStaticItemTimed)obj).IsOwner(m_gameClient.Player))
					flag |= 0x04;
				pak.WriteShort((ushort)flag);
				if (obj is GameStaticItem)
				{
					int newEmblemBitMask = ((obj as GameStaticItem).Emblem & 0x010000) << 9;
					pak.WriteInt((uint)newEmblemBitMask);//TODO other bits
				}
				else pak.WriteInt(0);

				string name = obj.Name;
				LanguageDataObject translation = null;
				if (obj is GameStaticItem)
				{
					translation = LanguageMgr.GetTranslation(m_gameClient, (GameStaticItem)obj);
					if (translation != null)
					{
						if (obj is WorldInventoryItem)
						{
							//if (!Util.IsEmpty(((DBLanguageItem)translation).Name))
							//    name = ((DBLanguageItem)translation).Name;
						}
						else
						{
							if (!Util.IsEmpty(((DBLanguageGameObject)translation).Name))
								name = ((DBLanguageGameObject)translation).Name;
						}
					}
				}
				pak.WritePascalString(name.Length > 48 ? name.Substring(0, 48) : name);

				if (obj is IDoor)
				{
					pak.WriteByte(4);
					pak.WriteInt((uint)(obj as IDoor).DoorID);
				}
				else pak.WriteByte(0x00);
				SendTCP(pak);
			}

			// Update Object Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)] = GameTimer.GetTickCount();
		}
		public virtual void SendObjectGuildID(GameObject obj, Guild guild)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ObjectGuildID)))
			{
				pak.WriteShort((ushort)obj.ObjectID);
				if (guild == null)
					pak.WriteInt(0x00);
				else
				{
					pak.WriteShort(guild.ID);
					pak.WriteShort(guild.ID);
				}
				pak.WriteShort(0x00); //seems random, not used by the client
				SendTCP(pak);
			}
		}
		public virtual void SendObjectRemove(GameObject obj)
		{
			// Remove from cache
			if (m_gameClient.GameObjectUpdateArray.ContainsKey(new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)))
			{
				long dummy;
				m_gameClient.GameObjectUpdateArray.TryRemove(new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID), out dummy);
			}

			int oType = 0;
			if (obj is GamePlayer)
				oType = 2;
			else if (obj is GameNPC)
				oType = (((GameLiving)obj).IsAlive ? 1 : 0);

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.RemoveObject)))
			{
				pak.WriteShort((ushort)obj.ObjectID);
				pak.WriteShort((ushort)oType);
				SendTCP(pak);
			}

		}
		public virtual void SendObjectUpdate(GameObject obj)
		{
			Zone z = obj.CurrentZone;

			if (z == null ||
				m_gameClient.Player == null ||
				m_gameClient.Player.IsVisibleTo(obj) == false)
			{
				return;
			}

			var xOffsetInZone = (ushort)(obj.X - z.XOffset);
			var yOffsetInZone = (ushort)(obj.Y - z.YOffset);
			ushort xOffsetInTargetZone = 0;
			ushort yOffsetInTargetZone = 0;
			ushort zOffsetInTargetZone = 0;

			int speed = 0;
			ushort targetZone = 0;
			byte flags = 0;
			int targetOID = 0;
			if (obj is GameNPC)
			{
				var npc = obj as GameNPC;
				flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);

				if (m_gameClient.Account.PrivLevel < 2)
				{
					// no name only if normal player
					if ((npc.Flags & GameNPC.eFlags.CANTTARGET) != 0)
						flags |= 0x01;
					if ((npc.Flags & GameNPC.eFlags.DONTSHOWNAME) != 0)
						flags |= 0x02;
				}
				if ((npc.Flags & GameNPC.eFlags.STATUE) != 0)
				{
					flags |= 0x01;
				}
				if (npc.IsUnderwater)
				{
					flags |= 0x10;
				}
				if ((npc.Flags & GameNPC.eFlags.FLYING) != 0)
				{
					flags |= 0x20;
				}

				if (npc.IsMoving && !npc.IsAtTargetPosition)
				{
					speed = npc.CurrentSpeed;
					if (npc.TargetPosition.X != 0 || npc.TargetPosition.Y != 0 || npc.TargetPosition.Z != 0)
					{
						Zone tz = npc.CurrentRegion.GetZone(npc.TargetPosition.X, npc.TargetPosition.Y);
						if (tz != null)
						{
							xOffsetInTargetZone = (ushort)(npc.TargetPosition.X - tz.XOffset);
							yOffsetInTargetZone = (ushort)(npc.TargetPosition.Y - tz.YOffset);
							zOffsetInTargetZone = (ushort)(npc.TargetPosition.Z);
							//Dinberg:Instances - zoneSkinID for object positioning clientside.
							targetZone = tz.ZoneSkinID;
						}
					}

					if (speed > 0x07FF)
					{
						speed = 0x07FF;
					}
					else if (speed < 0)
					{
						speed = 0;
					}
				}

				GameObject target = npc.TargetObject;
				if (npc.AttackState && target != null && target.ObjectState == GameObject.eObjectState.Active && !npc.IsTurningDisabled)
					targetOID = (ushort)target.ObjectID;
			}

			using (GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.ObjectUpdate)))
			{
				pak.WriteShort((ushort)speed);

				if (obj is GameNPC)
				{
					pak.WriteShort((ushort)(obj.Heading & 0xFFF));
				}
				else
				{
					pak.WriteShort(obj.Heading);
				}
				pak.WriteShort(xOffsetInZone);
				pak.WriteShort(xOffsetInTargetZone);
				pak.WriteShort(yOffsetInZone);
				pak.WriteShort(yOffsetInTargetZone);
				pak.WriteShort((ushort)obj.Z);
				pak.WriteShort(zOffsetInTargetZone);
				pak.WriteShort((ushort)obj.ObjectID);
				pak.WriteShort((ushort)targetOID);
				//health
				if (obj is GameLiving)
				{
					pak.WriteByte((obj as GameLiving).HealthPercent);
				}
				else
				{
					pak.WriteByte(0);
				}
				//Dinberg:Instances - zoneskinID for positioning of objects clientside.
				flags |= (byte)(((z.ZoneSkinID & 0x100) >> 6) | ((targetZone & 0x100) >> 5));
				pak.WriteByte(flags);
				pak.WriteByte((byte)z.ZoneSkinID);
				//Dinberg:Instances - targetZone already accomodates for this feat.
				pak.WriteByte((byte)targetZone);
				SendUDP(pak);
			}
			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)] = GameTimer.GetTickCount();

			if (obj is GameNPC)
			{
				(obj as GameNPC).NPCUpdatedCallback();
			}
		}

		public virtual void SendPetWindow(GameLiving pet, ePetWindowAction windowAction, eAggressionState aggroState, eWalkState walkState)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PetWindow)))
			{
				pak.WriteShort((ushort)(pet == null ? 0 : pet.ObjectID));
				pak.WriteByte(0x00); //unused
				pak.WriteByte(0x00); //unused
				switch (windowAction) //0-released, 1-normal, 2-just charmed? | Roach: 0-close window, 1-update window, 2-create window
				{
					case ePetWindowAction.Open: pak.WriteByte(2); break;
					case ePetWindowAction.Update: pak.WriteByte(1); break;
					default: pak.WriteByte(0); break;
				}
				switch (aggroState) //1-aggressive, 2-defensive, 3-passive
				{
					case eAggressionState.Aggressive: pak.WriteByte(1); break;
					case eAggressionState.Defensive: pak.WriteByte(2); break;
					case eAggressionState.Passive: pak.WriteByte(3); break;
					default: pak.WriteByte(0); break;
				}
				switch (walkState) //1-follow, 2-stay, 3-goto, 4-here
				{
					case eWalkState.Follow: pak.WriteByte(1); break;
					case eWalkState.Stay: pak.WriteByte(2); break;
					case eWalkState.GoTarget: pak.WriteByte(3); break;
					case eWalkState.ComeHere: pak.WriteByte(4); break;
					default: pak.WriteByte(0); break;
				}
				pak.WriteByte(0x00); //unused

				if (pet != null)
				{
					lock (pet.EffectList)
					{
						ArrayList icons = new ArrayList();
						foreach (IGameEffect effect in pet.EffectList)
						{
							if (icons.Count >= 8)
								break;
							if (effect.Icon == 0)
								continue;
							icons.Add(effect.Icon);
						}
						pak.WriteByte((byte)icons.Count); // effect count
														  // 0x08 - null terminated - (byte) list of shorts - spell icons on pet
						foreach (ushort icon in icons)
						{
							pak.WriteShort(icon);
						}
					}
				}
				else
					pak.WriteByte((byte)0); // effect count
				SendTCP(pak);
			}
		}
		public virtual void SendPingReply(ulong timestamp, ushort sequence)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PingReply)))
			{
				pak.WriteInt((uint)timestamp);
				pak.Fill(0x00, 4);
				pak.WriteShort((ushort)(sequence + 1));
				pak.Fill(0x00, 6);
				SendTCP(pak);
			}
		}
		public virtual void SendPlayerCreate(GamePlayer playerToCreate)
		{
			if (playerToCreate == null)
			{
				if (log.IsErrorEnabled)
					log.Error("SendPlayerCreate: playerToCreate == null");
				return;
			}

			if (m_gameClient.Player == null)
			{
				if (log.IsErrorEnabled)
					log.Error("SendPlayerCreate: _gameClient.Player == null");
				return;
			}

			Region playerRegion = playerToCreate.CurrentRegion;
			if (playerRegion == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerRegion == null");
				return;
			}

			Zone playerZone = playerToCreate.CurrentZone;
			if (playerZone == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("SendPlayerCreate: playerZone == null");
				return;
			}

			if (playerToCreate.IsVisibleTo(m_gameClient.Player) == false)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerCreate172)))
			{
				pak.WriteFloatLowEndian(playerToCreate.X);
				pak.WriteFloatLowEndian(playerToCreate.Y);
				pak.WriteFloatLowEndian(playerToCreate.Z);
				pak.WriteShort((ushort)playerToCreate.Client.SessionID);
				pak.WriteShort((ushort)playerToCreate.ObjectID);
				pak.WriteShort(playerToCreate.Heading);
				pak.WriteShort(playerToCreate.Model);
				pak.WriteByte(playerToCreate.GetDisplayLevel(m_gameClient.Player));

				int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
				if (playerToCreate.IsAlive == false) flags |= 0x01;
				if (playerToCreate.IsUnderwater) flags |= 0x02; //swimming
				if (playerToCreate.IsStealthed) flags |= 0x10;
				if (playerToCreate.IsWireframe) flags |= 0x20;
				if (playerToCreate.CharacterClass.ID == (int)eCharacterClass.Vampiir) flags |= 0x40; //Vamp fly
				pak.WriteByte((byte)flags);

				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeSize)); //1-4 = Eye Size / 5-8 = Nose Size
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.LipSize)); //1-4 = Ear size / 5-8 = Kin size
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.MoodType)); //1-4 = Ear size / 5-8 = Kin size
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeColor)); //1-4 = Skin Color / 5-8 = Eye Color                
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairColor)); //Hair: 1-4 = Color / 5-8 = unknown
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.FaceType)); //1-4 = Unknown / 5-8 = Face type
				pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairStyle)); //1-4 = Unknown / 5-8 = Hair Style

				pak.WriteByte(0x00); // new in 1.74
				pak.WriteByte(0x00); //unknown
				pak.WriteByte(0x00); //unknown
				pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
				pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
				pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
				//RR 12 / 13
				pak.WritePascalString(GameServer.ServerRules.GetPlayerPrefixName(m_gameClient.Player, playerToCreate));
				pak.WritePascalString(GameServer.ServerRules.GetPlayerTitle(m_gameClient.Player, playerToCreate)); // new in 1.74, NewTitle
				if (playerToCreate.IsOnHorse)
				{
					pak.WriteByte(playerToCreate.ActiveHorse.ID);
					if (playerToCreate.ActiveHorse.BardingColor == 0 && playerToCreate.ActiveHorse.Barding != 0 && playerToCreate.Guild != null)
					{
						int newGuildBitMask = (playerToCreate.Guild.Emblem & 0x010000) >> 9;
						pak.WriteByte((byte)(playerToCreate.ActiveHorse.Barding | newGuildBitMask));
						pak.WriteShortLowEndian((ushort)playerToCreate.Guild.Emblem);
					}
					else
					{
						pak.WriteByte(playerToCreate.ActiveHorse.Barding);
						pak.WriteShort(playerToCreate.ActiveHorse.BardingColor);
					}
					pak.WriteByte(playerToCreate.ActiveHorse.Saddle);
					pak.WriteByte(playerToCreate.ActiveHorse.SaddleColor);
				}
				else
				{
					pak.WriteByte(0); // trailing zero
				}

				SendTCP(pak);
			}

			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(playerToCreate.CurrentRegionID, (ushort)playerToCreate.ObjectID)] = GameTimer.GetTickCount();

			SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server

			if (playerToCreate.GuildBanner != null)
			{
				SendRvRGuildBanner(playerToCreate, true);
			}
		}

		/// <summary>
		/// This is used to build a server side "Position Object"
		/// Usually Position Packet Should only be relayed
		/// The only purpose of this method is refreshing postion when there is Lag
		/// </summary>
		/// <param name="player"></param>
		public virtual void SendPlayerForgedPosition(GamePlayer player)
		{
			// doesn't work in 1.124+
			return;

			using (GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.PlayerPosition)))
			{
				int heading = 4096 + player.Heading;
				pak.WriteFloatLowEndian(player.X);
				pak.WriteFloatLowEndian(player.Y);
				pak.WriteFloatLowEndian(player.Z);
				pak.WriteFloatLowEndian(player.CurrentSpeed);
				pak.WriteInt(0); // needs to be Zspeed
				pak.WriteShort((ushort)player.Client.SessionID);
				pak.WriteShort(player.CurrentZone.ZoneSkinID);
				// Write Speed
				if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
				{
					player.Heading = player.Steed.Heading;
					pak.WriteShort(0x1800);
				}
				else
				{
					short rSpeed = player.CurrentSpeed;
					if (player.IsIncapacitated)
						rSpeed = 0;

					ushort content;
					if (rSpeed < 0)
					{
						content = (ushort)((Math.Abs(rSpeed) > 511 ? 511 : Math.Abs(rSpeed)) + 0x200);
					}
					else
					{
						content = (ushort)(rSpeed > 511 ? 511 : rSpeed);
					}

					if (!player.IsAlive)
					{
						content += 5 << 10;
					}
					else
					{
						ushort state = 0;

						if (player.IsSwimming)
							state = 1;

						if (player.IsClimbing)
							state = 7;

						if (player.IsSitting)
							state = 4;

						content += (ushort)(state << 10);
					}

					content += (ushort)(player.IsStrafing ? 1 << 13 : 0 << 13);

					pak.WriteShort(content);
				}

				pak.WriteByte(0);
				pak.WriteByte(0);
				pak.WriteShort((ushort)heading);
				// Write Flags
				byte flagcontent = 0;

				if (player.IsDiving)
				{
					flagcontent += 0x04;
				}

				if (player.IsWireframe)
				{
					flagcontent += 0x01;
				}

				if (player.IsStealthed)
				{
					flagcontent += 0x02;
				}

				if (player.IsTorchLighted)
				{
					flagcontent += 0x80;
				}

				pak.WriteByte(flagcontent);
				pak.WriteByte((byte)(player.RPFlag ? 1 : 0));
				pak.WriteByte(0);
				pak.WriteByte(player.HealthPercent);
				pak.WriteByte(player.ManaPercent);
				pak.WriteByte(player.EndurancePercent);
				SendUDP(pak);
			}

			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(player.CurrentRegionID, (ushort)player.ObjectID)] = GameTimer.GetTickCount();
		}

		public virtual void SendPlayerFreeLevelUpdate()
		{
			GamePlayer player = m_gameClient.Player;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{
				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte(0x09); // subcode

				byte flag = player.FreeLevelState;

				TimeSpan t = new TimeSpan((long)(DateTime.Now.Ticks - player.LastFreeLeveled.Ticks));

				ushort time = 0;
				//time is in minutes
				switch (player.Realm)
				{
					case eRealm.Albion:
						time = (ushort)((ServerProperties.Properties.FREELEVEL_DAYS_ALBION * 24 * 60) - t.TotalMinutes);
						break;
					case eRealm.Midgard:
						time = (ushort)((ServerProperties.Properties.FREELEVEL_DAYS_MIDGARD * 24 * 60) - t.TotalMinutes);
						break;
					case eRealm.Hibernia:
						time = (ushort)((ServerProperties.Properties.FREELEVEL_DAYS_HIBERNIA * 24 * 60) - t.TotalMinutes);
						break;
				}

				//flag 1 = above level, 2 = elligable, 3= time until, 4 = level and time until, 5 = level until
				pak.WriteByte(flag); //flag
				pak.WriteShort(0); //unknown
				pak.WriteShort(time); //time
				SendTCP(pak);
			}
		}
		public virtual void SendPlayerInitFinished(byte mobs)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterInitFinished)))
			{
				pak.WriteByte(mobs);
				SendTCP(pak);
			}
		}
		public virtual void SendPlayerJump(bool headingOnly)
		{
			if (m_gameClient.Player == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterJump)))
			{
				pak.WriteInt((uint)(headingOnly ? 0 : m_gameClient.Player.X));
				pak.WriteInt((uint)(headingOnly ? 0 : m_gameClient.Player.Y));
				pak.WriteShort((ushort)m_gameClient.Player.ObjectID);
				pak.WriteShort((ushort)(headingOnly ? 0 : m_gameClient.Player.Z));
				pak.WriteShort(m_gameClient.Player.Heading);
				if (m_gameClient.Player.InHouse == false || m_gameClient.Player.CurrentHouse == null)
				{
					pak.WriteShort(0);
				}
				else
				{
					pak.WriteShort((ushort)m_gameClient.Player.CurrentHouse.HouseNumber);
				}
				SendTCP(pak);
			}
		}

		public virtual void SendPlayerPositionAndObjectID()
		{
			if (m_gameClient.Player == null) return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PositionAndObjectID)))
			{
				pak.WriteFloatLowEndian(m_gameClient.Player.X);
				pak.WriteFloatLowEndian(m_gameClient.Player.Y);
				pak.WriteFloatLowEndian(m_gameClient.Player.Z);
				pak.WriteShort((ushort)m_gameClient.Player.ObjectID); //This is the player's objectid not Sessionid!!!
				pak.WriteShort(m_gameClient.Player.Heading);

				int flags = 0;
				Zone zone = m_gameClient.Player.CurrentZone;
				if (zone == null) return;

				if (m_gameClient.Player.CurrentZone.IsDivingEnabled)
					flags = 0x80 | (m_gameClient.Player.IsUnderwater ? 0x01 : 0x00);

				if (zone.IsDungeon)
				{
					pak.WriteShort((ushort)(zone.XOffset / 0x2000));
					pak.WriteShort((ushort)(zone.YOffset / 0x2000));
				}
				else
				{
					pak.WriteShort(0);
					pak.WriteShort(0);
				}
				//Dinberg - Changing to allow instances...
				pak.WriteShort(m_gameClient.Player.CurrentRegion.Skin);
				pak.WriteByte((byte)(flags));
				if (m_gameClient.Player.CurrentRegion.IsHousing)
				{
					pak.WritePascalString(GameServer.Instance.Configuration.ServerName); //server name
				}
				else pak.WriteByte(0);
				pak.WriteByte(0); // rest is unknown for now
				pak.WriteByte(0); // flag?
				pak.WriteByte(0); // flag? these seemingly randomly have a value, most common is last 2 bytes are 34 08 
				pak.WriteByte(0); // flag?
				SendTCP(pak);
			}
		}

		public virtual void SendPlayerQuit(bool totalOut)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Quit)))
			{
				pak.WriteByte((byte)(totalOut ? 0x01 : 0x00));
				if (m_gameClient.Player == null)
					pak.WriteByte(0);
				else
					pak.WriteByte(m_gameClient.Player.Level);
				SendTCP(pak);
			}
		}
		public virtual void SendPlayerTitles()
		{
			var titles = m_gameClient.Player.Titles;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DetailWindow)))
			{

				pak.WriteByte(1); // new in 1.75
				pak.WriteByte(0); // new in 1.81
				pak.WritePascalString("Player Statistics"); //window caption

				byte line = 1;
				foreach (string str in m_gameClient.Player.FormatStatistics())
				{
					pak.WriteByte(line++);
					pak.WritePascalString(str);
				}

				pak.WriteByte(200);
				long titlesCountPos = pak.Position;
				pak.WriteByte((byte)titles.Count);
				line = 0;

				foreach (IPlayerTitle title in titles)
				{
					pak.WriteByte(line++);
					pak.WritePascalString(title.GetDescription(m_gameClient.Player));
				}

				long titlesLen = (pak.Position - titlesCountPos - 1); // include titles count
				if (titlesLen > byte.MaxValue)
					log.WarnFormat("Titles block is too long! {0} (player: {1})", titlesLen, m_gameClient.Player);

				//Trailing Zero!
				pak.WriteByte(0);
				SendTCP(pak);
			}
		}
		public virtual void SendPlayerTitleUpdate(GamePlayer player)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{
				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte(0x0B); // subcode
				IPlayerTitle title = player.CurrentTitle;
				if (title == PlayerTitleMgr.ClearTitle)
				{
					pak.WriteByte(0); // flag
					pak.WriteInt(0); // unk1 + str len
				}
				else
				{
					pak.WriteByte(1); // flag
					string val = GameServer.ServerRules.GetPlayerTitle(m_gameClient.Player, player);
					pak.WriteShort((ushort)val.Length);
					pak.WriteShort(0); // unk1
					pak.WriteStringBytes(val);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendQuestUpdate(AbstractQuest quest)
		{
			int questIndex = 1;
			// add check for null due to LD
			if (m_gameClient != null && m_gameClient.Player != null && m_gameClient.Player.QuestList != null)
			{
				lock (m_gameClient.Player.QuestList)
				{
					foreach (AbstractQuest q in m_gameClient.Player.QuestList)
					{
						if (q == quest)
						{
							SendQuestPacket(q, questIndex);
							break;
						}

						if (q.Step != -1)
							questIndex++;
					}
				}
			}
		}
		public virtual void SendQuestListUpdate()
		{
			if (m_gameClient == null || m_gameClient.Player == null)
			{
				return;
			}

			SendTaskInfo();

			int questIndex = 1;
			lock (m_gameClient.Player.QuestList)
			{
				foreach (AbstractQuest quest in m_gameClient.Player.QuestList)
				{
					SendQuestPacket((quest.Step == 0 || quest == null) ? null : quest, questIndex++);
				}
			}
		}

		public virtual void SendQuestOfferWindow(GameNPC questNPC, GamePlayer player, DataQuest quest)
		{
			SendQuestWindow(questNPC, player, quest, true);
		}

		public virtual void SendQuestOfferWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
			SendQuestWindow(questNPC, player, quest, true);
		}
		public virtual void SendQuestRewardWindow(GameNPC questNPC, GamePlayer player, DataQuest quest)
		{
			SendQuestWindow(questNPC, player, quest, false);
		}

		public virtual void SendQuestRewardWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest)
		{
			SendQuestWindow(questNPC, player, quest, false);
		}

		protected virtual void SendQuestWindow(GameNPC questNPC, GamePlayer player, DataQuest quest, bool offer)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				ushort QuestID = quest.ClientQuestID;
				pak.WriteShort((offer) ? (byte)0x22 : (byte)0x21); // Dialog
				pak.WriteShort(QuestID);
				pak.WriteShort((ushort)questNPC.ObjectID);
				pak.WriteByte(0x00); // unknown
				pak.WriteByte(0x00); // unknown
				pak.WriteByte(0x00); // unknown
				pak.WriteByte(0x00); // unknown
				pak.WriteByte((offer) ? (byte)0x02 : (byte)0x01); // Accept/Decline or Finish/Not Yet
				pak.WriteByte(0x01); // Wrap
				pak.WritePascalString(quest.Name);

				String personalizedSummary = BehaviourUtils.GetPersonalizedMessage(quest.Description, player);
				if (personalizedSummary.Length > 255)
				{
					pak.WritePascalString(personalizedSummary.Substring(0, 255)); // Summary is max 255 bytes or client will crash !
				}
				else
				{
					pak.WritePascalString(personalizedSummary);
				}

				if (offer)
				{
					String personalizedStory = BehaviourUtils.GetPersonalizedMessage(quest.Story, player);

					if (personalizedStory.Length > MAX_STORY_LENGTH)
					{
						pak.WriteShort(MAX_STORY_LENGTH);
						pak.WriteStringBytes(personalizedStory.Substring(0, MAX_STORY_LENGTH));
					}
					else
					{
						pak.WriteShort((ushort)personalizedStory.Length);
						pak.WriteStringBytes(personalizedStory);
					}
				}
				else
				{
					if (quest.FinishText.Length > MAX_STORY_LENGTH)
					{
						pak.WriteShort(MAX_STORY_LENGTH);
						pak.WriteStringBytes(quest.FinishText.Substring(0, MAX_STORY_LENGTH));
					}
					else
					{
						pak.WriteShort((ushort)quest.FinishText.Length);
						pak.WriteStringBytes(quest.FinishText);
					}
				}

				pak.WriteShort(QuestID);
				pak.WriteByte((byte)quest.StepTexts.Count); // #goals count
				foreach (string text in quest.StepTexts)
				{
					string t = text;

					// Need to protect for any text length > 255.  It does not crash client but corrupts RewardQuest display -Tolakram
					if (text.Length > 253)
					{
						t = text.Substring(0, 253);
					}

					pak.WritePascalString(String.Format("{0}\r", t));
				}
				pak.WriteInt((uint)(quest.MoneyReward())); // patch 0016
				pak.WriteByte((byte)quest.ExperiencePercent(player)); // patch 0016
				pak.WriteByte((byte)quest.FinalRewards.Count);
				foreach (ItemTemplate reward in quest.FinalRewards)
				{
					WriteItemData(pak, GameInventoryItem.Create(reward));
				}
				pak.WriteByte((byte)quest.NumOptionalRewardsChoice);
				pak.WriteByte((byte)quest.OptionalRewards.Count);
				foreach (ItemTemplate reward in quest.OptionalRewards)
				{
					WriteItemData(pak, GameInventoryItem.Create(reward));
				}
				SendTCP(pak);
			}
		}

		protected virtual void SendQuestWindow(GameNPC questNPC, GamePlayer player, RewardQuest quest, bool offer)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				ushort QuestID = QuestMgr.GetIDForQuestType(quest.GetType());
				pak.WriteShort((offer) ? (byte)0x22 : (byte)0x21); // Dialog
				pak.WriteShort(QuestID);
				pak.WriteShort((ushort)questNPC.ObjectID);
				pak.WriteByte(0x00); // unknown
				pak.WriteByte(0x00); // unknown
				pak.WriteByte(0x00); // unknown
				pak.WriteByte(0x00); // unknown
				pak.WriteByte((offer) ? (byte)0x02 : (byte)0x01); // Accept/Decline or Finish/Not Yet
				pak.WriteByte(0x01); // Wrap
				pak.WritePascalString(quest.Name);

				String personalizedSummary = BehaviourUtils.GetPersonalizedMessage(quest.Summary, player);
				if (personalizedSummary.Length > 255)
					pak.WritePascalString(personalizedSummary.Substring(0, 255)); // Summary is max 255 bytes !
				else
					pak.WritePascalString(personalizedSummary);

				if (offer)
				{
					String personalizedStory = BehaviourUtils.GetPersonalizedMessage(quest.Story, player);

					if (personalizedStory.Length > ServerProperties.Properties.MAX_REWARDQUEST_DESCRIPTION_LENGTH)
					{
						pak.WriteShort((ushort)ServerProperties.Properties.MAX_REWARDQUEST_DESCRIPTION_LENGTH);
						pak.WriteStringBytes(personalizedStory.Substring(0, ServerProperties.Properties.MAX_REWARDQUEST_DESCRIPTION_LENGTH));
					}
					else
					{
						pak.WriteShort((ushort)personalizedStory.Length);
						pak.WriteStringBytes(personalizedStory);
					}
				}
				else
				{
					if (quest.Conclusion.Length > (ushort)ServerProperties.Properties.MAX_REWARDQUEST_DESCRIPTION_LENGTH)
					{
						pak.WriteShort((ushort)ServerProperties.Properties.MAX_REWARDQUEST_DESCRIPTION_LENGTH);
						pak.WriteStringBytes(quest.Conclusion.Substring(0, (ushort)ServerProperties.Properties.MAX_REWARDQUEST_DESCRIPTION_LENGTH));
					}
					else
					{
						pak.WriteShort((ushort)quest.Conclusion.Length);
						pak.WriteStringBytes(quest.Conclusion);
					}
				}

				pak.WriteShort(QuestID);
				pak.WriteByte((byte)quest.Goals.Count); // #goals count
				foreach (RewardQuest.QuestGoal goal in quest.Goals)
				{
					pak.WritePascalString(String.Format("{0}\r", goal.Description));
				}
				pak.WriteInt((uint)(quest.Rewards.Money)); // unknown, new in 1.94
				pak.WriteByte((byte)quest.Rewards.ExperiencePercent(player));
				pak.WriteByte((byte)quest.Rewards.BasicItems.Count);
				foreach (ItemTemplate reward in quest.Rewards.BasicItems)
				{
					WriteItemData(pak, GameInventoryItem.Create(reward));
				}
				pak.WriteByte((byte)quest.Rewards.ChoiceOf);
				pak.WriteByte((byte)quest.Rewards.OptionalItems.Count);
				foreach (ItemTemplate reward in quest.Rewards.OptionalItems)
				{
					WriteItemData(pak, GameInventoryItem.Create(reward));
				}
				SendTCP(pak);
			}
		}
		protected virtual void SendQuestPacket(AbstractQuest q, int index)
		{
			if (q == null)
			{
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
				{
					pak.WriteByte((byte)index);
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte(0);
					SendTCP(pak);
					return;
				}
			}
			else if (q is RewardQuest)
			{
				RewardQuest quest = q as RewardQuest;
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
				{
					pak.WriteByte((byte)index);
					pak.WriteByte((byte)quest.Name.Length);
					pak.WriteShort(0x00); // unknown
					pak.WriteByte((byte)quest.Goals.Count);
					pak.WriteByte((byte)quest.Level);
					pak.WriteStringBytes(quest.Name);
					pak.WritePascalString(quest.Description);
					int goalindex = 0;
					foreach (RewardQuest.QuestGoal goal in quest.Goals)
					{
						goalindex++;
						String goalDesc = String.Format("{0}\r", goal.Description);
						pak.WriteShortLowEndian((ushort)goalDesc.Length);
						pak.WriteStringBytes(goalDesc);
						pak.WriteShortLowEndian((ushort)goal.ZoneID2);
						pak.WriteShortLowEndian((ushort)goal.XOffset2);
						pak.WriteShortLowEndian((ushort)goal.YOffset2);
						pak.WriteShortLowEndian(0x00);  // unknown
						pak.WriteShortLowEndian((ushort)goal.Type);
						pak.WriteShortLowEndian(0x00);  // unknown
						pak.WriteShortLowEndian((ushort)goal.ZoneID1);
						pak.WriteShortLowEndian((ushort)goal.XOffset1);
						pak.WriteShortLowEndian((ushort)goal.YOffset1);
						pak.WriteByte((byte)((goal.IsAchieved) ? 0x01 : 0x00));
						if (goal.QuestItem == null)
						{
							pak.WriteByte(0x00);
						}
						else
						{
							pak.WriteByte((byte)goalindex);
							WriteTemplateData(pak, goal.QuestItem, 1);
						}
					}
					SendTCP(pak);
					return;
				}
			}
			else
			{
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
				{
					pak.WriteByte((byte)index);

					string name = string.Format("{0} (Level {1})", q.Name, q.Level);
					string desc = string.Format("[Step #{0}]: {1}", q.Step, q.Description);
					if (name.Length > byte.MaxValue)
					{
						if (log.IsWarnEnabled)
						{
							log.Warn(q.GetType().ToString() + ": name is too long for 1.68+ clients (" + name.Length + ") '" + name + "'");
						}
						name = name.Substring(0, byte.MaxValue);
					}
					if (desc.Length > byte.MaxValue)
					{
						if (log.IsWarnEnabled)
						{
							log.Warn(q.GetType().ToString() + ": description is too long for 1.68+ clients (" + desc.Length + ") '" + desc + "'");
						}
						desc = desc.Substring(0, byte.MaxValue);
					}
					pak.WriteByte((byte)name.Length);
					pak.WriteShortLowEndian((ushort)desc.Length);
					pak.WriteByte(0); // Quest Zone ID ?
					pak.WriteByte(0);
					pak.WriteStringBytes(name); //Write Quest Name without trailing 0
					pak.WriteStringBytes(desc); //Write Quest Description without trailing 0                   

					SendTCP(pak);
				}
			}
		}

		public virtual void SendRealm(eRealm realm)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Realm)))
			{
				pak.WriteByte((byte)realm);
				SendTCP(pak);
			}
		}
		public virtual void SendRegions()
		{
			if (m_gameClient.Player != null)
			{
				if (!m_gameClient.Socket.Connected)
					return;
				Region region = WorldMgr.GetRegion(m_gameClient.Player.CurrentRegionID);
				if (region == null)
					return;
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ClientRegion)))
				{
					//				pak.WriteByte((byte)((region.Expansion + 1) << 4)); // Must be expansion
					pak.WriteByte(0); // but this packet sended when client in old region. but this field must show expanstion for jump destanation region
									  //Dinberg - trying to get instances to work.
					pak.WriteByte((byte)region.Skin); // This was pak.WriteByte((byte)region.ID);
					pak.Fill(0, 20);
					pak.FillString(region.ServerPort.ToString(), 5);
					pak.FillString(region.ServerPort.ToString(), 5);
					string ip = region.ServerIP;
					if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.") || ip.StartsWith("192.168."))
						ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
					pak.FillString(ip, 20);
					SendTCP(pak);
				}
			}
			else
			{
				RegionEntry[] entries = WorldMgr.GetRegionList();

				if (entries == null) return;
				int index = 0;
				int num = 0;
				int count = entries.Length;
				while (entries != null && count > index)
				{
					using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ClientRegions)))
					{
						for (int i = 0; i < 4; i++)
						{
							while (index < count && (int)m_gameClient.ClientType <= entries[index].expansion)
							{
								index++;
							}

							if (index >= count)
							{   //If we have no more entries
								pak.Fill(0x0, 52);
							}
							else
							{
								pak.WriteByte((byte)(++num));
								pak.WriteByte((byte)entries[index].id);
								pak.FillString(entries[index].name, 20);
								pak.FillString(entries[index].fromPort, 5);
								pak.FillString(entries[index].toPort, 5);
								//Try to fix the region ip so UDP is enabled!
								string ip = entries[index].ip;
								if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.13.") || ip.StartsWith("192.168."))
									ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
								pak.FillString(ip, 20);

								index++;
							}
						}
						SendTCP(pak);
					}
				}
			}
		}
		public virtual void SendRegionChanged()
		{
			if (m_gameClient.Player == null)
				return;
			SendRegions();
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.RegionChanged)))
			{
				//Dinberg - Changing to allow instances...
				pak.WriteShort(m_gameClient.Player.CurrentRegion.Skin);
				//Dinberg:Instances - also need to continue the bluff here, with zoneSkinID, for 
				//clientside positions of objects.
				pak.WriteShort(m_gameClient.Player.CurrentZone.ZoneSkinID); // Zone ID?
				pak.WriteShort(0x00); // ?
				pak.WriteShort(0x01); // cause region change ?
				pak.WriteByte(0x0C); //Server ID
				pak.WriteByte(0); // ?
				pak.WriteShort(0xFFBF); // ?
				SendTCP(pak);
			}
		}
		public virtual void SendRegionColorScheme(byte color)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{
				pak.WriteShort(0); // not used
				pak.WriteByte(0x05); // subcode
				pak.WriteByte(color);
				pak.WriteInt(0); // not used
				SendTCP(pak);
			}
		}
		public virtual void SendRvRGuildBanner(GamePlayer player, bool show)
		{
			if (player == null) return;

			//cannot show banners for players that have no guild.
			if (show && player.Guild == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)eServerPackets.VisualEffect);
			pak.WriteShort((ushort)player.ObjectID);
			pak.WriteByte(0xC); // show Banner
			pak.WriteByte((byte)((show) ? 0 : 1)); // 0-enable, 1-disable
			int newEmblemBitMask = ((player.Guild.Emblem & 0x010000) << 8) | (player.Guild.Emblem & 0xFFFF);
			pak.WriteInt((uint)newEmblemBitMask);
			SendTCP(pak);
		}
		public virtual void SendSessionID()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SessionID)))
			{
				pak.WriteShortLowEndian((ushort)m_gameClient.SessionID);
				SendTCP(pak);
			}
		}
		public virtual void SendSetControlledHorse(GamePlayer player)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ControlledHorse)))
			{

				if (player.HasHorse)
				{
					pak.WriteShort(0); // for set self horse OID must be zero
					pak.WriteByte(player.ActiveHorse.ID);
					if (player.ActiveHorse.BardingColor == 0 && player.ActiveHorse.Barding != 0 && player.Guild != null)
					{
						int newGuildBitMask = (player.Guild.Emblem & 0x010000) >> 9;
						pak.WriteByte((byte)(player.ActiveHorse.Barding | newGuildBitMask));
						pak.WriteShort((ushort)player.Guild.Emblem);
					}
					else
					{
						pak.WriteByte(player.ActiveHorse.Barding);
						pak.WriteShort(player.ActiveHorse.BardingColor);
					}
					pak.WriteByte(player.ActiveHorse.Saddle);
					pak.WriteByte(player.ActiveHorse.SaddleColor);
					pak.WriteByte(player.ActiveHorse.Slots);
					pak.WriteByte(player.ActiveHorse.Armor);
					pak.WritePascalString(player.ActiveHorse.Name == null ? "" : player.ActiveHorse.Name);
				}
				else
				{
					pak.Fill(0x00, 8);
				}
				SendTCP(pak);
			}
		}
		public virtual void SendSiegeWeaponCloseInterface()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponInterface)))
			{
				pak.WriteShort(0);
				pak.WriteShort(1);
				pak.Fill(0, 13);
				SendTCP(pak);
			}
		}

		/// <summary>
		/// new siege weapon animation 1.110 // patch 0021
		/// </summary>
		/// <param name="siegeWeapon"></param>
		public virtual void SendSiegeWeaponAnimation(GameSiegeWeapon siegeWeapon)
		{
			if (siegeWeapon == null)
				return;
			byte[] siegeID = new byte[siegeWeapon.ObjectID]; // test
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponAnimation)))
			{
				pak.WriteInt((uint)siegeWeapon.ObjectID);
				pak.WriteInt(
					(uint)
					(siegeWeapon.TargetObject == null
					 ? (siegeWeapon.GroundTarget == null ? 0 : siegeWeapon.GroundTarget.X)
					 : siegeWeapon.TargetObject.X));
				pak.WriteInt(
					(uint)
					(siegeWeapon.TargetObject == null
					 ? (siegeWeapon.GroundTarget == null ? 0 : siegeWeapon.GroundTarget.Y)
					 : siegeWeapon.TargetObject.Y));
				pak.WriteInt(
					(uint)
					(siegeWeapon.TargetObject == null
					 ? (siegeWeapon.GroundTarget == null ? 0 : siegeWeapon.GroundTarget.Z)
					 : siegeWeapon.TargetObject.Z));
				pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
				pak.WriteShort(siegeWeapon.Effect);
				pak.WriteShort((ushort)(siegeWeapon.SiegeWeaponTimer.TimeUntilElapsed));
				pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
				switch ((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction)
				{
					case 0x01: //aiming
						{
							pak.WriteByte(siegeID[1]); // lowest value of siegeweapon.ObjectID
							pak.WriteShort((ushort)(siegeWeapon.TargetObject == null ? 0x0000 : siegeWeapon.TargetObject.ObjectID));
							break;
						}
					case 0x02: //arming
						{
							pak.WriteByte(0x5F);
							pak.WriteShort(0xD000);
							break;
						}
					case 0x03: // loading
						{
							pak.Fill(0, 3);
							break;
						}
				}
				//pak.WriteShort(0x5FD0);
				//pak.WriteByte(0x00);
				SendTCP(pak);

			}
		}

		/// <summary>
		/// new siege weapon fireanimation 1.110 // patch 0021
		/// </summary>
		/// <param name="siegeWeapon">The siege weapon</param>
		/// <param name="timer">How long the animation lasts for</param>
		public virtual void SendSiegeWeaponFireAnimation(GameSiegeWeapon siegeWeapon, int timer)
		{
			if (siegeWeapon == null)
				return;
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponAnimation)))
			{
				pak.WriteInt((uint)siegeWeapon.ObjectID);
				pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? siegeWeapon.GroundTarget.X : siegeWeapon.TargetObject.X));
				pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? siegeWeapon.GroundTarget.Y : siegeWeapon.TargetObject.Y));
				pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? siegeWeapon.GroundTarget.Z + 50 : siegeWeapon.TargetObject.Z + 50));
				pak.WriteInt((uint)(siegeWeapon.TargetObject == null ? 0 : siegeWeapon.TargetObject.ObjectID));
				pak.WriteShort(siegeWeapon.Effect);
				pak.WriteShort((ushort)(timer));
				pak.WriteByte((byte)SiegeTimer.eAction.Fire);
				pak.WriteShort(0xE134); // default ammo type, the only type currently supported on DOL
				pak.WriteByte(0x08); // always this flag when firing
				SendTCP(pak);
			}
		}
		/// <summary>
		/// new siege interface packet 1119
		/// </summary>
		/// <param name="siegeWeapon"></param>
		/// <param name="time"></param>
		public virtual void SendSiegeWeaponInterface(GameSiegeWeapon siegeWeapon, int time)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SiegeWeaponInterface)))
			{
				ushort flag = (ushort)((siegeWeapon.EnableToMove ? 1 : 0) | siegeWeapon.AmmoType << 8);
				pak.WriteShort(flag); //byte Ammo,  byte SiegeMoving(1/0)
				pak.WriteByte(0);
				pak.WriteByte(0); // Close interface(1/0)
				pak.WriteByte((byte)(time));//time x 100 eg 50 = 5000ms
				pak.WriteByte((byte)siegeWeapon.Ammo.Count); // external ammo count
				pak.WriteByte((byte)siegeWeapon.SiegeWeaponTimer.CurrentAction);
				pak.WriteByte((byte)siegeWeapon.AmmoSlot);
				pak.WriteShort(siegeWeapon.Effect);
				pak.WriteShort(0); // SiegeHelperTimer ?
				pak.WriteShort(0); // SiegeTimer ?
				pak.WriteShort((ushort)siegeWeapon.ObjectID);

				string name = siegeWeapon.Name;

				LanguageDataObject translation = LanguageMgr.GetTranslation(m_gameClient, siegeWeapon);
				if (translation != null)
				{
					if (!Util.IsEmpty(((DBLanguageNPC)translation).Name))
						name = ((DBLanguageNPC)translation).Name;
				}

				//pak.WritePascalString(name + " (" + siegeWeapon.CurrentState.ToString() + ")");
				foreach (InventoryItem item in siegeWeapon.Ammo)
				{
					if (item == null)
					{
						pak.Fill(0x00, 24);
						continue;
					}
					pak.WriteByte((byte)siegeWeapon.Ammo.IndexOf(item));
					pak.WriteShort(0); // unique objectID , can probably be 0
					pak.WriteByte((byte)item.Level);
					pak.WriteByte(0); // value1
					pak.WriteByte(0); //value2
					pak.WriteByte(0); // unknown
					pak.WriteByte((byte)item.Object_Type);
					pak.WriteByte(1); // unknown
					pak.WriteByte(0);//
					pak.WriteByte((byte)item.Count);
					//pak.WriteByte((byte)(item.Hand * 64));
					//pak.WriteByte((byte)((item.Type_Damage * 64) + item.Object_Type));
					//pak.WriteShort((ushort)item.Weight);
					pak.WriteByte(item.ConditionPercent); // % of con
					pak.WriteByte(item.DurabilityPercent); // % of dur
					pak.WriteByte((byte)item.Quality); // % of qua
					pak.WriteByte((byte)item.Bonus); // % bonus
					pak.WriteByte((byte)item.BonusLevel); // guessing
					pak.WriteShort((ushort)item.Model);
					pak.WriteByte((byte)item.Extension);
					pak.WriteShort(0); // unknown
					pak.WriteByte(4); // unknown flags?
					pak.WriteShort(0); // unknown
					if (item.Count > 1)
						pak.WritePascalString(item.Count + " " + item.Name);
					else
						pak.WritePascalString(item.Name);
				}
				pak.WritePascalString(name + " (" + siegeWeapon.CurrentState.ToString() + ")");
				SendTCP(pak);
			}
		}

		public virtual void SendSpellEffectAnimation(GameObject spellCaster, GameObject spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SpellEffectAnimation)))
			{
				pak.WriteShort((ushort)spellCaster.ObjectID);
				pak.WriteShort(spellid);
				pak.WriteShort((ushort)(spellTarget == null ? 0 : spellTarget.ObjectID));
				pak.WriteShort(boltTime);
				pak.WriteByte((byte)(noSound ? 1 : 0));
				pak.WriteByte(success);
				SendTCP(pak);
			}
		}
		public virtual void SendStatusUpdate(byte sittingFlag)
		{
			if (m_gameClient.Player == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterStatusUpdate)))
			{
				pak.WriteByte(m_gameClient.Player.HealthPercent);
				pak.WriteByte(m_gameClient.Player.ManaPercent);
				pak.WriteByte(sittingFlag);
				pak.WriteByte(m_gameClient.Player.EndurancePercent);
				pak.WriteByte(m_gameClient.Player.ConcentrationPercent);
				//			pak.WriteShort((byte) (_gameClient.Player.IsAlive ? 0x00 : 0x0f)); // 0x0F if dead ??? where it now ?
				pak.WriteByte(0);// unk
				pak.WriteShort((ushort)m_gameClient.Player.MaxMana);
				pak.WriteShort((ushort)m_gameClient.Player.MaxEndurance);
				pak.WriteShort((ushort)m_gameClient.Player.MaxConcentration);
				pak.WriteShort((ushort)m_gameClient.Player.MaxHealth);
				pak.WriteShort((ushort)m_gameClient.Player.Health);
				pak.WriteShort((ushort)m_gameClient.Player.Endurance);
				pak.WriteShort((ushort)m_gameClient.Player.Mana);
				pak.WriteShort((ushort)m_gameClient.Player.Concentration);
				SendTCP(pak);
			}
		}
		protected virtual void SendTaskInfo()
		{
			string name = BuildTaskString();

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.QuestEntry)))
			{
				pak.WriteByte(0); //index
				pak.WriteShortLowEndian((ushort)name.Length);
				pak.WriteByte((byte)0);
				pak.WriteByte((byte)0);
				pak.WriteByte((byte)0);
				pak.WriteStringBytes(name); //Write Quest Name without trailing 0
				pak.WriteStringBytes(""); //Write Quest Description without trailing 0
				SendTCP(pak);
			}
		}
		public virtual void SendTime()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Time)))
			{
				if (m_gameClient != null && m_gameClient.Player != null)
				{
					pak.WriteInt(WorldMgr.GetCurrentGameTime(m_gameClient.Player));
					pak.WriteInt(WorldMgr.GetDayIncrement(m_gameClient.Player));
				}
				else
				{
					pak.WriteInt(WorldMgr.GetCurrentGameTime());
					pak.WriteInt(WorldMgr.GetDayIncrement());
				}
				SendTCP(pak);
			}
		}
		/// <summary>
		/// send trade window packet
		/// </summary>
		public virtual void SendTradeWindow()
		{
			if (m_gameClient.Player == null)
				return;
			if (m_gameClient.Player.TradeWindow == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TradeWindow)))
			{
				lock (m_gameClient.Player.TradeWindow.Sync)
				{
					foreach (InventoryItem item in m_gameClient.Player.TradeWindow.TradeItems)
					{
						pak.WriteByte((byte)item.SlotPosition);
					}
					pak.Fill(0x00, 10 - m_gameClient.Player.TradeWindow.TradeItems.Count);

					pak.WriteShort(0x0000);
					pak.WriteShort((ushort)Money.GetMithril(m_gameClient.Player.TradeWindow.TradeMoney));
					pak.WriteShort((ushort)Money.GetPlatinum(m_gameClient.Player.TradeWindow.TradeMoney));
					pak.WriteShort((ushort)Money.GetGold(m_gameClient.Player.TradeWindow.TradeMoney));
					pak.WriteShort((ushort)Money.GetSilver(m_gameClient.Player.TradeWindow.TradeMoney));
					pak.WriteShort((ushort)Money.GetCopper(m_gameClient.Player.TradeWindow.TradeMoney));

					pak.WriteShort(0x0000);
					pak.WriteShort((ushort)Money.GetMithril(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
					pak.WriteShort((ushort)Money.GetPlatinum(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
					pak.WriteShort((ushort)Money.GetGold(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
					pak.WriteShort((ushort)Money.GetSilver(m_gameClient.Player.TradeWindow.PartnerTradeMoney));
					pak.WriteShort((ushort)Money.GetCopper(m_gameClient.Player.TradeWindow.PartnerTradeMoney));

					pak.WriteShort(0x0000);
					ArrayList items = m_gameClient.Player.TradeWindow.PartnerTradeItems;
					if (items != null)
					{
						pak.WriteByte((byte)items.Count);
						pak.WriteByte(0x01);
					}
					else
					{
						pak.WriteShort(0x0000);
					}
					pak.WriteByte((byte)(m_gameClient.Player.TradeWindow.Repairing ? 0x01 : 0x00));
					pak.WriteByte((byte)(m_gameClient.Player.TradeWindow.Combine ? 0x01 : 0x00));
					if (items != null)
					{
						foreach (InventoryItem item in items)
						{
							pak.WriteByte((byte)item.SlotPosition);
							WriteItemData(pak, item);
						}
					}
					if (m_gameClient.Player.TradeWindow.Partner != null)
						pak.WritePascalString("Trading with " + m_gameClient.Player.GetName(m_gameClient.Player.TradeWindow.Partner)); // transaction with ...
					else
						pak.WritePascalString("Selfcrafting"); // transaction with ...
					SendTCP(pak);
				}
			}
		}

		/// <summary>
		/// SendTrainerWindow method
		/// </summary>
		public virtual void SendTrainerWindow()
		{
			if (m_gameClient == null || m_gameClient.Player == null)
				return;

			GamePlayer player = m_gameClient.Player;

			List<Specialization> specs = m_gameClient.Player.GetSpecList().Where(it => it.Trainable).ToList();
			IList<string> autotrains = player.CharacterClass.GetAutotrainableSkills();

			// Send Trainer Window with Trainable Specs
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)specs.Count);
				pak.WriteByte((byte)player.SkillSpecialtyPoints);
				pak.WriteByte(0); // Spec code
				pak.WriteByte(0);

				int i = 0;
				foreach (Specialization spec in specs)
				{
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)Math.Min(player.MaxLevel, spec.Level));
					pak.WriteByte((byte)(Math.Min(player.MaxLevel, spec.Level) + 1));
					pak.WritePascalString(spec.Name);
				}
				SendTCP(pak);
			}

			// send RA usable by this class
			var raList = SkillBase.GetClassRealmAbilities(m_gameClient.Player.CharacterClass.ID).Where(ra => !(ra is RR5RealmAbility));
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)raList.Count());
				pak.WriteByte((byte)player.RealmSpecialtyPoints);
				pak.WriteByte(1); // RA Code
				pak.WriteByte(0);

				int i = 0;
				foreach (RealmAbility ra in raList)
				{
					int level = player.GetAbilityLevel(ra.KeyName);
					pak.WriteByte((byte)i++);
					pak.WriteByte((byte)level);
					pak.WriteByte((byte)ra.CostForUpgrade(level));
					bool canBeUsed = ra.CheckRequirement(player);
					pak.WritePascalString(canBeUsed ? ra.Name : string.Format("[{0}]", ra.Name));
				}
				SendTCP(pak);
			}

			// Send Name Index for each spec.
			// Get ALL skills for player, ordered by spec key.
			List<Tuple<Specialization, List<Tuple<int, int, Skill>>>> skillDictCache = null;

			// get from cache
			if (m_gameClient.TrainerSkillCache == null)
			{
				skillDictCache = new List<Tuple<Specialization, List<Tuple<int, int, Skill>>>>();

				foreach (Specialization spec in specs)
				{
					var toAdd = new List<Tuple<int, int, Skill>>();

					foreach (Ability ab in spec.PretendAbilitiesForLiving(player, player.MaxLevel))
					{
						toAdd.Add(new Tuple<int, int, Skill>(5, ab.InternalID, ab));
					}

					foreach (KeyValuePair<SpellLine, List<Skill>> ls in spec.PretendLinesSpellsForLiving(player, player.MaxLevel).Where(k => !k.Key.IsBaseLine))
					{
						foreach (Skill sk in ls.Value)
						{
							toAdd.Add(new Tuple<int, int, Skill>((int)sk.SkillType, sk.InternalID, sk));
						}
					}

					foreach (Style st in spec.PretendStylesForLiving(player, player.MaxLevel))
					{
						toAdd.Add(new Tuple<int, int, Skill>((int)st.SkillType, st.InternalID, st));
					}

					skillDictCache.Add(new Tuple<Specialization, List<Tuple<int, int, Skill>>>(spec, toAdd.OrderBy(e => (e.Item3 is Ability) ? ((Ability)e.Item3).SpecLevelRequirement : (((e.Item3 is Style) ? ((Style)e.Item3).SpecLevelRequirement : e.Item3.Level))).ToList()));
				}



				// save to cache
				m_gameClient.TrainerSkillCache = skillDictCache;
			}

			skillDictCache = m_gameClient.TrainerSkillCache;

			// Send Names first
			int index = 0;
			for (int skindex = 0; skindex < skillDictCache.Count; skindex++)
			{
				using (GSTCPPacketOut pakindex = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
				{
					pakindex.WriteByte((byte)skillDictCache[skindex].Item2.Count); //size
					pakindex.WriteByte((byte)player.SkillSpecialtyPoints);
					pakindex.WriteByte(4); // name index code
					pakindex.WriteByte(0);
					pakindex.WriteByte((byte)index); // start index

					foreach (Skill sk in skillDictCache[skindex].Item2.Select(e => e.Item3))
					{
						// send name
						pakindex.WritePascalString(sk.Name);
						index++;
					}

					SendTCP(pakindex);
				}
			}

			// Send Skill Secondly
			using (GSTCPPacketOut pakskill = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{

				pakskill.WriteByte((byte)skillDictCache.Count); //size we send for all specs
				pakskill.WriteByte((byte)player.SkillSpecialtyPoints);
				pakskill.WriteByte(3); // Skill description code
				pakskill.WriteByte(0);
				pakskill.WriteByte((byte)0); // unk ?

				// Fill out an array that tells the client how many spec points are available at each of
				// this characters levels.  This seems to only be used for the 'Minimum Level' display on
				// the new trainer window.  I've changed the calls below to use AdjustedSpecPointsMultiplier
				// to enable servers that allow levels > 50 to train properly by modifying points available per level. - Tolakram

				// There is a bug here that is calculating too few spec points and causing level 50 players to 
				// be unable to train RA.  Setting this to max for now to disable 'Minimum Level' feature on train window.
				// I think bug is that auto train points must be added to this calculation.
				// -Tolakram

				for (byte i = 2; i <= 50; i++)
				{
					//int specpoints = 0;

					//if (i <= 5)
					//    specpoints = i;

					//if (i > 5)
					//    specpoints = i * _gameClient.Player.CharacterClass.AdjustedSpecPointsMultiplier / 10;

					//if (i > 40 && i != 50)
					//    specpoints += i * _gameClient.Player.CharacterClass.AdjustedSpecPointsMultiplier / 20;

					//paksub.WriteByte((byte)specpoints);
					pakskill.WriteByte((byte)255);
				}

				for (int skindex = 0; skindex < skillDictCache.Count; skindex++)
				{

					byte autotrain = 0;
					if (autotrains.Contains(specs[skindex].KeyName))
					{
						autotrain = (byte)Math.Floor((double)m_gameClient.Player.BaseLevel / 4);
					}

					if (pakskill.Length >= 2045)
						break;

					// Skill Index Header
					pakskill.WriteByte((byte)skindex); // skill index
					pakskill.WriteByte((byte)skillDictCache[skindex].Item2.Count); // Count
					pakskill.WriteByte(autotrain); // autotrain byte

					foreach (Tuple<int, int, Skill> sk in skillDictCache[skindex].Item2)
					{
						if (pakskill.Length >= 2040)
							break;

						if (sk.Item3 is Ability)
						{
							Ability ab = (Ability)sk.Item3;
							// skill description
							pakskill.WriteByte((byte)ab.SpecLevelRequirement); // level
																			   // tooltip
							pakskill.WriteShort((ushort)ab.Icon); // icon
							pakskill.WriteByte((byte)sk.Item1); // skill page
							pakskill.WriteByte((byte)0); // 
							pakskill.WriteByte((byte)0xFD); // line
							pakskill.WriteShort((ushort)sk.Item2); // ID
						}
						else if (sk.Item3 is Spell)
						{
							Spell sp = (Spell)sk.Item3;
							// skill description
							pakskill.WriteByte((byte)sp.Level); // level
																// tooltip
							pakskill.WriteShort(sp.InternalIconID > 0 ? sp.InternalIconID : sp.Icon); // icon
							pakskill.WriteByte((byte)sk.Item1); // skill page
							pakskill.WriteByte((byte)0); // 
							pakskill.WriteByte((byte)(sp.SkillType == eSkillPage.Songs ? 0xFF : 0xFE)); // line
							pakskill.WriteShort((ushort)sk.Item2); // ID
						}
						else if (sk.Item3 is Style)
						{
							Style st = (Style)sk.Item3;
							pakskill.WriteByte((byte)Math.Min(player.MaxLevel, st.SpecLevelRequirement));
							// tooltip
							pakskill.WriteShort((ushort)st.Icon);
							pakskill.WriteByte((byte)sk.Item1);
							pakskill.WriteByte((byte)st.OpeningRequirementType);
							pakskill.WriteByte((byte)st.OpeningRequirementValue);
							pakskill.WriteShort((ushort)sk.Item2);
						}
						else
						{
							// ??
							pakskill.WriteByte((byte)sk.Item3.Level);
							// tooltip
							pakskill.WriteShort((ushort)sk.Item3.Icon);
							pakskill.WriteByte((byte)sk.Item1);
							pakskill.WriteByte((byte)0);
							pakskill.WriteByte((byte)0);
							pakskill.WriteShort((ushort)sk.Item2);
						}
					}
				}

				SendTCP(pakskill);
			}

			// type 5 (realm abilities)
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				pak.WriteByte((byte)raList.Count());
				pak.WriteByte((byte)player.RealmSpecialtyPoints);
				pak.WriteByte(5);
				pak.WriteByte(0);

				foreach (RealmAbility ra in raList)
				{
					pak.WriteByte((byte)player.GetAbilityLevel(ra.KeyName));

					pak.WriteByte(0);
					pak.WriteByte((byte)ra.MaxLevel);

					for (int i = 0; i < ra.MaxLevel; i++)
						pak.WriteByte((byte)ra.CostForUpgrade(i));

					if (ra.CheckRequirement(m_gameClient.Player))
						pak.WritePascalString(ra.KeyName);
					else
						pak.WritePascalString(string.Format("[{0}]", ra.Name));
				}
				SendTCP(pak);
			}
			// Send tooltips
			if (ForceTooltipUpdate && m_gameClient.TrainerSkillCache != null)
				SendForceTooltipUpdate(m_gameClient.TrainerSkillCache.SelectMany(e => e.Item2).Select(e => e.Item3));
		}

		/// <summary>
		/// Send Delve for Provided Collection of Skills that need forced Tooltip Update.
		/// </summary>
		/// <param name="skills"></param>
		protected virtual void SendForceTooltipUpdate(IEnumerable<Skill> skills)
		{
			foreach (Skill t in skills)
			{
				if (t is Specialization)
					continue;

				if (t is RealmAbility)
				{
					if (m_gameClient.CanSendTooltip(27, t.InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveRealmAbility(m_gameClient, t.InternalID));
				}
				else if (t is Ability)
				{
					if (m_gameClient.CanSendTooltip(28, t.InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveAbility(m_gameClient, t.InternalID));
				}
				else if (t is Style)
				{
					if (m_gameClient.CanSendTooltip(25, t.InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveStyle(m_gameClient, t.InternalID));
				}
				else if (t is Spell)
				{
					if (t is Song || (t is Spell && ((Spell)t).NeedInstrument))
					{
						if (m_gameClient.CanSendTooltip(26, ((Spell)t).InternalID))
							SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveSong(m_gameClient, ((Spell)t).InternalID));
					}

					if (m_gameClient.CanSendTooltip(24, ((Spell)t).InternalID))
						SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveSpell(m_gameClient, ((Spell)t).InternalID));
				}
			}
		}

		public virtual void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount)
		{
			if (m_gameClient.Player == null)
			{
				return;
			}

			IList<int> tooltipids = new List<int>();

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.UpdateIcons)))
			{
				long initPos = pak.Position;

				int fxcount = 0;
				int entriesCount = 0;

				pak.WriteByte(0); // effects count set in the end
				pak.WriteByte(0); // unknown
				pak.WriteByte(Icons); // unknown
				pak.WriteByte(0); // unknown

				foreach (IGameEffect effect in m_gameClient.Player.EffectList)
				{
					if (effect.Icon != 0)
					{
						fxcount++;
						if (changedEffects != null && !changedEffects.Contains(effect))
						{
							continue;
						}

						// store tooltip update for gamespelleffect.
						if (ForceTooltipUpdate && (effect is GameSpellEffect))
						{
							Spell spell = ((GameSpellEffect)effect).Spell;
							tooltipids.Add(spell.InternalID);
						}

						//						log.DebugFormat("adding [{0}] '{1}'", fxcount-1, effect.Name);
						pak.WriteByte((byte)(fxcount - 1)); // icon index
						pak.WriteByte((effect is GameSpellEffect || effect.Icon > 5000) ? (byte)(fxcount - 1) : (byte)0xff);

						byte ImmunByte = 0;
						var gsp = effect as GameSpellEffect;
						if (gsp != null && gsp.IsDisabled)
							ImmunByte = 1;
						pak.WriteByte(ImmunByte); // new in 1.73; if non zero says "protected by" on right click

						// bit 0x08 adds "more..." to right click info
						pak.WriteShort(effect.Icon);
						//pak.WriteShort(effect.IsFading ? (ushort)1 : (ushort)(effect.RemainingTime / 1000));
						pak.WriteShort((ushort)(effect.RemainingTime / 1000));
						if (effect is GameSpellEffect)
							pak.WriteShort((ushort)((GameSpellEffect)effect).Spell.InternalID); //v1.110+ send the spell ID for delve info in active icon
						else
							pak.WriteShort(0);//don't override existing tooltip ids

						byte flagNegativeEffect = 0;
						if (effect is StaticEffect)
						{
							if (((StaticEffect)effect).HasNegativeEffect)
							{
								flagNegativeEffect = 1;
							}
						}
						else if (effect is GameSpellEffect)
						{
							if (!((GameSpellEffect)effect).SpellHandler.HasPositiveEffect)
							{
								flagNegativeEffect = 1;
							}
						}
						pak.WriteByte(flagNegativeEffect);

						pak.WritePascalString(effect.Name);
						entriesCount++;
					}
				}

				int oldCount = lastUpdateEffectsCount;
				lastUpdateEffectsCount = fxcount;

				while (oldCount > fxcount)
				{
					pak.WriteByte((byte)(fxcount++));
					pak.Fill(0, 10);
					entriesCount++;
					//					log.DebugFormat("adding [{0}] (empty)", fxcount-1);
				}

				if (changedEffects != null)
				{
					changedEffects.Clear();
				}

				if (entriesCount == 0)
				{
					return; // nothing changed - no update is needed
				}

				pak.Position = initPos;
				pak.WriteByte((byte)entriesCount);
				pak.Seek(0, SeekOrigin.End);

				SendTCP(pak);
			}

			// force tooltips update
			foreach (int entry in tooltipids)
			{
				if (m_gameClient.CanSendTooltip(24, entry))
					SendDelveInfo(DOL.GS.PacketHandler.Client.v168.DetailDisplayHandler.DelveSpell(m_gameClient, entry));
			}
		}

		public virtual void SendUpdatePoints()
		{
			if (m_gameClient.Player == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CharacterPointsUpdate)))
			{
				pak.WriteInt((uint)m_gameClient.Player.RealmPoints);
				pak.WriteShort(m_gameClient.Player.LevelPermill);
				pak.WriteShort((ushort)m_gameClient.Player.SkillSpecialtyPoints);
				pak.WriteInt((uint)m_gameClient.Player.BountyPoints);
				pak.WriteShort((ushort)m_gameClient.Player.RealmSpecialtyPoints);
				pak.WriteShort(m_gameClient.Player.ChampionLevelPermill);
				pak.WriteLongLowEndian((ulong)m_gameClient.Player.Experience);
				pak.WriteLongLowEndian((ulong)m_gameClient.Player.ExperienceForNextLevel);
				pak.WriteLongLowEndian(0);//champExp
				pak.WriteLongLowEndian(0);//champExpNextLevel
				SendTCP(pak);
			}
		}
		public virtual void SendUpdatePlayer()
		{
			GamePlayer player = m_gameClient.Player;
			if (player == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak.WriteByte(0x03); //subcode
				pak.WriteByte(0x0f); //number of entry
				pak.WriteByte(0x00); //subtype
				pak.WriteByte(0x00); //unk
									 //entry :

				pak.WriteByte(player.GetDisplayLevel(m_gameClient.Player)); //level
				pak.WritePascalString(player.Name); // player name
				pak.WriteByte((byte)(player.MaxHealth >> 8)); // maxhealth high byte ?
				pak.WritePascalString(player.CharacterClass.Name); // class name
				pak.WriteByte((byte)(player.MaxHealth & 0xFF)); // maxhealth low byte ?
				pak.WritePascalString( /*"The "+*/player.CharacterClass.Profession); // Profession
				pak.WriteByte(0x00); //unk
				pak.WritePascalString(player.CharacterClass.GetTitle(player, player.Level)); // player level
																							 //todo make function to calcule realm rank
																							 //client.Player.RealmPoints
																							 //todo i think it s realmpoint percent not realrank
				pak.WriteByte((byte)player.RealmLevel); //urealm rank
				pak.WritePascalString(player.RealmRankTitle(player.Client.Account.Language)); // Realm title
				pak.WriteByte((byte)player.RealmSpecialtyPoints); // realm skill points
				pak.WritePascalString(player.CharacterClass.BaseName); // base class
				pak.WriteByte((byte)(HouseMgr.GetHouseNumberByPlayer(player) >> 8)); // personal house high byte
				pak.WritePascalString(player.GuildName); // Guild name
				pak.WriteByte((byte)(HouseMgr.GetHouseNumberByPlayer(player) & 0xFF)); // personal house low byte
				pak.WritePascalString(player.LastName); // Last name
				pak.WriteByte((byte)(player.MLLevel + 1)); // ML Level (+1)
				pak.WritePascalString(player.RaceName); // Race name
				pak.WriteByte(0x0);

				if (player.GuildRank != null)
					pak.WritePascalString(player.GuildRank.Title); // Guild title
				else
					pak.WritePascalString("");
				pak.WriteByte(0x0);

				AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(player.CraftingPrimarySkill);
				if (skill != null)
					pak.WritePascalString(skill.Name); //crafter guilde: alchemist
				else
					pak.WritePascalString("None"); //no craft skill at start

				pak.WriteByte(0x0);
				pak.WritePascalString(player.CraftTitle.GetValue(player, player)); //crafter title: legendary alchemist
				pak.WriteByte(0x0);
				pak.WritePascalString(player.MLTitle.GetValue(player, player)); //ML title

				// new in 1.75
				pak.WriteByte(0x0);
				if (player.CurrentTitle != PlayerTitleMgr.ClearTitle)
					pak.WritePascalString(player.CurrentTitle.GetValue(player, player)); // new in 1.74 - Custom title
				else
					pak.WritePascalString("None");

				// new in 1.79
				if (player.Champion)
					pak.WriteByte((byte)(player.ChampionLevel + 1)); // Champion Level (+1)
				else
					pak.WriteByte(0x0);
				pak.WritePascalString(player.CLTitle.GetValue(player, player)); // Champion Title
				SendTCP(pak);
			}
		}
		public virtual void SendUpdatePlayerSkills()
		{
			if (m_gameClient.Player == null)
				return;

			// Get Skills as "Usable Skills" which are in network order ! (with forced update)
			List<Tuple<Skill, Skill>> usableSkills = m_gameClient.Player.GetAllUsableSkills(true);

			bool sent = false; // set to true once we can't send packet anymore !
			int index = 0; // index of our position in the list !
			int total = usableSkills.Count; // cache List count.
			int packetCount = 0; // Number of packet sent for the entire list
			while (!sent)
			{
				int packetEntry = 0; // needed to tell client how much skill we send
									 // using pak
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
				{
					// Write header
					pak.WriteByte(0x01); //subcode for skill
					pak.WriteByte((byte)0); //packet entries, can't know it for now...
					pak.WriteByte((byte)0x03); //subtype for following pages
					pak.WriteByte((byte)index); // packet first entry

					// getting pak filled
					while (index < total)
					{
						// this item will break the limit, send the packet before, keep index as is to continue !
						if ((index >= byte.MaxValue) || ((pak.Length + 8 + usableSkills[index].Item1.Name.Length) > 1400))
						{
							break;
						}

						// Enter Packet Values !! Format Level - Type - SpecialField - Bonus - Icon - Name
						Skill skill = usableSkills[index].Item1;
						Skill skillrelated = usableSkills[index].Item2;

						if (skill is Specialization)
						{
							Specialization spec = (Specialization)skill;
							pak.WriteByte((byte)spec.Level);
							pak.WriteShort((ushort)spec.InternalID); //new 1.112
							pak.WriteByte((byte)spec.SkillType);
							pak.WriteShort(0);
							pak.WriteByte((byte)(m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
							pak.WriteShort((ushort)spec.Icon);
							pak.WritePascalString(spec.Name);
						}
						else if (skill is Ability)
						{
							Ability ab = (Ability)skill;
							pak.WriteByte((byte)ab.Level);
							pak.WriteShort((ushort)ab.InternalID); //new 1.112
							pak.WriteByte((byte)ab.SkillType);
							pak.WriteShort(0);
							pak.WriteByte((byte)0);
							pak.WriteShort((ushort)ab.Icon);
							pak.WritePascalString(ab.Name);

						}
						else if (skill is Spell)
						{
							Spell spell = (Spell)skill;
							pak.WriteByte((byte)spell.Level);
							pak.WriteShort((ushort)spell.InternalID); //new 1.112
							pak.WriteByte((byte)spell.SkillType);

							// spec index for this Spell - Special for Song and Unknown Indexes...
							int spin = 0;
							if (spell.SkillType == eSkillPage.Songs)
							{
								spin = 0xFF;
							}
							else
							{
								// find this line Specialization index !
								if (skillrelated is SpellLine && !Util.IsEmpty(((SpellLine)skillrelated).Spec))
								{
									spin = usableSkills.FindIndex(sk => (sk.Item1 is Specialization) && ((Specialization)sk.Item1).KeyName == ((SpellLine)skillrelated).Spec);

									if (spin == -1)
										spin = 0xFE;
								}
								else
								{
									spin = 0xFE;
								}
							}

							pak.WriteShort((ushort)spin); // special index for spellline
							pak.WriteByte(0); // bonus
							pak.WriteShort(spell.InternalIconID > 0 ? spell.InternalIconID : spell.Icon); // icon
							pak.WritePascalString(spell.Name);
						}
						else if (skill is Style)
						{
							Style style = (Style)skill;
							pak.WriteByte((byte)style.SpecLevelRequirement);
							pak.WriteShort((ushort)style.InternalID); //new 1.112
							pak.WriteByte((byte)style.SkillType);

							// Special pre-requisite (First byte is Pre-requisite Icon / second Byte is prerequisite code...)
							int pre = 0;

							switch (style.OpeningRequirementType)
							{
								case Style.eOpening.Offensive:
									pre = (int)style.AttackResultRequirement; // last result of our attack against enemy hit, miss, target blocked, target parried, ...
									if (style.AttackResultRequirement == Style.eAttackResultRequirement.Style)
									{
										// get style requirement value... find prerequisite style index from specs beginning...
										int styleindex = Math.Max(0, usableSkills.FindIndex(it => (it.Item1 is Style) && it.Item1.ID == style.OpeningRequirementValue));
										int speccount = Math.Max(0, usableSkills.FindIndex(it => (it.Item1 is Specialization) == false));
										pre |= ((byte)(100 + styleindex - speccount)) << 8;
									}
									break;
								case Style.eOpening.Defensive:
									pre = 100 + (int)style.AttackResultRequirement; // last result of enemies attack against us hit, miss, you block, you parry, ...
									break;
								case Style.eOpening.Positional:
									pre = 200 + style.OpeningRequirementValue;
									break;
							}

							// style required?
							if (pre == 0)
								pre = 0x100;

							pak.WriteShort((ushort)pre);
							pak.WriteByte(GlobalConstants.GetSpecToInternalIndex(style.Spec)); // index specialization in bonus...
							pak.WriteShort((ushort)style.Icon);
							pak.WritePascalString(style.Name);
						}

						packetEntry++;
						index++;
					}

					// test if we finished sending packets
					if (index >= total || index >= byte.MaxValue)
						sent = true;

					// rewrite header for count.
					pak.Position = 4;
					pak.WriteByte((byte)packetEntry);

					if (!sent)
						pak.WriteByte((byte)99);

					SendTCP(pak);

				}

				packetCount++;
			}

			// Send List Cast Spells...
			SendNonHybridSpellLines();
			// clear trainer cache
			m_gameClient.TrainerSkillCache = null;

			if (ForceTooltipUpdate)
				SendForceTooltipUpdate(usableSkills.Select(t => t.Item1));
		}
		public virtual void SendUDPInitReply()
		{
			using (var pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.UDPInitReply)))
			{
				Region playerRegion = null;
				if (!m_gameClient.Socket.Connected) // || !_gameClient.UsingRC4) // not using RC4, wont accept UDP packets anyway.)
				{
					return;
				}
				if (m_gameClient.Player != null && m_gameClient.Player.CurrentRegion != null)
				{
					playerRegion = m_gameClient.Player.CurrentRegion;
				}
				if (playerRegion == null)
				{
					pak.Fill(0x0, 0x18);
				}
				else
				{
					//Try to fix the region ip so UDP is enabled!
					string ip = playerRegion.ServerIP;
					if (ip == "any" || ip == "0.0.0.0" || ip == "127.0.0.1" || ip.StartsWith("10.13.") || ip.StartsWith("192.168."))
					{
						ip = ((IPEndPoint)m_gameClient.Socket.LocalEndPoint).Address.ToString();
					}
					pak.FillString(ip, 22);
					pak.WriteShort(playerRegion.ServerPort);
				}
				SendUDP(pak, true);
			}
		}
		public virtual void SendVampireEffect(GameLiving living, bool show)
		{
			if (m_gameClient.Player == null || living == null)
				return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{

				pak.WriteShort((ushort)living.ObjectID);
				pak.WriteByte(0x4); // Vampire (can fly)
				pak.WriteByte((byte)(show ? 0 : 1)); // 0-enable, 1-disable
				pak.WriteInt(0);

				SendTCP(pak);
			}
		}
		/// <summary>
		/// Reply on Server Opening to Client Encryption Request
		/// Actually forces Encryption Off to work with Portal.
		/// </summary>
		public virtual void SendVersionAndCryptKey()
		{
			//Construct the new packet
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CryptKey)))
			{
				pak.WriteByte((byte)m_gameClient.ClientType);

				//Disable encryption (1110+ always encrypt)
				pak.WriteByte(0x00);

				// Reply with current version
				pak.WriteString((((int)m_gameClient.Version) / 1000) + "." + (((int)m_gameClient.Version) - 1000), 5);

				// revision, last seen (c) 0x63
				pak.WriteByte((byte)m_gameClient.MinorRev[0]);

				// Build number
				pak.WriteByte(m_gameClient.MajorBuild); // last seen : 0x44 0x05
				pak.WriteByte(m_gameClient.MinorBuild);
				SendTCP(pak);
			}
		}
		public virtual void SendWarlockChamberEffect(GamePlayer player)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect)))
			{

				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte((byte)3);

				SortedList sortList = new SortedList();
				sortList.Add(1, null);
				sortList.Add(2, null);
				sortList.Add(3, null);
				sortList.Add(4, null);
				sortList.Add(5, null);
				lock (player.EffectList)
				{
					foreach (IGameEffect fx in player.EffectList)
					{
						if (fx is GameSpellEffect)
						{
							GameSpellEffect effect = (GameSpellEffect)fx;
							if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == "Chamber"))
							{
								ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
								sortList[chamber.EffectSlot] = effect;
							}
						}
					}
					foreach (GameSpellEffect effect in sortList.Values)
					{
						if (effect == null)
						{
							pak.WriteByte((byte)0);
						}
						else
						{
							ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
							if (chamber.PrimarySpell != null && chamber.SecondarySpell == null)
							{
								pak.WriteByte((byte)3);
							}
							else if (chamber.PrimarySpell != null && chamber.SecondarySpell != null)
							{
								if (chamber.SecondarySpell.SpellType == "Lifedrain")
									pak.WriteByte(0x11);
								else if (chamber.SecondarySpell.SpellType.IndexOf("SpeedDecrease") != -1)
									pak.WriteByte(0x33);
								else if (chamber.SecondarySpell.SpellType == "PowerRegenBuff")
									pak.WriteByte(0x77);
								else if (chamber.SecondarySpell.SpellType == "DirectDamage")
									pak.WriteByte(0x66);
								else if (chamber.SecondarySpell.SpellType == "SpreadHeal")
									pak.WriteByte(0x55);
								else if (chamber.SecondarySpell.SpellType == "Nearsight")
									pak.WriteByte(0x44);
								else if (chamber.SecondarySpell.SpellType == "DamageOverTime")
									pak.WriteByte(0x22);
							}
						}
					}
				}
				//pak.WriteByte(0x11);
				//pak.WriteByte(0x22);
				//pak.WriteByte(0x33);
				//pak.WriteByte(0x44);
				//pak.WriteByte(0x55);
				pak.WriteInt(0);

				foreach (GamePlayer plr in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player != plr)
						plr.Client.PacketProcessor.SendTCP(pak);
				}

				SendTCP(pak);
			}
		}
		public virtual void SendWarmapBonuses()
		{
			if (m_gameClient.Player == null) return;
			int AlbTowers = 0;
			int MidTowers = 0;
			int HibTowers = 0;
			int AlbKeeps = 0;
			int MidKeeps = 0;
			int HibKeeps = 0;
			int OwnerDFTowers = 0;
			eRealm OwnerDF = eRealm.None;
			foreach (AbstractGameKeep keep in GameServer.KeepManager.GetFrontierKeeps())
			{

				switch ((eRealm)keep.Realm)
				{
					case eRealm.Albion:
						if (keep is GameKeep)
							AlbKeeps++;
						else
							AlbTowers++;
						break;
					case eRealm.Midgard:
						if (keep is GameKeep)
							MidKeeps++;
						else
							MidTowers++;
						break;
					case eRealm.Hibernia:
						if (keep is GameKeep)
							HibKeeps++;
						else
							HibTowers++;
						break;
					default:
						break;
				}
			}
			if (AlbTowers > MidTowers && AlbTowers > HibTowers)
			{
				OwnerDF = eRealm.Albion;
				OwnerDFTowers = AlbTowers;
			}
			else if (MidTowers > AlbTowers && MidTowers > HibTowers)
			{
				OwnerDF = eRealm.Midgard;
				OwnerDFTowers = MidTowers;
			}
			else if (HibTowers > AlbTowers && HibTowers > MidTowers)
			{
				OwnerDF = eRealm.Hibernia;
				OwnerDFTowers = HibTowers;
			}
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarmapBonuses)))
			{
				int RealmKeeps = 0;
				int RealmTowers = 0;
				switch ((eRealm)m_gameClient.Player.Realm)
				{
					case eRealm.Albion:
						RealmKeeps = AlbKeeps;
						RealmTowers = AlbTowers;
						break;
					case eRealm.Midgard:
						RealmKeeps = MidKeeps;
						RealmTowers = MidTowers;
						break;
					case eRealm.Hibernia:
						RealmKeeps = HibKeeps;
						RealmTowers = HibTowers;
						break;
					default:
						break;
				}
				pak.WriteByte((byte)RealmKeeps);
				pak.WriteByte((byte)(((byte)RelicMgr.GetRelicCount(m_gameClient.Player.Realm, eRelicType.Magic)) << 4 | (byte)RelicMgr.GetRelicCount(m_gameClient.Player.Realm, eRelicType.Strength)));
				pak.WriteByte((byte)OwnerDF);
				pak.WriteByte((byte)RealmTowers);
				pak.WriteByte((byte)OwnerDFTowers);
				SendTCP(pak);
			}
		}
		public virtual void SendWarmapUpdate(ICollection<IGameKeep> list)
		{
			if (m_gameClient.Player == null) return;

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarMapClaimedKeeps)))
			{
				int KeepCount = 0;
				int TowerCount = 0;
				foreach (AbstractGameKeep keep in list)
				{
					// New Agramon tower are counted as keep
					if (keep is GameKeep || (keep.KeepID & 0xFF) > 150)
						KeepCount++;
					else
						TowerCount++;
				}
				pak.WriteShort(0x0F00);
				pak.WriteByte((byte)KeepCount);
				pak.WriteByte((byte)TowerCount);
				byte albStr = 0;
				byte hibStr = 0;
				byte midStr = 0;
				byte albMagic = 0;
				byte hibMagic = 0;
				byte midMagic = 0;
				foreach (GameRelic relic in RelicMgr.getNFRelics())
				{
					switch (relic.OriginalRealm)
					{
						case eRealm.Albion:
							if (relic.RelicType == eRelicType.Strength)
							{
								albStr = (byte)relic.Realm;
							}
							if (relic.RelicType == eRelicType.Magic)
							{
								albMagic = (byte)relic.Realm;
							}
							break;
						case eRealm.Hibernia:
							if (relic.RelicType == eRelicType.Strength)
							{
								hibStr = (byte)relic.Realm;
							}
							if (relic.RelicType == eRelicType.Magic)
							{
								hibMagic = (byte)relic.Realm;
							}
							break;
						case eRealm.Midgard:
							if (relic.RelicType == eRelicType.Strength)
							{
								midStr = (byte)relic.Realm;
							}
							if (relic.RelicType == eRelicType.Magic)
							{
								midMagic = (byte)relic.Realm;
							}
							break;
					}
				}
				pak.WriteByte(albStr);
				pak.WriteByte(midStr);
				pak.WriteByte(hibStr);
				pak.WriteByte(albMagic);
				pak.WriteByte(midMagic);
				pak.WriteByte(hibMagic);
				foreach (AbstractGameKeep keep in list)
				{
					int keepId = keep.KeepID;

					/*if (ServerProperties.Properties.USE_NEW_KEEPS == 1 || ServerProperties.Properties.USE_NEW_KEEPS == 2)
	                {
	                    keepId -= 12;
	                    if ((keep.KeepID > 74 && keep.KeepID < 114) || (keep.KeepID > 330 && keep.KeepID < 370) || (keep.KeepID > 586 && keep.KeepID < 626) 
	                        || (keep.KeepID > 842 && keep.KeepID < 882) || (keep.KeepID > 1098 && keep.KeepID < 1138)) 
	                        keepId += 5;
	                }*/

					int id = keepId & 0xFF;
					int tower = keep.KeepID >> 8;
					int map = (id / 25) - 1;

					int index = id - (map * 25 + 25);

					// Special Agramon zone
					if ((keep.KeepID & 0xFF) > 150)
						index = keep.KeepID - 151;

					int flag = (byte)keep.Realm; // 3 bits
					Guild guild = keep.Guild;
					string name = "";
					// map is now 0 indexed
					pak.WriteByte((byte)(((map - 1) << 6) | (index << 3) | tower));
					if (guild != null)
					{
						flag |= (byte)eRealmWarmapKeepFlags.Claimed;
						name = guild.Name;
					}

					//Teleport
					if (m_gameClient.Account.PrivLevel > (int)ePrivLevel.Player)
					{
						flag |= (byte)eRealmWarmapKeepFlags.Teleportable;
					}
					else
					{
						if (GameServer.KeepManager.FrontierRegionsList.Contains(m_gameClient.Player.CurrentRegionID) && m_gameClient.Player.Realm == keep.Realm)
						{
							GameKeep theKeep = keep as GameKeep;
							if (theKeep != null)
							{
								if (theKeep.OwnsAllTowers && !theKeep.InCombat)
								{
									flag |= (byte)eRealmWarmapKeepFlags.Teleportable;
								}
							}
						}
					}

					if (keep.InCombat)
					{
						flag |= (byte)eRealmWarmapKeepFlags.UnderSiege;
					}

					pak.WriteByte((byte)flag);
					pak.WritePascalString(name);
				}
				SendTCP(pak);
			}
		}

		public virtual void SendWarmapDetailUpdate(List<List<byte>> fights, List<List<byte>> groups)
		{
			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.WarMapDetailUpdate)))
			{
				pak.WriteByte((byte)fights.Count);// count - Fights (Byte)
				pak.WriteByte((byte)groups.Count);// count - Groups (Byte)
												  // order first fights after than groups

				// zoneid  - byte // zoneid from zones.xml
				//			- A7 - Mid	- left			-	10100111		Map: Midgard
				//			- A8 - Mid	- middle		-	10101000			| X	|
				//			- A9 - Mid	- right			-	10101001		|x	  x		x	|
				//			- AA - Mid	- middle  - top	-	10101010
				//
				//			- AB - Hib	- top			-	10101011	171		Map: Hibernia
				//			- AC - Hib	- middle		-	10101100				|X		|
				//			- AD - Hib	- middle -left	-	10101101			|x	 x		|
				//			- AE - Hib	- bottom		-	10101110				|x		|

				//			- AF - Alb	- bottom		-	10101111			Map: Albion
				//			- B0 - Alb	- middle -right	-	10110000			|X	|
				//			- B1 - Alb	- middle -left	-	10110001			|x	 x	|
				//			- B2 - Alb	- top			-	10110010	178		|X	|

				// position   x/y offset  x<<4,y

				foreach (List<byte> obj in fights)
				{
					pak.WriteByte(obj[0]);// zoneid
					pak.WriteByte((byte)((obj[1] << 4) | (obj[2] & 0x0f))); // position
					pak.WriteByte(obj[3]);// color - ( Fights:  0x00 - Grey , 0x01 - RedBlue , 0x02 - RedGreen , 0x03 - GreenBlue )
					pak.WriteByte(obj[4]);// type  - ( Fights:  Size 0x00 - small  0x01 - medium  0x02 - big 0x03 - huge )
				}

				foreach (List<byte> obj in groups)
				{
					pak.WriteByte(obj[0]);// zoneid
					pak.WriteByte((byte)(obj[1] << 4 | obj[2])); // position
					byte realm = obj[3];

					pak.WriteByte((byte)((realm == 3) ? 0x04 : (realm == 2) ? 0x02 : 0x01));//	color   ( Groups:  0x01 - Alb  , 0x02 - Mid , 0x04 - Hib
					switch ((eRealm)obj[3])
					{
						//	type    ( Groups:	Alb:	type	   0x03,0x02,0x01	& 0x03
						//						Mid:	type << 2  0x0C,0x08,0x04 	& 0x03
						//						Hib:	type << 4  0x30,0x20,0x10	& 0x03  )
						case eRealm.Albion:
						default:
							pak.WriteByte(obj[4]);
							break;
						case eRealm.Midgard:
							pak.WriteByte((byte)(obj[4] << 2));
							break;
						case eRealm.Hibernia:
							pak.WriteByte((byte)(obj[4] << 4));
							break;
					}
				}

				SendTCP(pak);
			}
		}
		protected void WriteCustomTextWindowData(GSTCPPacketOut pak, IList<string> text)
		{
			byte line = 0;
			bool needBreak = false;

			foreach (var listStr in text)
			{
				string str = listStr;

				if (str != null)
				{
					if (pak.Position + 4 > MaxPacketLength) // line + pascalstringline(1) + trailingZero
						return;

					pak.WriteByte(++line);

					while (str.Length > byte.MaxValue)
					{
						string s = str.Substring(0, byte.MaxValue);

						if (pak.Position + s.Length + 2 > MaxPacketLength)
						{
							needBreak = true;
							break;
						}

						pak.WritePascalString(s);
						str = str.Substring(byte.MaxValue, str.Length - byte.MaxValue);
						if (line >= 200 || pak.Position + Math.Min(byte.MaxValue, str.Length) + 2 >= MaxPacketLength)
							// line + pascalstringline(1) + trailingZero
							return;

						pak.WriteByte(++line);
					}

					if (pak.Position + str.Length + 2 > MaxPacketLength) // str.Length + trailing zero
					{
						str = str.Substring(0, (int)Math.Max(Math.Min(1, str.Length), MaxPacketLength - pak.Position - 2));
						needBreak = true;
					}

					pak.WritePascalString(str);

					if (needBreak || line >= 200) // Check max packet length or max stings in window (0 - 199)
						break;
				}
			}
		}
		protected virtual void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, bool updateMap, GameLiving living)
		{
			pak.WriteByte((byte)(living.GroupIndex + 1)); // From 1 to 8
			if (living.CurrentRegion != m_gameClient.Player.CurrentRegion)
			{
				pak.WriteByte(0x00); // health
				pak.WriteByte(0x00); // mana
				pak.WriteByte(0x00); // endu
				pak.WriteByte(0x20); // player state (0x20 = another region)
				if (updateIcons)
				{
					pak.WriteByte((byte)(0x80 | living.GroupIndex));
					pak.WriteByte(0);
				}
				return;
			}
			var player = living as GamePlayer;

			pak.WriteByte(player?.CharacterClass?.HealthPercentGroupWindow ?? living.HealthPercent);
			pak.WriteByte(living.ManaPercent);
			pak.WriteByte(living.EndurancePercent); // new in 1.69

			byte playerStatus = 0;
			if (!living.IsAlive)
				playerStatus |= 0x01;
			if (living.IsMezzed)
				playerStatus |= 0x02;
			if (living.IsDiseased)
				playerStatus |= 0x04;
			if (SpellHelper.FindEffectOnTarget(living, "DamageOverTime") != null)
				playerStatus |= 0x08;
			if (player?.Client?.ClientState == GameClient.eClientState.Linkdead)
				playerStatus |= 0x10;
			if (living.DebuffCategory[(int)eProperty.SpellRange] != 0 || living.DebuffCategory[(int)eProperty.ArcheryRange] != 0)
				playerStatus |= 0x40;
			pak.WriteByte(playerStatus);
			// 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
			// 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region, 0x40 - NS

			if (updateIcons)
			{
				pak.WriteByte((byte)(0x80 | living.GroupIndex));
				lock (living.EffectList)
				{
					byte i = 0;
					foreach (IGameEffect effect in living.EffectList)
						if (effect is GameSpellEffect)
							i++;
					pak.WriteByte(i);
					foreach (IGameEffect effect in living.EffectList)
						if (effect is GameSpellEffect)
						{
							pak.WriteByte(0);
							pak.WriteShort(effect.Icon);
						}
				}
			}
			if (updateMap)
				WriteGroupMemberMapUpdate(pak, living);
		}
		protected virtual void WriteGroupMemberMapUpdate(GSTCPPacketOut pak, GameLiving living)
		{
			if (living.CurrentSpeed != 0)
			{
				Zone zone = living.CurrentZone;
				if (zone == null)
					return;
				pak.WriteByte((byte)(0x40 | living.GroupIndex));
				//Dinberg - ZoneSkinID for group members aswell.
				pak.WriteShort(zone.ZoneSkinID);
				pak.WriteShort((ushort)(living.X - zone.XOffset));
				pak.WriteShort((ushort)(living.Y - zone.YOffset));
			}
		}
		protected virtual void WriteHouseFurniture(GSTCPPacketOut pak, IndoorItem item, int index)
		{
			pak.WriteByte((byte)index);
			byte type = 0;
			if (item.Emblem > 0)
				item.Color = item.Emblem;
			if (item.Color > 0)
			{
				if (item.Color <= 0xFF)
					type |= 1; // colored
				else if (item.Color <= 0xFFFF)
					type |= 2; // old emblem
				else
					type |= 6; // new emblem
			}
			if (item.Size != 0)
				type |= 8; // have size
			pak.WriteByte(type);
			pak.WriteShort((ushort)item.Model);
			if ((type & 1) == 1)
				pak.WriteByte((byte)item.Color);
			else if ((type & 6) == 2)
				pak.WriteShort((ushort)item.Color);
			else if ((type & 6) == 6)
				pak.WriteShort((ushort)(item.Color & 0xFFFF));
			pak.WriteShort((ushort)item.X);
			pak.WriteShort((ushort)item.Y);
			pak.WriteShort((ushort)item.Rotation);
			if ((type & 8) == 8)
				pak.WriteByte((byte)item.Size);
			pak.WriteByte((byte)item.Position);
			pak.WriteByte((byte)(item.PlacementMode - 2));
		}
		/// <summary>
		/// New item data packet for 1.119
		/// </summary>		
		protected virtual void WriteItemData(GSTCPPacketOut pak, InventoryItem item)
		{
			if (item == null)
			{
				pak.Fill(0x00, 24); // +1 item.Effect changed to short
				return;
			}
			pak.WriteShort((ushort)0); // item uniqueID
			pak.WriteByte((byte)item.Level);

			int value1; // some object types use this field to display count
			int value2; // some object types use this field to display count
			switch (item.Object_Type)
			{
				case (int)eObjectType.GenericItem:
					value1 = item.Count & 0xFF;
					value2 = (item.Count >> 8) & 0xFF;
					break;
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
					value1 = item.Count;
					value2 = item.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = item.DPS_AF;
					value2 = item.Count;
					break;
				case (int)eObjectType.Instrument:
					value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF);
					value2 = 0;
					break; // unused
				case (int)eObjectType.Shield:
					value1 = item.Type_Damage;
					value2 = item.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.HouseWallObject:
				case (int)eObjectType.HouseFloorObject:
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = item.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = item.DPS_AF;
					value2 = item.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (item.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(item.DPS_AF));
			else
				pak.WriteByte((byte)(item.Hand << 6));

			pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
			pak.WriteByte(0x00); //unk 1.112
			pak.WriteShort((ushort)item.Weight);
			pak.WriteByte(item.ConditionPercent); // % of con
			pak.WriteByte(item.DurabilityPercent); // % of dur
			pak.WriteByte((byte)item.Quality); // % of qua
			pak.WriteByte((byte)item.Bonus); // % bonus
			pak.WriteByte((byte)item.BonusLevel); // 1.109
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Extension);
			int flag = 0;
			int emblem = item.Emblem;
			int color = item.Color;
			if (emblem != 0)
			{
				pak.WriteShort((ushort)emblem);
				flag |= (emblem & 0x010000) >> 16; // = 1 for newGuildEmblem
			}
			else
			{
				pak.WriteShort((ushort)color);
			}
			//flag |= 0x01; // newGuildEmblem
			flag |= 0x02; // enable salvage button
			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(m_gameClient.Player.CraftingPrimarySkill);
			if (skill != null && skill is AdvancedCraftingSkill/* && ((AdvancedCraftingSkill)skill).IsAllowedToCombine(_gameClient.Player, item)*/)
				flag |= 0x04; // enable craft button
			ushort icon1 = 0;
			ushort icon2 = 0;
			string spell_name1 = "";
			string spell_name2 = "";
			if (item.Object_Type != (int)eObjectType.AlchemyTincture)
			{
				if (item.SpellID > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID)
							{
								flag |= 0x08;
								icon1 = spl.Icon;
								spell_name1 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
				if (item.SpellID1 > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID1)
							{
								flag |= 0x10;
								icon2 = spl.Icon;
								spell_name2 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
			}
			pak.WriteByte((byte)flag);
			if ((flag & 0x08) == 0x08)
			{
				pak.WriteShort((ushort)icon1);
				pak.WritePascalString(spell_name1);
			}
			if ((flag & 0x10) == 0x10)
			{
				pak.WriteShort((ushort)icon2);
				pak.WritePascalString(spell_name2);
			}
			pak.WriteShort((ushort)item.Effect); // item effect changed to short
			string name = item.Name;
			if (item.Count > 1)
				name = item.Count + " " + name;
			if (item.SellPrice > 0)
			{
				if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					name += "[" + item.SellPrice.ToString() + " BP]";
				else
					name += "[" + Money.GetString(item.SellPrice) + "]";
			}
			if (name == null) name = "";
			if (name.Length > 55)
				name = name.Substring(0, 55);
			pak.WritePascalString(name);
		}

		/// <summary>
		/// patch 0020
		/// </summary>       
		protected virtual void WriteItemData(GSTCPPacketOut pak, InventoryItem item, int questID)
		{
			if (item == null)
			{
				pak.Fill(0x00, 24); //item.Effect changed to short 1.119
				return;
			}

			pak.WriteShort((ushort)questID); // need to send an objectID for reward quest delve to work 1.115+
			pak.WriteByte((byte)item.Level);

			int value1; // some object types use this field to display count
			int value2; // some object types use this field to display count
			switch (item.Object_Type)
			{
				case (int)eObjectType.GenericItem:
					value1 = item.Count & 0xFF;
					value2 = (item.Count >> 8) & 0xFF;
					break;
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
					value1 = item.Count;
					value2 = item.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = item.DPS_AF;
					value2 = item.Count;
					break;
				case (int)eObjectType.Instrument:
					value1 = (item.DPS_AF == 2 ? 0 : item.DPS_AF);
					value2 = 0;
					break; // unused
				case (int)eObjectType.Shield:
					value1 = item.Type_Damage;
					value2 = item.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.HouseWallObject:
				case (int)eObjectType.HouseFloorObject:
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = item.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = item.DPS_AF;
					value2 = item.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (item.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(item.DPS_AF));
			else
				pak.WriteByte((byte)(item.Hand << 6));

			pak.WriteByte((byte)((item.Type_Damage > 3 ? 0 : item.Type_Damage << 6) | item.Object_Type));
			pak.WriteByte(0x00); //unk 1.112
			pak.WriteShort((ushort)item.Weight);
			pak.WriteByte(item.ConditionPercent); // % of con
			pak.WriteByte(item.DurabilityPercent); // % of dur
			pak.WriteByte((byte)item.Quality); // % of qua
			pak.WriteByte((byte)item.Bonus); // % bonus
			pak.WriteByte((byte)item.BonusLevel); // 1.109
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Extension);
			int flag = 0;
			int emblem = item.Emblem;
			int color = item.Color;
			if (emblem != 0)
			{
				pak.WriteShort((ushort)emblem);
				flag |= (emblem & 0x010000) >> 16; // = 1 for newGuildEmblem
			}
			else
			{
				pak.WriteShort((ushort)color);
			}
			//flag |= 0x01; // newGuildEmblem
			flag |= 0x02; // enable salvage button
			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(m_gameClient.Player.CraftingPrimarySkill);
			if (skill != null && skill is AdvancedCraftingSkill/* && ((AdvancedCraftingSkill)skill).IsAllowedToCombine(_gameClient.Player, item)*/)
				flag |= 0x04; // enable craft button
			ushort icon1 = 0;
			ushort icon2 = 0;
			string spell_name1 = "";
			string spell_name2 = "";
			if (item.Object_Type != (int)eObjectType.AlchemyTincture)
			{
				if (item.SpellID > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID)
							{
								flag |= 0x08;
								icon1 = spl.Icon;
								spell_name1 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
				if (item.SpellID1 > 0/* && item.Charges > 0*/)
				{
					SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (chargeEffectsLine != null)
					{
						List<Spell> spells = SkillBase.GetSpellList(chargeEffectsLine.KeyName);
						foreach (Spell spl in spells)
						{
							if (spl.ID == item.SpellID1)
							{
								flag |= 0x10;
								icon2 = spl.Icon;
								spell_name2 = spl.Name; // or best spl.Name ?
								break;
							}
						}
					}
				}
			}
			pak.WriteByte((byte)flag);
			if ((flag & 0x08) == 0x08)
			{
				pak.WriteShort((ushort)icon1);
				pak.WritePascalString(spell_name1);
			}
			if ((flag & 0x10) == 0x10)
			{
				pak.WriteShort((ushort)icon2);
				pak.WritePascalString(spell_name2);
			}
			pak.WriteShort((ushort)item.Effect); // changed to short 1.119
			string name = item.Name;
			if (item.Count > 1)
				name = item.Count + " " + name;
			if (item.SellPrice > 0)
			{
				if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					name += "[" + item.SellPrice.ToString() + " BP]";
				else
					name += "[" + Money.GetString(item.SellPrice) + "]";
			}
			if (name == null) name = "";
			if (name.Length > 55)
				name = name.Substring(0, 55);
			pak.WritePascalString(name);
		}

		protected virtual void WriteTemplateData(GSTCPPacketOut pak, ItemTemplate template, int count)
		{
			if (template == null)
			{
				pak.Fill(0x00, 24); // 1.109 +1 byte
				return;
			}
			pak.WriteShort(0); // objectID
			pak.WriteByte((byte)template.Level);

			int value1;
			int value2;

			switch (template.Object_Type)
			{
				case (int)eObjectType.Arrow:
				case (int)eObjectType.Bolt:
				case (int)eObjectType.Poison:
				case (int)eObjectType.GenericItem:
					value1 = count; // Count
					value2 = template.SPD_ABS;
					break;
				case (int)eObjectType.Thrown:
					value1 = template.DPS_AF;
					value2 = count; // Count
					break;
				case (int)eObjectType.Instrument:
					value1 = (template.DPS_AF == 2 ? 0 : template.DPS_AF);
					value2 = 0;
					break;
				case (int)eObjectType.Shield:
					value1 = template.Type_Damage;
					value2 = template.DPS_AF;
					break;
				case (int)eObjectType.AlchemyTincture:
				case (int)eObjectType.SpellcraftGem:
					value1 = 0;
					value2 = 0;
					/*
					must contain the quality of gem for spell craft and think same for tincture
					*/
					break;
				case (int)eObjectType.GardenObject:
					value1 = 0;
					value2 = template.SPD_ABS;
					/*
					Value2 byte sets the width, only lower 4 bits 'seem' to be used (so 1-15 only)

					The byte used for "Hand" (IE: Mini-delve showing a weapon as Left-Hand
					usabe/TwoHanded), the lower 4 bits store the height (1-15 only)
					*/
					break;

				default:
					value1 = template.DPS_AF;
					value2 = template.SPD_ABS;
					break;
			}
			pak.WriteByte((byte)value1);
			pak.WriteByte((byte)value2);

			if (template.Object_Type == (int)eObjectType.GardenObject)
				pak.WriteByte((byte)(template.DPS_AF));
			else
				pak.WriteByte((byte)(template.Hand << 6));
			pak.WriteByte((byte)((template.Type_Damage > 3
				? 0
				: template.Type_Damage << 6) | template.Object_Type));
			pak.Fill(0x00, 1); // 1.109, +1 byte, no clue what this is  - Tolakram
			pak.WriteShort((ushort)template.Weight);
			pak.WriteByte(template.BaseConditionPercent);
			pak.WriteByte(template.BaseDurabilityPercent);
			pak.WriteByte((byte)template.Quality);
			pak.WriteByte((byte)template.Bonus);
			pak.WriteByte((byte)template.BonusLevel); // 1.109
			pak.WriteShort((ushort)template.Model);
			pak.WriteByte((byte)template.Extension);
			if (template.Emblem != 0)
				pak.WriteShort((ushort)template.Emblem);
			else
				pak.WriteShort((ushort)template.Color);
			pak.WriteByte((byte)template.Flags);
			pak.WriteShort((ushort)template.Effect);
			if (count > 1)
				pak.WritePascalString(String.Format("{0} {1}", count, template.Name));
			else
				pak.WritePascalString(template.Name);
		}

		public virtual void SendXFireInfo(byte flag)
		{
			if (m_gameClient == null || m_gameClient.Player == null)
				return;
			using (GSTCPPacketOut pak = new GSTCPPacketOut((byte)eServerPackets.XFire))
			{
				pak.WriteShort((ushort)m_gameClient.Player.ObjectID);
				pak.WriteByte(flag);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		/*
		 * public override void SendPlayerBanner(GamePlayer player, int GuildEmblem)
		{
			if (player == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VisualEffect));
			pak.WriteShort((ushort) player.ObjectID);
			pak.WriteByte(12);
			if (GuildEmblem == 0)
			{
				pak.WriteByte(1);
			}
			else
			{
				pak.WriteByte(0);
			}
			int newEmblemBitMask = ((GuildEmblem & 0x010000) << 8) | (GuildEmblem & 0xFFFF);
			pak.WriteInt((uint)newEmblemBitMask);
			SendTCP(pak);
		}

		 */
		public virtual void SendDebugMode(bool on)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DebugMode)))
			{
				if (m_gameClient.Account.PrivLevel == 1)
				{
					pak.WriteByte((0x00));
				}
				else
				{
					pak.WriteByte((byte)(on ? 0x01 : 0x00));
				}
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public void SendModelChange(GameObject obj, ushort newModel)
		{
			if (obj is GameNPC)
				SendModelAndSizeChange(obj, newModel, (obj as GameNPC).Size);
			else
				SendModelAndSizeChange(obj, newModel, 0);
		}

		public void SendModelAndSizeChange(GameObject obj, ushort newModel, byte newSize)
		{
			SendModelAndSizeChange((ushort)obj.ObjectID, newModel, newSize);
		}

		public virtual void SendModelAndSizeChange(ushort objectId, ushort newModel, byte newSize)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ModelChange)))
			{
				pak.WriteShort(objectId);
				pak.WriteShort(newModel);
				pak.WriteIntLowEndian(newSize);
				SendTCP(pak);
			}
		}

		public virtual void SendEmoteAnimation(GameObject obj, eEmote emote)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.EmoteAnimation)))
			{
				pak.WriteShort((ushort)obj.ObjectID);
				pak.WriteByte((byte)emote);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendUpdateMoney()
		{
			if (m_gameClient.Player == null)
				return;
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MoneyUpdate)))
			{
				pak.WriteByte((byte)m_gameClient.Player.Copper);
				pak.WriteByte((byte)m_gameClient.Player.Silver);
				pak.WriteShort((ushort)m_gameClient.Player.Gold);
				pak.WriteShort((ushort)m_gameClient.Player.Mithril);
				pak.WriteShort((ushort)m_gameClient.Player.Platinum);
				SendTCP(pak);
			}
		}

		public virtual void SendUpdateMaxSpeed()
		{
			//Speed is in % not a fixed value!
			if (m_gameClient.Player == null)
				return;
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MaxSpeed)))
			{
				// _gameClient.Player.LastMaxSpeed = _gameClient.Player.MaxSpeed; // patch 0024 experimental hackdetect
				pak.WriteShort((ushort)(m_gameClient.Player.MaxSpeed * 100 / GamePlayer.PLAYER_BASE_SPEED));
				pak.WriteByte((byte)(m_gameClient.Player.IsTurningDisabled ? 0x01 : 0x00));
				// water speed in % of land speed if its over 0 i think
				pak.WriteByte(
					(byte)
					Math.Min(byte.MaxValue,
							 ((m_gameClient.Player.MaxSpeed * 100 / GamePlayer.PLAYER_BASE_SPEED) *
							  (m_gameClient.Player.GetModified(eProperty.WaterSpeed) * .01))));
				SendTCP(pak);
			}
		}


		public virtual void SendStatusUpdate()
		{
			if (m_gameClient.Player == null)
				return;
			SendStatusUpdate((byte)(m_gameClient.Player.IsSitting ? 0x02 : 0x00));
		}



		public virtual void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SpellCastAnimation)))
			{
				pak.WriteShort((ushort)spellCaster.ObjectID);
				pak.WriteShort(spellID);
				pak.WriteShort(castingTime);
				pak.WriteShort(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendRiding(GameObject rider, GameObject steed, bool dismount)
		{
			int slot = 0;
			if (steed is GameNPC npc && rider is GamePlayer playerRider && dismount == false)
				slot = npc.RiderSlot(playerRider);
			if (slot == -1)
				log.Error($"SendRiding error, slot is -1 with rider {rider.Name} steed {steed.Name}");
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Riding)))
			{
				pak.WriteShort((ushort)rider.ObjectID);
				pak.WriteShort((ushort)steed.ObjectID);
				pak.WriteByte((byte)(dismount ? 0x00 : 0x01));
				pak.WriteByte((byte)slot);
				pak.WriteShort(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendGroupInviteCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte(0x05);
				pak.WriteShort((ushort)invitingPlayer.Client.SessionID); //data1
				pak.Fill(0x00, 6); //data2&data3
				pak.WriteByte(0x01);
				pak.WriteByte(0x00);
				if (inviteMessage.Length > 0)
					pak.WriteString(inviteMessage, inviteMessage.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendGuildInviteCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte(0x03);
				pak.WriteShort((ushort)invitingPlayer.ObjectID); //data1
				pak.Fill(0x00, 6); //data2&data3
				pak.WriteByte(0x01);
				pak.WriteByte(0x00);
				if (inviteMessage.Length > 0)
					pak.WriteString(inviteMessage, inviteMessage.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendGuildLeaveCommand(GamePlayer invitingPlayer, string inviteMessage)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte(0x08);
				pak.WriteShort((ushort)invitingPlayer.ObjectID); //data1
				pak.Fill(0x00, 6); //data2&data3
				pak.WriteByte(0x01);
				pak.WriteByte(0x00);
				if (inviteMessage.Length > 0)
					pak.WriteString(inviteMessage, inviteMessage.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}






		// i'm reusing the questsubscribe command for quest abort since its 99% the same, only different event dets fired
		// data 3 defines wether it's subscribe or abort
		public virtual void SendQuestSubscribeCommand(GameNPC invitingNPC, ushort questid, string inviteMessage)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte(0x64);
				pak.WriteShort(questid); //questid, data1
				pak.WriteShort((ushort)invitingNPC.ObjectID); //data2
				pak.WriteShort(0x00); // 0x00 means subscribe data3
				pak.WriteShort(0x00);
				pak.WriteByte(0x01); // yes/no response
				pak.WriteByte(0x01); // autowrap message
				if (inviteMessage.Length > 0)
					pak.WriteString(inviteMessage, inviteMessage.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		// i'm reusing the questsubscribe command for quest abort since its 99% the same, only different event dets fired
		// data 3 defines wether it's subscribe or abort
		public virtual void SendQuestAbortCommand(GameNPC abortingNPC, ushort questid, string abortMessage)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte(0x64);
				pak.WriteShort(questid); //questid, data1
				pak.WriteShort((ushort)abortingNPC.ObjectID); //data2
				pak.WriteShort(0x01); // 0x01 means abort data3
				pak.WriteShort(0x00);
				pak.WriteByte(0x01); // yes/no response
				pak.WriteByte(0x01); // autowrap message
				if (abortMessage.Length > 0)
					pak.WriteString(abortMessage, abortMessage.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4,
										  eDialogType type, bool autoWrapText, string message)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte((byte)code);
				pak.WriteShort(data1); //data1
				pak.WriteShort(data2); //data2
				pak.WriteShort(data3); //data3
				pak.WriteShort(data4); //data4
				pak.WriteByte((byte)type);
				pak.WriteByte((byte)(autoWrapText ? 0x01 : 0x00));
				if (message.Length > 0)
					pak.WriteString(message, message.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendCustomDialog(string msg, CustomDialogResponse callback)
		{
			if (m_gameClient.Player == null)
				return;

			lock (m_gameClient.Player)
			{
				if (m_gameClient.Player.CustomDialogCallback != null)
					m_gameClient.Player.CustomDialogCallback(m_gameClient.Player, 0x00);
				m_gameClient.Player.CustomDialogCallback = callback;
			}

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte((byte)eDialogCode.CustomDialog);
				pak.WriteShort((ushort)m_gameClient.SessionID); //data1
				pak.WriteShort(0x01); //custom dialog!	  //data2
				pak.WriteShort(0x00); //data3
				pak.WriteShort(0x00);
				pak.WriteByte((byte)(callback == null ? 0x00 : 0x01)); //ok or yes/no response
				pak.WriteByte(0x01); // autowrap text
				if (msg.Length > 0)
					pak.WriteString(msg, msg.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}
		// patch 0017
		public virtual void SendCustomDialog(string msg, CustomDialogResponse callback, bool customFormat)
		{
			if (m_gameClient.Player == null)
				return;

			lock (m_gameClient.Player)
			{
				if (m_gameClient.Player.CustomDialogCallback != null)
					m_gameClient.Player.CustomDialogCallback(m_gameClient.Player, 0x00);
				m_gameClient.Player.CustomDialogCallback = callback;
			}

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte((byte)eDialogCode.CustomDialog);
				pak.WriteShort((ushort)m_gameClient.SessionID); //data1
				pak.WriteShort(0x01); //custom dialog!	  //data2
				pak.WriteShort(0x00); //data3
				pak.WriteShort(0x00);
				pak.WriteByte((byte)(callback == null ? 0x00 : 0x01)); //ok or yes/no response
				pak.WriteByte((byte)(customFormat ? 0x00 : 0x01)); // autowrap text? false if customFormatted == true. Allows use of \n etc in string
				if (msg.Length > 0)
					pak.WriteString(msg, msg.Length);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		[Obsolete("Shouldn't be used in favor of new LoS Check Manager")]
		public virtual void SendCheckLOS(GameObject Checker, GameObject Target, CheckLOSResponse callback)
		{
			if (m_gameClient.Player == null)
				return;
			int TargetOID = (Target != null ? Target.ObjectID : 0);
			string key = string.Format("LOS C:0x{0} T:0x{1}", Checker.ObjectID, TargetOID);
			CheckLOSResponse old_callback = null;
			lock (m_gameClient.Player.TempProperties)
			{
				old_callback = (CheckLOSResponse)m_gameClient.Player.TempProperties.getProperty<object>(key, null);
				m_gameClient.Player.TempProperties.setProperty(key, callback);
			}
			if (old_callback != null)
				old_callback(m_gameClient.Player, 0, 0); // not sure for this,  i want targetOID there

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CheckLOSRequest)))
			{
				pak.WriteShort((ushort)Checker.ObjectID);
				pak.WriteShort((ushort)TargetOID);
				pak.WriteShort(0x00); // ?
				pak.WriteShort(0x00); // ?
				SendTCP(pak);
			}
		}

		public virtual void SendCheckLOS(GameObject source, GameObject target, CheckLOSMgrResponse callback)
		{
			if (m_gameClient.Player == null)
				return;

			int TargetOID = (target != null ? target.ObjectID : 0);
			int SourceOID = (source != null ? source.ObjectID : 0);

			string key = string.Format("LOSMGR C:0x{0} T:0x{1}", SourceOID, TargetOID);

			CheckLOSMgrResponse old_callback = null;
			lock (m_gameClient.Player.TempProperties)
			{
				old_callback = (CheckLOSMgrResponse)m_gameClient.Player.TempProperties.getProperty<object>(key, null);
				m_gameClient.Player.TempProperties.setProperty(key, callback);
			}
			if (old_callback != null)
				old_callback(m_gameClient.Player, 0, 0, 0);

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.CheckLOSRequest)))
			{
				pak.WriteShort((ushort)SourceOID);
				pak.WriteShort((ushort)TargetOID);
				pak.WriteShort(0x00); // ?
				pak.WriteShort(0x00); // ?
				SendTCP(pak);
			}
		}

		public void SendGroupMemberUpdate(bool updateIcons, bool updateMap, GameLiving living)
		{
			if (m_gameClient.Player?.Group == null)
				return;

			var group = m_gameClient.Player.Group;
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.GroupMemberUpdate)))
			{
				lock (group)
				{
					// make sure group is not modified before update is sent else player index could change _before_ update
					if (living.Group != group)
						return;
					WriteGroupMemberUpdate(pak, updateIcons, updateMap, living);
					pak.WriteByte(0x00);
				}
				SendTCP(pak);
			}
		}

		public void SendGroupMembersUpdate(bool updateIcons, bool updateMap)
		{
			if (m_gameClient.Player?.Group == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.GroupMemberUpdate)))
			{
				foreach (var living in m_gameClient.Player.Group.GetMembersInTheGroup())
					WriteGroupMemberUpdate(pak, updateIcons, updateMap, living);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendInventorySlotsUpdate(ICollection<int> slots)
		{
			// slots contain ints

			if (m_gameClient.Player == null)
				return;

			// clients crash if too long packet is sent
			// so we send big updates in parts
			if (slots == null || slots.Count <= ServerProperties.Properties.MAX_ITEMS_PER_PACKET)
			{
				SendInventorySlotsUpdateRange(slots, 0);
			}
			else
			{
				var updateSlots = new List<int>(ServerProperties.Properties.MAX_ITEMS_PER_PACKET);
				foreach (int slot in slots)
				{
					updateSlots.Add(slot);
					if (updateSlots.Count >= ServerProperties.Properties.MAX_ITEMS_PER_PACKET)
					{
						SendInventorySlotsUpdateRange(updateSlots, 0);
						updateSlots.Clear();
					}
				}
				if (updateSlots.Count > 0)
					SendInventorySlotsUpdateRange(updateSlots, 0);
			}
		}





		public virtual void SendInventoryItemsUpdate(ICollection<InventoryItem> itemsToUpdate)
		{
			SendInventoryItemsUpdate(eInventoryWindowType.Update, itemsToUpdate);
		}

		public virtual void SendInventoryItemsUpdate(eInventoryWindowType windowType, ICollection<InventoryItem> itemsToUpdate)
		{
			if (m_gameClient.Player == null)
				return;
			if (itemsToUpdate == null)
			{
				SendInventorySlotsUpdateRange(null, windowType);
				return;
			}

			// clients crash if too long packet is sent
			// so we send big updates in parts
			var slotsToUpdate = new List<int>(Math.Min(ServerProperties.Properties.MAX_ITEMS_PER_PACKET, itemsToUpdate.Count));
			foreach (InventoryItem item in itemsToUpdate)
			{
				if (item == null)
					continue;

				slotsToUpdate.Add(item.SlotPosition);
				if (slotsToUpdate.Count >= ServerProperties.Properties.MAX_ITEMS_PER_PACKET)
				{
					SendInventorySlotsUpdateRange(slotsToUpdate, windowType);
					slotsToUpdate.Clear();
					windowType = eInventoryWindowType.Update;
				}
			}
			if (slotsToUpdate.Count > 0)
			{
				SendInventorySlotsUpdateRange(slotsToUpdate, windowType);
			}
		}

		public virtual void SendDoorState(Region region, IDoor door)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DoorState)))
			{
				ushort zone = (ushort)(door.DoorID / 1000000);
				int doorType = door.DoorID / 100000000;
				uint flag = door.Flag;

				// by default give all unflagged above ground non keep doors a default sound (excluding TrialsOfAtlantis zones)
				if (flag == 0 && doorType != 7 && region != null && !region.IsDungeon && region.Expansion != (int)eClientExpansion.TrialsOfAtlantis)
				{
					flag = 1;
				}

				pak.WriteInt((uint)door.DoorID);
				pak.WriteByte((byte)(door.State == eDoorState.Open ? 0x01 : 0x00));
				pak.WriteByte((byte)flag);
				pak.WriteByte(0xFF);
				pak.WriteByte(0x0);
				SendTCP(pak);
			}
		}

		public virtual void SendMerchantWindow(MerchantTradeItems tradeItemsList, eMerchantWindowType windowType)
		{

			if (tradeItemsList != null)
			{
				for (byte page = 0; page < MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS; page++)
				{
					IDictionary itemsInPage = tradeItemsList.GetItemsInPage((int)page);
					if (itemsInPage == null || itemsInPage.Count == 0)
						continue;

					using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MerchantWindow)))
					{
						pak.WriteByte((byte)itemsInPage.Count); //Item count on this page
						pak.WriteByte((byte)windowType);
						pak.WriteByte((byte)page); //Page number
						pak.WriteByte(0x00); //Unused

						for (ushort i = 0; i < MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS; i++)
						{
							if (!itemsInPage.Contains((int)i))
								continue;

							var item = (ItemTemplate)itemsInPage[(int)i];
							if (item != null)
							{
								pak.WriteByte((byte)i); //Item index on page
								pak.WriteByte((byte)item.Level);
								// some objects use this for count
								int value1;
								int value2;
								switch (item.Object_Type)
								{
									case (int)eObjectType.Arrow:
									case (int)eObjectType.Bolt:
									case (int)eObjectType.Poison:
									case (int)eObjectType.GenericItem:
										{
											value1 = item.PackSize;
											value2 = value1 * item.Weight;
											break;
										}
									case (int)eObjectType.Thrown:
										{
											value1 = item.DPS_AF;
											value2 = item.PackSize;
											break;
										}
									case (int)eObjectType.Shield:
										{
											value1 = item.Type_Damage;
											value2 = item.Weight;
											break;
										}
									case (int)eObjectType.GardenObject:
										{
											value1 = 0;
											value2 = item.Weight;
											break;
										}
									default:
										{
											value1 = item.DPS_AF;
											value2 = item.Weight;
											break;
										}
								}
								pak.WriteByte((byte)value1);
								pak.WriteByte((byte)item.SPD_ABS);
								if (item.Object_Type == (int)eObjectType.GardenObject)
									pak.WriteByte((byte)(item.DPS_AF));
								else
									pak.WriteByte((byte)(item.Hand << 6));
								pak.WriteByte((byte)((item.Type_Damage << 6) | item.Object_Type));
								//1 if item cannot be used by your class (greyed out)
								if (m_gameClient.Player != null && m_gameClient.Player.HasAbilityToUseItem(item))
									pak.WriteByte(0x00);
								else
									pak.WriteByte(0x01);
								pak.WriteShort((ushort)value2);
								//Item Price
								pak.WriteInt((uint)item.Price);
								pak.WriteShort((ushort)item.Model);
								pak.WritePascalString(item.Name);
							}
							else
							{
								if (log.IsErrorEnabled)
									log.Error("Merchant item template '" +
											  ((MerchantItem)itemsInPage[page * MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS + i]).ItemTemplateID +
											  "' not found, abort!!!");
								return;
							}
						}
						SendTCP(pak);
					}
				}
			}
			else
			{
				using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MerchantWindow)))
				{
					pak.WriteByte(0); //Item count on this page
					pak.WriteByte((byte)windowType); //Unknown 0x00
					pak.WriteByte(0); //Page number
					pak.WriteByte(0x00); //Unused
					SendTCP(pak);
				}
			}
		}



		public virtual void SendCloseTradeWindow()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TradeWindow)))
			{
				pak.Fill(0x00, 40);
				SendTCP(pak);
			}
		}

		public virtual void SendPlayerDied(GamePlayer killedPlayer, GameObject killer)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerDeath)))
			{
				pak.WriteShort((ushort)killedPlayer.ObjectID);
				if (killer != null)
					pak.WriteShort((ushort)killer.ObjectID);
				else
					pak.WriteShort(0x00);
				pak.Fill(0x0, 4);
				SendTCP(pak);
			}
		}

		public virtual void SendPlayerRevive(GamePlayer revivedPlayer)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerRevive)))
			{
				pak.WriteShort((ushort)revivedPlayer.ObjectID);
				pak.WriteShort(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendUpdateCraftingSkills()
		{
			if (m_gameClient.Player == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak.WriteByte(0x08); //subcode
				pak.WriteByte((byte)m_gameClient.Player.CraftingSkills.Count); //count
				pak.WriteByte(0x03); //subtype
				pak.WriteByte(0x00); //unk

				foreach (KeyValuePair<eCraftingSkill, int> de in m_gameClient.Player.CraftingSkills)
				{
					AbstractCraftingSkill curentCraftingSkill = CraftingMgr.getSkillbyEnum((eCraftingSkill)de.Key);
					pak.WriteShort(Convert.ToUInt16(de.Value)); //points
					pak.WriteByte(curentCraftingSkill.Icon); //icon
					pak.WriteInt(1);
					pak.WritePascalString(curentCraftingSkill.Name); //name
				}
				SendTCP(pak);
			}
		}

		public virtual void SendUpdateWeaponAndArmorStats()
		{
			if (m_gameClient.Player == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak.WriteByte(0x05); //subcode
				pak.WriteByte(6); //number of entries
				pak.WriteByte(0x00); //subtype
				pak.WriteByte(0x00); //unk

				// weapondamage
				var wd = (int)(m_gameClient.Player.WeaponDamage(m_gameClient.Player.AttackWeapon) * 100.0);
				pak.WriteByte((byte)(wd / 100));
				pak.WritePascalString(" ");
				pak.WriteByte((byte)(wd % 100));
				pak.WritePascalString(" ");
				// weaponskill
				int ws = m_gameClient.Player.DisplayedWeaponSkill;
				pak.WriteByte((byte)(ws >> 8));
				pak.WritePascalString(" ");
				pak.WriteByte((byte)(ws & 0xff));
				pak.WritePascalString(" ");
				// overall EAF
				int eaf = m_gameClient.Player.EffectiveOverallAF;
				pak.WriteByte((byte)(eaf >> 8));
				pak.WritePascalString(" ");
				pak.WriteByte((byte)(eaf & 0xff));
				pak.WritePascalString(" ");

				SendTCP(pak);
			}
		}

		public virtual void SendEncumberance()
		{
			if (m_gameClient.Player == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Encumberance)))
			{
				pak.WriteShort((ushort)m_gameClient.Player.MaxEncumberance); // encumb total
				pak.WriteShort((ushort)m_gameClient.Player.Encumberance); // encumb used
				SendTCP(pak);
			}
		}


		public virtual void SendAddFriends(string[] friendNames)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.AddFriend)))
			{
				foreach (string friend in friendNames)
					pak.WritePascalString(friend);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendRemoveFriends(string[] friendNames)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.RemoveFriend)))
			{
				foreach (string friend in friendNames)
					pak.WritePascalString(friend);
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}

		public virtual void SendTimerWindow(string title, int seconds)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TimerWindow)))
			{
				pak.WriteShort((ushort)seconds);
				pak.WriteByte((byte)title.Length);
				pak.WriteByte(1);
				pak.WriteString((title.Length > byte.MaxValue ? title.Substring(0, byte.MaxValue) : title));
				SendTCP(pak);
			}
		}

		public virtual void SendCloseTimerWindow()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TimerWindow)))
			{
				pak.WriteShort(0);
				pak.WriteByte(0);
				pak.WriteByte(0);
				SendTCP(pak);
			}
		}

		public virtual void SendCustomTrainerWindow(int type, List<Tuple<Specialization, List<Tuple<Skill, byte>>>> tree)
		{
			if (m_gameClient.Player == null)
				return;

			GamePlayer player = m_gameClient.Player;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				if (tree != null && tree.Count > 0)
				{
					pak.WriteByte((byte)type); // index for Champion Line ID (returned for training request)
					pak.WriteByte((byte)0); // Spec points available for this player.
					pak.WriteByte(2); // Champion Window Type
					pak.WriteByte(0);
					pak.WriteByte((byte)tree.Count); // Count of sublines

					for (int skillIndex = 0; skillIndex < tree.Count; skillIndex++)
					{
						pak.WriteByte((byte)(skillIndex + 1));
						pak.WriteByte((byte)tree[skillIndex].Item2.Where(t => t.Item1 != null).Count()); // Count of item for this line

						for (int itemIndex = 0; itemIndex < tree[skillIndex].Item2.Count; itemIndex++)
						{
							Skill sk = tree[skillIndex].Item2[itemIndex].Item1;

							if (sk != null)
							{
								pak.WriteByte((byte)(itemIndex + 1));

								if (sk is Style)
								{
									pak.WriteByte(2);
								}
								else if (sk is Spell)
								{
									pak.WriteByte(3);
								}
								else
								{
									pak.WriteByte(4);
								}

								pak.WriteShortLowEndian(sk.Icon); // Icon should be style icon + 3352 ???
								pak.WritePascalString(sk.Name);

								// Skill Status
								pak.WriteByte(1); // 0 = disable, 1 = trained, 2 = can train

								// Attached Skill
								if (tree[skillIndex].Item2[itemIndex].Item2 == 2)
								{
									pak.WriteByte(2); // count of attached skills
									pak.WriteByte((byte)(skillIndex << 8 + itemIndex));
									pak.WriteByte((byte)((skillIndex + 2) << 8 + itemIndex));
								}
								else if (tree[skillIndex].Item2[itemIndex].Item2 == 3)
								{
									pak.WriteByte(3); // count of attached skills
									pak.WriteByte((byte)(skillIndex << 8 + itemIndex));
									pak.WriteByte((byte)((skillIndex + 1) << 8 + itemIndex));
									pak.WriteByte((byte)((skillIndex + 2) << 8 + itemIndex));
								}
								else
								{
									// doesn't support other count
									pak.WriteByte(0);
								}
							}
						}
					}

					SendTCP(pak);
				}
			}
		}


		public virtual void SendChampionTrainerWindow(int type)
		{
			if (m_gameClient.Player == null)
				return;

			GamePlayer player = m_gameClient.Player;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.TrainerWindow)))
			{
				// Get Player CL Spec
				var clspec = player.GetSpecList().Where(sp => sp is LiveChampionsSpecialization).Cast<LiveChampionsSpecialization>().FirstOrDefault();

				// check if the tree can be used
				List<Tuple<MiniLineSpecialization, List<Tuple<Skill, byte>>>> tree = null;
				if (clspec != null)
				{
					tree = clspec.GetTrainerTreeDisplay(player, clspec.RetrieveTypeForIndex(type));
				}

				if (tree != null && tree.Count > 0)
				{
					pak.WriteByte((byte)type); // index for Champion Line ID (returned for training request)
					pak.WriteByte((byte)player.ChampionSpecialtyPoints); // Spec points available for this player.
					pak.WriteByte(2); // Champion Window Type
					pak.WriteByte(0);
					pak.WriteByte((byte)tree.Count); // Count of sublines

					for (int skillIndex = 0; skillIndex < tree.Count; skillIndex++)
					{
						pak.WriteByte((byte)(skillIndex + 1));
						pak.WriteByte((byte)tree[skillIndex].Item2.Where(t => t.Item1 != null).Count()); // Count of item for this line

						for (int itemIndex = 0; itemIndex < tree[skillIndex].Item2.Count; itemIndex++)
						{
							Skill sk = tree[skillIndex].Item2[itemIndex].Item1;

							if (sk != null)
							{
								pak.WriteByte((byte)(itemIndex + 1));

								if (sk is Style)
								{
									pak.WriteByte(2);
								}
								else if (sk is Spell)
								{
									pak.WriteByte(3);
								}
								else
								{
									pak.WriteByte(1);
								}

								pak.WriteShortLowEndian(sk.Icon); // Icon should be style icon + 3352 ???
								pak.WritePascalString(sk.Name);

								// Skill Status
								pak.WriteByte(clspec.GetSkillStatus(tree, skillIndex, itemIndex).Item1); // 0 = disable, 1 = trained, 2 = can train

								// Attached Skill
								if (tree[skillIndex].Item2[itemIndex].Item2 == 2)
								{
									pak.WriteByte(2); // count of attached skills
									pak.WriteByte((byte)(skillIndex << 8 + itemIndex));
									pak.WriteByte((byte)((skillIndex + 2) << 8 + itemIndex));
								}
								else if (tree[skillIndex].Item2[itemIndex].Item2 == 3)
								{
									pak.WriteByte(3); // count of attached skills
									pak.WriteByte((byte)(skillIndex << 8 + itemIndex));
									pak.WriteByte((byte)((skillIndex + 1) << 8 + itemIndex));
									pak.WriteByte((byte)((skillIndex + 2) << 8 + itemIndex));
								}
								else
								{
									// doesn't support other count
									pak.WriteByte(0);
								}
							}
						}
					}

					SendTCP(pak);
				}
			}
		}



		public virtual void SendInterruptAnimation(GameLiving living)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.InterruptSpellCast)))
			{
				pak.WriteShort((ushort)living.ObjectID);
				pak.WriteShort(1);
				SendTCP(pak);
			}
		}

		public virtual void SendDisableSkill(ICollection<Tuple<Skill, int>> skills)
		{
			if (m_gameClient.Player == null)
				return;

			var disabledSpells = new List<Tuple<byte, byte, ushort>>();
			var disabledSkills = new List<Tuple<ushort, ushort>>();

			var listspells = m_gameClient.Player.GetAllUsableListSpells();
			var listskills = m_gameClient.Player.GetAllUsableSkills();
			int specCount = listskills.Where(sk => sk.Item1 is Specialization).Count();

			// Get through all disabled skills
			foreach (Tuple<Skill, int> disabled in skills)
			{

				// Check if spell
				byte lsIndex = 0;
				foreach (var ls in listspells)
				{
					int index = ls.Item2.FindIndex(sk => sk.SkillType == disabled.Item1.SkillType && sk.ID == disabled.Item1.ID);

					if (index > -1)
					{
						disabledSpells.Add(new Tuple<byte, byte, ushort>(lsIndex, (byte)index, (ushort)(disabled.Item2 > 0 ? disabled.Item2 / 1000 + 1 : 0)));
						break;
					}

					lsIndex++;
				}

				int skIndex = listskills.FindIndex(skt => disabled.Item1.SkillType == skt.Item1.SkillType && disabled.Item1.ID == skt.Item1.ID) - specCount;

				if (skIndex > -1)
					disabledSkills.Add(new Tuple<ushort, ushort>((ushort)skIndex, (ushort)(disabled.Item2 > 0 ? disabled.Item2 / 1000 + 1 : 0)));
			}

			if (disabledSkills.Count > 0)
			{
				// Send matching hybrid spell match
				using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DisableSkills)))
				{
					byte countskill = (byte)Math.Min(disabledSkills.Count, 255);
					if (countskill > 0)
					{
						pak.WriteShort(0); // duration unused
						pak.WriteByte(countskill); // count...
						pak.WriteByte(1); // code for hybrid skill

						for (int i = 0; i < countskill; i++)
						{
							pak.WriteShort(disabledSkills[i].Item1); // index
							pak.WriteShort(disabledSkills[i].Item2); // duration
						}

						SendTCP(pak);
					}
				}
			}

			if (disabledSpells.Count > 0)
			{
				var groupedDuration = disabledSpells.GroupBy(sp => sp.Item3);
				foreach (var groups in groupedDuration)
				{
					// Send matching list spell match
					using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.DisableSkills)))
					{
						byte total = (byte)Math.Min(groups.Count(), 255);
						if (total > 0)
						{
							pak.WriteShort(groups.Key); // duration
							pak.WriteByte(total); // count...
							pak.WriteByte(2); // code for list spells

							for (int i = 0; i < total; i++)
							{
								pak.WriteByte(groups.ElementAt(i).Item1); // line index
								pak.WriteByte(groups.ElementAt(i).Item2); // spell index
							}

							SendTCP(pak);
						}
					}
				}
			}
		}



		public virtual void SendLevelUpSound()
		{
			// not sure what package this is, but it triggers the mob color update
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.RegionSound)))
			{
				pak.WriteShort((ushort)m_gameClient.Player.ObjectID);
				pak.WriteByte(1); //level up sounds
				pak.WriteByte((byte)m_gameClient.Player.Realm);
				SendTCP(pak);
			}
		}

		public virtual void SendRegionEnterSound(byte soundId)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.RegionSound)))
			{
				pak.WriteShort((ushort)m_gameClient.Player.ObjectID);
				pak.WriteByte(2); //region enter sounds
				pak.WriteByte(soundId);
				SendTCP(pak);
			}
		}

		public virtual void SendDebugMessage(string format, params object[] parameters)
		{
			if (m_gameClient.Account.PrivLevel > (int)ePrivLevel.Player || ServerProperties.Properties.ENABLE_DEBUG)
				SendMessage(String.Format("[DEBUG] " + format, parameters), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public virtual void SendDebugPopupMessage(string format, params object[] parameters)
		{
			if (m_gameClient.Account.PrivLevel > (int)ePrivLevel.Player || ServerProperties.Properties.ENABLE_DEBUG)
				SendMessage(String.Format("[DEBUG] " + format, parameters), eChatType.CT_System, eChatLoc.CL_PopupWindow);
		}

		public virtual void SendEmblemDialogue()
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.EmblemDialogue)))
			{
				pak.Fill(0x00, 4);
				SendTCP(pak);
			}
		}

		//FOR GM to test param and see min and max of each param
		public virtual void SendWeather(uint x, uint width, ushort speed, ushort fogdiffusion, ushort intensity)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Weather)))
			{
				pak.WriteInt(x);
				pak.WriteInt(width);
				pak.WriteShort(fogdiffusion);
				pak.WriteShort(speed);
				pak.WriteShort(intensity);
				pak.WriteShort(0); // 0x0508, 0xEB51, 0xFFBF
				SendTCP(pak);
			}
		}

		public virtual void SendPlayerModelTypeChange(GamePlayer player, byte modelType)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerModelTypeChange)))
			{
				pak.WriteShort((ushort)player.ObjectID);
				pak.WriteByte(modelType);
				pak.WriteByte((byte)(modelType == 3 ? 0x08 : 0x00)); //unused?
				SendTCP(pak);
			}
		}

		public virtual void SendObjectDelete(GameObject obj)
		{
			// Remove from Cache
			if (m_gameClient.GameObjectUpdateArray.ContainsKey(new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)))
			{
				long dummy;
				m_gameClient.GameObjectUpdateArray.TryRemove(new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID), out dummy);
			}

			SendObjectDelete((ushort)obj.ObjectID);
		}

		public void SendObjectDelete(ushort oid)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ObjectDelete)))
			{
				pak.WriteShort(oid);
				pak.WriteShort(1); //TODO: unknown
				SendTCP(pak);
			}
		}

		public void SendChangeTarget(GameObject newTarget)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ChangeTarget)))
			{
				pak.WriteShort((ushort)(newTarget == null ? 0 : newTarget.ObjectID));
				pak.WriteShort(0); // unknown
				SendTCP(pak);
			}
		}

		public void SendChangeGroundTarget(Point3D newTarget)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ChangeGroundTarget)))
			{
				pak.WriteInt((uint)(newTarget == null ? 0 : newTarget.X));
				pak.WriteInt((uint)(newTarget == null ? 0 : newTarget.Y));
				pak.WriteInt((uint)(newTarget == null ? 0 : newTarget.Z));
				SendTCP(pak);
			}
		}



		public virtual void SendRemoveHouse(House house)
		{
			// Remove from cache
			if (m_gameClient.HouseUpdateArray.ContainsKey(new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber)))
			{
				long dummy;
				m_gameClient.HouseUpdateArray.TryRemove(new Tuple<ushort, ushort>(house.RegionID, (ushort)house.HouseNumber), out dummy);
			}

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseCreate)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteShort((ushort)house.Z);
				pak.WriteInt((uint)house.X);
				pak.WriteInt((uint)house.Y);
				pak.Fill(0x00, 15);
				pak.WriteByte(0x03);
				pak.WritePascalString("");

				SendTCP(pak);
			}
		}

		public virtual void SendHousePayRentDialog(string title)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.Dialog)))
			{
				pak.WriteByte(0x00);
				pak.WriteByte((byte)eDialogCode.HousePayRent);
				pak.Fill(0x00, 8); // empty
				pak.WriteByte(0x02); // type
				pak.WriteByte(0x01); // wrap
				if (title.Length > 0)
					pak.WriteString(title); // title ??
				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}




		public virtual void SendExitHouse(House house, ushort unknown = 0)
		{
			// do not send anything if client is leaving house due to linkdeath
			if (m_gameClient != null && m_gameClient.Player != null && m_gameClient.ClientState != GameClient.eClientState.Linkdead)
			{
				using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseExit)))
				{
					pak.WriteShort((ushort)house.HouseNumber);
					pak.WriteShort(unknown);
					SendTCP(pak);
				}
			}
		}

		public virtual void SendToggleHousePoints(House house)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseTogglePoints)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteByte(0x04);
				pak.WriteByte(0x00);

				SendTCP(pak);
			}
		}

		public virtual void SendHouseUsersPermissions(House house)
		{
			if (house == null)
				return;

			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseUserPermissions)))
			{
				pak.WriteByte((byte)house.HousePermissions.Count()); // number of permissions
				pak.WriteByte(0x00); // ?
				pak.WriteShort((ushort)house.HouseNumber); // house number

				foreach (var entry in house.HousePermissions)
				{
					// grab permission
					var perm = entry.Value;

					pak.WriteByte((byte)entry.Key); // Slot
					pak.WriteByte(0x00); // ?
					pak.WriteByte(0x00); // ?
					pak.WriteByte((byte)perm.PermissionType); // Type (Guild, Class, Race ...)
					pak.WriteByte((byte)perm.PermissionLevel); // Level (Friend, Visitor ...)
					pak.WritePascalString(perm.DisplayName);
				}

				SendTCP(pak);
			}
		}

		public virtual void SendFurniture(House house)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HousingItem)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteByte((byte)house.IndoorItems.Count);
				pak.WriteByte(0x80); //0x00 = update, 0x80 = complete package

				foreach (var entry in house.IndoorItems.OrderBy(entry => entry.Key))
				{
					var item = entry.Value;
					WriteHouseFurniture(pak, item, entry.Key);
				}

				SendTCP(pak);
			}
		}

		public virtual void SendFurniture(House house, int i)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HousingItem)))
			{
				pak.WriteShort((ushort)house.HouseNumber);
				pak.WriteByte(0x01); //cnt
				pak.WriteByte(0x00); //upd
				var item = (IndoorItem)house.IndoorItems[i];
				WriteHouseFurniture(pak, item, i);
				SendTCP(pak);
			}
		}

		public virtual void SendRentReminder(House house)
		{
			//0:00:58.047 S=>C 0xF7 show help window (topicIndex:106 houseLot?:4281)
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HelpWindow)))
			{
				pak.WriteShort(106); //short index
				pak.WriteShort((ushort)house.HouseNumber); //short lot
				SendTCP(pak);
			}
		}

		public virtual void SendStarterHelp()
		{
			//* 0:00:57.984 S=>C 0xF7 show help window (topicIndex:1 houseLot?:0)
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HelpWindow)))
			{
				pak.WriteShort(1); //short index
				pak.WriteShort(0); //short lot
				SendTCP(pak);
			}
		}

		public virtual void SendPlaySound(eSoundType soundType, ushort soundID)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlaySound)))
			{
				pak.WriteShort((ushort)soundType);
				pak.WriteShort(soundID);
				pak.Fill(0x00, 8);
				SendTCP(pak);
			}
		}

		public virtual void SendMovingObjectCreate(GameMovingObject obj)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.MovingObjectCreate)))
			{
				pak.WriteShort((ushort)obj.ObjectID);
				pak.WriteShort(0);
				pak.WriteShort(obj.Heading);
				pak.WriteShort((ushort)obj.Z);
				pak.WriteInt((uint)obj.X);
				pak.WriteInt((uint)obj.Y);
				pak.WriteShort(obj.Model);
				int flag = (obj.Type() | ((byte)obj.Realm == 3 ? 0x40 : (byte)obj.Realm << 4) | obj.GetDisplayLevel(m_gameClient.Player) << 9);
				pak.WriteShort((ushort)flag); //(0x0002-for Ship,0x7D42-for catapult,0x9602,0x9612,0x9622-for ballista)
				pak.WriteShort(obj.Emblem); //emblem
				pak.WriteShort(0);
				pak.WriteInt(0);

				string name = obj.Name;

				LanguageDataObject translation = LanguageMgr.GetTranslation(m_gameClient, obj);
				if (translation != null)
				{
					if (!Util.IsEmpty(((DBLanguageNPC)translation).Name))
						name = ((DBLanguageNPC)translation).Name;
				}

				pak.WritePascalString(name);/*pak.WritePascalString(obj.Name);*/
				pak.WriteByte(0); // trailing ?
				SendTCP(pak);
			}

			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)] = GameTimer.GetTickCount();

		}



		public virtual void SendSoundEffect(ushort soundId, ushort zoneId, ushort x, ushort y, ushort z, ushort radius)
		{
			using (var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.SoundEffect)))
			{
				pak.WriteShort(soundId);
				pak.WriteShort(zoneId);
				pak.WriteShort(x);
				pak.WriteShort(y);
				pak.WriteShort(z);
				pak.WriteShort(radius);
				SendTCP(pak);
			}
		}

		public virtual void SendRegionColorScheme()
		{
			SendRegionColorScheme(GameServer.ServerRules.GetColorHandling(m_gameClient));
		}

		public virtual void SendNPCCreate(GameNPC npc, bool updateCache)
		{
			if (m_gameClient.Player == null || npc.IsVisibleTo(m_gameClient.Player) == false)
				return;

			//Added by Suncheck - Mines are not shown to enemy players
			/* if (npc is GameMine) patch 0048
            {
                if (GameServer.ServerRules.IsAllowedToAttack((npc as GameMine).Owner, _gameClient.Player, true))
                {
                    return;
                }
            }*/

			if (npc is GameMovingObject)
			{
				SendMovingObjectCreate(npc as GameMovingObject);
				return;
			}

			using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.NPCCreate)))
			{
				int speed = 0;
				ushort speedZ = 0;
				if (npc == null)
					return;
				if (!npc.IsAtTargetPosition)
				{
					speed = npc.CurrentSpeed;
					speedZ = (ushort)npc.TickSpeedZ;
				}
				pak.WriteShort((ushort)npc.ObjectID);
				pak.WriteShort((ushort)(speed));
				pak.WriteShort(npc.Heading);
				pak.WriteShort((ushort)npc.Z);
				pak.WriteInt((uint)npc.X);
				pak.WriteInt((uint)npc.Y);
				pak.WriteShort(speedZ);
				pak.WriteShort(npc.Model);
				pak.WriteByte(npc.Size);
				byte level = npc.GetDisplayLevel(m_gameClient.Player);
				if ((npc.Flags & GameNPC.eFlags.STATUE) != 0)
				{
					level |= 0x80;
				}
				pak.WriteByte(level);

				byte flags = (byte)(GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, npc) << 6);
				if ((npc.Flags & GameNPC.eFlags.GHOST) != 0) flags |= 0x01;
				if (npc.Inventory != null) flags |= 0x02; //If mob has equipment, then only show it after the client gets the 0xBD packet
				if ((npc.Flags & GameNPC.eFlags.PEACE) != 0) flags |= 0x10;
				if ((npc.Flags & GameNPC.eFlags.FLYING) != 0) flags |= 0x20;
				if ((npc.Flags & GameNPC.eFlags.TORCH) != 0) flags |= 0x04;

				pak.WriteByte(flags);
				pak.WriteByte(0x20); //TODO this is the default maxstick distance

				string add = "";
				byte flags2 = 0x00;
				IControlledBrain brain = npc.Brain as IControlledBrain;
				//patch 0042
				if (brain != null)
				{
					flags2 |= 0x80; // have Owner
				}

				if ((npc.Flags & GameNPC.eFlags.CANTTARGET) != 0)
					if (m_gameClient.Account.PrivLevel > 1) add += "-DOR"; // indicates DOR flag for GMs
					else flags2 |= 0x01;
				if ((npc.Flags & GameNPC.eFlags.DONTSHOWNAME) != 0)
					if (m_gameClient.Account.PrivLevel > 1) add += "-NON"; // indicates NON flag for GMs
					else flags2 |= 0x02;

				if ((npc.Flags & GameNPC.eFlags.STEALTH) > 0)
					flags2 |= 0x04;

				eQuestIndicator questIndicator = npc.GetQuestIndicator(m_gameClient.Player);

				if (questIndicator == eQuestIndicator.Available)
					flags2 |= 0x08;//hex 8 - quest available
				if (questIndicator == eQuestIndicator.Finish)
					flags2 |= 0x10;//hex 16 - quest finish
								   //flags2 |= 0x20;//hex 32 - water mob?
								   //flags2 |= 0x40;//hex 64 - unknown
								   //flags2 |= 0x80;//hex 128 - has owner


				pak.WriteByte(flags2); // flags 2

				byte flags3 = 0x00;
				if (questIndicator == eQuestIndicator.Lesson)
					flags3 |= 0x01;
				if (questIndicator == eQuestIndicator.Lore)
					flags3 |= 0x02;
				if (questIndicator == eQuestIndicator.Pending) // new? patch 0031
					flags3 |= 0x20;
				pak.WriteByte(flags3); // new in 1.71 (region instance ID from StoC_0x20) OR flags 3?
				pak.WriteShort(0x00); // new in 1.71 unknown

				string name = npc.Name;
				string guildName = npc.GuildName;

				LanguageDataObject translation = LanguageMgr.GetTranslation(m_gameClient, npc);
				if (translation != null)
				{
					if (!Util.IsEmpty(((DBLanguageNPC)translation).Name))
						name = ((DBLanguageNPC)translation).Name;

					if (!Util.IsEmpty(((DBLanguageNPC)translation).GuildName))
						guildName = ((DBLanguageNPC)translation).GuildName;
				}

				if (name.Length + add.Length + 2 > 47) // clients crash with too long names
					name = name.Substring(0, 47 - add.Length - 2);
				if (add.Length > 0)
					name = string.Format("[{0}]{1}", name, add);

				pak.WritePascalString(name);

				if (guildName.Length > 47)
					pak.WritePascalString(guildName.Substring(0, 47));
				else pak.WritePascalString(guildName);

				pak.WriteByte(0x00);
				SendTCP(pak);
			}
		}
	}
}
