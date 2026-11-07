using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectCardiacArrest : RitualSifrahTokenEffectCardiacArrest
{
	public PsionicSifrahTokenEffectCardiacArrest()
	{
	}

	public PsionicSifrahTokenEffectCardiacArrest(int Chance)
		: base(Chance)
	{
	}
}
