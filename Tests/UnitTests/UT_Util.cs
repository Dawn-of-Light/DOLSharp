using System.Collections.Generic;
using DOL.GS;
using NUnit.Framework;

namespace DOL.UnitTests;

[TestFixture]
public class UT_Util
{
    [Test]
    public void SplitCSV_EmptyString_EmptyList()
    {
        var actual = Util.SplitCSV("");
        var expected = new List<string>();
        Assert.That(actual, Is.EqualTo(expected));
    }
}