using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectShatterMentalArmor : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectShatterMentalArmor()
	{
		Description = "accept becoming {{psionic|psionically cleaved}}";
		Tile = "Items/sw_esper.bmp";
		RenderString = "û";
		ColorString = "&m";
		DetailColor = 'R';
	}

	public RitualSifrahTokenEffectShatterMentalArmor(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{psionic|psionically cleaved}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 700 - Chance * 3 / 2;
	}

	public override int GetTiebreakerPriority()
	{
		return 1300 - Chance * 3;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("ShatterMentalArmor", Chance, null, null, Stack: true, Force: false, new ShatterMentalArmor("100-500".RollCached(), null, "1-2".RollCached()));
	}
}
