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
 * Store character ML steps validation
 */

using System;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName="CharacterXMasterLevel")]
	public class DBCharacterXMasterLevel : DataObject
	{
		protected string m_character_id;
		protected int m_mllevel;				// ML number
		protected int m_step;					// ML step number
		protected bool m_stepcompleted;			// ML completition flag
		protected DateTime m_validationdate;	// Validation date (for tracking purpose)

		public DBCharacterXMasterLevel()
		{
			AutoSave = false;
		}

		// Owner ID
		[DataElement(AllowDbNull = false)]
		public string Character_ID
		{
			get { return m_character_id; }
			set
			{
				Dirty = true;
				m_character_id = value;
			}
		}
		
		// ML number
		[DataElement(AllowDbNull = false)]
		public int MLLevel
		{
			get { return m_mllevel; }
			set
			{
				Dirty = true;
				m_mllevel = value;
			}
		}

		// ML step number
		[DataElement(AllowDbNull = false)]
		public int MLStep
		{
			get { return m_step; }
			set
			{
				Dirty = true;
				m_step = value;
			}
		}
		
		// ML completition flag
		[DataElement(AllowDbNull = true)]
		public bool StepCompleted
		{
			get { return m_stepcompleted; }
			set
			{
				Dirty = true;
				m_stepcompleted = value;
			}
		}

		// Validation date (for tracking purpose)
		[DataElement(AllowDbNull = true)]
		public DateTime ValidationDate
		{
			get { return m_validationdate; }
			set
			{
				Dirty = true;
				m_validationdate = value;
			}
		}
	}
}
