using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class PlaceRelicBuilder
{
	public string Relic;

	public string Blueprint;

	public bool AddCreditWedges = true;

	public bool BuildZoneWithRelic(Zone Z, GameObject relic)
	{
		Inventory inventory = Z.GetObjectWithTag("RelicContainer")?.Inventory;
		if (inventory != null)
		{
			inventory.AddObject(relic);
			inventory.ParentObject.SetImportant(flag: true, force: true);
			if (!AddCreditWedges)
			{
				return true;
			}
			switch (Z.NewTier)
			{
			case 0:
			case 1:
			case 2:
				inventory.AddObject("CyberneticsCreditWedge2");
				break;
			case 3:
				inventory.AddObject("CyberneticsCreditWedge3");
				break;
			case 4:
			case 5:
				inventory.AddObject("CyberneticsCreditWedge2");
				inventory.AddObject("CyberneticsCreditWedge2");
				break;
			case 6:
			case 7:
				inventory.AddObject("CyberneticsCreditWedge3");
				inventory.AddObject("CyberneticsCreditWedge3");
				break;
			case 8:
				inventory.AddObject("CyberneticsCreditWedge3");
				inventory.AddObject("CyberneticsCreditWedge3");
				inventory.AddObject("CyberneticsCreditWedge2");
				break;
			}
			return true;
		}
		int num = Stat.Random(1, 3);
		while (true)
		{
			if (num == 1)
			{
				foreach (GameObject item in Z.GetObjectsWithPart("Container").Shuffle())
				{
					if (item.HasPart<Inventory>())
					{
						item.Inventory.AddObject(relic);
						item.SetImportant(flag: true, force: true);
						return true;
					}
				}
			}
			if (num == 1 || num == 2)
			{
				foreach (GameObject item2 in Z.GetObjectsWithPart("Brain").Shuffle())
				{
					if (item2.HasPart<Inventory>() && item2.IsCombat && !item2.IsTemporary)
					{
						item2.Inventory.AddObject(relic);
						item2.SetImportant(flag: true, force: true);
						return true;
					}
				}
			}
			using (List<Cell>.Enumerator enumerator2 = Z.GetEmptyReachableCells().Shuffle().GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					enumerator2.Current.AddObject(relic);
					return true;
				}
			}
			switch (num)
			{
			case 3:
				num = 1;
				break;
			case 2:
				num = 1;
				break;
			default:
				return true;
			}
		}
	}

	public bool BuildZone(Zone Z)
	{
		if (!Relic.IsNullOrEmpty())
		{
			return BuildZoneWithRelic(Z, The.ZoneManager.GetCachedObjects(Relic));
		}
		if (!Blueprint.IsNullOrEmpty())
		{
			return BuildZoneWithRelic(Z, GameObject.Create(Blueprint));
		}
		return true;
	}
}
