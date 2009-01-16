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
	public class GameLocation : Point3D, IGameLocation
	{
		protected ushort m_regionId;
		protected ushort m_heading;
		protected String m_name;		
		
		public GameLocation(String name, ushort regionId, ushort zoneId, int x, int y, int z, ushort heading) : this(name,regionId,ConvertLocalXToGlobalX(x, zoneId),ConvertLocalYToGlobalY(y, zoneId),z, heading)
		{
		}

		public GameLocation(String name, ushort regionId, int x, int y, int z) : this(name, regionId, x, y, z, 0)
		{
		}

		public GameLocation(String name, ushort regionId, int x, int y, int z, ushort heading) : base(x, y, z)
		{
			m_regionId = regionId;
			m_name = name;
			m_heading = heading;
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
		/// RegionID of this point
		/// </summary>
		public ushort RegionID
		{
			get { return m_regionId; }
			set { m_regionId = value; }
		}

		/// <summary>
		/// Name of this point
		/// </summary>
		public String Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
	
		/// <summary>
		/// calculates distance between 2 points
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public int GetDistance( IGameLocation location )
		{
			if (this.RegionID == location.RegionID)
			{
				return base.GetDistance( location );
			}
			else
			{
				return -1;
			}
		}

		public static int ConvertLocalXToGlobalX(int localX, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return z.XOffset + localX;
		}

		public static int ConvertLocalYToGlobalY(int localY, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return z.YOffset + localY;
		}

		public static int ConvertGlobalXToLocalX(int globalX, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return globalX - z.XOffset;
		}

		public static int ConvertGlobalYToLocalY(int globalY, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return globalY - z.YOffset;
		}

        [Obsolete( "Use instance method GetDistance( IGameLocation location )" )]
        public static int GetDistance( int r1, int x1, int y1, int z1, int r2, int x2, int y2, int z2 )
        {
            GameLocation loc1 = new GameLocation( "loc1", (ushort)r1, x1, y1, z1 );
            GameLocation loc2 = new GameLocation( "loc2", (ushort)r2, x2, y2, z2 );

            return loc1.GetDistance( loc2 );
        }
	}
}
