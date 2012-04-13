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
	[DataTable(TableName = "CharacterXDataQuestBackup")]
	public class CharacterXDataQuestBackup : CharacterXDataQuest
	{
		string m_deletedOwnerName = "";
		private DateTime m_deleteDate;

		public CharacterXDataQuestBackup()
		{
			m_deleteDate = DateTime.Now;
		}

		public CharacterXDataQuestBackup(DOLCharactersBackup deleted, CharacterXDataQuest data)
		{
			DeletedOwnerName = deleted.Name;
			m_deleteDate = DateTime.Now;

			m_id = data.ID;
			m_characterID = deleted.ObjectId;
			m_dataQuestID = data.DataQuestID;
			m_step = data.Step;
			m_count = data.Count;
		}

		/// <summary>
		/// Name of the character - indexed but not unique for backups
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public string DeletedOwnerName
		{
			get
			{
				return m_deletedOwnerName;
			}
			set
			{
				Dirty = true;
				m_deletedOwnerName = value;
			}
		}

		/// <summary>
		/// The deletion date of this character
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public DateTime DeleteDate
		{
			get
			{
				return m_deleteDate;
			}
			set
			{
				Dirty = true;
				m_deleteDate = value;
			}
		}
	}
}