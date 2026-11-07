using System;
using XRL.Core;
using XRL.Rules;
using XRL.Wish;

namespace XRL.World.Effects;

[Serializable]
[HasWishCommand]
public class Hobbled : Effect, ITierInitialized
{
	public const int BASE_MOVE_SPEED_PENALTY = 50;

	public int Penalty;

	public Hobbled()
	{
		DisplayName = "{{C|hobbled}}";
	}

	public Hobbled(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 100);
	}

	public override int GetEffectType()
	{
		return 50332672;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "-50% Move Speed";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasStat("MoveSpeed") && Object.FireEvent("ApplyHobble"))
		{
			return false;
		}
		DidX("are", "hobbled", "!", null, null, null, Object);
		Object.ParticleText("*hobbled*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		if (Object.TryGetEffect<Hobbled>(out var Effect))
		{
			if (Effect.Duration < Duration)
			{
				Effect.Duration = Duration;
			}
			return false;
		}
		Penalty = 50;
		StatShifter.SetStatShift("MoveSpeed", Penalty);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		Penalty = 0;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			Duration--;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 50 && num < 60)
			{
				E.RenderString = "\u000f";
				E.ColorString = Object.GetTag("BleedParticleColor", "&r^r");
			}
		}
		return true;
	}

	[WishCommand("hobbled", null)]
	public static void Wish()
	{
		The.Player.ApplyEffect(new Hobbled(20));
	}
}
