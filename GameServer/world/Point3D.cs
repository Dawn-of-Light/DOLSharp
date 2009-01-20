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
	/// Defines a 3D point
	/// </summary>
	public class Point3D : Point2D, IPoint3D
	{
		/// <summary>
		/// The Z coord of this point
		/// </summary>
		protected int m_z;

		/// <summary>
		/// Constructs a new 3D point object
		/// </summary>
		public Point3D() : base(0, 0)
		{
		}

		/// <summary>
		/// Constructs a new 3D point object
		/// </summary>
		/// <param name="x">The X coord</param>
		/// <param name="y">The Y coord</param>
		/// <param name="z">The Z coord</param>
		public Point3D(int x, int y, int z) : base(x, y)
		{
			m_z = z;
		}

		/// <summary>
		/// Constructs a new 3D point object
		/// </summary>
		/// <param name="point">2D point</param>
		/// <param name="z">Z coord</param>
		public Point3D(IPoint2D point, int z) : this(point.X, point.Y, z)
		{
		}

		/// <summary>
		/// Constructs a new 3D point object
		/// </summary>
		/// <param name="point">3D point</param>
		public Point3D(IPoint3D point) : this(point.X, point.Y, point.Z)
		{
		}

		/// <summary>
		/// Z coord of this point
		/// </summary>
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}

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
			return string.Format("({0}, {1}, {2})", m_x.ToString(), m_y.ToString(), m_z.ToString());
		}

		/// <summary>
		/// Compares this point to any object
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if object is IPoint3D and equals this point</returns>
		public override bool Equals(object obj)
		{
			IPoint3D point = obj as IPoint3D;
			if (point == null)
				return false;
			return ((point.X == m_x) && (point.Y == m_y) && (point.Z == m_z));
		}

		/// <summary>
		/// calculates distance between 2 points
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static int GetDistance(IPoint3D p1, IPoint3D p2)
		{
			return GetDistance(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);
		}

		/// <summary>
		/// calculates distance between 2 points
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="z1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="z2"></param>
		/// <returns></returns>
		public static int GetDistance(int x1, int y1, int z1, int x2, int y2, int z2)
		{			
			long xdiff = (long)x1-x2;
			long ydiff = (long)y1-y2;
			long zdiff = (long)z1-z2;
			return (int)Math.Sqrt(xdiff*xdiff + ydiff*ydiff + zdiff*zdiff);
		}
	}
}
