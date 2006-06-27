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
	/// Faction table
	/// </summary>
	[DataTable(TableName="Faction")]
	public class DBFaction : DataObject
	{
		private bool m_autoSave;
		private string m_name;
		private int m_index;
		private int m_baseFriendship;

		/// <summary>
		/// Create faction
		/// </summary>
		public DBFaction()
		{
			m_autoSave=false;
			ID=0;
			m_baseFriendship=0;
			m_name=String.Empty;
		}

		/// <summary>
		/// autosave faction table
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
		/// Index of faction
		/// </summary>
		[PrimaryKey]
		public int ID
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
			}
		}

		/// <summary>
		/// Name of faction
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		/// <summary>
		/// base friendship/relationship/aggro level at start for playe when never it before
		/// 
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int BaseFriendShip
		{
			get { return m_baseFriendship; }
			set
			{m_baseFriendship = value;}
		}
	}
}
