using System;
using XRL.Language;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectCardiacArrest : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectCardiacArrest()
	{
		Description = "accept going into {{W|cardiac arrest}}";
		Tile = "Items/ms_broken_heart.png";
		RenderString = "\u0003";
		ColorString = "&K";
		DetailColor = 'r';
	}

	public RitualSifrahTokenEffectCardiacArrest(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of going into {{W|cardiac arrest}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 150 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 300 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("CardiacArrest", Chance, null, null, Stack: true);
	}
}
