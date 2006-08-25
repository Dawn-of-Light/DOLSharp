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
	public struct ActiveTaskEntity
	{
		private int m_abstractTask;
		private string m_itemName;
		private string m_rewardGiverName;
		private long m_startingPlayedTime;
		private bool m_targetKilled;
		private string m_targetMobName;
		private string m_taskType;

		public int AbstractTask
		{
			get { return m_abstractTask; }
			set { m_abstractTask = value; }
		}
		public string ItemName
		{
			get { return m_itemName; }
			set { m_itemName = value; }
		}
		public string RewardGiverName
		{
			get { return m_rewardGiverName; }
			set { m_rewardGiverName = value; }
		}
		public long StartingPlayedTime
		{
			get { return m_startingPlayedTime; }
			set { m_startingPlayedTime = value; }
		}
		public bool TargetKilled
		{
			get { return m_targetKilled; }
			set { m_targetKilled = value; }
		}
		public string TargetMobName
		{
			get { return m_targetMobName; }
			set { m_targetMobName = value; }
		}
		public string TaskType
		{
			get { return m_taskType; }
			set { m_taskType = value; }
		}
	}
}
