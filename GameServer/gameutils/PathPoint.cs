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
using DOL.Database;
using DOL.GS.Geometry;

namespace DOL.GS.Movement
{
    public class PathPoint
    {
        public Coordinate Coordinate { get; set; }

        [Obsolete("Use .Coordinate instead!")]
        public int X { get => Coordinate.X; set => Coordinate.With(x: value); }
        [Obsolete("Use .Coordinate instead!")]
        public int Y { get => Coordinate.Y; set => Coordinate.With(y: value); }
        [Obsolete("Use .Coordinate instead!")]
        public int Z { get => Coordinate.Z; set => Coordinate.With(z: value); }

        [Obsolete("This is going to be removed.")]
        public PathPoint(PathPoint pp) : this(pp.Coordinate, pp.MaxSpeed, pp.Type) { }

        public PathPoint(Coordinate coordinate, int maxspeed, ePathType type)
        {
            Coordinate = coordinate;
            MaxSpeed = maxspeed;
            Type = type;
        }

        public PathPoint(int x, int y, int z, int maxspeed, ePathType type)
            : this(Coordinate.Create(x, y, z), maxspeed, type) { }

        public PathPoint(DBPathPoint dbEntry, ePathType type)
        {
            Coordinate = Coordinate.Create(dbEntry.X, dbEntry.Y, dbEntry.Z);
            MaxSpeed = dbEntry.MaxSpeed;
            WaitTime = dbEntry.WaitTime;
            Type = type;
        }

        public Angle AngleToNextPathPoint
            => Coordinate.GetOrientationTo(Next.Coordinate);

        public int MaxSpeed { get; set; }

        public PathPoint Prev { get; set; }
        public PathPoint Next { get; set; }

        /// <summary>
        /// flag toggle when go through pathpoint
        /// </summary>
        public bool FiredFlag { get; set; } = false;

        public ePathType Type { get; set; }

        public int WaitTime { get; set; } = 0;

        public DBPathPoint GenerateDbEntry()
        {
            var dbPathPoint = new DBPathPoint();
            dbPathPoint.X = Coordinate.X;
            dbPathPoint.Y = Coordinate.Y;
            dbPathPoint.Z = Coordinate.Z;
            dbPathPoint.MaxSpeed = MaxSpeed;
            dbPathPoint.WaitTime = WaitTime;
            return dbPathPoint;
        }
    }
}
