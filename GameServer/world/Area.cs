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
using System.Collections.Generic;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Geometry;

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
				if (X+Width < zone.Offset.X)
					return false;
				if (X-Width >= zone.Offset.X + 65536)
					return false;
				if (Y+Height < zone.Offset.Y)
					return false;
				if (Y-Height >= zone.Offset.Y + 65536)
					return false;

				return true;
			}	

            public override bool IsContaining(Coordinate spot, bool ignoreZ = false)
            {
                long m_xdiff = (long)spot.X - X;
                if (m_xdiff < 0 || m_xdiff > Width) return false;

                long m_ydiff = (long)spot.Y - Y;
                if (m_ydiff < 0 || m_ydiff > Height) return false;

                return true;
            }

			public override void LoadFromDatabase(DBArea area)
			{
				m_dbArea = area;
                m_translationId = area.TranslationId;
				m_Description = area.Description;
				m_X = area.X;
				m_Y = area.Y;
				m_Width = area.Radius;
				m_Height = area.Radius;
			}
		}

		public class Circle : AbstractArea
		{
			protected int m_Radius;

			protected long m_distSq;

			public Circle()
				: base()
			{
			}

			public Circle( string desc, int x, int y, int z, int radius) : base(desc)
			{															
				m_Description = desc;
                Center = Coordinate.Create(x, y, z);
                m_Radius = radius;
					
				m_RadiusRadius = radius*radius;
			}

            public Circle(string desc, Coordinate center, int radius) : base(desc)
            {
                m_Description = desc;
                Center = center;
                m_Radius = radius;

                m_RadiusRadius = radius * radius;
            }

            public Coordinate Center { get; private set; }

            public int X => Center.X;
            public int Y => Center.Y;
            public int Z => Center.Z;

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
				if (X+Radius < zone.Offset.X)
					return false;
				if (X-Radius >= zone.Offset.X + 65536)
					return false;
				if (Y+Radius < zone.Offset.Y)
					return false;
				if (Y-Radius >= zone.Offset.Y + 65536)
					return false;

				return true;
			}

            public override bool IsContaining(Coordinate spot, bool ignoreZ = false)
            {
                // spot is not in square around circle no need to check for circle...
                long m_xdiff = (long)spot.X - X;
                if (m_xdiff > Radius)
                    return false;

                long m_ydiff = (long)spot.Y - Y;
                if (m_ydiff > Radius)
                    return false;


                // check if spot is in circle
                m_distSq = m_xdiff * m_xdiff + m_ydiff * m_ydiff;

                if (Z != 0 && spot.Z != 0 && !ignoreZ)
                {
                    long m_zdiff = (long)spot.Z - Z;
                    m_distSq += m_zdiff * m_zdiff;
                }

                return (m_distSq <= m_RadiusRadius);
            }

            public override void LoadFromDatabase(DBArea area)
            {
                m_translationId = area.TranslationId;
                m_Description = area.Description;
                Center = Coordinate.Create(area.X, area.Y, area.Z);
                m_Radius = area.Radius;
                m_RadiusRadius = area.Radius * area.Radius;
            }
        }

        public class Polygon : AbstractArea
        {
            /// <summary>
            /// The X coordinate of this Area (center, not important)
            /// </summary>
            protected int m_X;

            /// <summary>
            /// The Y coordinate of this Area (center, not important)
            /// </summary>
            protected int m_Y;

            /// <summary>
            /// Returns the Height of this Area
            /// </summary>
            protected int m_Radius;

            /// <summary>
            /// The radius of the area in Coordinates
            /// </summary>
            public int Radius
            {
                get { return m_Radius; }
            }

            /// <summary>
            /// The Points string
            /// </summary>
            protected string m_stringpoints;

            /// <summary>
            /// The Points list
            /// </summary>
            protected IList<Coordinate> m_points;

            public Polygon()
                : base()
            {
            }

            public Polygon(string desc, int x, int y, int z, int radius, string points)
                : base(desc)
            {
                m_Description = desc;
                m_X = x;
                m_Y = y;
                m_Radius = radius;
                StringPoints = points;
            }

            /// <summary>
            /// Returns the X Coordinate of this Area (center, not important)
            /// </summary>
            public int X
            {
                get { return m_X; }
            }

            /// <summary>
            /// Returns the Y Coordinate of this Area (center, not important)
            /// </summary>
            public int Y
            {
                get { return m_Y; }
            }

            /// <summary>
            /// Get / Set(init) the serialized points
            /// </summary>
            public string StringPoints
            {
                get
                {
                    return m_stringpoints;
                }
                set
                {
                    m_stringpoints = value;
                    m_points = new List<Coordinate>();
                    if (m_stringpoints.Length < 1) return;
                    string[] points = m_stringpoints.Split('|');
                    foreach (string point in points)
                    {
                        string[] pts = point.Split(';');
                        if (pts.Length != 2) continue;
                        int x = Convert.ToInt32(pts[0]);
                        int y = Convert.ToInt32(pts[1]);
                        var p = Coordinate.Create(x, y);
                        if (!m_points.Contains(p)) m_points.Add(p);
                    }
                }
            }

            /// <summary>
            /// Checks wether area intersects with given zone
            /// </summary>
            /// <param name="zone"></param>
            /// <returns></returns>
            public override bool IsIntersectingZone(Zone zone)
            {
                // TODO if needed
                if (X + Radius < zone.Offset.X)
                    return false;
                if (X - Radius >= zone.Offset.X + 65536)
                    return false;
                if (Y + Radius < zone.Offset.Y)
                    return false;
                if (Y - Radius >= zone.Offset.Y + 65536)
                    return false;

                return true;
            }

            public override bool IsContaining(Coordinate spot, bool ignoreZ = false)
            {
                if (m_points.Count < 3) return false;
                Coordinate p1, p2;
                bool inside = false;

                var lastPoint = m_points[m_points.Count - 1];

                foreach (var currentPoint in m_points)
                {
                    var newpt = currentPoint;

                    if (currentPoint.X > lastPoint.X) { p1 = lastPoint; p2 = currentPoint; }
                    else { p1 = currentPoint; p2 = lastPoint; }

                    if ((currentPoint.X < spot.X) == (spot.X <= lastPoint.X)
                        && (spot.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (spot.X - p1.X))
                        inside = !inside;

                    lastPoint = currentPoint;
                }
                return inside;
            }

            public override void LoadFromDatabase(DBArea area)
            {
                m_translationId = area.TranslationId;
                m_Description = area.Description;
                m_X = area.X;
                m_Y = area.Y;
                m_Radius = area.Radius;
                StringPoints = area.Points;
            }
        }

		public class BindArea : Circle
		{
			protected BindPoint m_dbBindPoint;

			public BindArea()
				: base()
			{
				m_displayMessage = false;
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
