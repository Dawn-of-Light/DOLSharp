using System;

namespace DOL.GS.Geometry;

public struct Vector
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
    public double Length2D => Math.Sqrt(X * X + Y * Y);
    public Angle Orientation => Angle.Radians(-Math.Atan2(X,Y));

    public static Vector Create(int x = 0, int y = 0, int z = 0)
        => new() { X = x, Y = y, Z = z };

    // Coordinate calculation functions in DOL are standard trigonometric functions, but
    // with some adjustments to account for the different coordinate system that DOL uses
    // compared to the standard Cartesian coordinates used in trigonometry.
    //
    // DOL Heading grid:
    //         2048/180°
    //             |
    // 1024/90° ------- 3072/270° (+x)
    //             |
    //             0 (+y)
    // 
    // The Cartesian grid is 0 at the right side of the X-axis and increases counter-clockwise.
    // The DOL Heading grid is 0 at the bottom of the Y-axis and increases clockwise.
    // General trigonometry and the System.Math library use the Cartesian grid.
    public static Vector Create(Angle orientation, double length, int z = 0)
    {
        var distanceXY = Math.Sqrt(length * length - z * z);
        if(length < 0) distanceXY *= -1;
        return Create(
            x: (int)Math.Round(Math.Sin(-orientation.InRadians) * distanceXY),
            y: (int)Math.Round(Math.Cos(orientation.InRadians) * distanceXY),
            z: z);
    }

    public Vector RotatedClockwise(Angle angle)
    {
        var cos = Math.Cos(angle.InRadians);
        var sin = Math.Sin(angle.InRadians);
        return Create(
            x: (int)Math.Round(cos * X - sin * Y),
            y: (int)Math.Round(cos * Y + sin * X),
            z: Z);
    }

    public override string ToString()
        => $"{X}, {Y}, {Z}";

    public override bool Equals(object obj)
    {
        if(obj is Vector vector)
        {
            return vector.X == X && vector.Y == Y && vector.Z == Z;
        }
        else return false;
    }

    public override int GetHashCode()
        => base.GetHashCode();

    public static Vector operator *(Vector vec, double factor)
        => Create((int)(vec.X * factor), (int)(vec.Y * factor), (int)(vec.Z * factor));

    public static Vector operator *(double factor, Vector vec)
        => vec * factor;

    public static Vector operator +(Vector vecA, Vector vecB)
        => Create(vecA.X + vecB.X, vecA.Y + vecB.Y, vecA.Z + vecB.Z);

    public static Vector operator -(Vector vecA, Vector vecB)
        => Create(vecA.X - vecB.X, vecA.Y - vecB.Y, vecA.Z - vecB.Z);

    public static readonly Vector Zero = Vector.Create(0, 0, 0);
}