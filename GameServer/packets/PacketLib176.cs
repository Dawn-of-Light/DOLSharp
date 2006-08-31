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
using DOL.Database;
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
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.FindGroupUpdate));
			if (list!=null)
			{
				pak.WriteByte((byte)list.Length);
				byte nbleader=0;
				byte nbsolo=0x1E;
				foreach(GamePlayer player in list)
				{
					if (player.PlayerGroup!=null)
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
					if(player.CurrentZone != null)
						pak.WriteShort(player.CurrentZone.ID);
					else
						pak.WriteShort(0); // ?
					pak.WriteByte(0); // duration
					pak.WriteByte(0); // objective
					pak.WriteByte(0);
					pak.WriteByte(0);
					pak.WriteByte((byte) (player.PlayerGroup!=null ? 1 : 0));
					pak.WriteByte(0);
				}
			}
			else
			{
				pak.WriteByte(0);
			}
			SendTCP(pak);
		}

		public override void SendLivingEquipmentUpdate(GameLiving living)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EquipmentUpdate));
			ICollection items = null;
			if (living.Inventory != null)
				items = living.Inventory.VisibleItems;

			pak.WriteShort((ushort) living.ObjectID);
			pak.WriteByte((byte) ((living.IsCloakHoodUp ? 0x01 : 0x00) | (int) living.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver

			pak.WriteByte((byte) living.VisibleActiveWeaponSlots);
			if (items != null)
			{
				pak.WriteByte((byte) items.Count);
				foreach (InventoryItem item in items)
				{
					pak.WriteByte((byte) item.SlotPosition); // TODO For extended guild patter used 0x80 & slot

					ushort model = (ushort) (item.Model & 0x1FFF);
					int texture = (item.Emblem != 0) ? item.Emblem : item.Color;

					if ((texture & ~0xFF) != 0)
						model |= 0x8000;
					else if ((texture & 0xFF) != 0)
						model |= 0x4000;
					if (item.Effect != 0)
						model |= 0x2000;

					pak.WriteShort(model);

					if (item.SlotPosition > Slot.RANGED || item.SlotPosition < Slot.RIGHTHAND)
						pak.WriteByte((byte) item.Extension);

					if ((texture & ~0xFF) != 0)
						pak.WriteShort((ushort) texture);
					else if ((texture & 0xFF) != 0)
						pak.WriteByte((byte) texture);
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
		public override  void SendFurniture(House house)
		{

			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HousingItem));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(Convert.ToByte(house.IndoorItems.Count));
			pak.WriteByte(0x80); //0x00 = update, 0x80 = complete package
			foreach(DictionaryEntry entry in new SortedList(house.IndoorItems))
			{
				IndoorItem item = (IndoorItem)entry.Value;
				pak.WriteByte((byte)((int)entry.Key));
				pak.WriteByte(0x01); // type item ?
				pak.WriteShort((ushort)item.Model);
				pak.WriteByte((byte)item.Color);
				pak.WriteShort((ushort)item.X);
				pak.WriteShort((ushort)item.Y);
				pak.WriteShort((ushort)item.Rotation);
				pak.WriteByte((byte)item.Position);
				pak.WriteByte((byte)(item.Placemode-2));
			}
			SendTCP(pak);
		}

		public override void SendFurniture(House house, int i)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.HousingItem));
			pak.WriteShort((ushort)house.HouseNumber);
			pak.WriteByte(0x01); //cnt
			pak.WriteByte(0x00); //upd
			IndoorItem item = (IndoorItem)house.IndoorItems[i];
			pak.WriteByte((byte)i);
			pak.WriteByte(0x01); // type item ?
			pak.WriteShort((ushort)item.Model);
			pak.WriteByte((byte)item.Color);
			pak.WriteShort((ushort)item.X);
			pak.WriteShort((ushort)item.Y);
			pak.WriteShort((ushort)item.Rotation);
			pak.WriteByte((byte)item.Position);
			pak.WriteByte((byte)(item.Placemode-2));
			SendTCP(pak);
		}

		public override void SendRvRGuildBanner(GamePlayer player, bool show)
		{
			if (player == null) return;

			//cannot show banners for players that have no guild.
			if (show && player.Guild == null)
				return;
			GSTCPPacketOut pak = new GSTCPPacketOut((byte)ePackets.VisualEffect);
			pak.WriteShort((ushort)player.ObjectID);
			pak.WriteByte(0xC); // show Banner
			pak.WriteByte((byte)((show) ? 0 : 1)); // 0-enable, 1-disable
			pak.WriteInt(player.Guild.theGuildDB.Emblem);
			SendTCP(pak);
		}

		public override void SendPlayerCreate(GamePlayer playerToCreate)
		{
			base.SendPlayerCreate(playerToCreate);
			if (playerToCreate.IsCarryingGuildBanner)
				playerToCreate.Out.SendRvRGuildBanner(playerToCreate, true);
		}

	}
}
