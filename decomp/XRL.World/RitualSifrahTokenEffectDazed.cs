using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectDazed : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectDazed()
	{
		Description = "accept becoming {{C|dazed}}";
		Tile = "Items/sw_crunch1.bmp";
		RenderString = "*";
		ColorString = "&C";
		DetailColor = 'W';
	}

	public RitualSifrahTokenEffectDazed(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{C|dazed}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 4000 - Chance * 4;
	}

	public override int GetTiebreakerPriority()
	{
		return 8000 - Chance * 8;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Dazed", Chance, null, null, Stack: true);
	}
}
