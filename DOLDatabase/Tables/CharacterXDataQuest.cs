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
	/// Holds all the GenericQuests available
	/// </summary>
	[DataTable(TableName = "CharacterXDataQuest")]
	public class CharacterXDataQuest : DataObject
	{
		private int m_id;
		private string m_characterID;
		private int m_dataQuestID;
		private short m_step;
		private short m_count;

		public CharacterXDataQuest()
		{
		}

		/// <summary>
		/// Create a new entry for this quest
		/// </summary>
		/// <param name="characterID"></param>
		/// <param name="dataQuestID"></param>
		public CharacterXDataQuest(string characterID, int dataQuestID)
		{
			m_characterID = characterID;
			m_dataQuestID = dataQuestID;
			m_step = 1;
			m_count = 0;
		}

		[PrimaryKey(AutoIncrement=true)]
		public int ID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// DOLCharacters_ID of this player
		/// </summary>
		[DataElement(Varchar = 100, AllowDbNull = false, IndexColumns = "DataQuestID")]
		public string Character_ID
		{
			get { return m_characterID; }
			set { m_characterID = value; Dirty = true; }
		}

		/// <summary>
		/// The ID of the DataQuest
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int DataQuestID
		{
			get { return m_dataQuestID; }
			set { m_dataQuestID = value; Dirty = true; }
		}

		/// <summary>
		/// 0 = completed
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public short Step
		{
			get { return m_step; }
			set { m_step = value; Dirty = true; }
		}

		/// <summary>
		/// How many times has this player done this quest?
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public short Count
		{
			get { return m_count; }
			set { m_count = value; Dirty = true; }
		}

	}
}