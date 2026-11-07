using System;
using System.Linq;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Interdicted : Effect
{
	private int PenaltyToApply = 10;

	private int CurrentSpeedPenalty;

	public string interdictorId;

	public Interdicted()
	{
		DisplayName = "{{C|interdicted}}";
		Duration = 1;
		PenaltyToApply = Stat.Random(1, 9) * 10;
	}

	public Interdicted(string interdictorId, int penalty)
		: this()
	{
		PenaltyToApply = penalty;
		this.interdictorId = interdictorId;
	}

	public override int GetEffectType()
	{
		return 33554560;
	}

	public override string GetDetails()
	{
		return "-10 Move Speed";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		foreach (Effect effect in Object.Effects)
		{
			if (effect is Interdicted interdicted && interdicted.interdictorId == interdictorId)
			{
				return false;
			}
		}
		if (Object.FireEvent(Event.New("ApplyInterdiction", "Duration", Duration)))
		{
			CurrentSpeedPenalty = PenaltyToApply;
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		CurrentSpeedPenalty = 0;
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "MoveSpeed", CurrentSpeedPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeginTakeAction");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "?";
				E.ColorString = "&c^b";
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandTakeAction" || E.ID == "EndTurn")
		{
			GameObject gameObject = Object.GetVisibleCombatObjects().FirstOrDefault((GameObject o) => o.ID == interdictorId);
			if (gameObject == null || !Object.HasLOSTo(gameObject))
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
}
