using Genkit;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class FractiPlanter : ZoneBuilderSandbox
{
	public string Size = "10-20";

	public string Amount = "3-4";

	public string OkFloors = "SaltPath";

	public bool BuildZone(Zone Z, Point2D P)
	{
		Cell cell = Z.GetCell(P);
		if (cell != null)
		{
			int num = Amount.RollCached();
			for (int i = 0; i < num; i++)
			{
				cell.AddObject("Fracti")?.GetPart<Fracti>()?.Grow(OkFloors, Size.RollCached());
			}
		}
		return true;
	}
}
