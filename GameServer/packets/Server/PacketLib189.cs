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
using log4net;
using DOL.GS.Quests;
using System.Reflection;
using DOL.Database;
using System.Collections;
using DOL.GS.Housing;
using System.Collections.Generic;

namespace DOL.GS.PacketHandler
{
	[PacketLib(189, GameClient.eClientVersion.Version189)]
	public class PacketLib189 : PacketLib188
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.88 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib189(GameClient client)
			: base(client)
		{

		}
		
		public override void SendLivingEquipmentUpdate(GameLiving living)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EquipmentUpdate));
			ICollection items = null;
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
					int texture = item.Color;
					if (item.Emblem != 0)
					{
						texture = (ushort)item.Emblem;
						if (item.SlotPosition == Slot.LEFTHAND || item.SlotPosition == Slot.CLOAK) // for test only cloack and shield 
							slot = slot | ((item.Emblem & 0x010000) >> 9); // slot & 0x80 if new emblem 
					}
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
						pak.WriteByte((byte)item.Effect);
				}
			}
			else
			{
				pak.WriteByte(0x00);
			}
			SendTCP(pak);
		}

		/// <summary>
		/// New inventory update handler. This handler takes into account that
		/// a slot on the client isn't necessarily the same as a slot on the
		/// server, e.g. house vaults.
		/// </summary>
		/// <param name="updateItems"></param>
		/// <param name="windowType"></param>
		public override void SendInventoryItemsUpdate(IDictionary<int, InventoryItem> updateItems, byte windowType)
		{
			if (m_gameClient.Player == null)
				return;

			Dictionary<int, InventoryItem> items = new Dictionary<int, InventoryItem>();

			if (updateItems == null || updateItems.Count == 0)
			{
				SendInventoryItemsPartialUpdate(items, windowType);
				return;
			}

			// Send packets with a maximum of 32 items.

			foreach (int slot in updateItems.Keys)
			{
				items.Add(slot, updateItems[slot]);
				if (items.Count >= 32)
				{
					SendInventoryItemsPartialUpdate(items, windowType);
					items.Clear();
					windowType = 0;
				}
			}

			if (items.Count > 0)
				SendInventoryItemsPartialUpdate(items, windowType);
		}

		/// <summary>
		/// New inventory update (32 slots max).
		/// </summary>
		/// <param name="items"></param>
		/// <param name="windowType"></param>
		public override void SendInventoryItemsPartialUpdate(IDictionary<int, InventoryItem> items, byte windowType)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InventoryUpdate));
			GameHouseVault houseVault = m_gameClient.Player.ActiveVault;
			pak.WriteByte((byte)(items.Count));
			pak.WriteByte(0x00); // new in 189b+, show shield in left hand 
			pak.WriteByte((byte)((m_gameClient.Player.IsCloakInvisible ? 0x01 : 0x00) | (m_gameClient.Player.IsHelmInvisible ? 0x02 : 0x00))); // new in 189b+, cloack/helm visibility 
			if (windowType == 0x04 && houseVault != null)
				pak.WriteByte((byte)(houseVault.Index + 1));	// Add the vault number to the window caption
			else
				pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			// ^ in 1.89b+, 0 bit - showing hooded cloack, if not hooded not show cloack at all ? 
			pak.WriteByte((byte)m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte(windowType); //preAction (0x00 - Do nothing) 
			foreach (int slot in items.Keys)
			{
				pak.WriteByte((byte)(slot));
				WriteItemData(pak, items[slot]);
			}
			SendTCP(pak);
		}

		/// <summary>
		/// Legacy inventory update (32 slots max). This handler silently
		/// assumes that a slot on the client matches a slot on the server.
		/// </summary>
		/// <param name="slots"></param>
		/// <param name="preAction"></param>
		protected override void SendInventorySlotsUpdateBase(ICollection slots, byte preAction)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InventoryUpdate));
			GameHouseVault houseVault = m_gameClient.Player.ActiveVault;
			pak.WriteByte((byte)(slots == null ? 0 : slots.Count));
			pak.WriteByte(0); // CurrentSpeed & 0xFF (not used for player, only for NPC)
			pak.WriteByte((byte)((m_gameClient.Player.IsCloakInvisible ? 0x01 : 0x00) | (m_gameClient.Player.IsHelmInvisible ? 0x02 : 0x00))); // new in 189b+, cloack/helm visibility 
			if (preAction == 0x04 && houseVault != null)
				pak.WriteByte((byte)(houseVault.Index + 1));	// Add the vault number to the window caption
			else
				pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			// ^ in 1.89b+, 0 bit - showing hooded cloack, if not hooded not show cloack at all ? 
			pak.WriteByte((byte)m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte(preAction); //preAction (0x00 - Do nothing) 
			if (slots != null)
			{
				foreach (int updatedSlot in slots)
				{
					if (updatedSlot >= (int)eInventorySlot.Consignment_First && updatedSlot <= (int)eInventorySlot.Consignment_Last)
						pak.WriteByte((byte)(updatedSlot - (int)eInventorySlot.Consignment_First + (int)eInventorySlot.HousingInventory_First));
					else
						pak.WriteByte((byte)(updatedSlot));
					WriteItemData(pak, m_gameClient.Player.Inventory.GetItem((eInventorySlot)(updatedSlot)));
				}
			}
			SendTCP(pak);
		}

		protected void WriteItemData(GSTCPPacketOut pak, InventoryItem item)
		{
			if (item == null)
			{
				pak.Fill(0x00, 19);
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
				name += "[" + Money.GetString(item.SellPrice) + "]";
			pak.WritePascalString(name);
		}


		public override void SendHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseCreate));
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

		public override void SendGarden(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseChangeGarden));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort(0); // new in 1.89b+
			pak.WriteByte((byte)house.OutdoorItems.Count);
			pak.WriteByte(0x80);
			foreach (DictionaryEntry entry in new SortedList(house.OutdoorItems))
			{
				OutdoorItem item = (OutdoorItem)entry.Value;
				pak.WriteByte((byte)((int)entry.Key));
				pak.WriteShort((ushort)item.Model);
				pak.WriteByte((byte)item.Position);
				pak.WriteByte((byte)item.Rotation);
			}
			SendTCP(pak);
		}

		public override void SendGarden(House house, int i)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseChangeGarden));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort(0); // new in 1.89b+
			pak.WriteByte(0x01);
			pak.WriteByte(0x00); // update
			OutdoorItem item = (OutdoorItem)house.OutdoorItems[i];
			pak.WriteByte((byte)i);
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Position);
			pak.WriteByte((byte)item.Rotation);
			SendTCP(pak);
		}

		public override void SendEnterHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HouseEnter));

			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort((ushort)25000);         //constant!
			pak.WriteInt((uint)house.X);
			pak.WriteInt((uint)house.Y);
			pak.WriteShort((ushort)house.Heading); //useless/ignored by client.
			pak.WriteByte(0x00);
			pak.WriteByte((byte)(house.GetGuildEmblemFlags() | (house.Emblem & 0x010000) >> 14));//new Guild Emblem
			pak.WriteShort((ushort)house.Emblem);	//emblem
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
}
