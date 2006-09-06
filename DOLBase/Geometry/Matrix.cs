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
	/// Represents 4x4 matrix
	/// </summary>
	public class Matrix : IMatrix
	{
		#region Properties

		/// <summary>
		/// array for calculation of determinant
		/// </summary>
		private static int[] DETERMINANT_ARRAY = 
		{
			0,0,0,1,1,2,8,8,8,9,9,10,
			5,6,7,6,7,7,13,14,15,14,15,15,
			1,2,3,2,3,3,9,10,11,10,11,11,
			4,4,4,5,5,6,12,12,12,13,13,14
		};


		/// <summary>
		/// Defines width and height of the matrix
		/// </summary>
		private const int MATRIX_WIDTH = 4;
		private const int MATRIX_HEIGHT = 4;

		/// <summary>
		/// Array keeping values of Matrix;
		/// </summary>
		protected float[] cells;

		#endregion

		#region get/set

		/// <summary>
		/// sets value of matrix cell
		/// </summary>
		/// <param name="x">X coordinate [1..4]</param>
		/// <param name="y">Y coordinate [1..4]</param>
		/// <param name="value">value of cell</param>
		public void SetCell(int x, int y, float value)
		{
			if ((x > MATRIX_WIDTH) || (y > MATRIX_HEIGHT))
				return;
			cells[MATRIX_WIDTH * (y - 1) + (x - 1)] = value;
		}

		/// <summary>
		/// returns value of matrix cell
		/// </summary>
		/// <param name="x">X coordinate [1..4]</param>
		/// <param name="y">Y coordinate [1..4]</param>
		/// <returns>value of cell</returns>
		public float GetCell(int x, int y)
		{
			if ((x > MATRIX_WIDTH) || (y > MATRIX_HEIGHT))
				return 0.0f;
			return cells[MATRIX_WIDTH * (y - 1) + (x - 1)];
		}

		/// <summary>
		/// Matrix [Y,X] - [1,1] value
		/// </summary>
		public float _11
		{
			get { return GetCell(1, 1); }
			set { SetCell(1, 1, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [1,2] value
		/// </summary>
		public float _12
		{
			get { return GetCell(2, 1); }
			set { SetCell(2, 1, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [1,3] value
		/// </summary>
		public float _13
		{
			get { return GetCell(3, 1); }
			set { SetCell(3, 1, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [1,4] value
		/// </summary>
		public float _14
		{
			get { return GetCell(4, 1); }
			set { SetCell(4, 1, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [2,1] value
		/// </summary>
		public float _21
		{
			get { return GetCell(1, 2); }
			set { SetCell(1, 2, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [2,2] value
		/// </summary>
		public float _22
		{
			get { return GetCell(2, 2); }
			set { SetCell(2, 2, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [2,3] value
		/// </summary>
		public float _23
		{
			get { return GetCell(3, 2); }
			set { SetCell(3, 2, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [2,4] value
		/// </summary>
		public float _24
		{
			get { return GetCell(4, 2); }
			set { SetCell(4, 2, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [3,1] value
		/// </summary>
		public float _31
		{
			get { return GetCell(1, 3); }
			set { SetCell(1, 3, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [3,2] value
		/// </summary>
		public float _32
		{
			get { return GetCell(2, 3); }
			set { SetCell(2, 3, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [3,3] value
		/// </summary>
		public float _33
		{
			get { return GetCell(3, 3); }
			set { SetCell(3, 3, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [3,4] value
		/// </summary>
		public float _34
		{
			get { return GetCell(4, 3); }
			set { SetCell(4, 3, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [4,1] value
		/// </summary>
		public float _41
		{
			get { return GetCell(1, 4); }
			set { SetCell(1, 4, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [4,2] value
		/// </summary>
		public float _42
		{
			get { return GetCell(2, 4); }
			set { SetCell(2, 4, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [4,3] value
		/// </summary>
		public float _43
		{
			get { return GetCell(3, 4); }
			set { SetCell(3, 4, value); }
		}

		/// <summary>
		/// Matrix [Y,X] - [4,4] value
		/// </summary>
		public float _44
		{
			get { return GetCell(4, 4); }
			set { SetCell(4, 4, value); }
		}

		#endregion

		#region Operators

		/// <summary>
		/// Adds two matrices
		/// </summary>
		/// <param name="m1">Matrix 1</param>
		/// <param name="m2">Matrix 2</param>
		/// <returns>Matrix (Matrix1+2)</returns>
		public static Matrix operator +(Matrix m1, Matrix m2)
		{
			int i;
			Matrix result = new Matrix();

			for (i = 0; i < MATRIX_HEIGHT * MATRIX_WIDTH; i++)
			{
				result.cells[i] = m1.cells[i] + m2.cells[i];
			}

			return result;
		}

		/// <summary>
		/// Subtracts matrices
		/// </summary>
		/// <param name="m1">Subtracted Matrix</param>
		/// <param name="m2">Subtracting Matrix</param>
		/// <returns>Matrix (M1-M2)</returns>
		public static Matrix operator -(Matrix m1, Matrix m2)
		{
			int i;
			Matrix result = new Matrix();

			for (i = 0; i < MATRIX_HEIGHT * MATRIX_WIDTH; i++)
			{
				result.cells[i] = m1.cells[i] - m2.cells[i];
			}

			return result;
		}

		/// <summary>
		/// performs scalar Matrix multiplication
		/// </summary>
		/// <param name="m">multiplicated matrix</param>
		/// <param name="scalar">multiplying scalar</param>
		/// <returns>Matrix (m1*scalar)</returns>
		public static Matrix operator *(Matrix m, float scalar)
		{
			for (int i = 0; i < MATRIX_HEIGHT * MATRIX_WIDTH; i++)
			{
				m.cells[i] *= scalar;
			}

			return m;
		}

		/// <summary>
		/// compares identity of two matrices
		/// </summary>
		/// <param name="m1">matrix 1</param>
		/// <param name="m2">matrix 2</param>
		/// <returns>true if both matrices carry same values</returns>
		public static bool operator ==(Matrix m1, Matrix m2)
		{
			bool result = true;

			for (int i = 0; i < MATRIX_HEIGHT * MATRIX_WIDTH; i++)
			{
				if (m1.cells[i] != m2.cells[i])
				{
					result = false;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// compares difference of two matrices
		/// </summary>
		/// <param name="m1">matrix 1</param>
		/// <param name="m2">matrix 2</param>
		/// <returns>true if matrices carry different values</returns>
		public static bool operator !=(Matrix m1, Matrix m2)
		{
			return !(m1 == m2);
		}

		public override int GetHashCode()
		{
			return (int)this.Determinant();
		}

		public override bool Equals(object obj)
		{
			if (obj is Matrix == false)
				return false;
			Matrix m = (Matrix)obj;
			return m == this;
		}

		/// <summary>
		/// performs Matrix * Matrix multiplication
		/// BEWARE: M1*M2 != M2*M1 (only in special occasions)
		/// </summary>
		/// <param name="m1">multiplied matrix</param>
		/// <param name="m2">multiplicating matrix</param>
		/// <returns>Matrix M1*M2</returns>
		public static Matrix Multiply(IMatrix m1, IMatrix m2)
		{
			Matrix result = new Matrix();

			int i, j, k;
			float calc;

			for (i = 1; i <= MATRIX_WIDTH; i++)
			{
				for (j = 1; j <= MATRIX_HEIGHT; j++)
				{
					calc = 0;
					for (k = 1; k <= MATRIX_HEIGHT; k++)
					{
						calc += m1.GetCell(i, k) * m2.GetCell(k, j);
					}
					result.SetCell(i, j, calc);
				}
			}

			return result;
		}

		/// <summary>
		/// calculates determinant for the Matrix
		/// </summary>
		/// <returns>determinant of the matrix</returns>
		private float Determinant()
		{
			float[] kpts = new float[12];
			for (int i = 0; i < 12; i++)
			{
				kpts[i] = this.cells[DETERMINANT_ARRAY[i]] * this.cells[DETERMINANT_ARRAY[12 + i]]
						 - this.cells[DETERMINANT_ARRAY[24 + i]] * this.cells[DETERMINANT_ARRAY[36 + i]];
			}

			return kpts[0] * kpts[11] - kpts[1] * kpts[10] + kpts[2] * kpts[9] +
				   kpts[3] * kpts[8] - kpts[4] * kpts[7] + kpts[5] * kpts[6];
		}

		/// <summary>
		/// performs inversion of current matrix
		/// </summary>
		public void Inverse()
		{
			//determinant calculation
			float[] kpts = new float[12];
			for (int i = 0; i < 12; i++)
			{
				kpts[i] = this.cells[DETERMINANT_ARRAY[i]] * this.cells[DETERMINANT_ARRAY[12 + i]]
						 - this.cells[DETERMINANT_ARRAY[24 + i]] * this.cells[DETERMINANT_ARRAY[36 + i]];
			}

			float det = kpts[0] * kpts[11] - kpts[1] * kpts[10] + kpts[2] * kpts[9] +
				   kpts[3] * kpts[8] - kpts[4] * kpts[7] + kpts[5] * kpts[6];


			//inverting step 1

			Matrix inv = new Matrix();

			inv.cells[0] = +cells[5] * kpts[11] - cells[6] * kpts[10] + cells[7] * kpts[9];
			inv.cells[1] = -cells[1] * kpts[11] + cells[2] * kpts[10] - cells[3] * kpts[9];
			inv.cells[2] = +cells[13] * kpts[5] - cells[14] * kpts[4] + cells[15] * kpts[3];
			inv.cells[3] = -cells[9] * kpts[5] + cells[10] * kpts[4] - cells[11] * kpts[3];
			inv.cells[4] = -cells[4] * kpts[11] + cells[6] * kpts[8] - cells[7] * kpts[7];
			inv.cells[5] = +cells[0] * kpts[11] - cells[2] * kpts[8] + cells[3] * kpts[7];
			inv.cells[6] = -cells[12] * kpts[5] + cells[14] * kpts[2] - cells[15] * kpts[1];
			inv.cells[7] = +cells[8] * kpts[5] - cells[10] * kpts[2] + cells[11] * kpts[1];
			inv.cells[8] = +cells[4] * kpts[10] - cells[5] * kpts[8] + cells[7] * kpts[6];
			inv.cells[9] = -cells[0] * kpts[10] + cells[1] * kpts[8] - cells[3] * kpts[6];
			inv.cells[10] = +cells[12] * kpts[4] - cells[13] * kpts[2] + cells[15] * kpts[0];
			inv.cells[11] = -cells[8] * kpts[4] + cells[9] * kpts[2] - cells[11] * kpts[0];
			inv.cells[12] = -cells[4] * kpts[9] + cells[5] * kpts[7] - cells[6] * kpts[6];
			inv.cells[13] = +cells[0] * kpts[9] - cells[1] * kpts[7] + cells[2] * kpts[6];
			inv.cells[14] = -cells[12] * kpts[3] + cells[13] * kpts[1] - cells[14] * kpts[0];
			inv.cells[15] = +cells[8] * kpts[3] - cells[9] * kpts[1] + cells[10] * kpts[0];

			//inverting step 2

			float invDet = 1.0f / det;
			for (int i = 0; i < MATRIX_HEIGHT * MATRIX_WIDTH; i++)
			{
				this.cells[i] *= invDet;
			}

			//save result;
			this.cells = inv.cells;
		}

		/// <summary>
		/// Checks if the Matrix is Identity
		/// </summary>
		/// <param name="m">Checked Matrix</param>
		/// <returns>true if Matrix is identity</returns>
		public static bool IsMatrixIdentity(IMatrix m)
		{
			MatrixIdentity idMatrix = new MatrixIdentity();
			return (Matrix)idMatrix == (Matrix)m;
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates zero matrix
		/// </summary>
		public Matrix()
		{
			cells = new float[MATRIX_HEIGHT * MATRIX_WIDTH];
			for (int i = 0; i < MATRIX_WIDTH * MATRIX_HEIGHT; i++)
			{
				this.cells[i] = 0;
			}
		}

		#endregion

		#region Static matrices

		/// <summary>
		/// private identity matrix
		/// </summary>
		protected static readonly MatrixIdentity m_identityMatrix;

		/// <summary>
		/// Identity matrix generator;
		/// </summary>
		/// <returns>Identity matrix</returns>
		public static IMatrix Identity()
		{
			return m_identityMatrix;
		}

		/// <summary>
		/// Static Rotation Maxis - X axis
		/// </summary>
		/// <param name="angle">Rotation angle</param>
		/// <returns></returns>
		public static IMatrix RotationX(double angle) 
		{
			IMatrix rot = Matrix.Identity();
			
			rot._22 = (float)Math.Cos(angle);
			rot._23 = (float)Math.Sin(angle);
			rot._32 = -rot._23;
			rot._33 = rot._22;

			return rot;
		}

		/// <summary>
		/// Static Rotation Maxis - Y axis
		/// </summary>
		/// <param name="angle">Rotation angle</param>
		/// <returns></returns>
		public static IMatrix RotationY(double angle)
		{
			IMatrix rot = Matrix.Identity();

			rot._11 = (float)Math.Cos(angle);
			rot._31 = (float)Math.Sin(angle);
			rot._13 = -rot._31;
			rot._33 = rot._11;

			return rot;
		}

		/// <summary>
		/// Static Rotation Maxis - Z axis
		/// </summary>
		/// <param name="angle">Rotation angle</param>
		/// <returns></returns>
		public static IMatrix RotationZ(double angle)
		{
			IMatrix rot = Matrix.Identity();

			rot._11 = (float)Math.Cos(angle);
			rot._12 = (float)Math.Sin(angle);
			rot._21 = -rot._12;
			rot._22 = rot._11;

			return rot;
		}

		/// <summary>
		/// Translation Matrix
		/// </summary>
		/// <param name="tX">X-axis translation</param>
		/// <param name="tY">Y-axis translation</param>
		/// <param name="tZ">Z-axis translation</param>
		/// <returns>Translation Matrix</returns>
		public static IMatrix TranslateMatrix(float tX, float tY, float tZ)
		{
			IMatrix ret = Matrix.Identity();

			ret._41 = tX;
			ret._42 = tY;
			ret._43 = tZ;

			return ret;
		}

		/// <summary>
		/// Translation Matrix
		/// </summary>
		/// <param name="vec">translation vector</param>
		/// <returns>Translation Matrix</returns>
		public static IMatrix TranslateMatrix(Vector3 vec)
		{
			return TranslateMatrix((float)vec.X, (float)vec.Y, (float)vec.Z);
		}

		/// <summary>
		/// Scaling Matrix
		/// </summary>
		/// <param name="sX">X-axis scale</param>
		/// <param name="sY">Y-axis scale</param>
		/// <param name="sZ">Z-asix scale</param>
		/// <returns>Scale Matrix</returns>
		public static IMatrix Scaling(float sX, float sY, float sZ)
		{
			IMatrix ret = Matrix.Identity();

			ret._11 = sX;
			ret._22 = sY;
			ret._33 = sZ;

			return ret;
		}

		/// <summary>
		/// Scaling Matrix
		/// </summary>
		/// <param name="sX">X-axis scale</param>
		/// <param name="sY">Y-axis scale</param>
		/// <param name="sZ">Z-asix scale</param>
		/// <returns>Scale Matrix</returns>
		public static IMatrix Scaling(Vector3 vec)
		{
			return Matrix.Scaling((float)vec.X,(float)vec.Y,(float)vec.Z);
		}


		#endregion
	}
}
