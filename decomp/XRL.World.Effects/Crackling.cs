using System;

namespace XRL.World.Effects;

[Serializable]
public class Crackling : Effect
{
	public Crackling()
	{
		DisplayName = "{{W|crackling}}";
		Duration = 9999;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override string GetDescription()
	{
		return "{{W|crackling}}";
	}

	public override string GetDetails()
	{
		return "Electromagnetically unstable.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_electricalEffect");
		return base.Apply(Object);
	}
}
