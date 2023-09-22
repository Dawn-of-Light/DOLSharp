using System;

namespace DOL.GS.Geometry;

public struct Angle
{
    private const int STEPS_TO_CIRCUMVOLUTION = 360 * 4096;
    private const int HEADING_TO_STEPS = STEPS_TO_CIRCUMVOLUTION / 4096;
    private const int DEGREE_TO_STEPS = STEPS_TO_CIRCUMVOLUTION / 360;
    private const double RADIANS_TO_STEPS = STEPS_TO_CIRCUMVOLUTION / 2 / Math.PI;

    private int steps;

    ///<remarks>for internal use only</remarks>
    private static Angle Steps(int steps)
    {
        steps %= STEPS_TO_CIRCUMVOLUTION; 
        if (steps < 0) steps += STEPS_TO_CIRCUMVOLUTION;
        return new() { steps = steps };
    }

    public static Angle Heading(int heading)
        => Steps(heading * HEADING_TO_STEPS);

    public static Angle Degrees(int degrees)
        => Steps(degrees * DEGREE_TO_STEPS);

    public static Angle Radians(double radians)
        => Steps((int)Math.Round(radians * RADIANS_TO_STEPS));

    public double InRadians => steps / RADIANS_TO_STEPS;
    public ushort InDegrees => (ushort)(steps / DEGREE_TO_STEPS);
    public ushort InHeading => (ushort)(steps / HEADING_TO_STEPS);

    public override bool Equals(object obj)
    {
        if (obj is Angle angle) return angle.steps == steps;
        return false;
    }

    public override int GetHashCode()
        => base.GetHashCode();

    public override string ToString()
        => steps.ToString();

    public static bool operator ==(Angle a, Angle b)
        => a.Equals(b);

    public static bool operator !=(Angle a, Angle b)
        => !a.Equals(b);

    public static Angle operator +(Angle a, Angle b)
        => Steps(a.steps + b.steps);

    public static Angle operator -(Angle a, Angle b)
        => Steps(a.steps - b.steps);

    public static readonly Angle Zero = new() { steps = 0 };
}