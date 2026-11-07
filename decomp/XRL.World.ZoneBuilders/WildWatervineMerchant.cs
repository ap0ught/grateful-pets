using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class WildWatervineMerchant
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < 2500; i++)
		{
			int x = Stat.Random(0, 79);
			int y = Stat.Random(0, 24);
			if (Z.GetCell(x, y).Objects.Count > 0 && Z.GetCell(x, y).Objects[0].HasPart<LiquidVolume>())
			{
				Z.GetCell(x, y).AddObject("Wild Water Merchant");
				return true;
			}
		}
		return false;
	}
}
