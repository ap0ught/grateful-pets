using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectConfused : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectConfused()
	{
		Description = "accept becoming disoriented";
		Tile = "Mutations/confusion.bmp";
		RenderString = "?";
		ColorString = "&G";
		DetailColor = 'R';
	}

	public RitualSifrahTokenEffectConfused(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming confused";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 1800 - Chance * 2;
	}

	public override int GetTiebreakerPriority()
	{
		return 3600 - Chance * 4;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Confused", Chance);
	}
}
