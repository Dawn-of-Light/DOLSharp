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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct ActiveQuestEntity
	{
		private int m_id;
		private int m_persistantGameObject;
		private string m_questType;
		private byte m_step1;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int PersistantGameObject
		{
			get { return m_persistantGameObject; }
			set { m_persistantGameObject = value; }
		}
		public string QuestType
		{
			get { return m_questType; }
			set { m_questType = value; }
		}
		public byte Step1
		{
			get { return m_step1; }
			set { m_step1 = value; }
		}
	}
}
