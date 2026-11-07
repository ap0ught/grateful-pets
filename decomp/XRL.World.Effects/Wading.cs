using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Wading : Effect
{
	private const int BASE_MOVE_SPEED_PENALTY = 20;

	public int? MoveSpeedShiftApplied;

	[NonSerialized]
	private long validatedOn;

	public Wading()
	{
		DisplayName = "{{B|wading}}";
		Duration = 1;
	}

	public override int GetEffectType()
	{
		int num = 16777344;
		if (Object == null || GetTargetMoveSpeedPenalty(Object) > 0)
		{
			num |= 0x2000000;
		}
		return num;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{B|wading}}";
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override string GetDetails()
	{
		CheckMoveSpeedPenalty();
		int num = -(MoveSpeedShiftApplied ?? 20);
		if (num == 0)
		{
			return "Moving at full speed.";
		}
		return num.Signed() + " move speed.";
	}

	public static int GetTargetMoveSpeedPenalty(GameObject obj)
	{
		int MoveSpeedPenalty = 20;
		GetWadingPerformanceEvent.GetFor(obj, ref MoveSpeedPenalty);
		return MoveSpeedPenalty;
	}

	public void CheckMoveSpeedPenalty()
	{
		if (Object != null)
		{
			MoveSpeedShiftApplied = GetTargetMoveSpeedPenalty(Object);
			StatShifter.SetStatShift("MoveSpeed", MoveSpeedShiftApplied.Value);
		}
	}

	public void RemoveMoveSpeedPenalty()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Wading>())
		{
			return false;
		}
		if (!Object.CanChangeMovementMode("Wading", ShowMessage: false, Involuntary: true))
		{
			return false;
		}
		CheckMoveSpeedPenalty();
		Object.FireEvent("StartWading");
		Object.MovementModeChanged("Wading", Involuntary: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		RemoveMoveSpeedPenalty();
		Object.MovementModeChanged("NotWading", Involuntary: true);
		Object.FireEvent("StopWading");
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EnteredCellEvent.ID)
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (Duration > 0 && E.Object == Object && E.To != "Flying" && E.To != "Swimming" && E.To != "Dodging" && E.To != "Juking" && E.To != "Jumping" && E.To != "Charging" && E.To != "Stuck" && E.To != "Engulfed")
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You cannot do that while wading.");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Validate(E.Cell);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			Validate();
			if (Duration > 0)
			{
				E.AddTag("{{y|[{{B|wading}}]}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "MovementModeChanged")
		{
			string stringParameter = E.GetStringParameter("To");
			if (stringParameter == "Flying" || stringParameter == "Swimming")
			{
				Object.RemoveEffect(this);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			E.RenderEffectIndicator("à", "Tiles2/status_swimming.bmp", "&b", "b", 45);
		}
		return true;
	}

	private void ApplyStats()
	{
		CheckMoveSpeedPenalty();
	}

	private void UnapplyStats()
	{
		RemoveMoveSpeedPenalty();
	}

	public void Validate(Cell C = null)
	{
		if (Object != null && (C != null || XRLCore.Core.Game.Segments > validatedOn))
		{
			validatedOn = XRLCore.Core.Game.Segments;
			if (C == null)
			{
				C = Object.CurrentCell;
			}
			if (C == null || !C.HasWadingDepthLiquid() || C.HasBridge() || Object.HasEffect<Swimming>() || Object.IsFlying)
			{
				Duration = 0;
				Object.RemoveEffect(this, NeedStackCheck: false);
			}
		}
	}
}
