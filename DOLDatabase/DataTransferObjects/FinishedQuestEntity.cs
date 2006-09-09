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
	public class FinishedQuestEntity
	{
		private byte m_count;
		private string m_finishedQuestType;
		private int m_persistantGameObject;

		public byte Count
		{
			get { return m_count; }
			set { m_count = value; }
		}
		public string FinishedQuestType
		{
			get { return m_finishedQuestType; }
			set { m_finishedQuestType = value; }
		}
		public int PersistantGameObject
		{
			get { return m_persistantGameObject; }
			set { m_persistantGameObject = value; }
		}
	}
}
