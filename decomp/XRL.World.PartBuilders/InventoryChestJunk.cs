using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		Inventory inventory = iPart as Inventory;
		bool flag = false;
		int i = 0;
		for (int num = Stat.Random(1, 10); i < num; i++)
		{
			try
			{
				inventory.AddObject(PopulationManager.CreateOneFrom("Junk 1", null, 0, 0, null, Context));
			}
			catch (Exception x)
			{
				if (!flag)
				{
					MetricsManager.LogError("exception in InventoryChestJunk.BuildPart()", x);
					flag = true;
				}
			}
		}
	}
}
