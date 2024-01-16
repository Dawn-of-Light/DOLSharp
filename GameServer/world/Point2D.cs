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
    [Obsolete("Use Coordinate instead!")]
	public class Point2D : IPoint2D
	{
		public const double HEADING_TO_RADIAN = (360.0/4096.0)*(Math.PI/180.0);
		public const double RADIAN_TO_HEADING = (180.0/Math.PI)*(4096.0/360.0);

		public Point2D() { }

		public Point2D(int x, int y)
		{
			X = x;
			Y = y;
		}

        public Point2D(IPoint2D point)
            : this(point.X, point.Y) { }

        #region IPoint2D Members

        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

		// Coordinate calculation functions in DOL are standard trigonometric functions, but
		// with some adjustments to account for the different coordinate system that DOL uses
		// compared to the standard Cartesian coordinates used in trigonometry.
		//
		// Cartesian grid:
		//        90
		//         |
		// 180 --------- 0
		//         |
		//        270
		//        
		// DOL Heading grid:
		//       2048
		//         |
		// 1024 ------- 3072
		//         |
		//         0
		// 
		// The Cartesian grid is 0 at the right side of the X-axis and increases counter-clockwise.
		// The DOL Heading grid is 0 at the bottom of the Y-axis and increases clockwise.
		// General trigonometry and the System.Math library use the Cartesian grid.

		public ushort GetHeading(IPoint2D point)
		{
			float dx = point.X - X;
			float dy = point.Y - Y;

			double heading = Math.Atan2(-dx, dy)*RADIAN_TO_HEADING;

			if (heading < 0)
				heading += 4096;

			return (ushort) heading;
		}

		public Point2D GetPointFromHeading(ushort heading, int distance)
		{
			double angle = heading*HEADING_TO_RADIAN;
			double targetX = X - (Math.Sin(angle)*distance);
			double targetY = Y + (Math.Cos(angle)*distance);

			var point = new Point2D();

			if (targetX > 0)
				point.X = (int) targetX;
			else
				point.X = 0;

			if (targetY > 0)
				point.Y = (int) targetY;
			else
				point.Y = 0;

			return point;
		}

		public int GetDistance(IPoint2D point)
		{
			double dx = (double) X - point.X;
			double dy = (double) Y - point.Y;

			return (int) Math.Sqrt(dx*dx + dy*dy);
		}

		public virtual void Clear()
		{
			X = 0;
			Y = 0;
		}

		#endregion

		/// <summary>
		/// Creates the string representation of this point
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("({0}, {1})", X.ToString(), Y.ToString());
		}

		public bool IsWithinRadius(IPoint2D point, int radius)
		{
			if (radius > ushort.MaxValue)
			{
				return GetDistance(point) <= radius;
			}

			uint rsquared = (uint) radius*(uint) radius;

			int dx = X - point.X;

			long dist = ((long) dx)*dx;

			if (dist > rsquared)
			{
				return false;
			}

			int dy = Y - point.Y;

			dist += ((long) dy)*dy;

			if (dist > rsquared)
			{
				return false;
			}

			return true;
		}
	}
}