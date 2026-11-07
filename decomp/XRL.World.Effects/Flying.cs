using System;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Flying : Effect
{
	public int Level = 1;

	public GameObject Source;

	public Flying()
	{
		DisplayName = "flying";
		Duration = 9999;
	}

	public Flying(int Level)
		: this()
	{
		this.Level = Level;
		Duration = 9999;
	}

	public Flying(int Level, GameObject Source)
		: this()
	{
		this.Level = Level;
		this.Source = Source;
		Duration = 9999;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 16777344;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		if (Object.GetEffect<Flying>() != this)
		{
			return null;
		}
		return "{{B|flying}}";
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "Can't be attacked in melee by non-flying creatures.\nIsn't affected by terrain.";
	}

	public override bool Apply(GameObject Object)
	{
		FlushNavigationCaches();
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		FlushNavigationCaches();
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Object.GetEffect<Flying>() != this)
		{
			return true;
		}
		if (Duration > 0)
		{
			E.RenderEffectIndicator("\u0018", "Tiles2/status_flying.bmp", "&y", "y", 5);
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Object == Object && Object.IsPlayerLed() && !Object.IsPlayer())
		{
			E.AddAction("Stop Flying", "direct to stop flying", "CompanionStopFlying", null, '_', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, Object);
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<MovementModeChangedEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "CompanionStopFlying")
		{
			Flight.StopFlying(Object, Object);
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference && Object.GetEffect<Flying>() == this)
		{
			E.AddTag("[{{B|flying}}]");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == Object && E.Source != null && !E.Source.IsFlying)
		{
			E.Value = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MovementModeChangedEvent E)
	{
		if (Duration > 0 && E.Object == Object && (E.To == "Engulfing" || E.To == "Engulfed"))
		{
			Flight.Land(E.Object);
		}
		return base.HandleEvent(E);
	}
}
