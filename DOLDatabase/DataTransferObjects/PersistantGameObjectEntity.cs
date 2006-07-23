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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct PersistantGameObjectEntity
	{
		private int m_id;
		private byte m_blockChance;
		private int m_doorId;
		private byte m_evadeChance;
		private int m_factionId;
		private byte m_flags;
		private string m_guildName;
		private int m_heading;
		private int m_inventoryId;
		private byte m_leftHandSwingChance;
		private byte m_level;
		private int m_lootListId;
		private int m_maxSpeedBase;
		private byte m_meleeDamageType;
		private int m_merchantWindowId;
		private int m_model;
		private string m_name;
		private byte m_parryChance;
		private string m_persistantGameObjectType;
		private byte m_realm;
		private int m_regionId;
		private int m_respawnInterval;
		private byte m_size;
		private byte m_toolType;
		private int m_x;
		private int m_y;
		private int m_z;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public byte BlockChance
		{
			get { return m_blockChance; }
			set { m_blockChance = value; }
		}
		public int DoorId
		{
			get { return m_doorId; }
			set { m_doorId = value; }
		}
		public byte EvadeChance
		{
			get { return m_evadeChance; }
			set { m_evadeChance = value; }
		}
		public int FactionId
		{
			get { return m_factionId; }
			set { m_factionId = value; }
		}
		public byte Flags
		{
			get { return m_flags; }
			set { m_flags = value; }
		}
		public string GuildName
		{
			get { return m_guildName; }
			set { m_guildName = value; }
		}
		public int Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}
		public int InventoryId
		{
			get { return m_inventoryId; }
			set { m_inventoryId = value; }
		}
		public byte LeftHandSwingChance
		{
			get { return m_leftHandSwingChance; }
			set { m_leftHandSwingChance = value; }
		}
		public byte Level
		{
			get { return m_level; }
			set { m_level = value; }
		}
		public int LootListId
		{
			get { return m_lootListId; }
			set { m_lootListId = value; }
		}
		public int MaxSpeedBase
		{
			get { return m_maxSpeedBase; }
			set { m_maxSpeedBase = value; }
		}
		public byte MeleeDamageType
		{
			get { return m_meleeDamageType; }
			set { m_meleeDamageType = value; }
		}
		public int MerchantWindowId
		{
			get { return m_merchantWindowId; }
			set { m_merchantWindowId = value; }
		}
		public int Model
		{
			get { return m_model; }
			set { m_model = value; }
		}
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public byte ParryChance
		{
			get { return m_parryChance; }
			set { m_parryChance = value; }
		}
		public string PersistantGameObjectType
		{
			get { return m_persistantGameObjectType; }
			set { m_persistantGameObjectType = value; }
		}
		public byte Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}
		public int RegionId
		{
			get { return m_regionId; }
			set { m_regionId = value; }
		}
		public int RespawnInterval
		{
			get { return m_respawnInterval; }
			set { m_respawnInterval = value; }
		}
		public byte Size
		{
			get { return m_size; }
			set { m_size = value; }
		}
		public byte ToolType
		{
			get { return m_toolType; }
			set { m_toolType = value; }
		}
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}
	}
}
