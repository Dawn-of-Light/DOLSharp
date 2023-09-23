using System;

namespace DOL.GS.Geometry;

public interface IMotion
{
    int StartTimeInMilliSeconds { get; }
    Coordinate Start { get; init; }
    Coordinate Destination { get; }
    short Speed { get; init; }
    Coordinate CurrentLocation { get; }
    double FullDistance { get; }
    double RemainingDistance { get; }

    Angle GetDirection();
}

public class Motion
{
    public static IMotion Create(Position start, Coordinate destination, short withSpeed)
    {
        if (destination.Equals(Coordinate.Nowhere)) return new GoAheadMotion() { Start = start.Coordinate, Speed = withSpeed, Direction = start.Orientation };
        else if (start.Equals(destination)) return new GoAheadMotion() { Start = start.Coordinate, Speed = 0, Direction = start.Orientation };
        else return new MotionToDestination() { Start = start.Coordinate, Destination = destination, Speed = withSpeed };
    }
}

public class MotionToDestination : IMotion
{
    public int StartTimeInMilliSeconds { get; } = Environment.TickCount;
    public Coordinate Start { get; init; } = Coordinate.Nowhere;
    public Coordinate Destination { get; init; } = Coordinate.Nowhere;
    public short Speed { get; init; } = 0;

    public Coordinate CurrentLocation
        => GetLocationAfter(Environment.TickCount - StartTimeInMilliSeconds);
    public double FullDistance => Destination.DistanceTo(Start, ignoreZ: true);
    public double RemainingDistance => Destination.DistanceTo(CurrentPosition, ignoreZ: true);

    public Coordinate GetLocationAfter(int elapsedTimeInMilliSeconds)
    {
        if (Speed == 0) return Start;

        var distanceTravelled = Speed * elapsedTimeInMilliSeconds * 0.001;
        if (distanceTravelled > FullDistance) return Destination;

        var progress = distanceTravelled / FullDistance;
        if (progress > 1) return Destination;
        else return Start + (Destination - Start) * progress;
    }

    public Angle GetDirection()
        => Start.GetOrientationTo(Destination);
}

public class GoAheadMotion : IMotion
{
    public int StartTimeInMilliSeconds { get; } = Environment.TickCount;
    public Coordinate Start { get; init; } = Coordinate.Nowhere;
    public Coordinate Destination { get => Coordinate.Nowhere; }
    public short Speed { get; init; } = 0;
    public Angle Direction { get; init; }

    public Coordinate CurrentLocation
        => GetLocationAfter(Environment.TickCount - StartTimeInMilliSeconds);
    public double FullDistance => double.PositiveInfinity;
    public double RemainingDistance => double.PositiveInfinity;

    public Coordinate GetLocationAfter(int elapsedTimeInMilliSeconds)
    {
        if (Speed == 0) return Start;
        var timeElapsedInSeconds = elapsedTimeInMilliSeconds * 0.001;
        var distanceTravelled = Speed * timeElapsedInSeconds;
        return Start + Vector.Create(Direction, distanceTravelled);
    }

    public Angle GetDirection() => Direction;
}