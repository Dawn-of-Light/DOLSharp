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
using System;

namespace DOL.Database.DataTransferObjects
{
	/// <summary>
	/// X/Y/Z point.
	/// </summary>
	public class DbPoint
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DBPoint"/> class.
		/// </summary>
		public DbPoint()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DBPoint"/> class.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		public DbPoint(int x, int y, int z)
		{
			m_x = x;
			m_y = y;
			m_z = z;
		}

		/// <summary>
		/// X coord.
		/// </summary>
		private int m_x;
		/// <summary>
		/// Y coord.
		/// </summary>
		private int m_y;
		/// <summary>
		/// Z coord.
		/// </summary>
		private int m_z;

		/// <summary>
		/// Gets or sets the X.
		/// </summary>
		/// <value>The X.</value>
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// Gets or sets the Y.
		/// </summary>
		/// <value>The Y.</value>
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		/// <summary>
		/// Gets or sets the Z.
		/// </summary>
		/// <value>The Z.</value>
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}
	}
}
