using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectLost : RitualSifrahTokenEffectLost
{
	public PsionicSifrahTokenEffectLost()
	{
	}

	public PsionicSifrahTokenEffectLost(int Chance)
		: base(Chance)
	{
	}
}
