using System;

namespace DOL.GS.Geometry;

public static class CoordinateTransitionExtensions
{
    [Obsolete("This extension is transitional and going to be removed.")]
    public static Point3D ToPoint3D(this Coordinate coordinate)
            => new Point3D(coordinate.X, coordinate.Y, coordinate.Z);

    [Obsolete("This extension is transitional and going to be removed.")]
    public static Coordinate ToCoordinate(this IPoint3D point)
            => Coordinate.Create(point.X, point.Y, point.Z);
}