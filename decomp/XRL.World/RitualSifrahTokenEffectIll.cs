using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectIll : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectIll()
	{
		Description = "accept becoming {{g|ill}}";
		Tile = "Items/sw_cherubic.bmp";
		RenderString = "á";
		ColorString = "&g";
		DetailColor = 'G';
	}

	public RitualSifrahTokenEffectIll(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{g|ill}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 800 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 1000 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Ill", Chance, "1000-2000", null, Stack: false, Force: false, new Ill(1, 1, "You feel nauseous."));
	}
}
