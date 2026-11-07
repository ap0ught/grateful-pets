using System;

namespace XRL.World.Effects;

[Serializable]
public abstract class ITonicEffect : Effect
{
	public override int GetEffectType()
	{
		return 4;
	}

	public override bool IsTonic()
	{
		return true;
	}

	public abstract void ApplyAllergy(GameObject target);
}
