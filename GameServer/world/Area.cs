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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Database;

namespace DOL.GS
{		
	/// <summary>
	/// Collection of basic area shapes
	/// Circle
	/// Square
	/// </summary>
	public class Area 
	{
		public class Square : AbstractArea
		{
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

			public Square()
				: base()
			{ }

			public Square(string desc, int x, int y, int width, int height): base(desc)
			{
				m_X = x;
				m_Y = y;
				m_Height = height;
				m_Width = width;
			}

			/// <summary>
			/// Returns the X Coordinate of this Area
			/// </summary>
			public int X
			{
				get { return m_X; }
			}

			/// <summary>
			/// Returns the Y Coordinate of this Area
			/// </summary>
			public int Y
			{
				get { return m_Y; }
			}

			/// <summary>
			/// Returns the Width of this Area
			/// </summary>
			public int Width
			{
				get { return m_Width; }
			}

			/// <summary>
			/// Returns the Height of this Area
			/// </summary>
			public int Height
			{
				get { return m_Height; }
			}

			/// <summary>
			/// Checks wether area intersects with given zone
			/// </summary>
			/// <param name="zone"></param>
			/// <returns></returns>
			public override bool IsIntersectingZone(Zone zone)
			{
				if (X+Width < zone.XOffset)
					return false;
				if (X-Width >= zone.XOffset + 65536)
					return false;
				if (Y+Height < zone.YOffset)
					return false;
				if (Y-Height >= zone.YOffset + 65536)
					return false;

				return true;
			}	

			/// <summary>
			/// Checks wether given point is within area boundaries
			/// </summary>
			/// <param name="p"></param>
			/// <returns></returns>
			public override bool IsContaining(IPoint3D p)
			{
				return IsContaining(p, true);
			}

			public override bool IsContaining(int x, int y, int z)
			{
				return IsContaining(x, y, z, true);
			}

			public override bool IsContaining(IPoint3D p, bool checkZ)
			{
				return IsContaining(p.X, p.Y, p.Z, checkZ);
			}

			public override bool IsContaining(int x, int y, int z, bool checkZ)
			{
				long m_xdiff = (long)x - X;
				if (m_xdiff < 0 || m_xdiff > Width)
					return false;

				long m_ydiff = (long)y - Y;
				if (m_ydiff < 0 || m_ydiff > Height)
					return false;

				/*
				//SH: Removed Z checks when one of the two Z values is zero(on ground)
				if (Z != 0 && spotZ != 0)
				{
					long m_zdiff = (long) spotZ - Z;
					if (m_zdiff> Radius)
						return false;
				}
				*/

				return true;
			}

			public override void LoadFromDatabase(DBArea area)
			{
				m_dbArea = area;
				m_Description = area.Description;
				m_X = area.X;
				m_Y = area.Y;
				m_Width = area.Radius;
				m_Height = area.Radius;
			}
		}

		public class Circle : AbstractArea
		{
			
			/// <summary>
			/// The X coordinate of this Area
			/// </summary>
			protected int m_X;

			/// <summary>
			/// The Y coordinate of this Area
			/// </summary>
			protected int m_Y;

			/// <summary>
			/// The Z coordinate of this Area
			/// </summary>
			protected int m_Z;

			/// <summary>
			/// The radius of the area in Coordinates
			/// </summary>
			protected int m_Radius;

			protected long m_distSq;

			public Circle()
				: base()
			{
			}

			public Circle( string desc, int x, int y, int z, int radius) : base(desc)
			{															
				m_Description = desc;
				m_X = x;
				m_Y = y;
				m_Z= z;
				m_Radius= radius;
					
				m_RadiusRadius = radius*radius;
			}

			/// <summary>
			/// Returns the X Coordinate of this Area
			/// </summary>
			public int X
			{
				get { return m_X; }
			}

			/// <summary>
			/// Returns the Y Coordinate of this Area
			/// </summary>
			public int Y
			{
				get { return m_Y; }
			}

			/// <summary>
			/// Returns the Width of this Area
			/// </summary>
			public int Z
			{
				get { return m_Z; }
			}

			/// <summary>
			/// Returns the Height of this Area
			/// </summary>
			public int Radius
			{
				get { return m_Radius; }
			}

			/// <summary>
			/// Cache for radius*radius to increase performance of circle check,
			/// radius is still needed for square check
			/// </summary>
			protected int m_RadiusRadius;
			

			/// <summary>
			/// Checks wether area intersects with given zone
			/// </summary>
			/// <param name="zone"></param>
			/// <returns></returns>
			public override bool IsIntersectingZone(Zone zone)
			{
				if (X+Radius < zone.XOffset)
					return false;
				if (X-Radius >= zone.XOffset + 65536)
					return false;
				if (Y+Radius < zone.YOffset)
					return false;
				if (Y-Radius >= zone.YOffset + 65536)
					return false;

				return true;
			}

			public override bool IsContaining(IPoint3D spot)
			{
				return IsContaining(spot, true);
			}

			public override bool IsContaining(int x, int y, int z, bool checkZ)
			{
				// spot is not in square around circle no need to check for circle...
				long m_xdiff = (long)x - X;
				if (m_xdiff > Radius)
					return false;

				long m_ydiff = (long)y - Y;
				if (m_ydiff > Radius)
					return false;


				// check if spot is in circle
				m_distSq = m_xdiff * m_xdiff + m_ydiff * m_ydiff;

				//SH: Removed Z checks when one of the two Z values is zero(on ground)
				if (Z != 0 && z != 0 && checkZ)
				{
					long m_zdiff = (long)z - Z;
					m_distSq += m_zdiff * m_zdiff;
				}

				return (m_distSq <= m_RadiusRadius);
			}

			public override bool IsContaining(int x, int y, int z)
			{
				return IsContaining(x, y, z, true);
			}

			/// <summary>
			/// Checks wether given point is within area boundaries
			/// </summary>
			/// <param name="p"></param>
			/// <param name="checkZ"></param>
			/// <returns></returns>
			public override bool IsContaining(IPoint3D p, bool checkZ)
			{
				return IsContaining(p.X, p.Y, p.Z, checkZ);
			}

			public override void LoadFromDatabase(DBArea area)
			{
				m_Description = area.Description;
				m_X = area.X;
				m_Y = area.Y;
				m_Z = area.Z;
				m_Radius = area.Radius;
				m_RadiusRadius = area.Radius * area.Radius;
			}
		}

		public class BindArea : Circle
		{
			protected BindPoint m_dbBindPoint;

			public BindArea()
				: base()
			{
			}

			public BindArea(string desc, BindPoint dbBindPoint)
				: base(desc, dbBindPoint.X, dbBindPoint.Y, dbBindPoint.Z, dbBindPoint.Radius)
			{
				m_dbBindPoint = dbBindPoint;
				m_displayMessage = false;
			}

			public BindPoint BindPoint
			{
				get { return m_dbBindPoint; }
			}

			public override void LoadFromDatabase(DBArea area)
			{
				base.LoadFromDatabase(area);

				m_dbBindPoint = new BindPoint();
				m_dbBindPoint.Radius = (ushort)area.Radius;
				m_dbBindPoint.X = area.X;
				m_dbBindPoint.Y = area.Y;
				m_dbBindPoint.Z = area.Z;
				m_dbBindPoint.Region = area.Region;
			}
		}

		public class SafeArea : Circle
		{
			public SafeArea()
				: base()
			{
				m_safeArea = true;
			}

			public SafeArea(string desc, int x, int y, int z, int radius)
				: base
				(desc, x, y, z, radius)
			{
				m_safeArea = true;
			}
		}
	}
}
