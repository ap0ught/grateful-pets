using System;
using UnityEngine;

[Serializable]
public class EffectInfo_Base
{
	public float duration = 1f;

	public exEase.Type curveType;

	public bool customCurve;

	public AnimationCurve curve;

	public Func<float, float> GetCurveFunction()
	{
		if (customCurve)
		{
			return (float _t) => curve.Evaluate(_t);
		}
		return exEase.GetEaseFunc(curveType);
	}
}
