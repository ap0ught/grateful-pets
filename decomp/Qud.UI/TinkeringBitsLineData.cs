using XRL.UI.Framework;
using XRL.World.Tinkering;

namespace Qud.UI;

public class TinkeringBitsLineData : PooledFrameworkDataElement<TinkeringBitsLineData>
{
	public char cBit;

	public string bit;

	public int amount;

	public BitCost activeCost;

	public TinkeringBitsLineData set(char cBit, string bit, int amount, BitCost activeCost)
	{
		this.cBit = cBit;
		this.bit = bit;
		this.amount = amount;
		this.activeCost = activeCost;
		return this;
	}

	public override void free()
	{
		activeCost = null;
		bit = null;
		amount = 0;
		base.free();
	}
}
