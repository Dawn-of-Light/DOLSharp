using System;
using NUnit.Framework;
using DOL.GS.Geometry;

namespace DOL.UnitTests;

[TestFixture]
public class UT_Angle
{
    [Test]
    public void Equals_TwoNewAnglesWithZeroDegrees_True()
    {
        var angleA = Angle.Degrees(0);
        var angleB = Angle.Degrees(0);

        Assert.That(angleA.Equals(angleB), Is.True);
    }

    [Test]
    public void Equals_TwoDifferentAngles_False()
    {
        var angleA = Angle.Degrees(0);
        var angleB = Angle.Degrees(1);

        Assert.That(angleA.Equals(angleB), Is.False);
    }

    [Test]
    public void Equals_MinusOneDegreesAnd359Degrees_True()
    {
        var angleA = Angle.Degrees(-1);
        var angleB = Angle.Degrees(359);

        Assert.That(angleA.Equals(angleB), Is.True);
    }

    [Test]
    public void Heading_One_InHeadingIsOne()
    {
        var angle = Angle.Heading(1);

        var expected = 1;
        var actual = angle.InHeading;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Degrees_One_InDegreesIsOne()
    {
        var angle = Angle.Heading(1);

        var expected = 1;
        var actual = angle.InHeading;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void PlusOperator_OneHeadingWithOneHeading_TwoHeading()
    {
        var actual = Angle.Heading(1) + Angle.Heading(1);

        var expected = Angle.Heading(2);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void MinusOperator_OneHeadingMinusOneHeading_AngleZero()
    {
        var actual = Angle.Heading(1) - Angle.Heading(1);

        var expected = Angle.Zero;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Radians_One2048thPi_OneHeading()
    {
        var actual = Angle.Radians(Math.PI/2048);

        var expected = Angle.Heading(1);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Radians_One180thPi_OneDegree()
    {
        var actual = Angle.Radians(Math.PI/180);

        var expected = Angle.Degrees(1);
        Assert.That(actual, Is.EqualTo(expected));
    }
}