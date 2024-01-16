namespace DOL.GS.Geometry;

public struct Coordinate
{
    private Vector coordinate { get; init; }

    public int X => coordinate.X;
    public int Y => coordinate.Y;
    public int Z => coordinate.Z;

    public static Coordinate Create(int x = 0, int y = 0, int z = 0)
        => new() { coordinate = Vector.Create(x, y, z) };

    public Coordinate With(int? x = null, int? y = null, int? z = null)
        => new() { coordinate = Vector.Create(x ?? X, y ?? Y, z ?? Z) };

    public double DistanceTo(Position pos, bool ignoreZ = false)
        => DistanceTo(pos.Coordinate, ignoreZ);

    public double DistanceTo(Coordinate loc, bool ignoreZ = false)
    {
        if (Equals(Nowhere) || loc.Equals(Nowhere)) return double.PositiveInfinity;

        if (ignoreZ) return (loc - this).Length2D;
        else return (loc - this).Length;
    }

    public Angle GetOrientationTo(Coordinate loc)
        => (loc - this).Orientation;

    public static Coordinate operator +(Coordinate loc, Vector v)
        => new() { coordinate = loc.coordinate + v };

    public static Coordinate operator -(Coordinate loc, Vector v)
        => new() { coordinate = loc.coordinate - v };

    public static Vector operator -(Coordinate locA, Coordinate locB)
        => Vector.Create(x: locA.X - locB.X, y: locA.Y - locB.Y, z: locA.Z - locB.Z);

    public static bool operator ==(Coordinate a, Coordinate b)
        => a.Equals(b);

    public static bool operator !=(Coordinate a, Coordinate b)
        => !a.Equals(b);

    public override bool Equals(object obj)
    {
        if (obj is Coordinate loc)
        {
            return X == loc.X && Y == loc.Y && Z == loc.Z;
        }
        return false;
    }

    public override int GetHashCode()
        => base.GetHashCode();

    public override string ToString()
        => $"{X}, {Y}, {Z}";

    public readonly static Coordinate Nowhere = Create(-1, -1, -1);
    public readonly static Coordinate Zero = Create(0, 0, 0);
}