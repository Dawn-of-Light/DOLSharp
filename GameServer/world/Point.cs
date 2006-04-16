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
using DOL.GS.Database;

namespace DOL.GS
{
	/// <summary>
	/// Defines a 3D point.
	/// </summary>
	public struct Point
	{
		/// <summary>
		/// The X coord of this point
		/// </summary>
		public int m_x;

		/// <summary>
		/// Get or set the x coohor
		/// </summary>
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// The Y coord of this point
		/// </summary>
		public int m_y;

		/// <summary>
		/// Get or set the x coohor
		/// </summary>
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		/// <summary>
		/// The Z coord of this point
		/// </summary>
		public int m_z;

		/// <summary>
		/// Get or set the x coohor
		/// </summary>
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}

		/// <summary>
		/// Constructs a new 3D point and initialize its X, Y and Z coords.
		/// </summary>
		/// <param name="x">The X coord.</param>
		/// <param name="y">The Y coord.</param>
		/// <param name="z">The Z coord.</param>
		public Point(int x, int y, int z)
		{
			m_x = x;
			m_y = y;
			m_z = z;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Point"/> class.
		/// </summary>
		/// <param name="p">The DB point.</param>
		public Point(DBPoint p)
		{
			m_x = p.X;
			m_y = p.Y;
			m_z = p.Z;
		}

		/// <summary>
		/// The point with X, Y and Z coords set to zero.
		/// </summary>
		public static readonly Point Zero = new Point(0, 0, 0);

		/// <summary>
		/// Calculates the hashcode of this point
		/// </summary>
		/// <returns>The hashcode</returns>
		public override int GetHashCode()
		{
			return m_x ^ m_y ^ m_z;
		}

		/// <summary>
		/// Creates the string representation of this point
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", X.ToString(), Y.ToString(), Z.ToString());
		}

		/// <summary>
		/// Compares this point to any object.
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if object is IPoint3D and equals this point</returns>
		public override bool Equals(object obj)
		{
			if (obj is Point == false)
				return false;
			Point point = (Point) obj;
			return point == this;
		}
		
		/// <summary>
		/// Compares 2 points.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns>true if equal.</returns>
		public static bool operator== (Point p1, Point p2)
		{
			return (p1.X == p2.X) && (p1.Y == p2.Y) && (p1.Z == p2.Z);
		}
		
		/// <summary>
		/// Compares 2 points.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns>true if not equal.</returns>
		public static bool operator!= (Point p1, Point p2)
		{
			return (p1.X != p2.X) || (p1.Y != p2.Y) || (p1.Z != p2.Z);
		}
		
		/// <summary>
		/// Adds one point to another.
		/// </summary>
		/// <param name="p1">Point 1</param>
		/// <param name="p2">Point 2</param>
		/// <returns>Point where x, y and z are a sum of 2 points.</returns>
		public static Point operator+ (Point p1, Point p2)
		{
			p1.X += p2.X;
			p1.Y += p2.Y;
			p1.Z += p2.Z;
			return p1;
		}
		
		/// <summary>
		/// Substracts one point from another.
		/// </summary>
		/// <param name="p1">Point 1</param>
		/// <param name="p2">Point 2</param>
		/// <returns>Point where x, y and z are difference of 2 points.</returns>
		public static Point operator- (Point p1, Point p2)
		{
			p1.X -= p2.X;
			p1.Y -= p2.Y;
			p1.Z -= p2.Z;
			return p1;
		}

        /// <summary>
        /// multiply all coordonate by a value.
        /// </summary>
        /// <param name="quoeficient">the value</param>
        /// <param name="p2">Point 2</param>
        /// <returns>Point where x, y and z are multiply by the value.</returns>
        public static Point operator *(int quoeficient, Point p2)
        {
            Point p1 = new Point();
            p1.X = quoeficient * p2.X;
            p1.Y = quoeficient * p2.Y;
            p1.Z = quoeficient * p2.Z;
            return p1;
        }

		/// <summary>
		/// Calculates distance to another point.
		/// </summary>
		/// <param name="point">Another point for distance calculation.</param>
		/// <returns>Distance to another point.</returns>
		public int GetDistance(Point point)
		{
			double xdiff = (double)m_x-point.m_x;
			double ydiff = (double)m_y-point.m_y;
			//SH: Removed Z checks when one of the two Z values is zero(on ground)
			if (m_z == 0 || point.m_z == 0)
				return (int) Math.Sqrt(xdiff*xdiff + ydiff*ydiff);
			double zdiff = (double)m_z-point.m_z;
			return (int) Math.Sqrt(xdiff*xdiff + ydiff*ydiff + zdiff*zdiff);
		}
		
		/// <summary>
		/// Checks the distance to another point.
		/// </summary>
		/// <param name="point">Point for distance check.</param>
		/// <param name="distance">Distance to check.</param>
		/// <returns>True if actual distance to point is equals or less than given.</returns>
		public bool CheckDistance(Point point, int distance)
		{
			// check if square fits into 4 bytes
			if (distance <= ushort.MaxValue)
				return CheckSquareDistance(point, (uint)distance*(uint)distance);
			return distance >= GetDistance(point);
		}

		/// <summary>
		/// Checks the distance to another point.
		/// Should be used only with compile-time constants.
		/// </summary>
		/// <param name="point">Point for distance check.</param>
		/// <param name="squareDistance">Square distance for check.</param>
		/// <returns>True if actual distance to point is equals or less than given.</returns>
		public bool CheckSquareDistance(Point point, uint squareDistance)
		{
			int xDiff = m_x - point.m_x;
			long dist = ((long)xDiff) * xDiff;

			if (dist > squareDistance)
			{
				return false;
			}

			int yDiff = m_y - point.m_y;
			dist += ((long)yDiff) * yDiff;

			if (dist > squareDistance)
			{
				return false;
			}


			//SH: Removed Z checks when one of the two Z values is zero (on ground)
			if ((m_z != 0) && (point.m_z != 0))
			{
				int zDiff = m_z - point.m_z;
				dist += ((long)zDiff) * zDiff;

				if (dist > squareDistance)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// This constant is used to calculate the heading quickly
		/// </summary>
		public const double HEADING_CONST = 651.89864690440329530934789477382;

		/// <summary>
		/// Calculates heading to another point.
		/// </summary>
		/// <param name="point">Another point for heading calculation.</param>
		/// <returns>Heading to another point in client's units (0 .. 4095).</returns>
		public ushort GetHeadingTo(Point point)
		{
			double dx = (double)point.m_x-m_x;
			double dy = (double)point.m_y-m_y;
			int heading = (int) (Math.Atan2(-dx,dy)*HEADING_CONST);
			return (ushort) (heading & 0xFFF);
		}
	}
}
