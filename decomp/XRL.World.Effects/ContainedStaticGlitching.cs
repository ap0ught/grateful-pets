using System;
using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ContainedStaticGlitching : Effect
{
	public string Table = "RandomLiquid";

	public ContainedStaticGlitching()
	{
		Duration = 1;
		DisplayName = "{{entropic|glitching}}";
	}

	public ContainedStaticGlitching(string Table)
		: this()
	{
		this.Table = Table;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override int GetEffectType()
	{
		return 512;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasPart<LiquidVolume>() && !Object.LiquidVolume.IsOpenVolume())
		{
			return !Object.HasEffect(typeof(ContainedStaticGlitching));
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<LiquidMixedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(LiquidMixedEvent E)
	{
		if (Object.LiquidVolume.IsMixed())
		{
			GameObject gameObject = Object;
			string table = Table;
			Object.RemoveEffect(this);
			LiquidWarmStatic.GlitchLiquidComponents(gameObject, table, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}
}
