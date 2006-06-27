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
using DOL.GS.Database;
using DOL.GS.Quests;
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
						pak.WriteShort((ushort)player.CurrentZone.ZoneID);
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

		public override void SendLivingEquipementUpdate(GameLiving living)
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
				foreach (VisibleEquipment item in items)
				{
					pak.WriteByte((byte) item.SlotPosition); // TODO For extended guild patter used 0x80 & slot

					ushort model = (ushort) (item.Model & 0x1FFF);
				
					if ((item.Color & ~0xFF) != 0)
						model |= 0x8000;
					else if ((item.Color & 0xFF) != 0)
						model |= 0x4000;
					if (item is Weapon && ((Weapon)item).GlowEffect != 0)
						model |= 0x2000;

					pak.WriteShort(model);

					if (item is Armor) pak.WriteByte(((Armor)item).ModelExtension);

					if ((item.Color & ~0xFF) != 0)
						pak.WriteShort((ushort) item.Color);
					else if ((item.Color & 0xFF) != 0)
						pak.WriteByte((byte) item.Color);
					if (item is Weapon && ((Weapon)item).GlowEffect != 0)
						pak.WriteShort((ushort) ((Weapon)item).GlowEffect);
				}
			}
			else
			{
				pak.WriteByte(0x00);
			}
			SendTCP(pak);
		}
		
		protected override void SendQuestPacket(AbstractQuest quest, int index)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));

			pak.WriteByte((byte) index);
			if (quest.Step <= 0)
			{
				pak.WriteByte(0);
				pak.WriteByte(0);
				pak.WriteByte(0);
			}
			else
			{
				string name = quest.Name;
				string desc = quest.Description;
				if (name.Length > byte.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": name is too long for 1.71 clients ("+name.Length+") '"+name+"'");
					name = name.Substring(0, byte.MaxValue);
				}
				if (desc.Length > ushort.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": description is too long for 1.71 clients ("+desc.Length+") '"+desc+"'");
					desc = desc.Substring(0, ushort.MaxValue);
				}
				if (name.Length + desc.Length > 2048-10)
				{
					if (log.IsWarnEnabled) log.Warn(quest.GetType().ToString() + ": name + description length is too long and would have crashed the client.\nName ("+name.Length+"): '"+name+"'\nDesc ("+desc.Length+"): '"+desc+"'");
					name = name.Substring(0, 32);
					desc = desc.Substring(0, 2048-10 - name.Length); // all that's left
				}
				pak.WriteByte((byte)name.Length);
				pak.WriteShortLowEndian((ushort)desc.Length);
				pak.WriteStringBytes(name); //Write Quest Name without trailing 0 
				pak.WriteStringBytes(desc); //Write Quest Description without trailing 0
			}
			SendTCP(pak);
		}
	}
}
