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
	public struct RegionEntity
	{
		private int m_id;
		private string m_description;
		private byte m_expansion;
		private bool m_isDivingEnabled;
		private bool m_isDungeon;
		private bool m_isInstance;
		private byte m_type;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}
		public byte Expansion
		{
			get { return m_expansion; }
			set { m_expansion = value; }
		}
		public bool IsDivingEnabled
		{
			get { return m_isDivingEnabled; }
			set { m_isDivingEnabled = value; }
		}
		public bool IsDungeon
		{
			get { return m_isDungeon; }
			set { m_isDungeon = value; }
		}
		public bool IsInstance
		{
			get { return m_isInstance; }
			set { m_isInstance = value; }
		}
		public byte Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
	}
}
