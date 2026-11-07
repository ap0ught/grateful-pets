using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class HasSlaves : IBondedLeader
{
	public string NumberOfSlaves = "2-4";

	public string SlaveTier = "4-5";

	public bool StripGear;

	public bool SlavesPlaced;

	public bool StopFightOnDeath;

	public HasSlaves()
	{
	}

	public HasSlaves(string NumberofSlaves, bool StripGear)
		: this()
	{
		NumberOfSlaves = NumberofSlaves;
		this.StripGear = StripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || SlavesPlaced))
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
		if (!SlavesPlaced)
		{
			SlavesPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			int i = 0;
			for (int num = NumberOfSlaves.RollCached(); i < num; i++)
			{
				int tier = SlaveTier.RollCached();
				string aCreatureBlueprint = EncountersAPI.GetACreatureBlueprint((GameObjectBlueprint bp) => bp.Tier == tier && IsSuitableSlave(bp));
				Cell firstEmptyAdjacentCell = cell.GetFirstEmptyAdjacentCell(1, 2);
				if (firstEmptyAdjacentCell != null)
				{
					GameObject gameObject = GameObject.Create(aCreatureBlueprint);
					gameObject.AddPart(new DomesticatedSlave(ParentObject, "Templar", "domesticated", null, "TemplarDomesticant", StripGear));
					firstEmptyAdjacentCell.AddObject(gameObject).MakeActive();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsSuitableSlave(GameObjectBlueprint BP)
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
		if (BP.HasTag("Unenslavable"))
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
		if (BP.IsBodyPartOccupied("Face") && BP.IsBodyPartOccupied("Head"))
		{
			return false;
		}
		return true;
	}
}
