using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectExhausted : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectExhausted()
	{
		Description = "accept becoming {{K|exhausted}}";
		Tile = "Items/sw_beautiful.bmp";
		RenderString = "\u001f";
		ColorString = "&K";
		DetailColor = 'b';
	}

	public RitualSifrahTokenEffectExhausted(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{K|exhausted}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 600 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 1000 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Exhausted", Chance, "10-40");
	}
}
