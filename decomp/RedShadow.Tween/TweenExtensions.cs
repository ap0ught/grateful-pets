using UnityEngine;

namespace RedShadow.Tween;

public static class TweenExtensions
{
	public static Tween<Vector3> moveTo(this Transform t, Vector3 endValue)
	{
		Tween<Vector3> tween = new Tween<Vector3>(t.position, (object value) =>
		{
			t.position = (Vector3)value;
		}, endValue);
		TweenManager.Instance.StartCoroutine(tween.start());
		return tween;
	}

	public static Tween<Vector3> moveFromTo(this Transform t, Vector3 startValue, Vector3 endValue)
	{
		Tween<Vector3> tween = new Tween<Vector3>(startValue, (object value) =>
		{
			t.position = (Vector3)value;
		}, endValue);
		TweenManager.Instance.StartCoroutine(tween.start());
		return tween;
	}

	public static Tween<Vector3> rotateTo(this Transform t, Vector3 endValue)
	{
		Tween<Vector3> tween = new Tween<Vector3>(t.eulerAngles, (object value) =>
		{
			t.eulerAngles = (Vector3)value;
		}, endValue);
		TweenManager.Instance.StartCoroutine(tween.start());
		return tween;
	}

	public static Tween<Color> colorTo(this Color t, Color endValue)
	{
		Tween<Color> tween = new Tween<Color>(t, (object value) =>
		{
			t = (Color)value;
		}, endValue);
		TweenManager.Instance.StartCoroutine(tween.start());
		return tween;
	}

	public static Tween<float> fadeTo(this Color t, float endValue)
	{
		Tween<float> tween = new Tween<float>(t.a, (object value) =>
		{
			t.a = (float)value;
		}, endValue);
		TweenManager.Instance.StartCoroutine(tween.start());
		return tween;
	}

	public static Tween<float> floatPropertyTo(this Material m, string propertyName, float endValue)
	{
		Tween<float> tween = new Tween<float>(m.GetFloat(propertyName), (object value) =>
		{
			m.SetFloat(propertyName, (float)value);
		}, endValue);
		TweenManager.Instance.StartCoroutine(tween.start());
		return tween;
	}
}
