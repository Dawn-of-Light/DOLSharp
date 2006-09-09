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
	public class BrainEntity
	{
		private int m_aBrain;
		private string m_aBrainType;
		private int m_aggroLevel;
		private int m_aggroRange;

		public int ABrain
		{
			get { return m_aBrain; }
			set { m_aBrain = value; }
		}
		public string ABrainType
		{
			get { return m_aBrainType; }
			set { m_aBrainType = value; }
		}
		public int AggroLevel
		{
			get { return m_aggroLevel; }
			set { m_aggroLevel = value; }
		}
		public int AggroRange
		{
			get { return m_aggroRange; }
			set { m_aggroRange = value; }
		}
	}
}
