using System;

namespace RedShadow.Tween;

public class FloatUpdater : IUpdater
{
	private readonly Action<object> _setter;

	private readonly float _startValue;

	private readonly float _endValue;

	public FloatUpdater(Action<object> setter, object startValue, object endValue)
	{
		_setter = setter;
		_startValue = (float)startValue;
		_endValue = (float)endValue;
	}

	public void update(float progress)
	{
		float num = _startValue + (_endValue - _startValue) * progress;
		_setter(num);
	}
}
