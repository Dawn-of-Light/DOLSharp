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
	public class Point2D : IPoint
	{
		#region Properties
		/// <summary>
		/// X,Y coordinates
		/// </summary>
		private int m_x;
		private int m_y;

		#endregion

		#region get/set

		/// <summary>
		/// X coordinate
		/// </summary>
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// Y coordinate
		/// </summary>
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		#endregion

		#region Calculations

		/// <summary>
		/// Returns distance to given point
		/// </summary>
		/// <param name="point">2D or 3D point</param>
		/// <returns>distance of 2 points</returns>
		public double DistanceTo(IPoint point)
		{
			int xDiff = this.X - point.X;
			int yDiff = this.Y - point.Y;
			return Math.Sqrt(xDiff*xDiff+yDiff*yDiff);
		}
		#endregion

		#region Constructor

		/// <summary>
		/// creates Point [X,Y] = [0,0]
		/// </summary>
		public Point2D()
		{
			m_x = 0;
			m_y = 0;
		}

		/// <summary>
		/// creates Point with given coordinates
		/// </summary>
		/// <param name="cX">X coordinate</param>
		/// <param name="cY">Y coordinate</param>
		public Point2D(int cX, int cY)
		{
			m_x = cX;
			m_y = cY;
		}

		#endregion

		#region Operators

		/// <summary>
		/// compares whether two points have identical coordinates
		/// </summary>
		/// <param name="p1">point 1</param>
		/// <param name="p2">point 2</param>
		/// <returns></returns>
		public static bool operator ==(Point2D p1, Point2D p2)
		{
			return ((p1.X == p2.X) && (p1.Y == p2.Y));
		}

		/// <summary>
		/// compares whether two points don't have identical coordinates
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static bool operator !=(Point2D p1, Point2D p2)
		{
			return !(p1==p2);
		}


		public override int GetHashCode()
		{
			return m_x ^ m_y;
		}

		public override bool Equals(object obj)
		{
			if (obj is Point2D == false)
				return false;
			Point2D point = (Point2D)obj;
			return point == this;
		}

		#endregion


	}
}
