using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Stasisfield : IPart
{
	public GameObject Creator;

	public override bool SameAs(IPart p)
	{
		if ((p as Stasisfield).Creator != Creator)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BlocksRadarEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EnteredCellEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != PooledEvent<InterruptAutowalkEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		ProcessStasis();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ProcessStasis();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ProcessStasis();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (E.Actor.HasTagOrProperty("ForcefieldNullifier"))
		{
			E.IndicateObject = ParentObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		ShutdownStasis();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BlocksRadarEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void ProcessStasis()
	{
		GameObject.Validate(ref Creator);
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			ParentObject.Obliterate();
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(cell.Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject != ParentObject && cell.Objects.Contains(gameObject) && Stasis.EligibleForStasis(gameObject) && !gameObject.IsInStasis() && gameObject.PhaseMatches(ParentObject))
			{
				gameObject.ForceApplyEffect(new Stasis());
			}
		}
	}

	public void ShutdownStasis()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				cell.Objects[i].GetEffect<Stasis>()?.CheckMaintenance(ParentObject);
			}
		}
	}
}
