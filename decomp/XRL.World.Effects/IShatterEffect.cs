using System;

namespace XRL.World.Effects;

[Serializable]
public abstract class IShatterEffect : Effect
{
	public override int GetEffectType()
	{
		return 100664320;
	}

	public abstract int GetPenalty();

	public abstract void IncrementPenalty();

	public abstract GameObject GetOwner();

	public abstract void SetOwner(GameObject Owner);
}
