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
	/// represents a point in 2 dimensional space
	/// </summary>
	public class Point2D : IPoint2D
	{
		/// <summary>
		/// The X coord of this point
		/// </summary>
		protected int m_x;
		/// <summary>
		/// The Y coord of this point
		/// </summary>
		protected int m_y;

		/// <summary>
		/// This constant is used to calculate the heading quickly
		/// </summary>
		public const double HEADING_CONST = 651.89864690440329530934789477382;

		/// <summary>
		/// Constructs a new 2D point object
		/// </summary>
		public Point2D() : this(0, 0)
		{
		}

		/// <summary>
		/// Constructs a new 2D point object
		/// </summary>
		/// <param name="x">The X coord</param>
		/// <param name="y">The Y coord</param>
		public Point2D(int x, int y)
		{
			m_x = x;
			m_y = y;
		}

		/// <summary>
		/// Constructs a new 2D point object
		/// </summary>
		/// <param name="point">The 2D point</param>
		public Point2D(IPoint2D point) : this(point.X, point.Y)
		{
		}

		/// <summary>
		/// X coord of this point
		/// </summary>
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// Y coord of this point
		/// </summary>
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		/// <summary>
		/// Calculates the hashcode of this point
		/// </summary>
		/// <returns>The hashcode</returns>
		public override int GetHashCode()
		{
			return m_x ^ m_y;
		}

		/// <summary>
		/// Creates the string representation of this point
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("({0}, {1})", m_x.ToString(), m_y.ToString());
		}

		/// <summary>
		/// Compares this point to any object
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if object is IPoint2D and equals this point</returns>
		public override bool Equals(object obj)
		{
			IPoint2D point = obj as IPoint2D;
			if (point == null)
				return false;
			return point.X == m_x && point.Y == m_y;
		}

		/// <summary>
		/// calculate heading from point1 to point2
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static ushort GetHeadingToLocation(IPoint2D p1, IPoint2D p2)
		{
			float dx = (long)p2.X-p1.X;
			float dy = (long)p2.Y-p1.Y;
			ushort heading = (ushort)(Math.Atan2(-dx,dy)*HEADING_CONST);
			if (heading<0) heading += 0x1000;
			return heading;
		}
	}
}
