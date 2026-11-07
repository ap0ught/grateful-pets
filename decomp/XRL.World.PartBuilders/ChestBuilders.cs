using System;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class ChestBuilders
{
	public static void BuildCommonChestInventory(Inventory pInventory, int tier, string Context = null)
	{
		if (5.in100())
		{
			BuildSpecialChestInventory(pInventory, tier, Context);
			return;
		}
		int num = Stat.Random(2, 4);
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			try
			{
				GameObject gameObject = PopulationManager.CreateOneFrom("Junk " + Tier.Fuzz(tier), null, 0, 0, null, Context);
				if (gameObject != null)
				{
					pInventory.AddObject(gameObject);
				}
			}
			catch (Exception x)
			{
				if (!flag)
				{
					MetricsManager.LogError("exception in BuildCommonChestInventory()", x);
					flag = true;
				}
			}
		}
	}

	public static void BuildTableChestInventory(Inventory pInventory, string Table, string Amount, int tier, string Context = null)
	{
		int num = Stat.Roll(Amount);
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			try
			{
				pInventory.AddObject(PopulationManager.CreateOneFrom(Table + Tier.Fuzz(tier), null, 0, 0, null, Context));
			}
			catch (Exception x)
			{
				if (!flag)
				{
					MetricsManager.LogError("exception in BuildTableChestInventory(" + Table + ")", x);
					flag = true;
				}
			}
		}
	}

	public static void BuildRareChestInventory(Inventory pInventory, int tier, string Context = null)
	{
		if (5.in100())
		{
			BuildSpecialChestInventory(pInventory, tier, Context);
			return;
		}
		int num = Stat.Random(1, 3);
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			try
			{
				Tier.Fuzz(tier);
				pInventory.AddObject(PopulationManager.CreateOneFrom("Junk " + tier + "R", null, 17, 0, null, Context));
			}
			catch (Exception x)
			{
				if (!flag)
				{
					MetricsManager.LogError("exception in BuildRareChestInventory(" + tier + ")", x);
					flag = true;
				}
			}
		}
	}

	public static void BuildSpecialChestInventory(Inventory pInventory, int Tier, string Context = null)
	{
		string populationName = "Junk 1";
		string dice = "1d4+2";
		switch (Stat.Random(1, 12))
		{
		case 1:
			populationName = "SpecialChestShoes";
			break;
		case 2:
			populationName = "Melee Weapons " + Tier;
			break;
		case 3:
			populationName = "Armor " + Tier;
			break;
		case 4:
			populationName = "Missile " + Tier;
			break;
		case 5:
			populationName = "Ammo " + Tier;
			break;
		case 6:
			populationName = "Cash " + Tier;
			break;
		case 7:
			populationName = "Utility " + Tier;
			break;
		case 8:
			populationName = "Food " + Tier;
			break;
		case 9:
			populationName = "Meds " + Tier;
			break;
		case 10:
			populationName = "Scrap " + Tier;
			break;
		case 11:
			populationName = "Trinkets " + Tier;
			break;
		case 12:
			populationName = "Artifact " + Tier;
			dice = "1d2+1";
			break;
		}
		bool flag = false;
		int i = 0;
		for (int num = dice.RollCached(); i < num; i++)
		{
			try
			{
				pInventory.AddObject(PopulationManager.CreateOneFrom(populationName, null, 0, 0, null, Context));
			}
			catch (Exception x)
			{
				if (!flag)
				{
					MetricsManager.LogError("exception in BuildSpecialChestInventory(" + Tier + ")", x);
					flag = true;
				}
			}
		}
	}
}
