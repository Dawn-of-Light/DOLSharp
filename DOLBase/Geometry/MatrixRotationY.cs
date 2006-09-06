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
	/// Represents Rotation Matrix (Y axis)
	/// </summary>
	public class MatrixRotationY : MatrixIdentity, IMatrix
	{

		#region Constructor


		/// <summary>
		/// Creates matrix for rotation around Y axis
		/// </summary>
		/// <param name="angle"></param>
		public MatrixRotationY(double angle)
			: base()
		{
			this._11 = (float)Math.Cos(angle);
			this._31 = (float)Math.Sin(angle);
			this._13 = -this._31;
			this._33 = this._11;
		}

		#endregion

	}
}
