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

		public override void SendObjectCreate(GameObject obj)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.ObjectCreate));
			pak.WriteShort((ushort)obj.ObjectID);
			if (obj is GameStaticItem)
				pak.WriteShort((ushort)(obj as GameStaticItem).Emblem);
			else pak.WriteShort(0);
			pak.WriteShort(obj.Heading);
			pak.WriteShort((ushort)obj.Z);
			pak.WriteInt((uint)obj.X);
			pak.WriteInt((uint)obj.Y);
			if (obj is GameNPC && obj.IsUnderwater)
				pak.WriteShort((ushort)(obj.Model | 0x8000));
			else pak.WriteShort(obj.Model);
			int flag = (obj.Realm & 3) << 4;
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
			pak.WritePascalString(obj.Name);
			if (obj is IDoor)
			{
				pak.WriteByte((byte)(obj as IDoor).Flag);
				pak.WriteInt((uint)(obj as IDoor).DoorID);
			}
			else pak.WriteByte(0x00);
			SendTCP(pak);
		}

		protected override void SendInventorySlotsUpdateBase(ICollection slots, byte preAction)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.InventoryUpdate));
			pak.WriteByte((byte)(slots == null ? 0 : slots.Count));
			pak.WriteByte((byte)((m_gameClient.Player.IsCloakHoodUp ? 0x01 : 0x00) | (int)m_gameClient.Player.ActiveQuiverSlot)); //bit0 is hood up bit4 to 7 is active quiver
			pak.WriteByte((byte)m_gameClient.Player.VisibleActiveWeaponSlots);
			pak.WriteByte(preAction); //preAction (0x00 - Do nothing)
			if (slots != null)
			{
				foreach (int updatedSlot in slots)
				{
					pak.WriteByte((byte)updatedSlot);
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
						case (int)eObjectType.Arrow:
						case (int)eObjectType.Bolt:
						case (int)eObjectType.Poison:
						case (int)eObjectType.GenericItem:
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
					if (item.Emblem != 0)
					{
						pak.WriteShort((ushort)item.Emblem);
						item.Effect |= (item.Emblem & 0x010000) >> 8; // = 1 for newGuildEmblem
					}
					else
						pak.WriteShort((ushort)item.Color);
					pak.WriteShort((ushort)item.Effect);
					if (item.Count > 1)
						pak.WritePascalString(item.Count + " " + item.Name);
					else
						pak.WritePascalString(item.Name);
				}
			}
			SendTCP(pak);
		}

		public override void SendLivingEquipmentUpdate(GameLiving living)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.EquipmentUpdate));
			ICollection items = null;
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

		/*
		 * public override void SendPlayerBanner(GamePlayer player, int GuildEmblem)
		{
			if (player == null) return;
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.VisualEffect));
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
			pak.WriteInt((ushort)player.Guild.theGuildDB.Emblem);
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
