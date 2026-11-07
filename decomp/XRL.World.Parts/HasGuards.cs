using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class HasGuards : IBondedLeader
{
	public string NumberOfGuards = "2-3";

	public bool GuardsPlaced;

	public bool StopFightOnDeath;

	[Obsolete("mod compat, will be removed after Q1 2024")]
	public string numberOfGuards
	{
		get
		{
			return NumberOfGuards;
		}
		set
		{
			NumberOfGuards = value;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || GuardsPlaced))
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
		if (!GuardsPlaced)
		{
			GuardsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			string populationName = "DynamicInheritsTable:Creature:Tier" + Tier.Constrain(cell.ParentZone.NewTier + 1);
			int i = 0;
			for (int num = NumberOfGuards.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 10)
				{
					GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(PopulationManager.RollOneFrom(populationName).Blueprint);
					if (IsSuitableGuard(blueprintIfExists))
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
						gameObject.AddPart(new HiredGuard(ParentObject));
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

	public static bool IsSuitableGuard(GameObjectBlueprint BP)
	{
		if (BP == null)
		{
			return false;
		}
		if (!BP.HasPart("Body"))
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
		if (!BP.GetPartParameter("Brain", "Mobile", Default: true))
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
		if (BP.HasTagOrProperty("NoGuard"))
		{
			return false;
		}
		return true;
	}
}
