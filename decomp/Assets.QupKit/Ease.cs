using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.QupKit;

public static class Ease
{
	public enum Type
	{
		Linear,
		Hermite,
		Sinerp,
		Coserp,
		Spring
	}

	private delegate float EaseHandler(float start, float end, float t);

	private static Dictionary<MonoBehaviour, Coroutine> Routines = new Dictionary<MonoBehaviour, Coroutine>();

	private static Dictionary<Type, EaseHandler> _types = new Dictionary<Type, EaseHandler>
	{
		{
			Type.Linear,
			Mathf.Lerp
		},
		{
			Type.Hermite,
			Hermite
		},
		{
			Type.Sinerp,
			Sinerp
		},
		{
			Type.Coserp,
			Coserp
		},
		{
			Type.Spring,
			Spring
		}
	};

	public static void Start(MonoBehaviour o, float start, float end, float t, UnityAction<float> update, UnityAction complete, Type type)
	{
		if (Routines.ContainsKey(o) && Routines[o] != null)
		{
			o.StopCoroutine(Routines[o]);
			Routines[o] = null;
		}
		else
		{
			Routines.Add(o, null);
		}
		o.StopAllCoroutines();
		Routines.Add(o, o.StartCoroutine(TweenCoroutine(o, start, end, t, update, complete, type)));
	}

	private static IEnumerator TweenCoroutine(MonoBehaviour o, float start, float end, float t, UnityAction<float> update, UnityAction complete, Type type)
	{
		float i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / t;
			update(_types[type](start, end, i));
			yield return null;
		}
		complete?.Invoke();
		Routines.Remove(o);
	}

	private static float Hermite(float start, float end, float t)
	{
		return Mathf.Lerp(start, end, t * t * (3f - 2f * t));
	}

	private static float Sinerp(float start, float end, float t)
	{
		return Mathf.Lerp(start, end, Mathf.Sin(t * MathF.PI * 0.5f));
	}

	private static float Coserp(float start, float end, float t)
	{
		return Mathf.Lerp(start, end, 1f - Mathf.Cos(t * MathF.PI * 0.5f));
	}

	private static float Spring(float start, float end, float t)
	{
		t = Mathf.Clamp01(t);
		t = (Mathf.Sin(t * MathF.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f) + t) * (1f + 1.2f * (1f - t));
		return start + (end - start) * t;
	}
}
