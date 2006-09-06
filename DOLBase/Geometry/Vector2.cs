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
	/// Vector2 is a class to represent any 2D vector
	/// Uses active approach so recalculates data on change
	/// </summary>
	public class Vector2 : IVector
	{
		#region Private Properties

		/// <summary>
		/// X coordinate of the vector
		/// </summary>
		protected double m_x;

		/// <summary>
		/// Y coordinate of the vector
		/// </summary>
		protected double m_y;

		/// <summary>
		/// size of vector
		/// </summary>
		protected double m_size;

		/// <summary>
		/// direction of vector (Rad)
		/// </summary>
		protected double m_yaw;

		#endregion

		#region Public Properties

		/// <summary>
		/// get/set for X coordinate of vector
		/// </summary>
		public double X
		{
			get { return m_x; }
			set
			{
				m_x = value;
				Recalculate(eVectorDefinition.Coordinates);
			}
		}

		/// <summary>
		/// get/set for Y coordinate of vector
		/// </summary>
		public double Y
		{
			get { return m_y; }
			set
			{
				m_y = value;
				Recalculate(eVectorDefinition.Coordinates);
			}
		}

		/// <summary>
		/// get/set for X coordinate of vector
		/// </summary>
		public int XInt
		{
			get { return (int)m_x; }
			set { X = (double) value; }
		}

		/// <summary>
		/// get/set for X coordinate of vector
		/// </summary>
		public int YInt
		{
			get { return (int)m_y; }
			set { Y = (double) value; }
		}

		/// <summary>
		/// get/set for size of the vector
		/// </summary>
		public double Size
		{
			get { return m_size; }
			set
			{
				m_size = value;
				Recalculate(eVectorDefinition.Goniometric);
			}
		}

		/// <summary>
		/// get/set for yaw angle vector is holding
		/// </summary>
		public double Yaw
		{
			get { return m_yaw; }
			set
			{
				m_yaw = value;
				Recalculate(eVectorDefinition.Goniometric);
			}
		}

		#endregion

		# region Calculations

		/// <summary>
		/// performs internal recalculation of missing attributes
		/// </summary>
		/// <param name="kind">type of changed value</param>
		private void Recalculate(eVectorDefinition kind)
		{
			switch (kind)
			{
				//recalculates size and yaw
				case eVectorDefinition.Coordinates:
					m_size = Math.Sqrt(X * X + Y * Y);
					m_yaw = Math.Tan(X / Y);
					break;
				
				//recalculates coordinates
				case eVectorDefinition.Goniometric:
					m_x = m_size * Math.Cos(m_yaw);
					m_y = m_size * Math.Sin(m_yaw);
					break;
				
				default: break;
			}
		}

		/// <summary>
		/// generates vector rotated by pi/2
		/// </summary>
		/// <returns>orthogonal vector to current vector</returns>
		public Vector2 Orthogonal()
		{
			Vector2 result = new Vector2(-Y, X);
			return result;
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates nul vector
		/// </summary>
		public Vector2()
		{
			m_x = 0;
			m_y = 0;
			m_size = 0.0f;
			m_yaw = 0.0f;
		}

		/// <summary>
		/// creates vector using INTEGER coordinates definition
		/// </summary>
		/// <param name="cX">X coordinate of vector</param>
		/// <param name="cY">Y coordinate of vector</param>
		public Vector2(int cX, int cY)
		{
			m_x = (double)cX;
			m_y = (double)cY;
			Recalculate(eVectorDefinition.Coordinates);
		}

		/// <summary>
		/// creates vector using FLOAT coordinates definition
		/// </summary>
		/// <param name="cX">X coordinate of vector</param>
		/// <param name="cY">Y coordinate of vector</param>
		public Vector2(double cX, double cY)
		{
			m_x = cX;
			m_y = cY;
			Recalculate(eVectorDefinition.Coordinates);
		}


		/// <summary>
		/// creates vector using size&angle definition
		/// </summary>
		/// <param name="cSize">Size of vector</param>
		/// <param name="cYaw">Direction of vector</param>
		public Vector2(float cSize, float cYaw)
		{
			m_size = (double) cSize;
			m_yaw = (double) cYaw;
			Recalculate(eVectorDefinition.Goniometric);
		}

		/// <summary>
		/// creates vector using two 2D points
		/// </summary>
		/// <param name="from">source point</param>
		/// <param name="to">destination point</param>
		public Vector2(IPoint from, IPoint to)
		{
			m_x = to.X - from.X;
			m_y = to.Y - from.Y;
			Recalculate(eVectorDefinition.Coordinates);
		}

		#endregion

		#region ToString

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" (X,Y)=(").Append(m_x).Append(",").Append(m_y).Append(")")
				.Append(" size=").Append(m_size)
				.Append(" yaw=").Append(m_yaw).Append(" Rad")
				.ToString();
		}

		#endregion

		#region Operators

		/// <summary>
		/// compares two vectors for identity
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>true if vectors are identical</returns>
		public static bool operator ==(Vector2 v1, Vector2 v2)
		{
			return ((v1.X == v2.X) && (v1.Y == v2.Y));
		}

		/// <summary>
		/// compares two vectors for difference
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>true if vectors are not identical</returns>
		public static bool operator !=(Vector2 v1, Vector2 v2)
		{
			return !(v1 == v2);
		}

		/// <summary>
		/// Adds two vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>vector1 + vector2</returns>
		public static Vector2 operator +(Vector2 v1, Vector2 v2)
		{
			Vector2 vec = new Vector2(v1.X + v2.X, v1.Y + v2.Y);
			return vec;
		}

		/// <summary>
		/// Substracts two vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>vector1 - vector2</returns>
		public static Vector2 operator -(Vector2 v1, Vector2 v2)
		{
			Vector2 vec = new Vector2(v1.X - v2.X, v1.Y - v2.Y);
			return vec;
		}

		/// <summary>
		/// Multiplies size of vector
		/// </summary>
		/// <param name="quoeficient">multiplication quoeficient</param>
		/// <param name="v">multiplied vector</param>
		/// <returns></returns>
		public static Vector2 operator *(double quoeficient, Vector2 v)
		{
			Vector2 vec = new Vector2((int)Math.Round(v.X * quoeficient),(int)Math.Round( v.Y * quoeficient));
			return vec;
		}

		public override int GetHashCode()
		{
			return XInt ^ YInt;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector2 == false)
				return false;
			Vector2 vector = (Vector2)obj;
			return vector == this;
		}

		#endregion

		#region Vector multiplication and rotation
		
		/// <summary>
		/// rotates vector by specified angle
		/// </summary>
		/// <param name="angle">rotation angle [Rad]</param>
		public void Rotate(double angle)
		{
			this.Yaw += angle;
			Recalculate(eVectorDefinition.Goniometric);
		}

		/// <summary>
		/// performs scalar multiplication of two vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>scalar multiplication (int)</returns>
		public static double Scalar(IVector v1, IVector v2)
		{
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		/// <summary>
		/// Checks orthogonality of this and specified vector
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public bool Orthogonal(IVector vector)
		{
			return Scalar(this, vector) == 0;
		}

		#endregion

	}
}
