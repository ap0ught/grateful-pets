using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Ecstatic : Effect
{
	public Ecstatic()
	{
		DisplayName = "{{G|ecstatic}}";
		Duration = 1;
	}

	public Ecstatic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 16777218;
	}

	public override string GetDetails()
	{
		return "+10 Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyEcstatic", "Duration", Duration)))
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
		StatShifter.SetStatShift("Speed", 10);
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts();
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

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "!";
				E.ColorString = "&G";
				return false;
			}
		}
		return true;
	}
}
