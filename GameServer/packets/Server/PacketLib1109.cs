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

		public override void SendTradeWindow()
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

			// Create a GameInventoryItem so item will display correctly in inventory window
			item = GameInventoryItem.Create(item);

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
				if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					name += "[" + item.SellPrice.ToString() + " BP]";
				else
					name += "[" + Money.GetShortString(item.SellPrice) + "]";
			}

			if (name.Length > MAX_NAME_LENGTH)
				name = name.Substring(0, MAX_NAME_LENGTH);

			pak.WritePascalString(name);
		}

		protected override void WriteTemplateData(GSTCPPacketOut pak, ItemTemplate template, int count)
		{
			if (template == null)
			{
				pak.Fill(0x00, 20);  // 1.109 +1 byte
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
			pak.WriteShort((ushort)template.Model);
			pak.WriteByte((byte)template.Extension);
			if (template.Emblem != 0)
				pak.WriteShort((ushort)template.Emblem);
			else
				pak.WriteShort((ushort)template.Color);
			pak.WriteByte((byte)0); // Flag
			pak.WriteByte((byte)template.Effect);
			if (count > 1)
				pak.WritePascalString(String.Format("{0} {1}", count, template.Name));
			else
				pak.WritePascalString(template.Name);
		}

	}
}
