using System;
using fsm;

public class AniTransition : TimerTransition
{
	public bool trigger;

	private float? exitTime;

	private Func<bool> onDoCheck;

	public AniTransition()
	{
		onCheck = OnCheck;
	}

	public AniTransition when(Func<bool> _onCheck = null)
	{
		onDoCheck = _onCheck;
		return this;
	}

	public AniTransition after(float _normalizedTime)
	{
		exitTime = _normalizedTime;
		return this;
	}

	private bool OnCheck()
	{
		if (trigger)
		{
			trigger = false;
			return true;
		}
		float num = ((AniState)source).normalizedTime;
		if (num == 0f)
		{
			num = 1f;
		}
		if (exitTime.HasValue && num < exitTime.Value)
		{
			return false;
		}
		if (onDoCheck != null)
		{
			return onDoCheck();
		}
		if (exitTime.HasValue)
		{
			return num >= exitTime.Value;
		}
		return false;
	}
}
