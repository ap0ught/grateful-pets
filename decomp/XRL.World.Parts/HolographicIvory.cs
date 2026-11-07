using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class HolographicIvory : IPart
{
	public string ClusterSize = "1";

	public string Damage = "1d10";

	public string BleedDamage = "1d2";

	public int BleedSave = 20;

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeDismemberedEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != EnteredCellEvent.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<GetScanTypeEvent>.ID && ID != ObjectCreatedEvent.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == PooledEvent<RespiresEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		E.Object.MakeImperviousToHeat();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject)
		{
			E.Value = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeDismemberedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		E.MinMatterPhase(4);
		return false;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.ScanType = Scanning.Scan.Tech;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ClusterSize != "1")
		{
			List<Cell> list = new List<Cell>(ParentObject.CurrentCell.GetLocalEmptyAdjacentCells());
			list.ShuffleInPlace();
			int num = Stat.Roll(ClusterSize);
			for (int i = 0; i < num && i < list.Count; i++)
			{
				if (15.in100())
				{
					list[i].AddObject(ParentObject.Blueprint);
					continue;
				}
				GameObject gameObject = GameObject.Create(ParentObject.Blueprint);
				Impaler part = gameObject.GetPart<Impaler>();
				if (part != null)
				{
					part.ClusterSize = "1";
				}
				list[i].AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object.IsCombatObject() && ParentObject.IsHostileTowards(E.Object) && E.Object.PhaseAndFlightMatches(ParentObject))
		{
			Hidden part = ParentObject.GetPart<Hidden>();
			if (!part.Found)
			{
				part.Found = true;
				DidX("strike", null, "!", null, null, null, E.Object);
				if (E.Object.TakeDamage(Damage.RollCached(), "from %t impalement.", "Stabbing Illusion", null, null, ParentObject))
				{
					E.Object.Bloodsplatter();
					E.Object.ApplyEffect(new HolographicBleeding(BleedDamage, BleedSave, ParentObject, Stack: false));
				}
			}
		}
		return base.HandleEvent(E);
	}
}
