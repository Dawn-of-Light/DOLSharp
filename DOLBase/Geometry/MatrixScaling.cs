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
	/// Represents Scaling Matrix
	/// </summary>
	public class MatrixScaling : MatrixIdentity, IMatrix
	{

		#region Constructor

		/// <summary>
		/// Creates Scaling matrix using given vector
		/// </summary>
		/// <param name="vector">scaling vector</param>
		public MatrixScaling(Vector3 vector)
			: base()
		{
			this._11 = (float)vector.X;
			this._22 = (float)vector.Y;
			this._33 = (float)vector.Z;
		}

		/// <summary>
		/// Creates Scaling matrix using given scales
		/// </summary>
		/// <param name="tX">X axis scale</param>
		/// <param name="tY">Y axis scale</param>
		/// <param name="tZ">Z axis scale</param>
		public MatrixScaling(float tX, float tY, float tZ)
			: this(new Vector3((double)tX, (double)tY, (double)tZ))
		{
			
		}

		#endregion

	}
}
