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
	public struct JumpPointMultiTargetLocationEntity
	{
		private int m_heading;
		private int m_jumpPoint;
		private byte m_realm;
		private int m_region;
		private int m_x;
		private int m_y;
		private int m_z;

		public int Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}
		public int JumpPoint
		{
			get { return m_jumpPoint; }
			set { m_jumpPoint = value; }
		}
		public byte Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}
		public int Region
		{
			get { return m_region; }
			set { m_region = value; }
		}
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}
	}
}
