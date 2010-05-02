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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Database Storage of Tasks
	/// </summary>
	[DataTable(TableName = "LootGenerator")]
	public class LootGenerator : DataObject
	{
		/// <summary>
		/// Trigger Mob
		/// </summary>
		protected string m_mobName = "";
		/// <summary>
		/// Trigger Guild
		/// </summary>
		protected string m_mobGuild = "";
		/// <summary>
		/// Trigger Faction
		/// </summary>
		protected string m_mobFaction = "";
		/// <summary>
		/// Trigger Region
		/// </summary>
		protected ushort m_regionID = 0;
		/// <summary>
		/// Class of the Loot Generator
		/// </summary>
		protected string m_lootGeneratorClass = "";
		/// <summary>
		/// Exclusive Priority
		/// </summary>
		protected int m_exclusivePriority = 0;

		/// <summary>
		/// Constructor
		/// </summary>
		public LootGenerator()
		{
			AllowAdd = false;
		}

		/// <summary>
		/// MobName
		/// </summary>
		[DataElement(AllowDbNull = true, Unique = false)]
		public String MobName
		{
			get { return m_mobName; }
			set
			{
				Dirty = true;
				m_mobName = value;
			}
		}

		/// <summary>
		/// MobGuild
		/// </summary>
		[DataElement(AllowDbNull = true, Unique = false)]
		public String MobGuild
		{
			get { return m_mobGuild; }
			set
			{
				Dirty = true;
				m_mobGuild = value;
			}
		}

		/// <summary>
		/// MobFaction
		/// </summary>
		[DataElement(AllowDbNull = true, Unique = false)]
		public String MobFaction
		{
			get { return m_mobFaction; }
			set
			{
				Dirty = true;
				m_mobFaction = value;
			}
		}

		/// <summary>
		/// Mobs Region ID
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = false)]
		public ushort RegionID
		{
			get { return m_regionID; }
			set
			{
				Dirty = true;
				m_regionID = value;
			}
		}

		/// <summary>
		/// LootGeneratorClass
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = false)]
		public String LootGeneratorClass
		{
			get { return m_lootGeneratorClass; }
			set
			{
				Dirty = true;
				m_lootGeneratorClass = value;
			}
		}

		/// <summary>
		/// ExclusivePriority
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = false)]
		public int ExclusivePriority
		{
			get { return m_exclusivePriority; }
			set
			{
				Dirty = true;
				m_exclusivePriority = value;
			}
		}

	}
}
