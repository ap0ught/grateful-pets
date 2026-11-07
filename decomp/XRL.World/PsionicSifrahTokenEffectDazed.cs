using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectDazed : RitualSifrahTokenEffectDazed
{
	public PsionicSifrahTokenEffectDazed()
	{
	}

	public PsionicSifrahTokenEffectDazed(int Chance)
		: base(Chance)
	{
	}
}
