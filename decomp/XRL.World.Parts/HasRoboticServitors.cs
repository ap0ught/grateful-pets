using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class HasRoboticServitors : IBondedLeader
{
	public string NumberOfRoboticServitors = "2-4";

	public bool StripGear;

	public bool ServitorsPlaced;

	public bool StopFightOnDeath = true;

	[Obsolete("mod compat, will be removed after Q1 2024")]
	public string numberOfRoboticServitors
	{
		get
		{
			return NumberOfRoboticServitors;
		}
		set
		{
			NumberOfRoboticServitors = value;
		}
	}

	public HasRoboticServitors()
	{
	}

	public HasRoboticServitors(string NumberOfRoboticServitors, bool StripGear)
		: this()
	{
		this.NumberOfRoboticServitors = NumberOfRoboticServitors;
		this.StripGear = StripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || ServitorsPlaced))
		{
			if (ID == OnDeathRemovalEvent.ID)
			{
				return StopFightOnDeath;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(OnDeathRemovalEvent E)
	{
		if (StopFightOnDeath)
		{
			ParentObject.StopFight();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!ServitorsPlaced)
		{
			ServitorsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			int tier = ParentObject.GetTier();
			List<string> list = new List<string>();
			list.Add("DynamicInheritsTable:Creature:Tier" + tier);
			list.Add("DynamicInheritsTable:Creature:Tier" + tier);
			list.Add("DynamicInheritsTable:Creature:Tier" + tier);
			if (tier >= 8)
			{
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier - 1));
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier - 2));
			}
			else if (tier <= 1)
			{
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier + 1));
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier + 2));
			}
			else
			{
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier + 1));
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier - 1));
			}
			int i = 0;
			for (int num = NumberOfRoboticServitors.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 50)
				{
					GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(PopulationManager.RollOneFrom(list.GetRandomElement()).Blueprint);
					if (IsSuitableServitor(blueprintIfExists))
					{
						gameObjectBlueprint = blueprintIfExists;
						break;
					}
				}
				if (gameObjectBlueprint != null)
				{
					Cell firstEmptyAdjacentCell = cell.GetFirstEmptyAdjacentCell(1, 2);
					if (firstEmptyAdjacentCell != null)
					{
						GameObject gameObject = GameObject.Create(gameObjectBlueprint.Name);
						gameObject.AddPart(new RoboticServitor(ParentObject, null, null, null, null, StripGear));
						firstEmptyAdjacentCell.AddObject(gameObject).MakeActive();
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsSuitableServitor(GameObjectBlueprint BP)
	{
		if (BP == null)
		{
			return false;
		}
		if (!BP.HasPart("Body"))
		{
			return false;
		}
		if (!BP.HasPart("Robot"))
		{
			return false;
		}
		if (!BP.HasPart("Brain"))
		{
			return false;
		}
		if (!BP.HasPart("Combat"))
		{
			return false;
		}
		if (!BP.HasStat("Level"))
		{
			return false;
		}
		if (!BP.GetPartParameter("Brain", "Mobile", Default: true))
		{
			return false;
		}
		if (BP.GetPartParameter("Brain", "Aquatic", Default: false))
		{
			return false;
		}
		if (BP.HasPart("AIWallWalker"))
		{
			return false;
		}
		if (BP.HasProperName())
		{
			return false;
		}
		return true;
	}
}
