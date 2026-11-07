using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedShadow.Tween;

public class Tween<T> : CustomYieldInstruction
{
	public delegate Vector3 ToVector3<T2>(T2 v);

	public delegate float EaseFunction(float start, float distance, float elapsedTime, float duration);

	private static readonly Dictionary<EaseType, EaseFunction> EaseFunctionMapping = new Dictionary<EaseType, EaseFunction>();

	private int _loopCount = 1;

	private EndMode _endMode;

	private Direction _direction;

	private float _duration = 1f;

	private EaseType _easeType;

	private readonly T _startValue;

	private readonly T _endValue;

	private float _startTime;

	private float _currentTime;

	private Direction _currentDirection;

	private float _currentLoop;

	private EaseFunction _easeFunction;

	private readonly Action<object> _setter;

	private readonly IUpdater _updater;

	public float Progress => getProgress();

	public Status Status { get; private set; }

	public override bool keepWaiting => update();

	private static Vector3 Identity(Vector3 v)
	{
		return v;
	}

	private static Vector3 TransformDotPosition(Transform t)
	{
		return t.position;
	}

	private static IEnumerable<float> NewCounter(int start, int end, int step)
	{
		for (int i = start; i <= end; i += step)
		{
			yield return i;
		}
	}

	private static IEnumerable<float> NewTimer(float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			yield return elapsedTime;
			elapsedTime += Time.deltaTime;
			if (elapsedTime >= duration)
			{
				yield return elapsedTime;
			}
		}
	}

	public static IEnumerable<Vector3> NewBezier(EaseFunction ease, Transform[] nodes, float duration)
	{
		IEnumerable<float> steps = NewTimer(duration);
		return NewBezier<Transform>(ease, nodes, TransformDotPosition, duration, steps);
	}

	public static IEnumerable<Vector3> NewBezier(EaseFunction ease, Transform[] nodes, int slices)
	{
		IEnumerable<float> steps = NewCounter(0, slices + 1, 1);
		return NewBezier<Transform>(ease, nodes, TransformDotPosition, slices + 1, steps);
	}

	public static IEnumerable<Vector3> NewBezier(EaseFunction ease, Vector3[] points, float duration)
	{
		IEnumerable<float> steps = NewTimer(duration);
		return NewBezier<Vector3>(ease, points, Identity, duration, steps);
	}

	public static IEnumerable<Vector3> NewBezier(EaseFunction ease, Vector3[] points, int slices)
	{
		IEnumerable<float> steps = NewCounter(0, slices + 1, 1);
		return NewBezier<Vector3>(ease, points, Identity, slices + 1, steps);
	}

	private static IEnumerable<Vector3> NewBezier<T2>(EaseFunction ease, IList nodes, ToVector3<T2> toVector3, float maxStep, IEnumerable<float> steps)
	{
		if (nodes.Count < 2)
		{
			yield break;
		}
		Vector3[] points = new Vector3[nodes.Count];
		foreach (float step in steps)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				points[i] = toVector3((T2)nodes[i]);
			}
			yield return Bezier(ease, points, step, maxStep);
		}
	}

	private static Vector3 Bezier(EaseFunction ease, Vector3[] points, float elapsedTime, float duration)
	{
		for (int num = points.Length - 1; num > 0; num--)
		{
			for (int i = 0; i < num; i++)
			{
				points[i].x = ease(points[i].x, points[i + 1].x - points[i].x, elapsedTime, duration);
				points[i].y = ease(points[i].y, points[i + 1].y - points[i].y, elapsedTime, duration);
				points[i].z = ease(points[i].z, points[i + 1].z - points[i].z, elapsedTime, duration);
			}
		}
		return points[0];
	}

	public static IEnumerable<Vector3> NewCatmullRom(Transform[] nodes, int slices, bool loop)
	{
		return NewCatmullRom<Transform>(nodes, TransformDotPosition, slices, loop);
	}

	public static IEnumerable<Vector3> NewCatmullRom(Vector3[] points, int slices, bool loop)
	{
		return NewCatmullRom<Vector3>(points, Identity, slices, loop);
	}

	private static IEnumerable<Vector3> NewCatmullRom<T2>(IList nodes, ToVector3<T2> toVector3, int slices, bool loop)
	{
		if (nodes.Count < 2)
		{
			yield break;
		}
		yield return toVector3((T2)nodes[0]);
		int last = nodes.Count - 1;
		for (int current = 0; loop || current < last; current++)
		{
			if (loop && current > last)
			{
				current = 0;
			}
			int previous;
			if (current != 0)
			{
				previous = current - 1;
			}
			else
			{
				previous = (loop ? last : current);
			}
			int start = current;
			int end = ((current != last) ? (current + 1) : ((!loop) ? current : 0));
			int next = ((end != last) ? (end + 1) : ((!loop) ? end : 0));
			int stepCount = slices + 1;
			for (int step = 1; step <= stepCount; step++)
			{
				yield return CatmullRom(toVector3((T2)nodes[previous]), toVector3((T2)nodes[start]), toVector3((T2)nodes[end]), toVector3((T2)nodes[next]), step, stepCount);
			}
		}
	}

	private static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime, float duration)
	{
		float num = elapsedTime / duration;
		float num2 = num * num;
		float num3 = num2 * num;
		return previous * (-0.5f * num3 + num2 - 0.5f * num) + start * (1.5f * num3 + -2.5f * num2 + 1f) + end * (-1.5f * num3 + 2f * num2 + 0.5f * num) + next * (0.5f * num3 - 0.5f * num2);
	}

	public static float interp(EaseType type, float elapsedTime, float duration)
	{
		if (EaseFunctionMapping.Count == 0)
		{
			initializeEaseTypes();
		}
		return EaseFunctionMapping[type](0f, 1f, elapsedTime, duration);
	}

	public static EaseFunction getEaseFunction(EaseType type)
	{
		if (EaseFunctionMapping.Count == 0)
		{
			initializeEaseTypes();
		}
		return EaseFunctionMapping[type];
	}

	private static void initializeEaseTypes()
	{
		EaseFunctionMapping[EaseType.Linear] = linear;
		EaseFunctionMapping[EaseType.EaseInQuad] = easeInQuad;
		EaseFunctionMapping[EaseType.EaseOutQuad] = easeOutQuad;
		EaseFunctionMapping[EaseType.EaseInOutQuad] = easeInOutQuad;
		EaseFunctionMapping[EaseType.EaseInCubic] = easeInCubic;
		EaseFunctionMapping[EaseType.EaseOutCubic] = easeOutCubic;
		EaseFunctionMapping[EaseType.EaseInOutCubic] = easeInOutCubic;
		EaseFunctionMapping[EaseType.EaseInQuart] = easeInQuart;
		EaseFunctionMapping[EaseType.EaseOutQuart] = easeOutQuart;
		EaseFunctionMapping[EaseType.EaseInOutQuart] = easeInOutQuart;
		EaseFunctionMapping[EaseType.EaseInQuint] = easeInQuint;
		EaseFunctionMapping[EaseType.EaseOutQuint] = easeOutQuint;
		EaseFunctionMapping[EaseType.EaseInOutQuint] = easeInOutQuint;
		EaseFunctionMapping[EaseType.EaseInSine] = easeInSine;
		EaseFunctionMapping[EaseType.EaseOutSine] = easeOutSine;
		EaseFunctionMapping[EaseType.EaseInOutSine] = easeInOutSine;
		EaseFunctionMapping[EaseType.EaseInExpo] = easeInExpo;
		EaseFunctionMapping[EaseType.EaseOutExpo] = easeOutExpo;
		EaseFunctionMapping[EaseType.EaseInOutExpo] = easeInOutExpo;
		EaseFunctionMapping[EaseType.EaseInCirc] = easeInCirc;
		EaseFunctionMapping[EaseType.EaseOutCirc] = easeOutCirc;
		EaseFunctionMapping[EaseType.EaseInOutCirc] = easeInOutCirc;
		EaseFunctionMapping[EaseType.EaseInElastic] = easeInElastic;
		EaseFunctionMapping[EaseType.EaseOutElastic] = easeOutElastic;
		EaseFunctionMapping[EaseType.EaseInOutElastic] = easeInOutElastic;
		EaseFunctionMapping[EaseType.EaseInBounce] = easeInBounce;
		EaseFunctionMapping[EaseType.EaseOutBounce] = easeOutBounce;
		EaseFunctionMapping[EaseType.EaseInOutBounce] = easeInOutBounce;
		EaseFunctionMapping[EaseType.EaseInBack] = easeInBack;
		EaseFunctionMapping[EaseType.EaseOutBack] = easeOutBack;
		EaseFunctionMapping[EaseType.EaseInOutBack] = easeInOutBack;
	}

	private static float linear(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * (elapsedTime / duration) + start;
	}

	private static float easeInQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime + start;
	}

	private static float easeOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return (0f - distance) * elapsedTime * (elapsedTime - 2f) + start;
	}

	private static float easeInOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime + start;
		}
		elapsedTime--;
		return (0f - distance) / 2f * (elapsedTime * (elapsedTime - 2f) - 1f) + start;
	}

	private static float easeInCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime + start;
	}

	private static float easeOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime--;
		return distance * (elapsedTime * elapsedTime * elapsedTime + 1f) + start;
	}

	private static float easeInOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (elapsedTime * elapsedTime * elapsedTime + 2f) + start;
	}

	private static float easeInQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}

	private static float easeOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime--;
		return (0f - distance) * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1f) + start;
	}

	private static float easeInOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return (0f - distance) / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2f) + start;
	}

	private static float easeInQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}

	private static float easeOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime--;
		return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 1f) + start;
	}

	private static float easeInOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2f) + start;
	}

	private static float easeInSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return (0f - distance) * Mathf.Cos(elapsedTime / duration * (MathF.PI / 2f)) + distance + start;
	}

	private static float easeOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * Mathf.Sin(elapsedTime / duration * (MathF.PI / 2f)) + start;
	}

	private static float easeInOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return (0f - distance) / 2f * (Mathf.Cos(MathF.PI * elapsedTime / duration) - 1f) + start;
	}

	private static float easeInExpo(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * Mathf.Pow(2f, 10f * (elapsedTime / duration - 1f)) + start;
	}

	private static float easeOutExpo(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * (0f - Mathf.Pow(2f, -10f * elapsedTime / duration) + 1f) + start;
	}

	private static float easeInOutExpo(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * Mathf.Pow(2f, 10f * (elapsedTime - 1f)) + start;
		}
		elapsedTime--;
		return distance / 2f * (0f - Mathf.Pow(2f, -10f * elapsedTime) + 2f) + start;
	}

	private static float easeInCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return (0f - distance) * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
	}

	private static float easeOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime--;
		return distance * Mathf.Sqrt(1f - elapsedTime * elapsedTime) + start;
	}

	private static float easeInOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return (0f - distance) / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) + 1f) + start;
	}

	private static float easeOutElastic(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		if ((elapsedTime /= duration) == 1f)
		{
			return start + distance;
		}
		float num = duration * 0.3f;
		float num2 = num / 4f;
		return distance * Mathf.Pow(2f, -10f * elapsedTime) * Mathf.Sin((elapsedTime * duration - num2) * (MathF.PI * 2f) / num) + distance + start;
	}

	private static float easeInElastic(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		if ((elapsedTime /= duration) == 1f)
		{
			return start + distance;
		}
		float num = duration * 0.3f;
		float num2 = num / 4f;
		return 0f - distance * Mathf.Pow(2f, 10f * --elapsedTime) * Mathf.Sin((elapsedTime * duration - num2) * (MathF.PI * 2f) / num) + start;
	}

	private static float easeInOutElastic(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		if ((elapsedTime /= duration / 2f) == 2f)
		{
			return start + distance;
		}
		float num = duration * 0.45000002f;
		float num2 = num / 4f;
		if (elapsedTime < 1f)
		{
			return -0.5f * (distance * Mathf.Pow(2f, 10f * --elapsedTime) * Mathf.Sin((elapsedTime * duration - num2) * (MathF.PI * 2f) / num)) + start;
		}
		return distance * Mathf.Pow(2f, -10f * --elapsedTime) * Mathf.Sin((elapsedTime * duration - num2) * (MathF.PI * 2f) / num) * 0.5f + distance + start;
	}

	private static float easeOutBounce(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		if ((elapsedTime /= duration) < 372f / 1023f)
		{
			return distance * (7.5625f * elapsedTime * elapsedTime) + start;
		}
		if (elapsedTime < 744f / 1023f)
		{
			return distance * (7.5625f * (elapsedTime -= 558f / 1023f) * elapsedTime + 0.75f) + start;
		}
		if (elapsedTime < 930f / 1023f)
		{
			return distance * (7.5625f * (elapsedTime -= 837f / 1023f) * elapsedTime + 0.9375f) + start;
		}
		return distance * (7.5625f * (elapsedTime -= 21f / 22f) * elapsedTime + 63f / 64f) + start;
	}

	private static float easeInBounce(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance - easeOutBounce(0f, distance, duration - elapsedTime, duration) + start;
	}

	private static float easeInOutBounce(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		if (elapsedTime < duration / 2f)
		{
			return easeInBounce(0f, distance, elapsedTime * 2f, duration) * 0.5f + start;
		}
		return easeOutBounce(0f, distance, elapsedTime * 2f - duration, duration) * 0.5f + distance * 0.5f + start;
	}

	private static float easeOutBack(float start, float distance, float elapsedTime, float duration)
	{
		return distance * ((elapsedTime = elapsedTime / duration - 1f) * elapsedTime * (2.70158f * elapsedTime + 1.70158f) + 1f) + start;
	}

	private static float easeInBack(float start, float distance, float elapsedTime, float duration)
	{
		return distance * (elapsedTime /= duration) * elapsedTime * (2.70158f * elapsedTime - 1.70158f) + start;
	}

	private static float easeInOutBack(float start, float distance, float elapsedTime, float duration)
	{
		float num = 1.70158f;
		if ((elapsedTime /= duration / 2f) < 1f)
		{
			return distance / 2f * (elapsedTime * elapsedTime * (((num *= 1.525f) + 1f) * elapsedTime - num)) + start;
		}
		return distance / 2f * ((elapsedTime -= 2f) * elapsedTime * (((num *= 1.525f) + 1f) * elapsedTime + num) + 2f) + start;
	}

	public Tween(T startValue, Action<object> setter, T endValue)
	{
		_setter = setter;
		_startValue = startValue;
		_endValue = endValue;
		if (typeof(T) == typeof(Vector3))
		{
			_updater = new Vector3Updater(_setter, startValue, endValue);
		}
		else if (typeof(T) == typeof(Color))
		{
			_updater = new ColorUpdater(_setter, startValue, endValue);
		}
		else
		{
			if (!(typeof(T) == typeof(float)))
			{
				throw new NotImplementedException("Missing Tween Updater implementation");
			}
			_updater = new FloatUpdater(_setter, startValue, endValue);
		}
		reset();
	}

	public IEnumerator start()
	{
		reset();
		Status = Status.Playing;
		yield return this;
	}

	public void stop()
	{
		if (Status == Status.Playing || Status == Status.Paused)
		{
			Status = Status.Stopped;
		}
	}

	public void pause()
	{
		if (Status == Status.Playing)
		{
			Status = Status.Paused;
		}
	}

	public void resume()
	{
		if (Status == Status.Paused)
		{
			Status = Status.Playing;
		}
	}

	public void reset()
	{
		_currentLoop = 0f;
		_startTime = Time.time;
		_currentTime = Time.time;
		_currentDirection = _direction;
		_setter((_currentDirection == Direction.Forward) ? _startValue : _endValue);
		_easeFunction = getEaseFunction(_easeType);
		Status = Status.Initialized;
	}

	public bool update()
	{
		if (Status == Status.Paused)
		{
			return true;
		}
		if (Status == Status.Stopped)
		{
			return false;
		}
		if (Status == Status.Initialized)
		{
			start();
		}
		_currentTime += Time.deltaTime;
		while (_currentTime - _startTime >= _duration)
		{
			_currentLoop++;
			if (_currentLoop >= (float)_loopCount)
			{
				Status = Status.Finished;
				updateValue(1f);
				return false;
			}
			_startTime += _duration;
			if (_endMode == EndMode.PingPong)
			{
				_currentDirection = ((_direction == Direction.Forward) ? Direction.Reverse : Direction.Forward);
			}
		}
		float progress = (_currentTime - _startTime) / _duration;
		updateValue(progress);
		return true;
	}

	private void updateValue(float progress)
	{
		if (Status == Status.Finished)
		{
			switch (_endMode)
			{
			case EndMode.Stop:
			case EndMode.PingPong:
			{
				T val2 = ((_currentDirection == Direction.Forward) ? _endValue : _startValue);
				_setter(val2);
				break;
			}
			case EndMode.Reset:
			{
				T val = ((_currentDirection == Direction.Forward) ? _startValue : _endValue);
				_setter(val);
				break;
			}
			}
		}
		else
		{
			float num = _easeFunction(0f, 1f, progress * _duration, _duration);
			if (_currentDirection == Direction.Reverse)
			{
				num = 1f - num;
			}
			_updater.update(num);
		}
	}

	private float getProgress()
	{
		float num = _duration * _currentLoop + (_currentTime - _startTime);
		float num2 = _duration * (float)_loopCount;
		return num / num2;
	}

	public Tween<T> loopCount(int count)
	{
		_loopCount = count;
		return this;
	}

	public Tween<T> endMode(EndMode endMode)
	{
		_endMode = endMode;
		return this;
	}

	public Tween<T> direction(Direction direction)
	{
		_direction = direction;
		return this;
	}

	public Tween<T> duration(float duration)
	{
		_duration = duration;
		return this;
	}

	public Tween<T> easeType(EaseType easeType)
	{
		_easeType = easeType;
		return this;
	}
}
