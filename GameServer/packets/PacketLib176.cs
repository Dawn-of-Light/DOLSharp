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
					Zone currentZone = player.Region.GetZone(player.Position);
					if(currentZone != null)
						pak.WriteShort((ushort)currentZone.ZoneID);
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
			
			pak.WriteShort((ushort) living.ObjectID);
			pak.WriteByte((byte) (living.Inventory != null && living.Inventory.IsCloakHoodUp ? 0x01 : 0x00)); //bit0 is hood up
			pak.WriteByte(living.VisibleActiveWeaponSlots);

			if (living.Inventory != null)
			{
				ICollection items = living.Inventory.VisibleItems;
				pak.WriteByte((byte) items.Count);
				foreach (GenericItemBase item in items)
				{
					pak.WriteByte((byte) item.SlotPosition); // TODO For extended guild patter used 0x80 & slot
					
					int color = 0;
					int glowEffect = 0;
					byte modelExtension = 0;
					if(item is VisibleEquipment)
					{
						color = ((VisibleEquipment)item).Color;
						if (item is Weapon) glowEffect = ((Weapon)item).GlowEffect;
						else if (item is Armor) modelExtension = ((Armor)item).ModelExtension;
					}
					else if(item is NPCEquipment)
					{
						color = ((NPCEquipment)item).Color;
						if(item is NPCWeapon) glowEffect = ((NPCWeapon)item).GlowEffect;
						else if (item is NPCArmor) modelExtension = ((NPCArmor)item).ModelExtension;
					}

					ushort model = (ushort) (item.Model & 0x1FFF);
					if ((color & ~0xFF) != 0)
						model |= 0x8000;
					else if ((color & 0xFF) != 0)
						model |= 0x4000;
					if (glowEffect != 0)
						model |= 0x2000;

					pak.WriteShort(model);

					if (!(item is NPCWeapon)) pak.WriteByte(modelExtension);

					if ((color & ~0xFF) != 0)
						pak.WriteShort((ushort) color);
					else if ((color & 0xFF) != 0)
						pak.WriteByte((byte) color);
					if (glowEffect != 0)
						pak.WriteShort((ushort) glowEffect);
				}
			}
			else
			{
				pak.WriteByte(0x00);
			}
			SendTCP(pak);
		}

        protected override void SendQuestPacket(PlayerJournalEntry entry, int index)
		{
			GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(ePackets.QuestEntry));

			pak.WriteByte((byte) index);
			if (entry == null)
			{
				pak.WriteByte(0);
				pak.WriteByte(0);
				pak.WriteByte(0);
			}
			else
			{
				string name = entry.Name;
				string desc = entry.Description;
				if (name.Length > byte.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": name is too long for 1.71 clients ("+name.Length+") '"+name+"'");
					name = name.Substring(0, byte.MaxValue);
				}
				if (desc.Length > ushort.MaxValue)
				{
					if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": description is too long for 1.71 clients ("+desc.Length+") '"+desc+"'");
					desc = desc.Substring(0, ushort.MaxValue);
				}
				if (name.Length + desc.Length > 2048-10)
				{
					if (log.IsWarnEnabled) log.Warn(entry.GetType().ToString() + ": name + description length is too long and would have crashed the client.\nName ("+name.Length+"): '"+name+"'\nDesc ("+desc.Length+"): '"+desc+"'");
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
