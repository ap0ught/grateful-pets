using System;

namespace XRL.World.Parts;

[Serializable]
public class HasCompanions : IBondedLeader
{
	public string NumberOfCompanions = "2-4";

	public bool StripGear;

	public bool CompanionsPlaced;

	public bool StopFightOnDeath;

	[Obsolete("mod compat, will be removed after Q1 2024")]
	public string numberOfCompanions
	{
		get
		{
			return NumberOfCompanions;
		}
		set
		{
			NumberOfCompanions = value;
		}
	}

	public HasCompanions()
	{
	}

	public HasCompanions(string NumberOfCompanions, bool StripGear)
		: this()
	{
		this.NumberOfCompanions = NumberOfCompanions;
		this.StripGear = StripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || CompanionsPlaced))
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
		if (!CompanionsPlaced)
		{
			CompanionsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			string populationName = "DynamicInheritsTable:Creature:Tier" + ParentObject.GetTier();
			int i = 0;
			for (int num = NumberOfCompanions.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 10)
				{
					GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(PopulationManager.RollOneFrom(populationName).Blueprint);
					if (IsSuitableCompanion(blueprintIfExists))
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
						gameObject.AddPart(new Companion(ParentObject, null, null, null, null, StripGear));
						firstEmptyAdjacentCell.AddObject(gameObject).MakeActive();
					}
				}
			}
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsSuitableCompanion(GameObjectBlueprint BP)
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
