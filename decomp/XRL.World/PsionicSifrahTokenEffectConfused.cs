using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectConfused : RitualSifrahTokenEffectConfused
{
	public PsionicSifrahTokenEffectConfused()
	{
	}

	public PsionicSifrahTokenEffectConfused(int Chance)
		: base(Chance)
	{
	}
}
