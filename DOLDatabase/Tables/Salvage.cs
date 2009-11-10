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
	/// The salvage table
	/// </summary>
	[DataTable(TableName="Salvage")]
	public class DBSalvage : DataObject
	{
		private int m_objectType;
		private int m_salvageLevel;
		private string m_id_nb;
		static bool	m_autoSave;

		/// <summary>
		/// Create salvage
		/// </summary>
		public DBSalvage()
		{
			m_autoSave=false;
		}

		/// <summary>
		/// auto save Db or not
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		/// <summary>
		/// Object type of item to salvage
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int ObjectType
		{
			get
			{
				return m_objectType;
			}
			set
			{
				Dirty = true;
				m_objectType = value;
			}
		}

		/// <summary>
		/// The salvage level of the row
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int SalvageLevel
		{
			get
			{
				return m_salvageLevel;
			}
			set
			{
				Dirty = true;
				m_salvageLevel = value;
			}
		}

		/// <summary>
		/// Index of item to craft
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string Id_nb
		{
			get
			{
				return m_id_nb;
			}
			set
			{
				Dirty = true;
				m_id_nb = value;
			}
		}

		/// <summary>
		/// The raw material to give when salvage
		/// </summary>
		[Relation(LocalField = "Id_nb", RemoteField = "Id_nb", AutoLoad = true, AutoDelete=false)]
		public ItemTemplate RawMaterial;
	}
}
