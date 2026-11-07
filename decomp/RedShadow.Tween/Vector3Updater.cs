using System;
using UnityEngine;

namespace RedShadow.Tween;

public class Vector3Updater : IUpdater
{
	private readonly Action<object> _setter;

	private readonly Vector3 _startValue;

	private readonly Vector3 _endValue;

	public Vector3Updater(Action<object> setter, object startValue, object endValue)
	{
		_setter = setter;
		_startValue = (Vector3)startValue;
		_endValue = (Vector3)endValue;
	}

	public void update(float progress)
	{
		Vector3 vector = _startValue + (_endValue - _startValue) * progress;
		_setter(vector);
	}
}
