using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenCharge : SocialSifrahTokenCharge
{
	public RitualSifrahTokenCharge()
	{
	}

	public RitualSifrahTokenCharge(int Amount)
		: base(Amount)
	{
	}
}
