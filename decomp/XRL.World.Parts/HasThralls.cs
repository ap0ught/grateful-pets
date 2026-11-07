using System;

namespace XRL.World.Parts;

[Serializable]
public class HasThralls : IBondedLeader
{
	public string NumberOfThralls = "2-4";

	public bool StripGear;

	public bool ThrallsPlaced;

	public bool StopFightOnDeath;

	public HasThralls()
	{
	}

	public HasThralls(string NumberOfThralls, bool StripGear)
		: this()
	{
		this.NumberOfThralls = NumberOfThralls;
		this.StripGear = StripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || ThrallsPlaced))
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
		if (!ThrallsPlaced)
		{
			ThrallsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			string populationName = "DynamicInheritsTable:Creature:Tier" + ParentObject.GetTier();
			int i = 0;
			for (int num = NumberOfThralls.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 10)
				{
					GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(PopulationManager.RollOneFrom(populationName).Blueprint);
					if (IsSuitableThrall(blueprintIfExists))
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
						gameObject.AddPart(new PsychicThrall(ParentObject, "Seekers", null, "psychic thrall", "PsychicThrall", StripGear));
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

	public static bool IsSuitableThrall(GameObjectBlueprint BP)
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
		if (BP.HasPart("MentalShield"))
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
		if (BP.HasProperName() || BP.HasPart("Uplift"))
		{
			return false;
		}
		return true;
	}
}
