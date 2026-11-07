using System;
using XRL.Wish;

namespace XRL.World.Effects;

[Serializable]
[HasWishCommand]
public class FungalVisionary : Effect, ITierInitialized
{
	public static int VisionLevel;

	public FungalVisionary()
	{
		DisplayName = "{{O|shimmering}}";
		Duration = 1;
	}

	public FungalVisionary(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = 1000;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 8194;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "-60 Quickness\nCan see into dimensions half a step over.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyFungalVisionary", "Duration", Duration)))
		{
			return false;
		}
		FungalVisionary effect = Object.GetEffect<FungalVisionary>();
		if (effect != null)
		{
			effect.Duration = Math.Max(Duration, effect.Duration);
			return false;
		}
		ApplyStats();
		if (Object.IsPlayer())
		{
			VisionLevel = 1;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		if (Object.IsPlayer())
		{
			VisionLevel = 0;
		}
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("Speed", -60);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<AfterPlayerBodyChangeEvent>.ID)
		{
			return ID == SingletonEvent<AfterGameLoadedEvent>.ID;
		}
		return true;
	}

	public static void CheckVisionLevel()
	{
		VisionLevel = (The.Player.HasEffect<FungalVisionary>() ? 1 : 0);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		CheckVisionLevel();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (E.NewBody == null || !E.NewBody.HasEffect<FungalVisionary>())
		{
			VisionLevel = 0;
		}
		else
		{
			VisionLevel = 1;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		CheckVisionLevel();
		return base.HandleEvent(E);
	}

	[WishCommand("shimmering", null)]
	public void Wish()
	{
		The.Player.ForceApplyEffect(new FungalVisionary(1000));
	}
}
