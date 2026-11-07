using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectLost : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectLost()
	{
		Description = "accept becoming lost";
		Tile = "Items/sw_campfire.bmp";
		RenderString = "?";
		ColorString = "&w";
		DetailColor = 'W';
	}

	public RitualSifrahTokenEffectLost(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming lost";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 2000 - Chance * 2;
	}

	public override int GetTiebreakerPriority()
	{
		return 4000 - Chance * 4;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Lost", Chance);
	}
}
