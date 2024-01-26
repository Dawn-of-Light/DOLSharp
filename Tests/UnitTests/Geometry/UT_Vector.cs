using System;
using DOL.GS.Geometry;
using NUnit.Framework;

namespace DOL.UnitTests;

[TestFixture]
public class UT_Vector
{
    [Test]
    public void Equals_TwoNewVectors_True()
    {
        var vectorA = Vector.Create();
        var vectorB = Vector.Create();

        Assert.That(vectorA.Equals(vectorB), Is.True);
    }

    [Test]
    public void Equals_VectorWithDifferentX_False()
    {
        var vectorA = Vector.Create(x: 0);
        var vectorB = Vector.Create(x: 1);

        Assert.That(vectorA.Equals(vectorB), Is.False);
    }

    [Test]
    public void PlusOperator_TwoVectorsWithLengthOne_VectorWithLength2()
    {
        var vector = Vector.Create(orientation: Angle.Zero, length: 1);

        var actual = (vector + vector).Length;

        var expectedLength = 2;
        Assert.That(actual, Is.EqualTo(expectedLength));
    }

    [Test]
    public void Create_OrientationZeroWithLengthOne_SameAsVectorWithYOne()
    {
        var actual = Vector.Create(orientation: Angle.Zero, length: 1);

        var expected = Vector.Create(y: 1);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Create_Orientation45DegreesWithLengthOneHundred_VectorWithXMinus71AndY71()
    {
        var actual = Vector.Create(orientation: Angle.Degrees(45), length: 100);

        var expected = Vector.Create(x: -71, y: 71);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void RotatedClockwise_VectorXOneHundredBy90Degrees_VectorWithYOneHundred()
    {
        var vector = Vector.Create(x: 100);

        var actual = vector.RotatedClockwise(Angle.Degrees(90));

        var expected = Vector.Create(y: 100);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void RotatedClockwise_VectorXOneHundredBy45Degrees_VectorWithX71AndY71()
    {
        var vector = Vector.Create(x: 100);

        var actual = vector.RotatedClockwise(Angle.Degrees(45));

        //rounded
        var expected = Vector.Create(x: 71, y: 71);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void RotatedClockwise_VectorX50By45Degrees_VectorWithX35AndY35()
    {
        var vector = Vector.Create(x: 50);

        var actual = vector.RotatedClockwise(Angle.Degrees(45));

        //rounded
        var expected = Vector.Create(x: 35, y: 35);
        Assert.That(actual, Is.EqualTo(expected));
    }



    [Test]
    public void Length_VectorX1Y2Z2_3()
    {
        var vector = Vector.Create(x: 1, y: 2, z: 2);

        var actual = vector.Length;

        var expected = 3d;
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Length2D_VectorX3Y4Z100_5()
    {
        var vector = Vector.Create(x: 3, y: 4, z: 100);

        var actual = vector.Length2D;

        var expected = 5d;
        Assert.That(actual, Is.EqualTo(expected));
    }
}