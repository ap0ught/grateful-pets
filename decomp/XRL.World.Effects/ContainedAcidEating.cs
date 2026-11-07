using System;
using XRL.Liquids;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ContainedAcidEating : Effect, ITierInitialized
{
	private string LiquidID = "acid";

	public ContainedAcidEating()
	{
		Duration = 1;
		DisplayName = "{{Y|sizzling}}";
	}

	public override int GetEffectType()
	{
		return 512;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as ContainedAcidEating).LiquidID != LiquidID)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		LiquidVolume liquidVolume = ValidVolume();
		if (liquidVolume != null)
		{
			return "Dissolving from acid.\nTaking " + MinDamage(liquidVolume) + "-" + MaxDamage(liquidVolume) + " per turn.";
		}
		return "No damage.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart<LiquidVolume>())
		{
			return false;
		}
		if (Object.GetPart<LiquidVolume>().MaxVolume <= 0)
		{
			return false;
		}
		if (new LiquidAcid().SafeContainer(Object))
		{
			return false;
		}
		return !Object.HasEffect<ContainedAcidEating>();
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != PooledEvent<CanBeTradedEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference && CanAffect(Object))
		{
			E.AddAdjective(DisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.01);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		LiquidVolume liquidVolume = ValidVolume();
		if (liquidVolume == null || !CanAffect(Object, liquidVolume))
		{
			Object.RemoveEffect(this);
		}
		else
		{
			int num = Stat.Random(MinDamage(liquidVolume), MaxDamage(liquidVolume));
			if (num > 0)
			{
				Object.TakeDamage(num, "from {{G|acid}}!", "Acid");
			}
		}
		return base.HandleEvent(E);
	}

	private int MinDamage(LiquidVolume V)
	{
		return Math.Max(1, (int)((float)V.Volume * ((float)V.ComponentLiquids[LiquidID] / 1000f / 16f)));
	}

	private int MaxDamage(LiquidVolume V)
	{
		return Math.Max(2, (int)((float)V.Volume * ((float)V.ComponentLiquids[LiquidID] / 1000f)));
	}

	private LiquidVolume ValidVolume()
	{
		LiquidVolume liquidVolume = Object.LiquidVolume;
		if (liquidVolume.Volume <= 0)
		{
			return null;
		}
		if (!liquidVolume.ComponentLiquids.ContainsKey(LiquidID))
		{
			return null;
		}
		return liquidVolume;
	}

	public int MinDamage()
	{
		LiquidVolume liquidVolume = ValidVolume();
		if (liquidVolume == null)
		{
			return 0;
		}
		return MinDamage(liquidVolume);
	}

	public int MaxDamage()
	{
		LiquidVolume liquidVolume = ValidVolume();
		if (liquidVolume == null)
		{
			return 0;
		}
		return MinDamage(liquidVolume);
	}

	private bool CanAffect(GameObject GO, LiquidVolume V)
	{
		return !LiquidVolume.GetLiquid(LiquidID).SafeContainer(GO);
	}

	public bool CanAffect(GameObject GO)
	{
		LiquidVolume liquidVolume = ValidVolume();
		if (liquidVolume == null)
		{
			return false;
		}
		return CanAffect(GO, liquidVolume);
	}
}
