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
using System.Collections.Generic;
namespace DOL.Database2
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
	[Serializable]//TableName="Path")]
	public class DBPath : DatabaseObject
	{
		protected string m_pathID = "invalid";
		protected int m_type;//etype

        public DBPath()
            : base()
		{
		}

		public DBPath(string pathid, ePathType type)
		{
			m_pathID = pathid;
			m_type = (int)type;
		}

		//[DataElement(AllowDbNull = false,Unique=true)]
		public String PathID {
			get { return m_pathID; }
			set { m_pathID = value; }
		}

		
		public int PathType {
			get { return m_type; }
			set { m_type = value; }
		}
		//[Relation(LocalField = "PathID", RemoteField = "PathID", AutoLoad = true, AutoDelete=true)]
		public List<DBPathPoint> PathPoints;
        public override void FillObjectRelations()
        {
            PathPoints = DatabaseLayer.Instance.SelectObjects<DBPathPoint>("PathID", ID);
            base.FillObjectRelations();
        }
	}
}
