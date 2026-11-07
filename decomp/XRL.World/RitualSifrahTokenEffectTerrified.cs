using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenEffectTerrified : SifrahPrioritizableToken
{
	public int Chance = 100;

	public RitualSifrahTokenEffectTerrified()
	{
		Description = "accept becoming {{W|terrified}}";
		Tile = "Items/sw_chimera.bmp";
		RenderString = "\u0013";
		ColorString = "&g";
		DetailColor = 'W';
	}

	public RitualSifrahTokenEffectTerrified(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of becoming {{W|terrified}}";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 1600 - Chance * 2;
	}

	public override int GetTiebreakerPriority()
	{
		return 3200 - Chance * 4;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance(EffectInstance: (!ContextObject.IsCreature) ? new Terrified("20-40".RollCached(), ContextObject.GetCurrentCell() ?? The.Player.GetCurrentCell()) : new Terrified("20-40".RollCached(), ContextObject), EffectName: "Terrified", Chance: Chance);
	}
}
