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
using System.Text;
using System;

namespace DOL.Geometry
{
	/// <summary>
	/// Point2D is a class to represent any 2D point
	/// </summary>
	public class Point3D : Point2D, IPoint
	{
		#region Properties
		/// <summary>
		/// Z coordinates
		/// </summary>
		private int m_z;
		
		#endregion

		#region get/set

		/// <summary>
		/// Z coordinate
		/// </summary>
		public int Z
		{
			get { return m_z;}
			set { m_z = value;}
		}

		#endregion

		#region Calculations

		/// <summary>
		/// Calculates distance to choosen point
		/// </summary>
		/// <param name="point">2D or 3D point</param>
		/// <returns>distance of two points</returns>
		public new double DistanceTo(IPoint point)
		{
			Point3D p3D = point as Point3D;

			int xDiff = this.X - point.X;
			int yDiff = this.Y - point.Y;

			double result;

			if (p3D != null)
			{
				int zDiff = this.Z - p3D.Z;
				result = Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
			}
			else
			{
				result = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
			}

			return result;

		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates Point [X,Y,Z] = [0,0,0]
		/// </summary>
		public Point3D()
			: base()
		{
			m_z = 0;
		}

		/// <summary>
		/// Creates Point with given coordinates
		/// </summary>
		/// <param name="cX">X coordinate</param>
		/// <param name="cY">Y coordinate</param>
		/// <param name="cZ">Z coordinate</param>
		public Point3D(int cX, int cY, int cZ)
			: base(cX, cY)
		{
			m_z = cZ;
		}

		#endregion

		#region Operators

		/// <summary>
		/// compares whether two points have identical coordinates
		/// </summary>
		/// <param name="p1">point 1</param>
		/// <param name="p2">point 2</param>
		/// <returns></returns>
		public static bool operator ==(Point3D p1, Point3D p2)
		{
			return ((p1.X == p2.X) && (p1.Y == p2.Y) && (p1.Z == p2.Z));
		}

		/// <summary>
		/// compares whether two points don't have identical coordinates
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static bool operator !=(Point3D p1, Point3D p2)
		{
			return !(p1 == p2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ m_z;
		}

		public override bool Equals(object obj)
		{
			if (obj is Point3D == false)
				return false;
			Point3D point = (Point3D)obj;
			return point == this;
		}

		#endregion
	}
}
