using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SpawningEggSac : IPart
{
	public string Turns = "10-20";

	public int TurnsLeft = int.MinValue;

	public string SpawnCount = "2-4";

	public string SpawnBlueprint = "Svardym Hatchling";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public void tickEgg()
	{
		if (TurnsLeft == int.MinValue)
		{
			TurnsLeft = Stat.Roll(Turns);
		}
		TurnsLeft--;
		if (TurnsLeft > 0)
		{
			return;
		}
		int num = Stat.Roll(SpawnCount);
		int num2 = num;
		Cell cell = ParentObject.CurrentCell;
		List<Cell> list = ((cell != null) ? cell.GetConnectedSpawnLocations(num2).Shuffle() : null);
		if (list.IsNullOrEmpty())
		{
			return;
		}
		foreach (Cell item in list)
		{
			item.AddObject(SpawnBlueprint);
			num2--;
			if (num2 <= 0)
			{
				break;
			}
		}
		if (ParentObject.IsVisible())
		{
			IComponent<GameObject>.AddPlayerMessage("The membrane of the egg sac snots apart.");
			if (num > 1)
			{
				IComponent<GameObject>.AddPlayerMessage("The svardym eggs hatch.");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage("The svardym egg hatches.");
			}
		}
		ParentObject.PlayWorldSound("sfx_creature_appear_eggCrack");
		ParentObject.Slimesplatter(SelfSplatter: false);
		ParentObject.Destroy();
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ParentObject.CurrentCell != null)
		{
			tickEgg();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
