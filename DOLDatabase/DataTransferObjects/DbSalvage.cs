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
using System.Collections;

namespace DOL.Database.DataTransferObjects
{
	/// <summary>
	/// The salvage table
	/// </summary>
	public class DbSalvage
	{
		private int m_id;
		private int m_objectType;
		private int m_salvageLevel;
		private DbItemTemplate m_materialItemtemplate;
		
		public int SalvageID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}

		/// <summary>
		/// Object type of item to salvage
		/// </summary>
		public int ObjectType
		{
			get
			{
				return m_objectType;
			}
			set
			{
				m_objectType = value;
			}
		}

		/// <summary>
		/// The salvage level of the row
		/// </summary>
		public int SalvageLevel
		{
			get
			{
				return m_salvageLevel;
			}
			set
			{
				m_salvageLevel = value;
			}
		}

		/// <summary>
		/// Index of item to craft
		/// </summary>
		public DbItemTemplate MaterialItemtemplate
		{
			get
			{
				if (m_materialItemtemplate == null) m_materialItemtemplate = new DbItemTemplate();
				return m_materialItemtemplate;
			}
			set
			{
				m_materialItemtemplate = value;
			}
		}
	}
}
