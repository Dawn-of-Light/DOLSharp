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
	public enum ePathType : int
	{
		Once = 1,
		Path_Reverse = 2,
		Loop = 3,
	}

	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName="Path")]
	public class DBPath : DataObject
	{
		protected ushort m_region = 0;
		protected string m_pathID = "invalid";
		protected int m_type;//etype
		
		public DBPath()
		{
		}

		public DBPath(string pathid, ePathType type)
		{
			m_pathID = pathid;
			m_type = (int)type;
		}

		[DataElement(AllowDbNull = false,Unique=true)]
		public String PathID {
			get { return m_pathID; }
			set { m_pathID = value; }
		}

		[DataElement(AllowDbNull = false)]
		public int PathType {
			get { return m_type; }
			set { m_type = value; }
		}

		/// <summary>
		/// Used in PathDesigner tool, only. Not in DoL code
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public ushort RegionID
		{
			get { return m_region; }
			set { m_region = value; }
		}

		override public bool AutoSave
		{
			get { return false; }
			set {}
		}
		
		[Relation(LocalField = "PathID", RemoteField = "PathID", AutoLoad = true, AutoDelete=true)]
		public DBPathPoint[] PathPoints;
	}
}
