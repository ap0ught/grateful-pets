using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectShamed : SocialSifrahTokenEffectShamed
{
	public RitualSifrahTokenEffectShamed()
	{
	}

	public RitualSifrahTokenEffectShamed(int Chance)
		: base(Chance)
	{
	}
}
