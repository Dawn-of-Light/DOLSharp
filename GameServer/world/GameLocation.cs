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
		public static int GetDistance(IGameLocation p1, IGameLocation p2)
		{
			if (p1.RegionID == p2.RegionID) 
			{
				return GetDistance(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);
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

		/// <summary>
		/// calculates distance between 2 locations
		/// </summary>
		/// <param name="r1"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="z1"></param>
		/// <param name="r2"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="z2"></param>
		/// <returns></returns>
		public static int GetDistance(int r1, int x1, int y1, int z1, int r2, int x2, int y2, int z2)
		{			
			if (r1 == r2) 
			{
				long xdiff = (long)x1-x2;
				long ydiff = (long)y1-y2;
				long zdiff = (long)z1-z2;
				return (int)Math.Sqrt(xdiff*xdiff + ydiff*ydiff + zdiff*zdiff);
			} 
			else 
			{
				return -1;
			}
		}
	}
}
