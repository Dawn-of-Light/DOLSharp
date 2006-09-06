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
	/// Vector3 is a class to represent any 3D vector
	/// Uses active approach so recalculates data on change
	/// </summary>
	public class Vector3: Vector2, IVector, IPoint3D
	{
		#region Private Properties

		/// <summary>
		/// Z coordinate of the vector
		/// </summary>
		private double m_z;

		/// <summary>
		/// roll angle of vector
		/// </summary>
		private double m_roll;

		/// <summary>
		/// pitch angle of vector
		/// </summary>
		private double m_pitch;

		#endregion

		#region Public Properties

		/// <summary>
		/// get/set for Z coordinate of vector
		/// </summary>
		public double Z
		{
			get { return m_z; }
			set
			{
				m_z = value;
				Recalculate(eVectorDefinition.Coordinates);
			}
		}

		/// <summary>
		/// get/set for Z coordinate of vector
		/// </summary>
		public int ZInt
		{
			get { return (int)m_z; }
			set { Z = (double) value; }
		}


		/// <summary>
		/// get/set for roll angle vector is holding
		/// </summary>
		public double Roll
		{
			get { return m_roll; }
			set
			{
				m_roll = value;
				Recalculate(eVectorDefinition.Goniometric);
			}
		}

		/// <summary>
		/// get/set for pitch angle vector is holding
		/// </summary>
		public double Pitch
		{
			get { return m_pitch; }
			set
			{
				m_pitch = value;
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
					m_size = Math.Sqrt(X * X + Y * Y + Z * Z);
					m_yaw = Math.Tan(X / Y);
					m_roll = Math.Tan(Y / Z);
					m_pitch = Math.Tan(X / Z);
					break;

				//recalculates coordinates
				case eVectorDefinition.Goniometric:
					m_x = m_size * Math.Cos(m_yaw);
					m_y = m_size * Math.Sin(m_yaw);
					m_z = m_size * Math.Sin(m_roll);
					break;

				default: break;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates nul vector
		/// </summary>
		public Vector3()
		{
			m_x = 0;
			m_y = 0;
			m_z = 0;
			m_size = 0.0f;
			m_yaw = 0.0f;
			m_roll = 0.0f;
			m_pitch = 0.0f;
		}

		/// <summary>
		/// creates vector using INTEGER coordinates definition
		/// </summary>
		/// <param name="cX">X coordinate of vector</param>
		/// <param name="cY">Y coordinate of vector</param>
		/// <param name="cZ">Z coordinate of vector</param>
		public Vector3(int cX, int cY, int cZ)
		{
			m_x = (double)cX;
			m_y = (double)cY;
			m_z = (double)cZ;
			Recalculate(eVectorDefinition.Coordinates);
		}

		/// <summary>
		/// creates vector using DOUBLE coordinates definition
		/// </summary>
		/// <param name="cX">X coordinate of vector</param>
		/// <param name="cY">Y coordinate of vector</param>
		/// <param name="cZ">Z coordinate of vector</param>
		public Vector3(double cX, double cY, double cZ)
		{
			m_x = cX;
			m_y = cY;
			m_z = cZ;
			Recalculate(eVectorDefinition.Coordinates);
		}

		/// <summary>
		/// creates vector using size&angle definition
		/// </summary>
		/// <param name="cSize">Size of vector</param>
		/// <param name="cYaw">Yaw angle of vector</param>
		/// <param name="cRoll">Roll angle of vector</param>
		/// <param name="cPitch">Pitch angle of vector</param>
		public Vector3(double cSize, double cYaw, double cRoll, double cPitch)
		{
			m_size = cSize;
			m_yaw = cYaw;
			m_roll = cRoll;
			m_pitch = cPitch;
			Recalculate(eVectorDefinition.Goniometric);
		}

		/// <summary>
		/// creates vector using two 3D points
		/// </summary>
		/// <param name="from">source point</param>
		/// <param name="to">destination point</param>
		public Vector3(Point3D from, Point3D to)
		{
			m_x = to.X - from.X;
			m_y = to.Y - from.Y;
			m_z = to.Z - from.Z;
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
				.Append(" (X,Y,Z)=(").Append(m_x).Append(",").Append(m_y).Append(",").Append(m_z).Append(")")
				.Append(" size=").Append(m_size)
				.Append(" yaw=").Append(m_yaw).Append(" Rad")
				.Append(" roll=").Append(m_roll).Append(" Rad")
				.Append(" pitch=").Append(m_pitch).Append(" Rad")
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
		public static bool operator ==(Vector3 v1, Vector3 v2)
		{
			return ((v1.X == v2.X) && (v1.Y == v2.Y) && (v1.Z == v2.Z));
		}

		/// <summary>
		/// compares two vectors for difference
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>true if vectors are not identical</returns>
		public static bool operator !=(Vector3 v1, Vector3 v2)
		{
			return !(v1 == v2);
		}

		/// <summary>
		/// Adds two vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>vector1 + vector2</returns>
		public static Vector3 operator +(Vector3 v1, Vector3 v2)
		{
			Vector3 vec = new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
			return vec;
		}

		/// <summary>
		/// Substracts two vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>vector1 - vector2</returns>
		public static Vector3 operator -(Vector3 v1, Vector3 v2)
		{
			Vector3 vec = new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
			return vec;
		}

		/// <summary>
		/// Multiplies size of vector
		/// </summary>
		/// <param name="quoeficient">multiplication quoeficient</param>
		/// <param name="v">multiplied vector</param>
		/// <returns></returns>
		public static Vector3 operator *(double quoeficient, Vector3 v)
		{
			Vector3 vec = new Vector3(
				v.X * quoeficient, 
				v.Y * quoeficient,
				v.Z * quoeficient
				);
			return vec;
		}

		public override int GetHashCode()
		{
			return XInt ^ YInt ^ ZInt;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector3 == false)
				return false;
			Vector3 vector = (Vector3)obj;
			return vector == this;
		}

		#endregion

		#region Vector multiplication and rotation

		/// <summary>
		/// rotates vector around X axis by specified angle
		/// </summary>
		/// <param name="angle">rotation angle [Rad]</param>
		public void RotX(double angle)
		{
			this.Roll += angle;
		}

		/// <summary>
		/// rotates vector around Y axis by specified angle
		/// </summary>
		/// <param name="angle">rotation angle [Rad]</param>
		public void RotY(double angle)
		{
			this.Pitch += angle;
		}

		/// <summary>
		/// rotates vector around Z axis by specified angle
		/// </summary>
		/// <param name="angle">rotation angle [Rad]</param>
		public void RotZ(double angle)
		{
			this.Yaw += angle;
		}


		/// <summary>
		/// performs scalar multiplication of two vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>scalar multiplication (int)</returns>
		public static new double Scalar(IVector v1, IVector v2)
		{
			Vector2 v1_2D = v1 as Vector2;
			Vector2 v2_2D = v2 as Vector2;
			Vector3 v1_3D = v1 as Vector3;
			Vector3 v2_3D = v2 as Vector3;

			double result = v1.X * v2.X + v1.Y * v2.Y;

			// if we perform scalar multiplication of 2D and 3D vectors, 
			// the value of result changes only if BOTH are 3D
			if (v1_2D == null)
			{
				if (v2_2D == null)
				{
					result += v1_3D.Z * v2_3D.Z;
				}
			}

			return result;
		}

		/// <summary>
		/// Checks orthogonality of this and specified vector
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public new bool Orthogonal(IVector vector)
		{
			return Scalar(this, vector) == 0;
		}

		/// <summary>
		/// Performs vector multiplication of given vectors
		/// </summary>
		/// <param name="v1">vector 1</param>
		/// <param name="v2">vector 2</param>
		/// <returns>vector 1 x vector 2</returns>
		public static Vector3 Multiply(Vector3 v1, Vector3 v2)
		{
			Vector3 result = new Vector3(
				v1.Y * v2.Z - v1.Z * v2.Y,
				v1.Z * v2.X - v1.X * v2.Z,
				v1.X * v2.Y - v1.Y * v2.X);
			return result;
		}

		#endregion
	}
}
