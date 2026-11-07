using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class SmokingAreaE
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
		Z.GetCell(79, 5).AddObject("SmokecasterW");
		Z.GetCell(79, 7).AddObject("SmokecasterW");
		Z.GetCell(79, 15).AddObject("SmokecasterW");
		return true;
	}
}
