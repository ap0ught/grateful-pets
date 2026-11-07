using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectPoisoned : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectPoisoned()
	{
		Description = "accept becoming {{g|ill}}";
		Tile = "Items/sw_stinger.bmp";
		RenderString = "ô";
		ColorString = "&w";
		DetailColor = 'G';
	}

	public RitualSifrahTokenEffectPoisoned(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{G|poisoned}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 350 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 450 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Poisoned", Chance, null, null, Stack: true, Force: false, new Poisoned("12-36".RollCached(), "1d2+2".RollCached() + "d2", 10));
	}
}
