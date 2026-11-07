using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Dazed : Effect, ITierInitialized
{
	public int Penalty = 4;

	public int SpeedPenalty = 10;

	public bool DontStunIfPlayer;

	public Dazed()
	{
		DisplayName = "{{C|dazed}}";
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 200);
	}

	public override string GetDescription()
	{
		return DisplayName;
	}

	public Dazed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Dazed(int Duration, bool DontStunIfPlayer)
		: this(Duration)
	{
		this.DontStunIfPlayer = DontStunIfPlayer;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		Dazed dazed = e as Dazed;
		if (dazed.Penalty != Penalty)
		{
			return false;
		}
		if (dazed.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		if (dazed.DontStunIfPlayer != DontStunIfPlayer)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "-4 Agility\n-4 Intelligence\n-10 Move Speed";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.Brain == null)
		{
			return false;
		}
		if (Object.HasEffect<Dazed>())
		{
			if (!DontStunIfPlayer || !Object.IsPlayer() || !Object.HasEffect<Stun>())
			{
				Object.ApplyEffect(new Stun(1, 30, DontStunIfPlayer));
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyDazed", "Duration", Duration)))
		{
			return false;
		}
		DidX("are", "dazed", null, null, null, null, Object);
		Object.ParticleText("*dazed*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		Penalty = 4;
		SpeedPenalty = 10;
		StatShifter.SetStatShift(Object, "Intelligence", -Penalty);
		StatShifter.SetStatShift(Object, "Agility", -Penalty);
		StatShifter.SetStatShift(Object, "MoveSpeed", SpeedPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
		Penalty = 0;
		SpeedPenalty = 0;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0 && Duration != 9999 && !Object.HasEffect<Stun>())
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("ApplyDazed");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
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
		if (E.ID == "ApplyDazed")
		{
			if (Duration > 0)
			{
				return false;
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
