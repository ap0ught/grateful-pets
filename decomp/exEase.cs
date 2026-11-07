using System;
using System.Collections.Generic;
using UnityEngine;

public class exEase
{
	public enum Type
	{
		Linear,
		QuadIn,
		QuadOut,
		QuadInOut,
		QuadOutIn,
		CubicIn,
		CubicOut,
		CubicInOut,
		CubicOutIn,
		QuartIn,
		QuartOut,
		QuartInOut,
		QuartOutIn,
		QuintIn,
		QuintOut,
		QuintInOut,
		QuintOutIn,
		SineIn,
		SineOut,
		SineInOut,
		SineOutIn,
		ExpoIn,
		ExpoOut,
		ExpoInOut,
		ExpoOutIn,
		CircIn,
		CircOut,
		CircInOut,
		CircOutIn,
		ElasticIn,
		ElasticOut,
		ElasticInOut,
		ElasticOutIn,
		BackIn,
		BackOut,
		BackInOut,
		BackOutIn,
		BounceIn,
		BounceOut,
		BounceInOut,
		BounceOutIn,
		Smooth,
		Fade,
		Spring
	}

	private static Dictionary<Type, Func<float, float>> easeFunctions;

	private static bool initialized;

	public static void Init()
	{
		if (!initialized)
		{
			initialized = true;
			easeFunctions = new Dictionary<Type, Func<float, float>>();
			easeFunctions[Type.Linear] = Linear;
			easeFunctions[Type.QuadIn] = QuadIn;
			easeFunctions[Type.QuadOut] = QuadOut;
			easeFunctions[Type.QuadInOut] = QuadInOut;
			easeFunctions[Type.QuadOutIn] = QuadOutIn;
			easeFunctions[Type.CubicIn] = CubicIn;
			easeFunctions[Type.CubicOut] = CubicOut;
			easeFunctions[Type.CubicInOut] = CubicInOut;
			easeFunctions[Type.CubicOutIn] = CubicOutIn;
			easeFunctions[Type.QuartIn] = QuartIn;
			easeFunctions[Type.QuartOut] = QuartOut;
			easeFunctions[Type.QuartInOut] = QuartInOut;
			easeFunctions[Type.QuartOutIn] = QuartOutIn;
			easeFunctions[Type.QuintIn] = QuintIn;
			easeFunctions[Type.QuintOut] = QuintOut;
			easeFunctions[Type.QuintInOut] = QuintInOut;
			easeFunctions[Type.QuintOutIn] = QuintOutIn;
			easeFunctions[Type.SineIn] = SineIn;
			easeFunctions[Type.SineOut] = SineOut;
			easeFunctions[Type.SineInOut] = SineInOut;
			easeFunctions[Type.SineOutIn] = SineOutIn;
			easeFunctions[Type.ExpoIn] = ExpoIn;
			easeFunctions[Type.ExpoOut] = ExpoOut;
			easeFunctions[Type.ExpoInOut] = ExpoInOut;
			easeFunctions[Type.ExpoOutIn] = ExpoOutIn;
			easeFunctions[Type.CircIn] = CircIn;
			easeFunctions[Type.CircOut] = CircOut;
			easeFunctions[Type.CircInOut] = CircInOut;
			easeFunctions[Type.CircOutIn] = CircOutIn;
			easeFunctions[Type.ElasticIn] = ElasticIn_Simple;
			easeFunctions[Type.ElasticOut] = ElasticOut_Simple;
			easeFunctions[Type.ElasticInOut] = ElasticInOut_Simple;
			easeFunctions[Type.ElasticOutIn] = ElasticOutIn_Simple;
			easeFunctions[Type.BackIn] = BackIn_Simple;
			easeFunctions[Type.BackOut] = BackOut_Simple;
			easeFunctions[Type.BackInOut] = BackInOut_Simple;
			easeFunctions[Type.BackOutIn] = BackOutIn_Simple;
			easeFunctions[Type.BounceIn] = BounceIn_Simple;
			easeFunctions[Type.BounceOut] = BounceOut_Simple;
			easeFunctions[Type.BounceInOut] = BounceInOut_Simple;
			easeFunctions[Type.BounceOutIn] = BounceOutIn_Simple;
			easeFunctions[Type.Smooth] = Smooth;
			easeFunctions[Type.Fade] = Fade;
			easeFunctions[Type.Spring] = Spring;
		}
	}

	public static Func<float, float> GetEaseFunc(Type _type)
	{
		Init();
		return easeFunctions[_type];
	}

	public static float Linear(float _t)
	{
		return _t;
	}

	public static float QuadIn(float _t)
	{
		return _t * _t;
	}

	public static float QuadOut(float _t)
	{
		return (0f - _t) * (_t - 2f);
	}

	public static float QuadInOut(float _t)
	{
		_t *= 2f;
		if (_t < 1f)
		{
			return _t * _t / 2f;
		}
		_t--;
		return -0.5f * (_t * (_t - 2f) - 1f);
	}

	public static float QuadOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return QuadOut(_t * 2f) / 2f;
		}
		return QuadIn(2f * _t - 1f) / 2f + 0.5f;
	}

	public static float CubicIn(float _t)
	{
		return _t * _t * _t;
	}

	public static float CubicOut(float _t)
	{
		_t--;
		return _t * _t * _t + 1f;
	}

	public static float CubicInOut(float _t)
	{
		_t *= 2f;
		if (_t < 1f)
		{
			return 0.5f * _t * _t * _t;
		}
		_t -= 2f;
		return 0.5f * (_t * _t * _t + 2f);
	}

	public static float CubicOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return CubicOut(2f * _t) / 2f;
		}
		return CubicIn(2f * _t - 1f) / 2f + 0.5f;
	}

	public static float QuartIn(float _t)
	{
		return _t * _t * _t * _t;
	}

	public static float QuartOut(float _t)
	{
		_t--;
		return 0f - (_t * _t * _t * _t - 1f);
	}

	public static float QuartInOut(float _t)
	{
		_t *= 2f;
		if (_t < 1f)
		{
			return 0.5f * _t * _t * _t * _t;
		}
		_t -= 2f;
		return -0.5f * (_t * _t * _t * _t - 2f);
	}

	public static float QuartOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return QuartOut(2f * _t) / 2f;
		}
		return QuartIn(2f * _t - 1f) / 2f + 0.5f;
	}

	public static float QuintIn(float _t)
	{
		return _t * _t * _t * _t * _t;
	}

	public static float QuintOut(float _t)
	{
		_t--;
		return _t * _t * _t * _t * _t + 1f;
	}

	public static float QuintInOut(float _t)
	{
		_t *= 2f;
		if (_t < 1f)
		{
			return 0.5f * _t * _t * _t * _t * _t;
		}
		_t -= 2f;
		return 0.5f * (_t * _t * _t * _t * _t + 2f);
	}

	public static float QuintOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return QuintOut(2f * _t) / 2f;
		}
		return QuintIn(2f * _t - 1f) / 2f + 0.5f;
	}

	public static float SineIn(float _t)
	{
		if (_t != 1f)
		{
			return 0f - Mathf.Cos(_t * MathF.PI / 2f) + 1f;
		}
		return 1f;
	}

	public static float SineOut(float _t)
	{
		return Mathf.Sin(_t * MathF.PI / 2f);
	}

	public static float SineInOut(float _t)
	{
		return -0.5f * (Mathf.Cos(MathF.PI * _t) - 1f);
	}

	public static float SineOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return SineOut(2f * _t) / 2f;
		}
		return SineIn(2f * _t - 1f) / 2f + 0.5f;
	}

	public static float ExpoIn(float _t)
	{
		if (_t != 0f && _t != 1f)
		{
			return Mathf.Pow(2f, 10f * (_t - 1f)) - 0.001f;
		}
		return _t;
	}

	public static float ExpoOut(float _t)
	{
		if (_t != 1f)
		{
			return 1.001f * (0f - Mathf.Pow(2f, -10f * _t) + 1f);
		}
		return 1f;
	}

	public static float ExpoInOut(float _t)
	{
		if (_t == 0f)
		{
			return 0f;
		}
		if (_t == 1f)
		{
			return 1f;
		}
		_t *= 2f;
		if (_t < 1f)
		{
			return 0.5f * Mathf.Pow(2f, 10f * (_t - 1f)) - 0.005f;
		}
		return 0.5025f * (0f - Mathf.Pow(2f, -10f * (_t - 1f)) + 2f);
	}

	public static float ExpoOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return ExpoOut(2f * _t) / 2f;
		}
		return ExpoIn(2f * _t - 1f) / 2f + 0.5f;
	}

	public static float CircIn(float _t)
	{
		return 0f - (Mathf.Sqrt(1f - _t * _t) - 1f);
	}

	public static float CircOut(float _t)
	{
		_t--;
		return Mathf.Sqrt(1f - _t * _t);
	}

	public static float CircInOut(float _t)
	{
		_t *= 2f;
		if (_t < 1f)
		{
			return -0.5f * (Mathf.Sqrt(1f - _t * _t) - 1f);
		}
		_t -= 2f;
		return 0.5f * (Mathf.Sqrt(1f - _t * _t) + 1f);
	}

	public static float CircOutIn(float _t)
	{
		if (_t < 0.5f)
		{
			return CircOut(2f * _t) / 2f;
		}
		return CircIn(2f * _t - 1f) / 2f + 0.5f;
	}

	private static float ElasticInHelper(float _t, float _b, float _c, float _d, float _a, float _p)
	{
		if (_t == 0f)
		{
			return _b;
		}
		float num = _t / _d;
		if (num == 1f)
		{
			return _b + _c;
		}
		float num2;
		if (_a < Mathf.Abs(_c))
		{
			_a = _c;
			num2 = _p / 4f;
		}
		else
		{
			num2 = _p / 2f * MathF.PI * Mathf.Asin(_c / _a);
		}
		num--;
		return 0f - _a * Mathf.Pow(2f, 10f * num) * Mathf.Sin((num * _d - num2) * 2f * MathF.PI / _p) + _b;
	}

	private static float ElasticOutHelper(float _t, float _b, float _c, float _d, float _a, float _p)
	{
		if (_t == 0f)
		{
			return 0f;
		}
		if (_t == 1f)
		{
			return _c;
		}
		float num;
		if (_a < _c)
		{
			_a = _c;
			num = _p / 4f;
		}
		else
		{
			num = _p / 2f * MathF.PI * Mathf.Asin(_c / _a);
		}
		return _a * Mathf.Pow(2f, -10f * _t) * Mathf.Sin((_t - num) * 2f * MathF.PI / _p) + _c;
	}

	public static float ElasticIn(float _t, float _a, float _p)
	{
		return ElasticInHelper(_t, 0f, 1f, 1f, _a, _p);
	}

	public static float ElasticOut(float _t, float _a, float _p)
	{
		return ElasticOutHelper(_t, 0f, 1f, 1f, _a, _p);
	}

	public static float ElasticInOut(float _t, float _a, float _p)
	{
		if (_t == 0f)
		{
			return 0f;
		}
		_t *= 2f;
		if (_t == 2f)
		{
			return 1f;
		}
		float num;
		if (_a < 1f)
		{
			_a = 1f;
			num = _p / 4f;
		}
		else
		{
			num = _p / 2f * MathF.PI * Mathf.Asin(1f / _a);
		}
		if (_t < 1f)
		{
			return -0.5f * (_a * Mathf.Pow(2f, 10f * (_t - 1f)) * Mathf.Sin((_t - 1f - num) * 2f * MathF.PI / _p));
		}
		return _a * Mathf.Pow(2f, -10f * (_t - 1f)) * Mathf.Sin((_t - 1f - num) * 2f * MathF.PI / _p) * 0.5f + 1f;
	}

	public static float ElasticOutIn(float _t, float _a, float _p)
	{
		if (_t < 0.5f)
		{
			return ElasticOutHelper(_t * 2f, 0f, 0.5f, 1f, _a, _p);
		}
		return ElasticInHelper(2f * _t - 1f, 0.5f, 0.5f, 1f, _a, _p);
	}

	public static float ElasticIn_Simple(float _t)
	{
		return ElasticIn(_t, 0.1f, 0.05f);
	}

	public static float ElasticOut_Simple(float _t)
	{
		return ElasticOut(_t, 0.1f, 0.05f);
	}

	public static float ElasticInOut_Simple(float _t)
	{
		return ElasticInOut(_t, 0.1f, 0.05f);
	}

	public static float ElasticOutIn_Simple(float _t)
	{
		return ElasticOutIn(_t, 0.1f, 0.05f);
	}

	public static float BackIn(float _t, float _s)
	{
		return _t * _t * ((_s + 1f) * _t - _s);
	}

	public static float BackOut(float _t, float _s)
	{
		_t--;
		return _t * _t * ((_s + 1f) * _t + _s) + 1f;
	}

	public static float BackInOut(float _t, float _s)
	{
		_t *= 2f;
		if (_t < 1f)
		{
			_s *= 1.55f;
			return 0.5f * (_t * _t * ((_s + 1f) * _t - _s));
		}
		_t -= 2f;
		_s *= 1.55f;
		return 0.5f * (_t * _t * ((_s + 1f) * _t + _s) + 2f);
	}

	public static float BackOutIn(float _t, float _s)
	{
		if (_t < 0.5f)
		{
			return BackOut(2f * _t, _s) / 2f;
		}
		return BackIn(2f * _t - 1f, _s) / 2f + 0.5f;
	}

	public static float BackIn_Simple(float _t)
	{
		return BackIn(_t, 2f);
	}

	public static float BackOut_Simple(float _t)
	{
		return BackOut(_t, 2f);
	}

	public static float BackInOut_Simple(float _t)
	{
		return BackInOut(_t, 2f);
	}

	public static float BackOutIn_Simple(float _t)
	{
		return BackOutIn(_t, 2f);
	}

	private static float BounceOutHelper(float _t, float _c, float _a)
	{
		if (_t == 1f)
		{
			return _c;
		}
		if (_t < 372f / 1023f)
		{
			return _c * (7.565f * _t * _t);
		}
		if (_t < 744f / 1023f)
		{
			_t -= 558f / 1023f;
			return (0f - _a) * (1f - (7.565f * _t * _t + 0.5f)) + _c;
		}
		if (_t < 930f / 1023f)
		{
			_t -= 837f / 1023f;
			return (0f - _a) * (1f - (7.565f * _t * _t + 0.935f)) + _c;
		}
		_t -= 21f / 22f;
		return (0f - _a) * (1f - (7.565f * _t * _t + 0.98435f)) + _c;
	}

	public static float BounceIn(float _t, float _a)
	{
		return 1f - BounceOutHelper(1f - _t, 1f, _a);
	}

	public static float BounceOut(float _t, float _a)
	{
		return BounceOutHelper(_t, 1f, _a);
	}

	public static float BounceInOut(float _t, float _a)
	{
		if (_t < 0.5f)
		{
			return BounceIn(2f * _t, _a) / 2f;
		}
		if (_t != 1f)
		{
			return BounceOut(2f * _t - 1f, _a) / 2f + 0.5f;
		}
		return 1f;
	}

	public static float BounceOutIn(float _t, float _a)
	{
		if (_t < 0.5f)
		{
			return BounceOutHelper(_t * 2f, 0.5f, _a);
		}
		return 1f - BounceOutHelper(2f - 2f * _t, 0.5f, _a);
	}

	public static float BounceIn_Simple(float _t)
	{
		return BounceIn(_t, 2f);
	}

	public static float BounceOut_Simple(float _t)
	{
		return BounceOut(_t, 2f);
	}

	public static float BounceInOut_Simple(float _t)
	{
		return BounceInOut(_t, 2f);
	}

	public static float BounceOutIn_Simple(float _t)
	{
		return BounceOutIn(_t, 2f);
	}

	public static float Smooth(float _t)
	{
		if (_t <= 0f)
		{
			return 0f;
		}
		if (_t >= 1f)
		{
			return 1f;
		}
		return _t * _t * (3f - 2f * _t);
	}

	public static float Fade(float _t)
	{
		if (_t <= 0f)
		{
			return 0f;
		}
		if (_t >= 1f)
		{
			return 1f;
		}
		return _t * _t * _t * (_t * (_t * 6f - 15f) + 10f);
	}

	public static float Spring(float _t)
	{
		_t = Mathf.Clamp01(_t);
		_t = (Mathf.Sin(_t * MathF.PI * (0.2f + 2.5f * _t * _t * _t)) * Mathf.Pow(1f - _t, 2.2f) + _t) * (1f + 1.2f * (1f - _t));
		return _t;
	}

	public static float Punch(float _amplitude, float _t)
	{
		float num = 9f;
		if (_t == 0f)
		{
			return 0f;
		}
		if (_t == 1f)
		{
			return 0f;
		}
		float num2 = 0.3f;
		num = num2 / (MathF.PI * 2f) * Mathf.Asin(0f);
		return _amplitude * Mathf.Pow(2f, -10f * _t) * Mathf.Sin((_t * 1f - num) * (MathF.PI * 2f) / num2);
	}

	public static float PingPong(float _t, Func<float, float> _ease)
	{
		float num = Mathf.PingPong(_t, 0.5f);
		return _ease(num / 0.5f);
	}
}
