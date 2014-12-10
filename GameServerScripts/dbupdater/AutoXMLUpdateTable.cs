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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.GS.DatabaseUpdate
{
	/// <summary>
	/// DataTable to track already registered XML Loading Files Played
	/// Prevent Loading XML package Multiple times on the same database.
	/// </summary>
	[DataTable(TableName="AutoXMLUpdate")]
	public class AutoXMLUpdateRecord : DataObject
	{
		protected int m_autoXMLUpdateID;
		protected string m_filePackage;
		protected string m_fileHash;
		protected string m_loadResult;
		
		/// <summary>
		/// Primary Key AutoInc
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public int AutoXMLUpdateID {
			get { return m_autoXMLUpdateID; }
			set { Dirty = true; m_autoXMLUpdateID = value; }
		}

		/// <summary>
		/// FileName from which XML Data has been loaded.
		/// </summary>
		[DataElement(Varchar = 255, AllowDbNull = false, Index = true)]
		public string FilePackage {
			get { return m_filePackage; }
			set { Dirty = true; m_filePackage = value; }
		}

		/// <summary>
		/// File Hash to track for changes.
		/// </summary>
		[DataElement(Varchar = 255, AllowDbNull = false, Index = true)]
		public string FileHash {
			get { return m_fileHash; }
			set { Dirty = true; m_fileHash = value; }
		}

		/// <summary>
		/// Last Loading Result
		/// </summary>
		[DataElement(Varchar = 255, AllowDbNull = false)]
		public string LoadResult {
			get { return m_loadResult; }
			set { Dirty = true; m_loadResult = value; }
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public AutoXMLUpdateRecord()
		{
		}
	}
}
