using NUnit.Framework;

namespace XRL.World.Parts;

public class DrillTest
{
	[TestCase(1500, 1)]
	[TestCase(150, 1)]
	[TestCase(101, 1)]
	[TestCase(100, 1)]
	[TestCase(99, 2)]
	[TestCase(75, 2)]
	[TestCase(51, 2)]
	[TestCase(50, 2)]
	[TestCase(49, 3)]
	[TestCase(34, 3)]
	[TestCase(33, 4)]
	[TestCase(26, 4)]
	[TestCase(25, 4)]
	[TestCase(24, 5)]
	[TestCase(17, 6)]
	[TestCase(16, 7)]
	[TestCase(15, 7)]
	[TestCase(14, 8)]
	[TestCase(13, 8)]
	[TestCase(12, 9)]
	[TestCase(11, 10)]
	[TestCase(10, 10)]
	[TestCase(9, 12)]
	[TestCase(8, 13)]
	[TestCase(6, 17)]
	[TestCase(5, 20)]
	[TestCase(4, 25)]
	[TestCase(3, 34)]
	[TestCase(2, 50)]
	[TestCase(1, 100)]
	[TestCase(0, 0)]
	[TestCase(-1, 0)]
	[TestCase(-100, 0)]
	[TestCase(-1000, 0)]
	public void GetWallHitsRequired(int Percentage, int Expected)
	{
		Assert.AreEqual(Expected, Drill.GetWallHitsRequired(Percentage));
	}

	[TestCase(1000.0, 1)]
	[TestCase(99.9, 2)]
	[TestCase(50.0, 2)]
	[TestCase(49.9, 3)]
	[TestCase(33.4, 3)]
	[TestCase(33.3, 4)]
	[TestCase(25.0, 4)]
	[TestCase(24.9, 5)]
	[TestCase(12.5, 8)]
	[TestCase(12.4, 9)]
	[TestCase(-1000.0, 0)]
	public void GetWallHitsRequired(double Percentage, int Expected)
	{
		Assert.AreEqual(Expected, Drill.GetWallHitsRequired(Percentage));
	}
}
