using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class WarTrance : Effect, ITierInitialized
{
	public WarTrance()
	{
		Duration = 2;
		DisplayName = "{{r|war trance}}";
	}

	public WarTrance(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(50, 1000);
	}

	public override int GetEffectType()
	{
		return 83886082;
	}

	public override string GetStateDescription()
	{
		return "in a {{r|war trance}}";
	}

	public override string GetDetails()
	{
		return "+5 Quickness\n+6 Willpower";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyWarTrance", "Duration", Duration)))
		{
			return false;
		}
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges()
	{
		StatShifter.SetStatShift("Speed", 5);
		StatShifter.SetStatShift("Willpower", 6);
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "!";
				E.ColorString = "&R";
			}
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			Duration--;
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}
}
