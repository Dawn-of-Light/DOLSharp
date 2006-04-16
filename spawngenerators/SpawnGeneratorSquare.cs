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
using System.Collections;
using System.Reflection;
using log4net;
using DOL.GS;
using DOL.Events;

namespace DOL.GS.SpawnGenerators
{
	/// <summary>
	/// The spawn generator class based on square area
	/// </summary>
	public class SpawnGeneratorSquare : SpawnGeneratorBase
	{
		#region Declaration

		/// <summary>
		/// The X coordinate of this Area
		/// </summary>
		protected int m_X;

		/// <summary>
		/// The Y coordinate of this Area 
		/// </summary>
		protected int m_Y;

		/// <summary>
		/// The width of this Area 
		/// </summary>
		protected int m_Width;

		/// <summary>
		/// The height of this Area 
		/// </summary>
		protected int m_Height;

		/// <summary>
		/// Returns the X Coordinate of this Area
		/// </summary>
		public int X
		{
			get { return m_X; }
			set { m_X = value; }
		}

		/// <summary>
		/// Returns the Y Coordinate of this Area
		/// </summary>
		public int Y
		{
			get { return m_Y; }
			set { m_Y = value; }
		}

		/// <summary>
		/// Returns the Width of this Area
		/// </summary>
		public int Width
		{
			get { return m_Width; }
			set { m_Width = value; }
		}

		/// <summary>
		/// Returns the Height of this Area
		/// </summary>
		public int Height
		{
			get { return m_Height; }
			set { m_Height = value; }
		}

		#endregion

		/// <summary>
		/// Get a random loc inside this spawn generator area
		/// </summary>
		/// <returns>the rand point</returns>
		public override Point GetRandomLocation()
		{
			return new Point(Util.Random(m_X ,m_X + m_Width), Util.Random(m_Y, m_Y + m_Height), 0);
		}
	}
}