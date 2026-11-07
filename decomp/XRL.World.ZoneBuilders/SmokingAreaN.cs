using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class SmokingAreaN
{
	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < 600; i++)
		{
			int x = Stat.Random(0, 79);
			int y = Stat.Random(0, 24);
			GameObject firstObjectWithPart = Z.GetCell(x, y).GetFirstObjectWithPart("LiquidVolume");
			if (firstObjectWithPart != null)
			{
				LiquidVolume liquidVolume = firstObjectWithPart.LiquidVolume;
				if (liquidVolume != null && !liquidVolume.ComponentLiquids.ContainsKey("blood"))
				{
					liquidVolume.MixWith(new LiquidVolume("blood", liquidVolume.Volume));
				}
			}
		}
		Z.GetCell(10, 0).AddObject("SmokecasterS");
		Z.GetCell(40, 0).AddObject("SmokecasterS");
		Z.GetCell(70, 0).AddObject("SmokecasterS");
		return true;
	}
}
