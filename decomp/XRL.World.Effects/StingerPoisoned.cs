using System;

namespace XRL.World.Effects;

[Serializable]
public class StingerPoisoned : Poisoned
{
	public StingerPoisoned()
	{
	}

	public StingerPoisoned(int Duration, string DamageIncrement, int Level, GameObject Owner = null)
		: base(Duration, DamageIncrement, Level, Owner)
	{
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<StingerPoisoned>(out var Effect))
		{
			Effect.Duration = (Effect.Duration + Duration) / 2;
			Effect.DamageIncrement = Convert.ToInt32(Effect.DamageIncrement.Split('d')[0]) + Convert.ToInt32(DamageIncrement.Split('d')[0]) + "d2";
			Effect.Level = (Effect.Level + Level) / 2;
			return false;
		}
		if (Object.FireEvent("ApplyPoison") && ApplyEffectEvent.Check(Object, "Poison", this))
		{
			DidX("have", "been poisoned", "!", null, null, null, Object);
			return true;
		}
		return false;
	}
}
