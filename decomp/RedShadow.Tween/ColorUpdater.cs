using System;
using UnityEngine;

namespace RedShadow.Tween;

public class ColorUpdater : IUpdater
{
	private readonly Action<object> _setter;

	private readonly Color _startValue;

	private readonly Color _endValue;

	public ColorUpdater(Action<object> setter, object startValue, object endValue)
	{
		_setter = setter;
		_startValue = (Color)startValue;
		_endValue = (Color)endValue;
	}

	public void update(float progress)
	{
		Color color = _startValue + (_endValue - _startValue) * progress;
		_setter(color);
	}
}
