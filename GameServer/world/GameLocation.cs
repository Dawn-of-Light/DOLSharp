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
using DOL.GS.Geometry;

namespace DOL.GS
{
    [Obsolete("Use DOL.GS.Geometry.Position instead!")]
    public class GameLocation : IGameLocation
    {
        public GameLocation(String name, ushort regionId, ushort zoneId, int x, int y, int z, ushort heading)
            : this(name, Position.Create(regionId, Coordinate.Create(x,y,z)+WorldMgr.GetZone(zoneId).Offset, heading)) { }

        public GameLocation(String name, ushort regionId, int x, int y, int z)
            : this(name, Position.Create(regionId, x, y, z, heading: 0)) { }

        public GameLocation(String name, ushort regionId, int x, int y, int z, ushort heading)
            : this(name, Position.Create(regionId, x, y, z, heading)) { }

        public GameLocation(String name, Position position)
        {
            Position = position;
            Name = name;
        }

        public GameLocation(Position position)
        {
            Position = position;
        }

        public Position Position { get; set; } = Position.Zero;

        public int X
        {
            get => Position.X;
            set => Position.With(x: value);
        }

        public int Y
        {
            get => Position.Y;
            set => Position.With(y: value);
        }

        public int Z
        {
            get => Position.Z;
            set => Position.With(z: value);
        }

        public ushort Heading
        {
            get => Position.Orientation.InHeading;
            set => Position.With(heading: value);
        }

        public ushort RegionID
        {
            get => Position.RegionID;
            set => Position.With(regionID: value);
        }

        public String Name { get; set; } = null;

        public int GetDistance(IGameLocation location)
        {
            if (this.RegionID != location.RegionID) return -1;

            return (int)Position.Coordinate.DistanceTo(location.Position.Coordinate);
        }

		public static int ConvertLocalXToGlobalX(int localX, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return z.Offset.X + localX;
		}

		public static int ConvertLocalYToGlobalY(int localY, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return z.Offset.Y + localY;
		}

		public static int ConvertGlobalXToLocalX(int globalX, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return globalX - z.Offset.X;
		}

		public static int ConvertGlobalYToLocalY(int globalY, ushort zoneId)
		{
			Zone z = WorldMgr.GetZone(zoneId);
			return globalY - z.Offset.Y;
		}

        public static int GetDistance( int r1, int x1, int y1, int z1, int r2, int x2, int y2, int z2 )
        {
            if (r1 != r2) return -1;
            var loc1 = Coordinate.Create(x1,y1,z1);
            var loc2 = Coordinate.Create(x2,y2,z2);

            return (int)loc1.DistanceTo(loc2);
        }
	}
}
