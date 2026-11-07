using System;
using XRL.Language;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Unpiloted : Effect
{
	public int SpeedPenaltyPercent;

	public bool CanTakeAction = true;

	[NonSerialized]
	private Vehicle _Vehicle;

	public Vehicle Vehicle => _Vehicle ?? (_Vehicle = Object.GetPart<Vehicle>());

	public Unpiloted()
	{
		DisplayName = "{{C|unpiloted}}";
		Duration = 1;
	}

	public Unpiloted(int Percent, bool CanTakeAction = true)
		: this()
	{
		SpeedPenaltyPercent = Percent;
		this.CanTakeAction = CanTakeAction;
	}

	public override int GetEffectType()
	{
		return 33554560;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (!CanTakeAction)
		{
			return "Can't take actions.";
		}
		string text = ((SpeedPenaltyPercent > 0 && SpeedPenaltyPercent % 100 == 0) ? (Grammar.Multiplicative(SpeedPenaltyPercent / 100 + 1) + " as slowly") : ("at " + SpeedPenaltyPercent.ToString("+0;-#") + "% action cost"));
		return "This creature performs non-movement actions " + text + ".";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect(typeof(Unpiloted)))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Unpiloted", this))
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != SingletonEvent<BeforeTakeActionEvent>.ID || CanTakeAction) && (ID != PooledEvent<BeforeSetFeelingEvent>.ID || CanTakeAction) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == SingletonEvent<GetEnergyCostEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		if (CanTakeAction)
		{
			return base.HandleEvent(E);
		}
		return false;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!string.IsNullOrEmpty(Vehicle?.PilotID))
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type.IsNullOrEmpty() || !E.Type.Contains("Move"))
		{
			E.PercentageReduction -= SpeedPenaltyPercent;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (!CanTakeAction)
		{
			E.Feeling = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeSetFeelingEvent E)
	{
		if (!CanTakeAction)
		{
			E.Feeling = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (!CanTakeAction)
		{
			Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
		}
	}
}
