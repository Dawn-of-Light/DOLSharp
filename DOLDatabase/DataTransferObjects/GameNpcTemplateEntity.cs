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
	public class GameNpcTemplateEntity
	{
		private int m_id;
		private byte m_blockChance;
		private byte m_evadeChance;
		private int m_factionId;
		private byte m_flags;
		private string m_gameNPCTemplateType;
		private string m_guildName;
		private int m_inventoryId;
		private byte m_leftHandSwingChance;
		private int m_lootListId;
		private byte m_maxLevel;
		private byte m_maxSize;
		private int m_maxSpeedBase;
		private byte m_meleeDamageType;
		private byte m_minLevel;
		private byte m_minSize;
		private int m_model;
		private string m_name;
		private byte m_parryChance;
		private byte m_realm;
		private int m_respawnInterval;

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
		public string GameNPCTemplateType
		{
			get { return m_gameNPCTemplateType; }
			set { m_gameNPCTemplateType = value; }
		}
		public string GuildName
		{
			get { return m_guildName; }
			set { m_guildName = value; }
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
		public int LootListId
		{
			get { return m_lootListId; }
			set { m_lootListId = value; }
		}
		public byte MaxLevel
		{
			get { return m_maxLevel; }
			set { m_maxLevel = value; }
		}
		public byte MaxSize
		{
			get { return m_maxSize; }
			set { m_maxSize = value; }
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
		public byte MinLevel
		{
			get { return m_minLevel; }
			set { m_minLevel = value; }
		}
		public byte MinSize
		{
			get { return m_minSize; }
			set { m_minSize = value; }
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
		public byte Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}
		public int RespawnInterval
		{
			get { return m_respawnInterval; }
			set { m_respawnInterval = value; }
		}
	}
}
