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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using DOL.Database;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;

using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1112, GameClient.eClientVersion.Version1112)]
    public class PacketLib1112 : PacketLib1111
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.112
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1112(GameClient client)
            : base(client)
        {

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
			{

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
								int skillCount = specs.Count + skills.Count + styleList.Count;
	
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
									pak.WriteShort(spec.ID); //new 1.112
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
										pak.WriteShort(skill.ID); //new 1.112
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
										pak.WriteShort(style.ID); //new 1.112
										pak.WriteByte((byte)eSkillPage.Styles);
	
										int pre = 0;
	
										try
										{
											switch (style.OpeningRequirementType)
											{
												case Style.eOpening.Offensive:
													pre = 0 + (int)style.AttackResultRequirement; // last result of our attack against enemy
													// hit, miss, target blocked, target parried, ...
													if (style.AttackResultRequirement == Style.eAttackResultRequirement.Style)
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
	
											pak.WriteByte((byte)spell.Value.Key.Level);
											pak.WriteShort(spell.Value.Key.TooltipId); //new 1.112
	
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
	
				//m_gameClient.Player.CachedSkills = cachedSkills;
	
				if (pak.Length > 7)
				{
					pak.Position = 4;
					pak.WriteByte((byte)(maxSkills - firstSkills)); //number of entry
					pak.WriteByte(0x03); //subtype
					pak.WriteByte((byte)firstSkills);
					SendTCP(pak);
				}
			}
			
			Dictionary<byte, Dictionary<byte, Spell>> cachedSpells = new Dictionary<byte, Dictionary<byte, Spell>>();
			Dictionary<byte, SpellLine> cachedSpellLines = new Dictionary<byte, SpellLine>();
			Dictionary<Spell, ushort> cachedSpell2Index = new Dictionary<Spell, ushort>();
			//Dictionary<ushort, byte> cachedSpell2Level = new Dictionary<ushort, byte>();
			byte linenumber = 0;

			lock(spellLines)
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
							pakSL.WriteShortLowEndian(0); // level, not used when spell line
							pakSL.WriteShort(0); //new 1.112
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

									pakSL.WriteShortLowEndian((byte)spell.Level);
									pakSL.WriteShort(spell.TooltipId); //new 1.112
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

			//m_gameClient.Player.CachedSpellLines = cachedSpellLines;
			//m_gameClient.Player.CachedSpells = cachedSpells;
			//m_gameClient.Player.CachedSpell2Index = cachedSpell2Index;
			//m_gameClient.Player.CachedSpell2Level = cachedSpell2Level;

			using (GSTCPPacketOut pak3 = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
			{
				pak3.WriteByte(0x02); //subcode
				pak3.WriteByte(0x00);
				pak3.WriteByte(99); //subtype (new subtype 99 in 1.80e)
				pak3.WriteByte(0x00);
				SendTCP(pak3);
			}
		}


		protected override void WriteTemplateData(GSTCPPacketOut pak, ItemTemplate template, int count)
		{
			if (template == null)
			{
				pak.Fill(0x00, 21); // 1.109 +1 byte
				return;
			}

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
			pak.WriteShort((ushort)template.Effect);
			if (count > 1)
				pak.WritePascalString(String.Format("{0} {1}", count, template.Name));
			else
				pak.WritePascalString(template.Name);
		}


		protected override void WriteItemData(GSTCPPacketOut pak, InventoryItem item)
		{
			if (item == null)
			{
				pak.Fill(0x00, 21);
				return;
			}

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
			//						flag |= 0x01; // newGuildEmblem
			flag |= 0x02; // enable salvage button
			AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(m_gameClient.Player.CraftingPrimarySkill);
			if (skill != null && skill is AdvancedCraftingSkill/* && ((AdvancedCraftingSkill)skill).IsAllowedToCombine(m_gameClient.Player, item)*/)
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
			pak.WriteByte((byte)item.Effect);
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
		/// This is used to build a server side "Position Object"
		/// Usually Position Packet Should only be relayed
		/// The only purpose of this method is refreshing postion when there is Lag
		/// </summary>
		/// <param name="player"></param>
		public override void SendPlayerForgedPosition(GamePlayer player)
		{
			using (GSUDPPacketOut pak = new GSUDPPacketOut(GetPacketCode(eServerPackets.PlayerPosition)))
			{
				// PID
				pak.WriteShort((ushort)player.Client.SessionID);
				
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

				// Get Off Corrd
				int offX = player.X - player.CurrentZone.XOffset;
				int offY = player.Y - player.CurrentZone.YOffset;

				pak.WriteShort((ushort)player.Z);
				pak.WriteShort((ushort)offX);
				pak.WriteShort((ushort)offY);
				
				// Write Zone
				pak.WriteShort(player.CurrentZone.ZoneSkinID);

				// Copy Heading && Falling or Write Steed
				if (player.Steed != null && player.Steed.ObjectState == GameObject.eObjectState.Active)
				{
					pak.WriteShort((ushort)player.Steed.ObjectID);
					pak.WriteShort((ushort)player.Steed.RiderSlot(player));
				}
				else
				{
					// Set Player always on ground, this is an "anti lag" packet
					ushort contenthead = (ushort)(player.Heading + (true ? 0x1000 : 0));
					pak.WriteShort(contenthead);
					// No Fall Speed.
					pak.WriteShort(0);
				}
				
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

				// Write health + Attack
				byte healthcontent = (byte)(player.HealthPercent + (player.AttackState ? 0x80 : 0));
			
				pak.WriteByte(healthcontent);
				
				// Send Remaining
				pak.WriteByte(player.ManaPercent);
				pak.WriteByte(player.EndurancePercent);
				pak.WriteByte((byte)(player.RPFlag ? 1 : 0));
				pak.WriteByte(0);

				SendUDP(pak);	
			}
			
			// Update Cache
			m_gameClient.GameObjectUpdateArray[new Tuple<ushort, ushort>(player.CurrentRegionID, (ushort)player.ObjectID)] = GameTimer.GetTickCount();
		}

    }
}
