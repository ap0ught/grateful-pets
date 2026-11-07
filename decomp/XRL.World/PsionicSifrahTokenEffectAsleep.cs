using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectAsleep : RitualSifrahTokenEffectAsleep
{
	public PsionicSifrahTokenEffectAsleep()
	{
	}

	public PsionicSifrahTokenEffectAsleep(int Chance)
		: base(Chance)
	{
	}
}
