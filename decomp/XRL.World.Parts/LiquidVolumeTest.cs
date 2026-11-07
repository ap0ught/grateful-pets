using NUnit.Framework;

namespace XRL.World.Parts;

public class LiquidVolumeTest
{
	public LiquidVolume dstVol = new LiquidVolume();

	public LiquidVolume srcVol = new LiquidVolume();

	[TestCase(new object[] { "water", 1, "water", 1, "water" })]
	[TestCase(new object[] { "water", 1, "salt", 1, "water-500,salt-500" })]
	[TestCase(new object[] { "water-500,salt-500", 10, "blood", 5, "water-334,salt-333,blood-333" })]
	[TestCase(new object[] { "water-500,salt-500", 10, "water-500,salt-500", -1, "water-500,salt-500" })]
	[TestCase(new object[] { "water-500,salt-500", 10, "water", -5, "salt" })]
	[TestCase(new object[] { "water-500,salt-500", 20, "water", -5, "water-333,salt-667" })]
	[TestCase(new object[] { "water-333,salt-667", 15, "water", 5, "water-499,salt-501" })]
	[TestCase(new object[] { "water", 1, "salt", -1, null })]
	public void MixWith(string dstLiquid, int dstAmount, string srcLiquid, int srcAmount, string resLiquid)
	{
		dstVol.InitialLiquid = dstLiquid;
		dstVol.Volume = dstAmount;
		srcVol.InitialLiquid = srcLiquid;
		srcVol.Volume = srcAmount;
		dstVol.MixWith(srcVol);
		Assert.AreEqual(resLiquid, dstVol.GetLiquidDesignation());
		Assert.AreEqual(dstAmount + srcAmount, dstVol.Volume);
	}

	[TestCase(new object[] { "water", 2, 1, "water" })]
	[TestCase(new object[] { "oil-500,water-500", 2, 1, "oil" })]
	public void UseDramsByEvaporativity(string liquid, int amount, int useAmount, string resLiquid)
	{
		dstVol.InitialLiquid = liquid;
		dstVol.Volume = amount;
		dstVol.UseDramsByEvaporativity(useAmount);
		Assert.AreEqual(resLiquid, dstVol.GetLiquidDesignation());
		Assert.AreEqual(amount - useAmount, dstVol.Volume);
	}
}
