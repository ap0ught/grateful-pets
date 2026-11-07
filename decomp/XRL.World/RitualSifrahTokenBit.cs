using System;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenBit : SocialSifrahTokenBit
{
	public RitualSifrahTokenBit()
	{
		Description = "offer bit";
	}

	public RitualSifrahTokenBit(BitType bitType)
		: base(bitType)
	{
		Description = "offer " + bitType.Description;
	}
}
