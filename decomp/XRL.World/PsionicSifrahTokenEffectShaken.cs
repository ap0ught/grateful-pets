using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenEffectShaken : RitualSifrahTokenEffectShaken
{
	public PsionicSifrahTokenEffectShaken()
	{
	}

	public PsionicSifrahTokenEffectShaken(int Chance)
		: base(Chance)
	{
	}
}
