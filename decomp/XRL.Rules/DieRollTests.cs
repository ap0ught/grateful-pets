using NUnit.Framework;

namespace XRL.Rules;

public class DieRollTests
{
	[TestCase("1d6", 2, "1d8")]
	[TestCase("1d6", -2, "1d4")]
	[TestCase("2d6", 2, "2d8")]
	[TestCase("2d6", -2, "2d4")]
	[TestCase("1d6+3", 2, "1d8+3")]
	[TestCase("1d6-3", -2, "1d4-3")]
	[TestCase("2d6-3", 2, "2d8-3")]
	[TestCase("2d6+3", -2, "2d4+3")]
	[TestCase("1d6+3", 5, "1d11+3")]
	[TestCase("1d6-3", -4, "1d2-3")]
	[TestCase("2d6-3", 5, "2d11-3")]
	[TestCase("2d6+3", -4, "2d2+3")]
	[TestCase("1d4-1d6", 13, "1d17-1d6")]
	[TestCase("3+3d8-2d4", -1, "3+3d7-2d4")]
	[TestCase("3+3d8x7", -1, "3+3d7x7")]
	[TestCase("3+3d8/7", 10, "3+3d18/7")]
	[TestCase("2", 4, "2+1d4")]
	[TestCase("7", -6, "7-1d6")]
	[TestCase("7", -12, "7-1d12")]
	[TestCase("1-2", 4, "1-6")]
	[TestCase("5-7", 8, "5-15")]
	[TestCase("3d12[Test]", 8, "3d20[Test]")]
	[TestCase("1d6|1d10", 2, "1d8|1d10")]
	[TestCase("1d6|1d8|1d10", 2, "1d8|1d8|1d10")]
	public void AdjustDieSize(string startRoll, int adjustment, string endRoll)
	{
		Assert.AreEqual(endRoll, DieRoll.AdjustDieSize(startRoll, adjustment));
	}

	[TestCase("1d6", 2, "1d6+2")]
	[TestCase("1d6+2", 4, "1d6+6")]
	[TestCase("1d6+2", -4, "1d6-2")]
	[TestCase("2d6+2", -10, "2d6-8")]
	[TestCase("1d4-1d6", 3, "1d4-1d6+3")]
	[TestCase("1d4-2+1d6", 3, "1d4+1+1d6")]
	[TestCase("1d4-2+1d6", 2, "1d4+1d6")]
	[TestCase("3+3d8-2d4", -1, "2+3d8-2d4")]
	[TestCase("3+3d8x7", 1, "4+3d8x7")]
	[TestCase("3+3d8/7", 10, "13+3d8/7")]
	[TestCase("2", 4, "6")]
	[TestCase("7", -6, "1")]
	[TestCase("7", -8, "-1")]
	[TestCase("7", -10, "-3")]
	[TestCase("1-2", 4, "5-6")]
	[TestCase("5-7", 8, "13-15")]
	[TestCase("1-2x3", 6, "7-8x3")]
	[TestCase("1-2/3", 9, "10-11/3")]
	[TestCase("1-2", -4, "1-2-4")]
	[TestCase("3d12[Test]", 8, "3d12+8[Test]")]
	[TestCase("1d6|1d10", 2, "1d6|1d10+2")]
	[TestCase("1d6|1d8|1d10", 2, "1d6|1d8|1d10+2")]
	public void AdjustResult(string startRoll, int adjustment, string endRoll)
	{
		Assert.AreEqual(endRoll, DieRoll.AdjustResult(startRoll, adjustment));
	}

	[TestCase("2d6", ExpectedResult = 7)]
	[TestCase("2-8", ExpectedResult = 5)]
	[TestCase("1d2+4", ExpectedResult = 5.5)]
	[TestCase("1d3x1d5", ExpectedResult = 6)]
	public double AverageTests(string dieRoll)
	{
		return dieRoll.GetCachedDieRoll().Average();
	}
}
