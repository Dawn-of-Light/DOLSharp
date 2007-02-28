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
/* Created by Schaf
 * Last modified by Schaf on 10.12.2004 20:09
 */
 
using System;

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// The Database Entry for an Outdoor Housing Item
	/// </summary>
	[DataTable(TableName = "DBOutdoorItem")]
	public class DBHouseOutdoorItem : DataObject
	{
		private int m_housenumber;
		private int m_model;
		private int m_position;
		private int m_rotation;

		private string m_baseitemid;

		private static bool m_autoSave;

		/// <summary>
		/// The Constructor
		/// </summary>
		public DBHouseOutdoorItem()
		{
			m_autoSave = false;
		}

		/// <summary>
		/// The House Number
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int HouseNumber
		{
			get
			{
				return m_housenumber;
			}
			set
			{
				Dirty = true;
				m_housenumber = value;
			}
		}

		/// <summary>
		/// The Model
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Model
		{
			get
			{
				return m_model;
			}
			set
			{
				Dirty = true;
				m_model = value;
			}
		}

		/// <summary>
		/// The Position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Position
		{
			get
			{
				return m_position;
			}
			set
			{
				Dirty = true;
				m_position = value;
			}
		}

		/// <summary>
		/// The Rotation
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Rotation
		{
			get
			{
				return m_rotation;
			}
			set
			{
				Dirty = true;
				m_rotation = value;
			}
		}

		/// <summary>
		/// The Base Item ID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string BaseItemID
		{
			get
			{
				return m_baseitemid;
			}
			set
			{
				Dirty = true;
				m_baseitemid = value;
			}
		}

		/// <summary>
		/// Autosave
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
	}
}
