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
using System.Collections.Generic;
using System.Linq;

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
					while(index < total)
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
		}

		/// <summary>
		/// Send non hybrid and advanced spell lines
		/// </summary>
		public override void SendNonHybridSpellLines()
		{
			GamePlayer player = m_gameClient.Player;
			if (player == null)
				return;

			List<Tuple<SpellLine, List<Skill>>> spellsXLines = player.GetAllUsableListSpells(true);
			
			int lineIndex = 0;
			foreach (var spXsl in spellsXLines)
			{
				// Prepare packet
				using(var pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.VariousUpdate)))
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

			// Enable craft button if the item can be modified and the player has alchemy or spellcrafting
			eCraftingSkill skill = CraftingMgr.GetCraftingSkill(item);
			switch (skill)
			{
				case eCraftingSkill.ArmorCrafting:
				case eCraftingSkill.Fletching:
				case eCraftingSkill.Tailoring:
				case eCraftingSkill.WeaponCrafting:
					if (m_gameClient.Player.CraftingSkills.ContainsKey(eCraftingSkill.Alchemy)
						|| m_gameClient.Player.CraftingSkills.ContainsKey(eCraftingSkill.SpellCrafting))
						flag |= 0x04; // enable craft button
					break;

				default:
					break;
			}

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
					player.Orientation = player.Steed.Orientation;
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

                var zoneCoordinate = player.Coordinate - player.CurrentZone.Offset;
				pak.WriteShort((ushort)zoneCoordinate.Z);
				pak.WriteShort((ushort)zoneCoordinate.X);
				pak.WriteShort((ushort)zoneCoordinate.Y);
				
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
					ushort contenthead = (ushort)(player.Orientation.InHeading + (true ? 0x1000 : 0));
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
