using DOL.Database;

namespace DOL.GS.Geometry;

public static class DataObjectPositionExtensions
{
    public static Position GetPosition(this Mob mob)
        => Position.Create(mob.Region, mob.X, mob.Y, mob.Z, Angle.Heading(mob.Heading));

    public static Position GetPosition(this Teleport teleport)
        => Position.Create((ushort)teleport.RegionID, teleport.X, teleport.Y, teleport.Z, Angle.Heading(teleport.Heading));

    public static Position GetSourcePosition(this ZonePoint zonePoint)
        => Position.Create(zonePoint.SourceRegion, zonePoint.SourceX, zonePoint.SourceY, zonePoint.SourceZ);

    public static Position GetTargetPosition(this ZonePoint zonePoint)
        => Position.Create(zonePoint.TargetRegion, zonePoint.TargetX, zonePoint.TargetY, zonePoint.TargetZ, Angle.Heading(zonePoint.TargetHeading));

    public static Position GetPosition(this DOLCharacters dolc)
        => Position.Create((ushort)dolc.Region, dolc.Xpos, dolc.Ypos, dolc.Zpos, Angle.Heading(dolc.Direction));

    public static void SetPosition(this DOLCharacters dolc, Position pos)
    {
        dolc.Region = pos.RegionID;
        dolc.Xpos = pos.X;
        dolc.Ypos = pos.Y;
        dolc.Zpos = pos.Z;
        dolc.Direction = pos.Orientation.InHeading;
    }

    public static Position GetBindPosition(this DOLCharacters dolc)
        => Position.Create((ushort)dolc.BindRegion, dolc.BindXpos, dolc.BindYpos, dolc.BindZpos, Angle.Heading(dolc.BindHeading));

    public static Position GetPosition(this DBKeep dbKeep)
        => Position.Create(dbKeep.Region, dbKeep.X, dbKeep.Y, dbKeep.Z, Angle.Degrees(dbKeep.Heading));

    public static void SetPosition(this DBKeep dbKeep, Position pos)
    {
        dbKeep.Region = pos.RegionID;
        dbKeep.X = pos.X;
        dbKeep.Y = pos.Y;
        dbKeep.Z = pos.Z;
        dbKeep.Heading = (ushort)pos.Orientation.InDegrees;
    }
}