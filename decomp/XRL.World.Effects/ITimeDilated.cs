using XRL.Rules;

namespace XRL.World.Effects;

public abstract class ITimeDilated : Effect, ITierInitialized
{
	public int SpeedPenalty;

	public ITimeDilated()
	{
		DisplayName = "time-dilated";
		Duration = 1;
	}

	public ITimeDilated(int SpeedPenalty)
		: this()
	{
		this.SpeedPenalty = SpeedPenalty;
	}

	public virtual void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 200);
		SpeedPenalty = Stat.Random(1, 9) * 10;
	}

	public abstract bool DoTimeDilationVisualEffects();

	public override int GetEffectType()
	{
		return 117444608;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == PooledEvent<RealityStabilizeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check())
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(Object))
		{
			E.Add("time", 1);
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "time-dilated ({{C|" + -SpeedPenalty + "}} Quickness)";
	}

	public override string GetDetails()
	{
		return -SpeedPenalty + " Quickness";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyTimeDilated"))
		{
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("ApplyTimeDilated");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CanApplyTimeDilated");
		base.Register(Object, Registrar);
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		SpeedPenalty = 0;
		base.Remove(Object);
	}

	public virtual void ApplyChanges()
	{
		StatShifter.SetStatShift("Speed", -SpeedPenalty);
	}

	public virtual void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && DoTimeDilationVisualEffects())
		{
			E.ColorString += "^b";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyTimeDilated" || E.ID == "ApplyTimeDilated")
		{
			return false;
		}
		if (E.ID == "BeforeDeepCopyWithoutEffects")
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
