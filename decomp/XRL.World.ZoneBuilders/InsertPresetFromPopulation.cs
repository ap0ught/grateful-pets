using UnityEngine;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class InsertPresetFromPopulation : ZoneBuilderSandbox
{
	public string Population;

	public bool Important;

	public bool BuildZone(Zone Z)
	{
		string blueprint = PopulationManager.RollOneFrom(Population).Blueprint;
		int num = 0;
		int num2 = 0;
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(blueprint);
		if (gameObject == null)
		{
			Debug.LogError("Bad preset blueprint: " + blueprint);
		}
		if (gameObject.HasPart<MapChunkPlacement>())
		{
			num = gameObject.GetPart<MapChunkPlacement>().Width;
			num2 = gameObject.GetPart<MapChunkPlacement>().Height;
		}
		if (gameObject.HasPart<MultiMapChunkPlacement>())
		{
			MultiMapChunkPlacement part = gameObject.GetPart<MultiMapChunkPlacement>();
			num = part.MapsWide * part.Width;
			num2 = part.MapsHigh * part.Height;
		}
		foreach (Cell item in Z.GetCells().Shuffle())
		{
			if (item.X < Z.Width - num && item.Y < Z.Height - num2)
			{
				for (int i = 0; i < item.X + num; i++)
				{
					int num3 = 0;
					while (num3 < item.Y + num2)
					{
						if (!Z.GetCell(i, num3).HasObjectWithPart("StairsUp") && !Z.GetCell(i, num3).HasObjectWithPart("StairsDown"))
						{
							num3++;
							continue;
						}
						goto IL_0158;
					}
				}
			}
			GameObject gameObject2 = item.AddObject(gameObject);
			if (Important)
			{
				gameObject2?.SetImportant(flag: true);
			}
			break;
			IL_0158:;
		}
		new ForceConnections().BuildZone(Z);
		return true;
	}
}
