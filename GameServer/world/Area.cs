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
using DOL.GS.Utils;

namespace DOL.GS
{		
	/// <summary>
	/// The square area.
	/// </summary>
	public class Square : AbstractArea
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
		/// Checks wether area intersects with given zone
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		public override bool IsIntersectingZone(Zone zone)
		{
			if (m_X+m_Width < zone.XOffset)
				return false;
			if (m_X-m_Width >= zone.XOffset + 65536)
				return false;
			if (m_Y+m_Height < zone.YOffset)
				return false;
			if (m_Y-m_Height >= zone.YOffset + 65536)
				return false;

			return true;
		}	

		/// <summary>
		/// Checks wether given point is within area boundaries
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public override bool IsContaining(Point p)
		{							
			long m_xdiff = (long) p.X - m_X;
			if (m_xdiff < 0 || m_xdiff > m_Width)
				return false;

			long m_ydiff = (long) p.Y - m_Y;
			if (m_ydiff < 0 || m_ydiff > m_Height)
				return false;

			return true;
		}

		/// <summary>
		/// Check if two areas are equals
		/// </summary>
		/// <param name="area">the area to compare with</param>
		/// <returns>true if equals</returns>
		public override bool IsEqual(AbstractArea area)
		{
			Square sq = area as Square;
			if(sq == null)
				return false;

			if(sq.X != m_X || sq.Y != m_Y || sq.Height != m_Height || sq.Width != m_Width)
				return false;

			return base.Equals(area);
		}
	}

	/// <summary>
	/// The sphere area.
	/// </summary>
	public class Circle : AbstractArea
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
		/// The radius of the area in Coordinates
		/// </summary>
		protected int m_Radius;

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
		/// Gets that circle's radius.
		/// </summary>
		public int Radius
		{
			get { return m_Radius; }
			set { m_Radius = value; }
		}

		#endregion

		/// <summary>
		/// Checks wether area intersects with given zone.
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		public override bool IsIntersectingZone(Zone zone)
		{
			if (m_X+m_Radius < zone.XOffset)
				return false;
			if (m_X-m_Radius >= zone.XOffset + 65536)
				return false;
			if (m_Y+m_Radius < zone.YOffset)
				return false;
			if (m_Y-m_Radius >= zone.YOffset + 65536)
				return false;

			return true;
		}	

		/// <summary>
		/// Checks wether given point is within area boundaries.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public override bool IsContaining(Point p)
		{						
			// spot is not in square around circle no need to check for circle...
			int xdiff = p.X - m_X;
			if (FastMath.Abs(xdiff) > m_Radius)
				return false;

			int ydiff = p.Y - m_Y;
			if (FastMath.Abs(ydiff) > m_Radius)
				return false;

			// check if spot is in circle
			return (xdiff*xdiff + ydiff*ydiff) <= (m_Radius * m_Radius);
		}

		/// <summary>
		/// Check if two areas are equals
		/// </summary>
		/// <param name="area">the area to compare with</param>
		/// <returns>true if equals</returns>
		public override bool IsEqual(AbstractArea area)
		{
			Circle sq = area as Circle;
			if(sq == null)
				return false;

			if(sq.X != m_X || sq.Y != m_Y || sq.Radius != m_Radius)
				return false;

			return base.Equals(area);
		}
	}
}
