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
using DOL.Database.Attributes;

namespace DOL.Database
{
    /// <summary>
    /// Cross Reference Table Between character Class and Specialization Careers.
    /// </summary>
    [DataTable(TableName="ClassXSpecialization")]
	public class ClassXSpecialization : DataObject
	{
		private int m_classXSpecializationID;
		
		/// <summary>
		/// Table Primary Key
		/// </summary>
		[PrimaryKey(AutoIncrement=true)]
		public int ClassXSpecializationID {
			get { return m_classXSpecializationID; }
			set { Dirty = true; m_classXSpecializationID = value; }
		}
		
		private int m_classID;
		
		/// <summary>
		/// Class ID attached to this specialization (0 = all)
		/// </summary>
		[DataElement(AllowDbNull=true, Index=true)]
		public int ClassID {
			get { return m_classID; }
			set { Dirty = true; m_classID = value; }
		}
		
		private string m_specKeyName;
		
		/// <summary>
		/// Specialization Key
		/// </summary>
		[DataElement(AllowDbNull=false, Index=true, Varchar=100)]
		public string SpecKeyName {
			get { return m_specKeyName; }
			set { Dirty = true; m_specKeyName = value; }
		}
		
		private int m_levelAcquired;
		
		/// <summary>
		/// Level at which Specialization is enabled. (default 0 = always enabled)
		/// </summary>
		[DataElement(AllowDbNull=true, Index=true)]
		public int LevelAcquired {
			get { return m_levelAcquired; }
			set { Dirty = true; m_levelAcquired = value; }
		}
				
		/// <summary>
		/// Constructor
		/// </summary>
		public ClassXSpecialization()
		{
		}
	}
}
