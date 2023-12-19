namespace DOL.GS.Geometry;

public struct Position
{
    public ushort RegionID { get; init; } = 0;
    public Coordinate Coordinate { get; init; } = Coordinate.Zero;
    public Angle Orientation { get; init; } = Angle.Zero;

    public int X => Coordinate.X;
    public int Y => Coordinate.Y;
    public int Z => Coordinate.Z;

    public Region Region => WorldMgr.GetRegion(RegionID);

    public Position() { }

    public static Position Create(ushort regionID, int x, int y, int z, ushort heading)
        => new() { RegionID = regionID, Coordinate = Coordinate.Create(x, y, z), Orientation = Angle.Heading(heading) };

    public static Position Create(ushort regionID, int x = 0, int y = 0, int z = 0, Angle? orientation = null)
        => new() { RegionID = regionID, Coordinate = Coordinate.Create(x, y, z), Orientation = orientation ?? Angle.Zero };

    public static Position CreateInZone(ushort zoneID, int x = 0, int y = 0, int z = 0, ushort heading = 0)
    {
        var zone = WorldMgr.GetZone(zoneID);
        return Create(zone.ZoneRegion.ID, x + zone.Offset.X, y + zone.Offset.Y, z + zone.Offset.Z, heading);
    }

    public static Position Create(ushort regionID, Coordinate coordinate, ushort heading = 0)
        => new() { RegionID = regionID, Coordinate = coordinate, Orientation = Angle.Heading(heading) };

    public static Position Create(ushort regionID, Coordinate coordinate, Angle orientation)
        => new() { RegionID = regionID, Coordinate = coordinate, Orientation = orientation };

    public Position With(ushort? regionID = null, int? x = null, int? y = null, int? z = null, ushort? heading = null)
    {
        var newOrientation = heading != null ? Angle.Heading((ushort)heading) : Orientation;
        var newRegionID = regionID ?? RegionID;
        return Create(newRegionID, Coordinate.With(x, y, z), newOrientation);
    }

    public Position With(Coordinate coordinate)
        => Create(RegionID, coordinate, Orientation);

    public Position With(Angle orientation)
        => Create(RegionID, Coordinate, orientation);

    public Position TurnedAround()
        => With(orientation: Orientation + Angle.Degrees(180));

    public static Position operator +(Position a, Vector b)
        => a.With(coordinate: a.Coordinate + b);

    public static Position operator -(Position a, Vector b)
        => a.With(coordinate: a.Coordinate - b);

    public static bool operator ==(Position a, Position b)
        => a.Equals(b);

    public static bool operator !=(Position a, Position b)
        => !a.Equals(b);

    public override bool Equals(object obj)
    {
        if (obj is Position otherPos)
        {
            return otherPos.RegionID == RegionID
                && otherPos.Coordinate.Equals(Coordinate)
                && otherPos.Orientation == Orientation;
        }
        return false;
    }

    public override int GetHashCode()
        => base.GetHashCode();

    public override string ToString()
        => $"({Coordinate}, {Orientation.InHeading})";

    public readonly static Position Nowhere = Create(regionID: ushort.MaxValue, Coordinate.Nowhere, Angle.Zero);
    public readonly static Position Zero = new();
}