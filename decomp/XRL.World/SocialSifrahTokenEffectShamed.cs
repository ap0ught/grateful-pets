using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenEffectShamed : SifrahPrioritizableToken
{
	public int Chance = 100;

	public SocialSifrahTokenEffectShamed()
	{
		Description = "accept becoming {{r|shamed}}";
		Tile = "Items/sw_mask_null.bmp";
		RenderString = "\u0006";
		ColorString = "&b";
		DetailColor = 'r';
	}

	public SocialSifrahTokenEffectShamed(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{r|shamed}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 500 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 2000 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Shamed", Chance, "100-500", null, Stack: true);
	}
}
