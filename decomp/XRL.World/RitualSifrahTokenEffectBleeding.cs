using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectBleeding : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectBleeding()
	{
		Description = "accept beginning to {{r|bleed}}";
		Tile = "Items/ms_droplet.png";
		RenderString = "\u00ad";
		ColorString = "&r";
		DetailColor = 'Y';
	}

	public RitualSifrahTokenEffectBleeding(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of beginning to {{r|bleed}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 250 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 400 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("CardiacArrest", Chance, null, null, Stack: true, Force: false, new Bleeding("1d2"));
	}
}
