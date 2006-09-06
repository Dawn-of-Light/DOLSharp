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
	public class MatrixTranslation: MatrixIdentity, IMatrix
	{

		#region Constructor

		/// <summary>
		/// Creates Translation Matrix using given vector
		/// </summary>
		/// <param name="translationVector">Translation vector</param>
		public MatrixTranslation(Vector3 translationVector)
			: base()
		{
			this._41 = (float) translationVector.X;
			this._42 = (float) translationVector.Y;
			this._43 = (float) translationVector.Z;
		}

		/// <summary>
		/// Creates Translation Matrix using given translation values
		/// </summary>
		/// <param name="tX">X axis translation</param>
		/// <param name="tY">Y axis translation</param>
		/// <param name="tZ">Z axis translation</param>
		public MatrixTranslation(float tX, float tY, float tZ)
			: this(new Vector3((double)tX, (double)tY, (double)tZ))
		{
			
		}


		#endregion

	}
}
