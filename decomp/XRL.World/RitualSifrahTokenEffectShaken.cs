using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectShaken : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectShaken()
	{
		Description = "accept becoming shaken";
		Tile = "Items/sw_tongue_twist.bmp";
		RenderString = "\u001d";
		ColorString = "&W";
		DetailColor = 'G';
	}

	public RitualSifrahTokenEffectShaken(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming shaken";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 1500 - Chance * 2;
	}

	public override int GetTiebreakerPriority()
	{
		return 3000 - Chance * 4;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Shaken", Chance, null, null, Stack: true, Force: false, new Shaken("500-1500".RollCached(), "1d20".RollCached()));
	}
}
