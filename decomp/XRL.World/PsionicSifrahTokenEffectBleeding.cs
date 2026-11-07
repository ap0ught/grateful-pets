using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectBleeding : RitualSifrahTokenEffectBleeding
{
	public PsionicSifrahTokenEffectBleeding()
	{
	}

	public PsionicSifrahTokenEffectBleeding(int Chance)
		: base(Chance)
	{
	}
}
