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
            public Square()
                : base()
            { }

            public Square(string desc, int x, int y, int width, int height) : base(desc)
            {
                X = x;
                Y = y;
                Height = height;
                Width = width;
            }

            /// <summary>
            /// Returns the X Coordinate of this Area
            /// </summary>
            public int X { get; protected set; }

            /// <summary>
            /// Returns the Y Coordinate of this Area
            /// </summary>
            public int Y { get; protected set; }

            /// <summary>
            /// Returns the Width of this Area
            /// </summary>
            public int Width { get; protected set; }

            /// <summary>
            /// Returns the Height of this Area
            /// </summary>
            public int Height { get; protected set; }

            /// <summary>
            /// Checks wether area intersects with given zone
            /// </summary>
            /// <param name="zone"></param>
            /// <returns></returns>
            public override bool IsIntersectingZone(Zone zone)
            {
                if (X + Width < zone.XOffset)
                {
                    return false;
                }

                if (X - Width >= zone.XOffset + 65536)
                {
                    return false;
                }

                if (Y + Height < zone.YOffset)
                {
                    return false;
                }

                if (Y - Height >= zone.YOffset + 65536)
                {
                    return false;
                }

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
                {
                    return false;
                }

                long m_ydiff = (long)y - Y;
                if (m_ydiff < 0 || m_ydiff > Height)
                {
                    return false;
                }

                return true;
            }

            public override void LoadFromDatabase(DBArea area)
            {
                m_dbArea = area;
                m_translationId = area.TranslationId;
                Description = area.Description;
                X = area.X;
                Y = area.Y;
                Width = area.Radius;
                Height = area.Radius;
            }
        }

        public class Circle : AbstractArea
        {
            protected long m_distSq;

            public Circle()
                : base()
            {
            }

            public Circle(string desc, int x, int y, int z, int radius) : base(desc)
            {
                Description = desc;
                X = x;
                Y = y;
                Z = z;
                Radius = radius;

                m_RadiusRadius = radius * radius;
            }

            /// <summary>
            /// Returns the X Coordinate of this Area
            /// </summary>
            public int X { get; protected set; }

            /// <summary>
            /// Returns the Y Coordinate of this Area
            /// </summary>
            public int Y { get; protected set; }

            /// <summary>
            /// Returns the Width of this Area
            /// </summary>
            public int Z { get; protected set; }

            /// <summary>
            /// Returns the Height of this Area
            /// </summary>
            public int Radius { get; protected set; }

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
                if (X + Radius < zone.XOffset)
                {
                    return false;
                }

                if (X - Radius >= zone.XOffset + 65536)
                {
                    return false;
                }

                if (Y + Radius < zone.YOffset)
                {
                    return false;
                }

                if (Y - Radius >= zone.YOffset + 65536)
                {
                    return false;
                }

                return true;
            }

            public override bool IsContaining(IPoint3D spot)
            {
                return IsContaining(spot, true);
            }

            public override bool IsContaining(int x, int y, int z, bool checkZ)
            {
                // spot is not in square around circle no need to check for circle...
                long xdiff = (long)x - X;
                if (xdiff > Radius)
                {
                    return false;
                }

                long ydiff = (long)y - Y;
                if (ydiff > Radius)
                {
                    return false;
                }

                // check if spot is in circle
                m_distSq = xdiff * xdiff + ydiff * ydiff;

                if (Z != 0 && z != 0 && checkZ)
                {
                    long zdiff = (long)z - Z;
                    m_distSq += zdiff * zdiff;
                }

                return m_distSq <= m_RadiusRadius;
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
                m_translationId = area.TranslationId;
                Description = area.Description;
                X = area.X;
                Y = area.Y;
                Z = area.Z;
                Radius = area.Radius;
                m_RadiusRadius = area.Radius * area.Radius;
            }
        }

        public class Polygon : AbstractArea
        {
            /// <summary>
            /// The radius of the area in Coordinates
            /// </summary>
            public int Radius { get; protected set; }

            /// <summary>
            /// The Points string
            /// </summary>
            protected string m_stringpoints;

            /// <summary>
            /// The Points list
            /// </summary>
            protected IList<Point2D> m_points;

            public Polygon()
                : base()
            {
            }

            public Polygon(string desc, int x, int y, int z, int radius, string points)
                : base(desc)
            {
                Description = desc;
                X = x;
                Y = y;
                Radius = radius;
                StringPoints = points;
            }

            /// <summary>
            /// Returns the X Coordinate of this Area (center, not important)
            /// </summary>
            public int X { get; protected set; }

            /// <summary>
            /// Returns the Y Coordinate of this Area (center, not important)
            /// </summary>
            public int Y { get; protected set; }

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
                    m_points = new List<Point2D>();
                    if (m_stringpoints.Length < 1)
                    {
                        return;
                    }

                    string[] points = m_stringpoints.Split('|');
                    foreach (string point in points)
                    {
                        string[] pts = point.Split(';');
                        if (pts.Length != 2)
                        {
                            continue;
                        }

                        int x = Convert.ToInt32(pts[0]);
                        int y = Convert.ToInt32(pts[1]);
                        Point2D p = new Point2D(x, y);
                        if (!m_points.Contains(p))
                        {
                            m_points.Add(p);
                        }
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
                if (X + Radius < zone.XOffset)
                {
                    return false;
                }

                if (X - Radius >= zone.XOffset + 65536)
                {
                    return false;
                }

                if (Y + Radius < zone.YOffset)
                {
                    return false;
                }

                if (Y - Radius >= zone.YOffset + 65536)
                {
                    return false;
                }

                return true;
            }

            public override bool IsContaining(int x, int y, int z, bool checkZ)
            {
                return IsContaining(new Point3D(x, y, z));
            }

            public override bool IsContaining(int x, int y, int z)
            {
                return IsContaining(new Point3D(x, y, z));
            }

            public override bool IsContaining(IPoint3D obj, bool checkZ)
            {
                return IsContaining(obj);
            }

            public override bool IsContaining(IPoint3D obj)
            {
                if (m_points.Count < 3)
                {
                    return false;
                }

                bool inside = false;
                Point2D oldpt = new Point2D(m_points[m_points.Count - 1].X, m_points[m_points.Count - 1].Y);

                foreach (Point2D pt in m_points)
                {
                    Point2D newpt = new Point2D(pt.X, pt.Y);
                    Point2D p1;
                    Point2D p2;

                    if (newpt.X > oldpt.X)
                    {
                        p1 = oldpt;
                        p2 = newpt;
                    }
                    else
                    {
                        p1 = newpt;
                        p2 = oldpt;
                    }

                    if ((newpt.X < obj.X) == (obj.X <= oldpt.X) &&
                        (obj.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (obj.X - p1.X))
                    {
                        inside = !inside;
                    }

                    oldpt = newpt;
                }

                return inside;
            }

            public override void LoadFromDatabase(DBArea area)
            {
                m_translationId = area.TranslationId;
                Description = area.Description;
                X = area.X;
                Y = area.Y;
                Radius = area.Radius;
                StringPoints = area.Points;
            }
        }

        public class BindArea : Circle
        {
            public BindPoint BindPoint { get; protected set; }

            public BindArea()
                : base()
            {
                DisplayMessage = false;
            }

            public BindArea(string desc, BindPoint dbBindPoint)
                : base(desc, dbBindPoint.X, dbBindPoint.Y, dbBindPoint.Z, dbBindPoint.Radius)
            {
                BindPoint = dbBindPoint;
                DisplayMessage = false;
            }

            public override void LoadFromDatabase(DBArea area)
            {
                base.LoadFromDatabase(area);

                BindPoint = new BindPoint
                {
                    Radius = (ushort)area.Radius,
                    X = area.X,
                    Y = area.Y,
                    Z = area.Z,
                    Region = area.Region
                };
            }
        }

        public class SafeArea : Circle
        {
            public SafeArea()
                : base()
            {
                IsSafeArea = true;
            }

            public SafeArea(string desc, int x, int y, int z, int radius)
                : base(desc, x, y, z, radius)
            {
                IsSafeArea = true;
            }
        }
    }
}
