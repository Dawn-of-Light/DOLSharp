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
	/// Represents TranslationMatrix
	/// </summary> 
	public class Quaternion
	{
		#region Private Properties

		/// <summary>
		/// real part of quaternion
		/// </summary>
		private double p_w;

		/// <summary>
		/// imaginary part of quaternion (i)
		/// </summary>
		private double p_x;

		/// <summary>
		/// imaginary part of quaternion (j)
		/// </summary>
		private double p_y;

		/// <summary>
		/// imaginary part of quaternion (k)
		/// </summary>
		private double p_z;

		#endregion

		#region get/set

		/// <summary>
		/// real part of quaternion
		/// </summary>
		public double W
		{
			get { return p_w; }
			set { p_w = value; }
		}

		/// <summary>
		/// imaginary _i_ part of quaternion
		/// </summary>
		public double X
		{
			get { return p_x; }
			set { p_x = value; }
		}

		/// <summary>
		/// imaginary _j_ part of quaternion
		/// </summary>
		public double Y
		{
			get { return p_y; }
			set { p_y = value; }
		}

		/// <summary>
		/// imaginary _k_ part of quaternion
		/// </summary>
		public double Z
		{
			get { return p_z; }
			set { p_z = value; }
		}

		/// <summary>
		/// get/set of imaginary part of quaternion
		/// </summary>
		public Vector3 ImaginaryVector
		{
			get 
			{ 
				return new Vector3(p_x, p_y, p_z); 
			}
			set
			{
				p_x = value.X;
				p_y = value.Y;
				p_z = value.Z;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates empty quaternion
		/// </summary>
		public Quaternion()
		{
			p_w = p_x = p_y = p_z = 0;
		}

		/// <summary>
		/// Creates quaternion using parameters
		/// </summary>
		/// <param name="cW">real part</param>
		/// <param name="cX">imaginary i part</param>
		/// <param name="cY">imaginary j part</param>
		/// <param name="cZ">imaginary k part</param>
		public Quaternion(double cW, double cX, double cY, double cZ)
		{
			p_w = cW;
			p_x = cX;
			p_y = cY;
			p_z = cZ;
		}

		/// <summary>
		/// Creates quaternion using parameters
		/// </summary>
		/// <param name="cW">real part</param>
		/// <param name="vec">vector presenting imaginary part</param>
		public Quaternion(double cW, Vector3 vec)
		{
			p_w = cW;
			ImaginaryVector = vec;
		}

		#endregion

		#region Calculations

		/// <summary>
		/// Adds two quaternions
		/// </summary>
		/// <param name="q1">quaternion 1</param>
		/// <param name="q2">quaternion 2</param>
		/// <returns>q1+q2</returns>
		public static Quaternion Add(Quaternion q1, Quaternion q2)
		{
			return new Quaternion(
				q1.W + q2.W,
				q1.X + q2.X,
				q1.Y + q2.Y,
				q1.Z + q2.Z);
		}

		/// <summary>
		/// Subtracts two quaternions
		/// </summary>
		/// <param name="q1">Subtracted quaternion</param>
		/// <param name="q2">Subtracting quaternion</param>
		/// <returns>q1-q2</returns>
		public static Quaternion Subtract(Quaternion q1, Quaternion q2)
		{
			return new Quaternion(
				q1.W - q2.W,
				q1.X - q2.X,
				q1.Y - q2.Y,
				q1.Z - q2.Z);
		}

		/// <summary>
		/// Quaternion multiplication
		/// </summary>
		/// <param name="q1">Multiplicated quaternion</param>
		/// <param name="q2">Multiplicating quaternion</param>
		/// <returns>q1*q2</returns>
		public static Quaternion Multiply(Quaternion q1, Quaternion q2)
		{
			Quaternion result = new Quaternion();
			result.W = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
			result.X = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
			result.Y = q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z;
			result.Z = q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X;

			return result;
		}

		/// <summary>
		/// Quaternion multiplication with scalar
		/// </summary>
		/// <param name="q1">Multiplicated quaternion</param>
		/// <param name="d">Multiplier</param>
		/// <returns>q1*d</returns>
		public static Quaternion Multiply(Quaternion q1, double d)
		{
			return new Quaternion(
				q1.W * d,
				q1.X * d,
				q1.Y * d,
				q1.Z * d);
		}

		/// <summary>
		/// Scalar multiplication of two quaternions
		/// </summary>
		/// <param name="q1">q1</param>
		/// <param name="q2">q2</param>
		/// <returns>q1.q2</returns>
		public static double DotProduct(Quaternion q1, Quaternion q2)
		{
			return q1.W * q2.W + q1.X * q2.X + q1.Y * q2.Y + q1.Z * q2.Z;
		}

		/// <summary>
		/// Quaternion size
		/// </summary>
		/// <returns>|this quaternion|</returns>
		public double Size()
		{
			return Math.Sqrt(p_w * p_w + p_x * p_x + p_y * p_y + p_z * p_z);
		}

		/// <summary>
		/// Conjugation to current quaternion
		/// </summary>
		/// <returns>q' = (w,-x,-y,-z)</returns>
		public Quaternion Conjugate()
		{
			return new Quaternion(p_w, -p_x, -p_y, -p_z);
		}

		/// <summary>
		/// Performs normalization of quaternion
		/// </summary>
		public void Normalize()
		{
			double size = this.Size();

			if (size != 0)
			{
				p_w /= size;
				p_x /= size;
				p_y /= size;
				p_z /= size;
			}
		}
		
		#endregion

		#region Operators

		/// <summary>
		/// Adds two quaternions
		/// </summary>
		/// <param name="q1">quaternion 1</param>
		/// <param name="q2">quaternion 2</param>
		/// <returns>q1+q2</returns>
		public static Quaternion operator +(Quaternion q1, Quaternion q2)
		{
			return Quaternion.Add(q1, q2);
		}

		/// <summary>
		/// Subtracts two quaternions
		/// </summary>
		/// <param name="q1">Subtracted quaternion</param>
		/// <param name="q2">Subtracting quaternion</param>
		/// <returns>q1-q2</returns>
		public static Quaternion operator -(Quaternion q1, Quaternion q2)
		{
			return Quaternion.Subtract(q1, q2);
		}

		/// <summary>
		/// Quaternion multiplication
		/// </summary>
		/// <param name="q1">Multiplicated quaternion</param>
		/// <param name="q2">Multiplicating quaternion</param>
		/// <returns>q1*q2</returns>
		public static Quaternion operator *(Quaternion q1, Quaternion q2)
		{
			return Quaternion.Multiply(q1, q2);
		}

		/// <summary>
		/// Quaternion multiplication with scalar
		/// </summary>
		/// <param name="q1">Multiplicated quaternion</param>
		/// <param name="d">Multiplier</param>
		/// <returns>q1*d</returns>
		public static Quaternion operator *(Quaternion q1, double d)
		{
			return Quaternion.Multiply(q1, d);
		}

		#endregion


	}
}
