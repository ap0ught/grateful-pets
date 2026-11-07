using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenEffectLovesick : SifrahPrioritizableToken
{
	public int Chance = 100;

	public SocialSifrahTokenEffectLovesick()
	{
		Description = "accept becoming {{lovesickness|lovesick}}";
		Tile = "Items/sw_heart.bmp";
		RenderString = "\u0003";
		ColorString = "&R";
		DetailColor = 'Y';
	}

	public SocialSifrahTokenEffectLovesick(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(Chance) + "% chance of becoming {{lovesickness|lovesick}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 300 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 1000 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Lovesick", Chance, "3000-3600", null, Stack: false, Force: false, new Lovesick(1, ContextObject));
	}
}
