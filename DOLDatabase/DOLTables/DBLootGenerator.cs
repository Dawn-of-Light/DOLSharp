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
	[DataTable(TableName="LootGenerator")]
	public class DBLootGenerator : DataObject
	{		
		protected string	m_mobName = "";
		protected string	m_mobGuild = "";
		protected string	m_mobFaction = "";
		protected string	m_lootGeneratorClass = "";
		protected int		m_exclusivePriority = 0;
				
		static bool			m_autoSave;

		public DBLootGenerator()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get	{return m_autoSave;}
			set	{m_autoSave = value;}
		}

		[DataElement(AllowDbNull=true,Unique=false)]
		public String MobName
		{
			get {return m_mobName;}
			set	
			{
				Dirty = true;
				m_mobName = value;
			}
		}

		[DataElement(AllowDbNull=true,Unique=false)]
		public String MobGuild
		{
			get {return m_mobGuild;}
			set	
			{
				Dirty = true;
				m_mobGuild = value;
			}
		}

		[DataElement(AllowDbNull=true,Unique=false)]
		public String MobFaction
		{
			get {return m_mobFaction;}
			set	
			{
				Dirty = true;
				m_mobFaction = value;
			}
		}

		[DataElement(AllowDbNull=false,Unique=false)]
		public String LootGeneratorClass
		{
			get {return m_lootGeneratorClass;}
			set	
			{
				Dirty = true;
				m_lootGeneratorClass = value;
			}
		}

		[DataElement(AllowDbNull=false,Unique=false)]
		public int ExclusivePriority
		{
			get {return m_exclusivePriority;}
			set	
			{
				Dirty = true;
				m_exclusivePriority = value;
			}
		}

	}
}
