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

using DOL.Database;
using DOL.Language;
using DOL.GS.Housing;

using log4net;

namespace DOL.GS.PacketHandler
{
	[PacketLib(176, GameClient.eClientVersion.Version176)]
	public class PacketLib176 : PacketLib175
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructs a new PacketLib for Version 1.76 clients
		/// </summary>
		/// <param name="client">the gameclient this lib is associated with</param>
		public PacketLib176(GameClient client):base(client)
		{
		}

		public override void SendFindGroupWindowUpdate(GamePlayer[] list)
		{
			if (m_gameClient.Player==null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.FindGroupUpdate));
			if (list!=null)
			{
				pak.WriteByte((byte)list.Length);
				byte nbleader=0;
				byte nbsolo=0x1E;
				foreach(GamePlayer player in list)
				{
					if (player.Group!=null)
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
					if(player.CurrentZone != null)
						pak.WriteShort(player.CurrentZone.ZoneSkinID);
					else
						pak.WriteShort(0); // ?
					pak.WriteByte(0); // duration
					pak.WriteByte(0); // objective
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte((byte) (player.Group!=null ? 1 : 0));
					pak.WriteByte(0);
				}
			}
			else
			{
				pak.WriteByte(0);
			}
			SendTCP(pak);
		}

		public override void SendObjectCreate(GameObject obj)
		{
			if (obj == null)
				return;

			if (obj.IsVisibleTo(m_gameClient.Player) == false)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ObjectCreate));
			pak.WriteShort((ushort)obj.ObjectID);
			if (obj is GameStaticItem)
				pak.WriteShort((ushort)(obj as GameStaticItem).Emblem);
			else pak.WriteShort(0);
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
            DataObject translation = null;
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

		protected override void SendInventorySlotsUpdateRange(ICollection<int> slots, eInventoryWindowType windowType)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.InventoryUpdate));
			pak.WriteByte((byte)(slots == null ? 0 : slots.Count));
			pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			pak.WriteByte((byte)m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte((byte)windowType); //preAction (0x00 - Do nothing)
			if (slots != null)
			{
				foreach (int updatedSlot in slots)
				{
					if (updatedSlot >= (int)eInventorySlot.Consignment_First && updatedSlot <= (int)eInventorySlot.Consignment_Last)
						pak.WriteByte((byte)(updatedSlot - (int)eInventorySlot.Consignment_First + (int)eInventorySlot.HousingInventory_First));
					else
						pak.WriteByte((byte)(updatedSlot));
					InventoryItem item = null;
					item = m_gameClient.Player.Inventory.GetItem((eInventorySlot)updatedSlot);

					if (item == null)
					{
						pak.Fill(0x00, 19);
						continue;
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
					int effect = item.Effect;
					if (item.Emblem != 0)
					{
						pak.WriteShort((ushort)item.Emblem);
						effect |= (item.Emblem & 0x010000) >> 8; // = 1 for newGuildEmblem
					}
					else
						pak.WriteShort((ushort)item.Color);
					pak.WriteShort((ushort)effect);
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
					pak.WritePascalString(name);
				}
			}
			SendTCP(pak);
		}

		public override void SendLivingEquipmentUpdate(GameLiving living)
		{
			if (m_gameClient.Player == null || living.IsVisibleTo(m_gameClient.Player) == false)
				return;

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.EquipmentUpdate));

			ICollection<InventoryItem> items = null;
			if (living.Inventory != null)
				items = living.Inventory.VisibleItems;

			pak.WriteShort((ushort)living.ObjectID);
			pak.WriteByte((byte)((living.IsCloakHoodUp ? 0x01 : 0x00) | (int)living.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver

			pak.WriteByte((byte)living.VisibleActiveWeaponSlots);
			if (items != null)
			{
				pak.WriteByte((byte)items.Count);
				foreach (InventoryItem item in items)
				{
					ushort model = (ushort)(item.Model & 0x1FFF);
					int slot = item.SlotPosition;
					int texture = (item.Emblem != 0) ? item.Emblem : item.Color;
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
						pak.WriteByte((byte)item.Effect);
				}
			}
			else
			{
				pak.WriteByte(0x00);
			}
			SendTCP(pak);
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

		public override void SendHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseCreate));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteShort((ushort)house.Z);
			pak.WriteInt((uint)house.X);
			pak.WriteInt((uint)house.Y);
			pak.WriteShort((ushort)house.Heading);
			pak.WriteShort((ushort)house.PorchRoofColor);
			pak.WriteShort((ushort)(house.GetPorchAndGuildEmblemFlags() | (house.Emblem & 0x010000) >> 13));//new Guild Emblem
			pak.WriteShort((ushort)house.Emblem);
			pak.WriteByte((byte)house.Model);
			pak.WriteByte((byte)house.RoofMaterial);
			pak.WriteByte((byte)house.WallMaterial);
			pak.WriteByte((byte)house.DoorMaterial);
			pak.WriteByte((byte)house.TrussMaterial);
			pak.WriteByte((byte)house.PorchMaterial);
			pak.WriteByte((byte)house.WindowMaterial);
			pak.WriteByte(0x03);
			pak.WritePascalString(house.Name);

			SendTCP(pak);
		}

		public override void SendEnterHouse(House house)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.HouseEnter));

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

			SendTCP(pak);
		}

		protected override void WriteHouseFurniture(GSTCPPacketOut pak, IndoorItem item, int index)
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

		public override void SendRvRGuildBanner(GamePlayer player, bool show)
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

		public override void SendPlayerCreate(GamePlayer playerToCreate)
		{
			base.SendPlayerCreate(playerToCreate);
			if (playerToCreate.GuildBanner != null)
				playerToCreate.Out.SendRvRGuildBanner(playerToCreate, true);
		}

	}
}
