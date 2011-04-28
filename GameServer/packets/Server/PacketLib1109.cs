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
using DOL.Database;
using System.Collections;
using System.Collections.Generic;
using log4net;


namespace DOL.GS.PacketHandler
{
    [PacketLib(1109, GameClient.eClientVersion.Version1109)]
    public class PacketLib1109 : PacketLib1108
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a new PacketLib for Client Version 1.109
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib1109(GameClient client)
            : base(client)
        {

        }


		/// <summary>
		/// 1.109 items have an additional byte prior to item.Weight
		/// </summary>
		/// <param name="pak"></param>
		/// <param name="item"></param>
		protected override void WriteItemData(GSTCPPacketOut pak, InventoryItem item)
		{
			if (item == null)
			{
				pak.Fill(0x00, 20); // 1.109 +1 byte
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

			pak.Fill(0x00, 1); // 1.109, +1 byte, no clue what this is  - Tolakram

			pak.WriteShort((ushort)item.Weight);
			pak.WriteByte(item.ConditionPercent); // % of con
			pak.WriteByte(item.DurabilityPercent); // % of dur
			pak.WriteByte((byte)item.Quality); // % of qua
			pak.WriteByte((byte)item.Bonus); // % bonus
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Extension);
			int flag = 0;
			if (item.Emblem != 0)
			{
				pak.WriteShort((ushort)item.Emblem);
				flag |= (item.Emblem & 0x010000) >> 16; // = 1 for newGuildEmblem
			}
			else
				pak.WriteShort((ushort)item.Color);
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
				SpellLine chargeEffectsLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);

				if (chargeEffectsLine != null)
				{
					if (item.SpellID > 0/* && item.Charges > 0*/)
					{
						Spell spell = SkillBase.FindSpell(item.SpellID, chargeEffectsLine);
						if (spell != null)
						{
							flag |= 0x08;
							icon1 = spell.Icon;
							spell_name1 = spell.Name; // or best spl.Name ?
						}
					}
					if (item.SpellID1 > 0/* && item.Charges > 0*/)
					{
						Spell spell = SkillBase.FindSpell(item.SpellID1, chargeEffectsLine);
						if (spell != null)
						{
							flag |= 0x10;
							icon2 = spell.Icon;
							spell_name2 = spell.Name; // or best spl.Name ?
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
				if (ConsignmentMoney.UseBP)
					name += "[" + item.SellPrice.ToString() + " BP]";
				else
					name += "[" + Money.GetShortString(item.SellPrice) + "]";
			}

			if (name.Length > MAX_NAME_LENGTH)
				name = name.Substring(0, MAX_NAME_LENGTH);

			pak.WritePascalString(name);
		}


    }
}
