using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectAsleep : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectAsleep()
	{
		Description = "accept falling {{c|asleep}}";
		Tile = "Items/sw_bed.bmp";
		RenderString = "\u001c";
		ColorString = "&w";
		DetailColor = 'y';
	}

	public RitualSifrahTokenEffectAsleep(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of falling {{c|asleep}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 1000 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 2000 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Asleep", Chance, "2000-4000");
	}
}
