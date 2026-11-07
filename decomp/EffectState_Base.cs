using System;

public class EffectState_Base
{
	public Func<float, float> func;

	protected float timer;

	protected bool start;

	public virtual bool Tick(float _delta)
	{
		return true;
	}
}
