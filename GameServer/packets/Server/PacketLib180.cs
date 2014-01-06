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
using System.Reflection;

using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.GS.Effects;

using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(180, GameClient.eClientVersion.Version180)]
	public class PacketLib180 : PacketLib179
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.80 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib180(GameClient client):base(client)
		{
		}

		public override void SendSetControlledHorse(GamePlayer player)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ControlledHorse));

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

		public override void SendControlledHorse(GamePlayer player, bool flag)
		{
			if (player == null || player.ObjectState != GameObject.eObjectState.Active)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ControlledHorse));
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

		public override void SendPlayerCreate(GamePlayer playerToCreate)
		{
			if (playerToCreate == null)
			{
				if (log.IsErrorEnabled)
					log.Error("SendPlayerCreate: playerToCreate == null");
				return;
			}

			//hide stealthed GM's
			if (playerToCreate.Client.Account.PrivLevel >= (uint)ePrivLevel.GM && playerToCreate.IsStealthed && m_gameClient.Account.PrivLevel == (uint)ePrivLevel.Player)
				return;

			if (m_gameClient.Player == null)
			{
				if (log.IsErrorEnabled)
					log.Error("SendPlayerCreate: m_gameClient.Player == null");
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

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.PlayerCreate172));

			pak.WriteShort((ushort)playerToCreate.Client.SessionID);
			pak.WriteShort((ushort)playerToCreate.ObjectID);
			pak.WriteShort(playerToCreate.Model);
			pak.WriteShort((ushort)playerToCreate.Z);
			//Dinberg:Instances - send out the 'fake' zone ID to the client for positioning purposes.
			pak.WriteShort(playerZone.ZoneSkinID);
			pak.WriteShort((ushort)playerRegion.GetXOffInZone(playerToCreate.X, playerToCreate.Y));
			pak.WriteShort((ushort)playerRegion.GetYOffInZone(playerToCreate.X, playerToCreate.Y));
			pak.WriteShort(playerToCreate.Heading);

			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeSize)); //1-4 = Eye Size / 5-8 = Nose Size
			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.LipSize)); //1-4 = Ear size / 5-8 = Kin size
			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.MoodType)); //1-4 = Ear size / 5-8 = Kin size
			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.EyeColor)); //1-4 = Skin Color / 5-8 = Eye Color
			pak.WriteByte(playerToCreate.GetDisplayLevel(m_gameClient.Player));
			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairColor)); //Hair: 1-4 = Color / 5-8 = unknown
			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.FaceType)); //1-4 = Unknown / 5-8 = Face type
			pak.WriteByte(playerToCreate.GetFaceAttribute(eCharFacePart.HairStyle)); //1-4 = Unknown / 5-8 = Hair Style

			int flags = (GameServer.ServerRules.GetLivingRealm(m_gameClient.Player, playerToCreate) & 0x03) << 2;
			if (playerToCreate.IsAlive == false) flags |= 0x01;
			if (playerToCreate.IsUnderwater) flags |= 0x02; //swimming
			if (playerToCreate.IsStealthed) flags |= 0x10;
			if (playerToCreate.IsWireframe) flags |= 0x20;
			if (playerToCreate.CharacterClass.ID == (int)eCharacterClass.Vampiir) flags |= 0x40; //Vamp fly
			pak.WriteByte((byte)flags);
			pak.WriteByte(0x00); // new in 1.74

			pak.WritePascalString(GameServer.ServerRules.GetPlayerName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerGuildName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(GameServer.ServerRules.GetPlayerLastName(m_gameClient.Player, playerToCreate));
			//RR 12 / 13
			pak.WritePascalString(GameServer.ServerRules.GetPlayerPrefixName(m_gameClient.Player, playerToCreate));
			pak.WritePascalString(playerToCreate.CurrentTitle.GetValue(playerToCreate)); // new in 1.74, NewTitle
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
			SendObjectGuildID(playerToCreate, playerToCreate.Guild); //used for nearest friendly/enemy object buttons and name colors on PvP server

			if (playerToCreate.GuildBanner != null)
			{
				SendRvRGuildBanner(playerToCreate, true);
			}
		}

		public override void CheckLengthHybridSkillsPacket(ref GSTCPPacketOut pak, ref int maxSkills, ref int first)
		{
			if(pak.Length > 1500)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - first));
				pak.WriteByte( (byte)( first == 0 ? 99 : 0x03 ) ); //subtype
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

		public override void SendUpdatePlayerSkills()
		{
			if (m_gameClient.Player == null)
				return;
			IList specs = m_gameClient.Player.GetSpecList();
			IList skills = m_gameClient.Player.GetNonTrainableSkillList();
			IList styleList = m_gameClient.Player.GetStyleList();
			List<SpellLine> spellLines = m_gameClient.Player.GetSpellLines();
			Hashtable styleTable = new Hashtable();
			int maxSkills = 0;
			int firstSkills = 0;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate));
			bool sendHybridList = m_gameClient.Player.CharacterClass.ClassType != eClassType.ListCaster;

			List<Skill> cachedSkills = new List<Skill>();

			lock (skills.SyncRoot)
			{
				lock (styleList.SyncRoot)
				{
					lock (specs.SyncRoot)
					{
						lock (m_gameClient.Player.lockSpellLinesList)
						{
							int skillCount = specs.Count + skills.Count + styleList.Count;// +masterLevelSpells.Count;

							if (sendHybridList)
								skillCount += m_gameClient.Player.GetSpellCount();

							pak.WriteByte(0x01); //subcode
							pak.WriteByte((byte)skillCount); //number of entry
							pak.WriteByte(0x03); //subtype
							pak.WriteByte((byte)firstSkills);

							foreach (Specialization spec in specs)
							{
								CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
								pak.WriteByte((byte)spec.Level);
								pak.WriteByte((byte)eSkillPage.Specialization);
								pak.WriteShort(0);
								pak.WriteByte((byte)(m_gameClient.Player.GetModifiedSpecLevel(spec.KeyName) - spec.Level)); // bonus
								pak.WriteShort(spec.Icon);
								pak.WritePascalString(spec.Name);
							}

							int count = 0;
							foreach (Skill skill in skills)
							{
								if (cachedSkills.Contains(skill))
								{
									log.Error("SendUpdatePlayerSkills : duplicate Skill : " + skill.Name + " / ID=" + skill.ID);
								}
								else
								{
									count++;
									CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);
									pak.WriteByte(0);
									byte type = (byte)eSkillPage.Abilities;
									if (skill is RealmAbility)
									{
										type = (byte)eSkillPage.RealmAbilities;
									}
									pak.WriteByte(type);
									pak.WriteShort(0);
									pak.WriteByte(0);
									pak.WriteShort(skill.Icon);
									pak.WritePascalString(m_gameClient.Player.GetSkillName(skill));
									cachedSkills.Add(skill);
								}
							}

							foreach (Style style in styleList)
							{
								if (cachedSkills.Contains(style))
								{
									log.Error("SendUpdatePlayerSkills : duplicate Style : " + style.Name + " / ID=" + style.ID);
								}
								else
								{
									CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);

									styleTable[(int)style.ID] = count++;
									if (style.Spec == GlobalSpellsLines.Champion_Spells)
									{
										pak.WriteByte((byte)style.Level);
									}
									else
									{
										pak.WriteByte((byte)style.SpecLevelRequirement);
									}
									pak.WriteByte((byte)eSkillPage.Styles);

									int pre = 0;

									try
									{
										switch (style.OpeningRequirementType)
										{
											case Style.eOpening.Offensive:
												pre = 0 + (int)style.AttackResultRequirement; // last result of our attack against enemy
												// hit, miss, target blocked, target parried, ...
												if (style.AttackResultRequirement == Style.eAttackResult.Style)
													pre |= ((100 + (int)styleTable[style.OpeningRequirementValue]) << 8);
												break;
											case Style.eOpening.Defensive:
												pre = 100 + (int)style.AttackResultRequirement; // last result of enemies attack against us
												// hit, miss, you block, you parry, ...
												break;
											case Style.eOpening.Positional:
												pre = 200 + style.OpeningRequirementValue;
												break;
										}
									}
									catch (Exception ex)
									{
										log.Error("Error loading style " + style.ID + " for player " + m_gameClient.Player.Name + ", openingrequirementvalue = " + style.OpeningRequirementValue, ex);
									}

									// style required?
									if (pre == 0)
									{
										pre = 0x100;
									}

									pak.WriteShort((ushort)pre);
									pak.WriteByte(GlobalConstants.GetSpecToInternalIndex(style.Spec)); // index specialization
									pak.WriteShort((ushort)style.Icon);
									pak.WritePascalString(style.Name);
									cachedSkills.Add(style);
								}
							}

							if (sendHybridList)
							{
								Dictionary<string, KeyValuePair<Spell, SpellLine>> spells = m_gameClient.Player.GetUsableSpells(spellLines, false);

								foreach (KeyValuePair<string, KeyValuePair<Spell, SpellLine>> spell in spells)
								{
									if (cachedSkills.Contains(spell.Value.Key))
									{
										log.Error("SendUpdatePlayerSkills : duplicate hybrid allspell : " + spell.Value.Key.Name + " / ID=" + spell.Value.Key.ID);
									}
									else
									{
										CheckLengthHybridSkillsPacket(ref pak, ref maxSkills, ref firstSkills);

										int lineIndex = specs.IndexOf(m_gameClient.Player.GetSpecialization(spell.Value.Value.Spec));

										if (lineIndex == -1)
										{
											lineIndex = 0xFE; // Nightshade special value
										}

										//log.DebugFormat("{0} : {1} - icon {2}, {3}, level {4}", spell.Value.Value.Name, lineIndex, spell.Value.Key.Icon, spell.Value.Key.Name, spell.Value.Key.Level);

										pak.WriteByte((byte)spell.Value.Key.Level);

										if (spell.Value.Key.InstrumentRequirement == 0)
										{
											pak.WriteByte((byte)eSkillPage.Spells);
											pak.WriteByte(0);
											pak.WriteByte((byte)lineIndex);
										}
										else
										{
											pak.WriteByte((byte)eSkillPage.Songs);
											pak.WriteByte(0);
											pak.WriteByte(0xFF);
										}

										pak.WriteByte(0);
										pak.WriteShort(spell.Value.Key.Icon);
										pak.WritePascalString(spell.Value.Key.Name);
										cachedSkills.Add(spell.Value.Key);
									}
								}
							}
						}
					}
				}
			}

			m_gameClient.Player.CachedSkills = cachedSkills;

			if (pak.Length > 7)
			{
				pak.Position = 4;
				pak.WriteByte((byte)(maxSkills - firstSkills)); //number of entry
				pak.WriteByte(0x03); //subtype
				pak.WriteByte((byte)firstSkills);
				SendTCP(pak);
			}

			//SendNonHybridSpellLines();

			//Using cache for UseSpellHandler purpose, hurting my eyes to see so many iterations when a caster cast a single spell
			Dictionary<byte, System.Collections.Generic.Dictionary<byte, Spell>> cachedSpells = new Dictionary<byte, Dictionary<byte, Spell>>();
			Dictionary<byte, SpellLine> cachedSpellLines = new Dictionary<byte, SpellLine>();
			Dictionary<Spell, ushort> cachedSpell2Index = new Dictionary<Spell, ushort>();
			//Dictionary<ushort, byte> cachedSpell2Level = new Dictionary<ushort, byte>();
			byte linenumber = 0;

			lock (spellLines)
			{
				foreach (SpellLine line in spellLines)
				{
					int sp = 0;
					int linelevel = m_gameClient.Player.IsAdvancedSpellLine(line) ? m_gameClient.Player.MLLevel : line.Level;
					// We only handle list caster spells or advanced lines
					if (m_gameClient.Player.CharacterClass.ClassType == eClassType.ListCaster || m_gameClient.Player.IsAdvancedSpellLine(line))
					{
						// make a copy
						var spells = new List<Spell>(SkillBase.GetSpellList(line.KeyName)); // copy
						int spellCount = 0;
						for (int i = 0; i < spells.Count; i++)
						{
							if ((spells[i]).Level <= linelevel/* && spells[i].SpellType != "StyleHandler"*/)
							{
								spellCount++;
							}
						}

						using (var pakSL = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
						{
							pakSL.WriteByte(0x02); //subcode
							pakSL.WriteByte((byte)(spellCount + 1)); //number of entry
							pakSL.WriteByte(0x02); //subtype
							pakSL.WriteByte(linenumber); //number of line
							pakSL.WriteByte(0); // level, not used when spell line
							pakSL.WriteShort(0); // icon, not used when spell line
							pakSL.WritePascalString(line.Name);
							cachedSpellLines.Add(linenumber, line);

							foreach (Spell spell in spells)
							{
								if (spell.Level <= linelevel/* && spell.SpellType != "StyleHandler"*/)
								{
									if (!cachedSpells.ContainsKey(linenumber))
										cachedSpells.Add(linenumber, new Dictionary<byte, Spell>());

									if (cachedSpells[linenumber].ContainsKey((byte)spell.Level))
									{
										log.Error("SendListCaster : duplicate level : " + spell.Level + " / name=" + spell.Name + " / class=" + m_gameClient.Player.CharacterClass.Name);
										log.Error("name=" + cachedSpells[linenumber][(byte)spell.Level].Name);
										continue;
									}
									
									pakSL.WriteByte((byte)spell.Level);
									pakSL.WriteShort(spell.Icon);
									pakSL.WritePascalString(spell.Name);
									cachedSpells[linenumber].Add((byte)spell.Level, spell);
									cachedSpell2Index.Add(spell, (ushort)((sp << 8) + linenumber));
									//cachedSpell2Level.Add(spell.ID, (byte)spell.Level);
									sp++;
								}
							}

							SendTCP(pakSL);
							linenumber++;
						}
					}
				}
			}

			m_gameClient.Player.CachedSpellLines = cachedSpellLines;
			m_gameClient.Player.CachedSpells = cachedSpells;
			m_gameClient.Player.CachedSpell2Index = cachedSpell2Index;
			//m_gameClient.Player.CachedSpell2Level = cachedSpell2Level;

			if (m_gameClient.Version >= GameClient.eClientVersion.Version181)
			{
				using (GSTCPPacketOut pak181 = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
				{
					pak181.WriteByte(0x02); //subcode
					pak181.WriteByte(0x00);
					pak181.WriteByte(99); //subtype (new subtype 99 in 1.80e)
					pak181.WriteByte(0x00);
					SendTCP(pak181);
				}
			}
		}
	}
}
