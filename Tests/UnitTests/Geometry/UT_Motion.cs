using DOL.GS.Geometry;
using NUnit.Framework;

namespace DOL.UnitTests;

[TestFixture]
public class UT_Motion
{
    [Test]
    public void GetLocationAfter_Zero_Start()
    {
        var start = Position.Zero;

        var actual = Motion.Create(start, destination: Coordinate.Nowhere, withSpeed: 0)
            .GetPositonAfter(0);

        Assert.That(actual, Is.EqualTo(start));
    }

    [Test]
    public void GetLocationAfter_FromZeroAfter1000MilliSecondsWithSpeed100AndOrientationZero_Plus100Y()
    {
        var start = Position.Zero;
        var motion = Motion.Create(start, destination: Coordinate.Nowhere, withSpeed: 100);

        var actual = motion.GetPositonAfter(1000);

        var expected = start + Vector.Create(y: 100);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetLocationAfter_FromZeroAfter1000MilliSecondsWithSpeed100AndOrientationZeroAndDestination50Y_Destination()
    {
        var start = Position.Zero;
        var destination = start + Vector.Create(y: 50);
        var motion = Motion.Create(start, destination.Coordinate, withSpeed: 100);

        var actual = motion.GetPositonAfter(1000);

        var expected = destination;
        Assert.That(actual, Is.EqualTo(destination));
    }
}