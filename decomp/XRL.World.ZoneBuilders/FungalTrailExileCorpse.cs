using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class FungalTrailExileCorpse : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		bool flag = false;
		int num = 20;
		foreach (Location2D item in YieldGaussianCellCluster(Stat.Random(40, 60), Stat.Random(8, 12), 2f, 4f))
		{
			Cell cell = Z.GetCell(item);
			if (cell.IsEmpty())
			{
				if (num-- < 0)
				{
					break;
				}
				if (!flag)
				{
					cell.AddObject("ExileCorpse");
					flag = true;
				}
				else if (!cell.HasObjectWithBlueprint("Godshroom"))
				{
					cell.AddObject("Godshroom");
				}
			}
		}
		return flag;
	}
}
