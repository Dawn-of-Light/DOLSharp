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
	public struct AreaEntity
	{
		private int m_id;
		private string m_areaType;
		private string m_description;
		private int m_height;
		private bool m_isBroadcastEnabled;
		private int m_radius;
		private int m_regionId;
		private byte m_sound;
		private int m_width;
		private int m_x;
		private int m_y;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string AreaType
		{
			get { return m_areaType; }
			set { m_areaType = value; }
		}
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}
		public int Height
		{
			get { return m_height; }
			set { m_height = value; }
		}
		public bool IsBroadcastEnabled
		{
			get { return m_isBroadcastEnabled; }
			set { m_isBroadcastEnabled = value; }
		}
		public int Radius
		{
			get { return m_radius; }
			set { m_radius = value; }
		}
		public int RegionId
		{
			get { return m_regionId; }
			set { m_regionId = value; }
		}
		public byte Sound
		{
			get { return m_sound; }
			set { m_sound = value; }
		}
		public int Width
		{
			get { return m_width; }
			set { m_width = value; }
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
	}
}
