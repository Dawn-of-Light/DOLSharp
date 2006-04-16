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

namespace DOL.GS
{
	/// <summary>
	/// 
	/// </summary>
	public class GameLocation : IGameLocation
	{
		protected Region m_region;
		protected Point m_position;
		protected ushort m_heading;
		protected String m_name;		
		
		public GameLocation(String name, ushort regionId, ushort zoneId, int x, int y, int z, ushort heading)
			: this(name, regionId, zoneId, new Point(x, y, z), heading)
		{
		}

		public GameLocation(String name, ushort regionId, ushort zoneId, Point position, ushort heading)
			: this(name, WorldMgr.GetRegion(regionId), WorldMgr.GetRegion(regionId).GetZone(zoneId).ToRegionPosition(position), heading)
		{
		}

		public GameLocation(String name, ushort regionId, int x, int y, int z)
			: this(name, regionId, new Point(x, y, z))
		{
		}

		public GameLocation(String name, ushort regionId, Point position)
			: this(name, WorldMgr.GetRegion(regionId), position, 0)
		{
		}

		public GameLocation(String name, ushort regionId, int x, int y, int z, ushort heading)
			: this(name, WorldMgr.GetRegion(regionId), new Point(x, y, z), heading)
		{
		}

		public GameLocation(String name, Region region, Point position, ushort heading)
		{
			m_region = region;
			m_position = position;
			m_name = name;
			m_heading = heading;
		}

		/// <summary>
		/// Gets or sets the object's region.
		/// </summary>
		public Region Region
		{
			get { return m_region; }
			set { m_region = value; }
		}

		/// <summary>
		/// Gets or sets the object's position in the region.
		/// </summary>
		public Point Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		/// <summary>
		/// heading of this point
		/// </summary>
		public ushort Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}

		/// <summary>
		/// Name of this point
		/// </summary>
		public String Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
	}
}
