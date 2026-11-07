using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Burning : SoundRendererEffect
{
	[NonSerialized]
	private string FlameSoundID;

	public Burning()
	{
		DisplayName = "{{R|burning}}";
	}

	public Burning(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDescription()
	{
		if (Object == null || !Object.IsPlayer())
		{
			return base.GetDescription();
		}
		return null;
	}

	public override int GetEffectType()
	{
		return 33554944;
	}

	public override string GetStateDescription()
	{
		return "{{R|on fire}}";
	}

	public static bool CanApply(GameObject Object)
	{
		if (!CanApplyEffectEvent.Check<Burning>(Object))
		{
			return false;
		}
		return true;
	}

	public static bool ApplyTo(GameObject Object)
	{
		if (!CanApply(Object))
		{
			return false;
		}
		if (Object.HasEffect<Burning>())
		{
			return false;
		}
		return Object.ApplyEffect(new Burning(1));
	}

	public override bool Apply(GameObject Object)
	{
		Object.PlayWorldSound("sfx_statusEffect_ignited");
		if (AutoAct.IsActive())
		{
			if (Object.IsPlayer())
			{
				AutoAct.Interrupt("you caught fire");
			}
			else if (Object.IsPlayerLedAndPerceptible() && !Object.IsTrifling)
			{
				AutoAct.Interrupt("you " + Object.GetPerceptionVerb() + " " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + " catch fire" + (Object.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(Object))), null, Object, IsThreat: true);
			}
		}
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		Object.PlayWorldSound("sfx_statusEffect_fireExtinguished");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCompanionStatusEvent>.ID && ID != BeforeRenderEvent.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		AddLight(3);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("on fire", 50);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject gameObject = Object;
		Physics physics = gameObject.Physics;
		if (!physics.IsAflame())
		{
			gameObject.RemoveEffect(this);
		}
		else
		{
			Object.TakeDamage(GetBurningAmount(gameObject).RollCached(), "from the fire%S!", "Fire", null, null, Environmental: physics.AmbientTemperature >= physics.FlameTemperature || RadiatesHeatEvent.Check(physics.CurrentCell) || RadiatesHeatAdjacentEvent.Check(physics.CurrentCell), Owner: physics.InflamedBy, Attacker: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: true, Indirect: true, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: true);
			if (!gameObject.GetTag("HotBurn").IsNullOrEmpty() && int.TryParse(gameObject.GetTag("HotBurn"), out var result))
			{
				physics.Temperature += result;
			}
		}
		return base.HandleEvent(E);
	}

	public static string GetBurningAmount(GameObject Object)
	{
		if (!Object.IsValid())
		{
			return "1";
		}
		int num = Object.Physics.Temperature - Object.Physics.FlameTemperature;
		if (num <= 300)
		{
			if (num < 0 || num <= 100)
			{
				return "1";
			}
			return "1-2";
		}
		if (num <= 700)
		{
			if (num <= 500)
			{
				return "2-3";
			}
			return "3-4";
		}
		if (num <= 900)
		{
			return "4-5";
		}
		return "5-6";
	}

	public override string GetDetails()
	{
		return GetBurningAmount(Object) + " damage per turn.";
	}

	public override bool Render(RenderEvent E)
	{
		int num = Stat.RandomCosmetic(1, 4);
		if (Object.HasEffect(typeof(CoatedInPlasma)))
		{
			switch (num)
			{
			case 1:
				E.ColorString = "&G";
				E.BackgroundString = "^g";
				break;
			case 2:
				E.ColorString = "&g";
				E.BackgroundString = "^Y";
				break;
			case 3:
				E.ColorString = "&G";
				E.BackgroundString = "^k";
				break;
			case 4:
				E.ColorString = "&Y";
				E.BackgroundString = "^k";
				break;
			}
		}
		else
		{
			switch (num)
			{
			case 1:
				E.ColorString = "&R";
				E.BackgroundString = "^W";
				break;
			case 2:
				E.ColorString = "&r";
				E.BackgroundString = "^W";
				break;
			case 3:
				E.ColorString = "&R";
				E.BackgroundString = "^k";
				break;
			case 4:
				E.ColorString = "&Y";
				E.BackgroundString = "^k";
				break;
			}
		}
		if (Stat.RandomCosmetic(1, 20) == 1)
		{
			Object.Smoke();
		}
		return true;
	}

	public override void Render(RenderSoundEvent E)
	{
		if (Options.UseFireSounds && Object.Physics.IsReal)
		{
			if (FlameSoundID == null)
			{
				FlameSoundID = Guid.NewGuid().ToString();
			}
			E.Char.soundExtra.Add(FlameSoundID, "sfx_statusEffect_onFire_lp");
		}
	}
}
