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
	public class PointF3D : IPoint3D
	{
		#region Properties
		
		/// <summary>
		/// X coordinates
		/// </summary>
		private double m_x;

		/// <summary>
		/// Y coordinates
		/// </summary>
		private double m_y;

		/// <summary>
		/// Z coordinates
		/// </summary>
		private double m_z;

		
		#endregion

		#region get/set

		/// <summary>
		/// X coordinate
		/// </summary>
		public double X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// Y coordinate
		/// </summary>
		public double Y
		{
			get { return m_y;}
			set { m_y = value;}
		}

		/// <summary>
		/// Z coordinate
		/// </summary>
		public double Z
		{
			get { return m_z; }
			set { m_z = value; }
		}

		#endregion

		#region Calculations

		/// <summary>
		/// Calculates distance to choosen point
		/// </summary>
		/// <param name="point">3D point</param>
		/// <returns>distance of two points</returns>
		public new double DistanceTo(IPoint3D point)
		{
			double result;

			double xDiff = this.X - (double)point.X;
			double yDiff = this.Y - (double)point.Y;
			double zDiff = this.Z - (double)point.Z;
				
			result = Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
			return result;

		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates Point [X,Y,Z] = [0.0,0.0,0.0]
		/// </summary>
		public PointF3D()
		{
			m_x = 0;
			m_y = 0;
			m_z = 0;
		}

		/// <summary>
		/// Creates Point with given coordinates
		/// </summary>
		/// <param name="cX">X coordinate</param>
		/// <param name="cY">Y coordinate</param>
		/// <param name="cZ">Z coordinate</param>
		public PointF3D(double cX, double cY, double cZ)
		{
			m_x = cX;
			m_y = cY;
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
		public static bool operator ==(PointF3D p1, PointF3D p2)
		{
			return ((p1.X == p2.X) && (p1.Y == p2.Y) && (p1.Z == p2.Z));
		}

		/// <summary>
		/// compares whether two points don't have identical coordinates
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static bool operator !=(PointF3D p1, PointF3D p2)
		{
			return !(p1 == p2);
		}

		public override int GetHashCode()
		{
			return (int)m_x ^ (int)m_y ^ (int)m_z;
		}

		public override bool Equals(object obj)
		{
			if (obj is PointF3D == false)
				return false;
			PointF3D point = (PointF3D)obj;
			return point == this;
		}

		#endregion
	}
}
