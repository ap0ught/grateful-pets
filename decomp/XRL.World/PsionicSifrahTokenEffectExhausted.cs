using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectExhausted : RitualSifrahTokenEffectExhausted
{
	public PsionicSifrahTokenEffectExhausted()
	{
	}

	public PsionicSifrahTokenEffectExhausted(int Chance)
		: base(Chance)
	{
	}
}
