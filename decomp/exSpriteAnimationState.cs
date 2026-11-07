using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class exSpriteAnimationState
{
	[NonSerialized]
	public exSpriteAnimationClip clip;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public WrapMode wrapMode;

	[NonSerialized]
	public exSpriteAnimationClip.StopAction stopAction;

	[NonSerialized]
	public float length;

	[NonSerialized]
	public int totalFrames;

	[NonSerialized]
	public float speed = 1f;

	[NonSerialized]
	public float time;

	[NonSerialized]
	public int frame = -1;

	[NonSerialized]
	private int[] frameInfoFrames;

	[NonSerialized]
	private int cachedIndex = -1;

	[NonSerialized]
	private Dictionary<int, List<exSpriteAnimationClip.EventInfo>> frameToEventDict;

	public exSpriteAnimationState(exSpriteAnimationClip _animClip)
		: this(_animClip.name, _animClip)
	{
	}

	public exSpriteAnimationState(string _name, exSpriteAnimationClip _animClip)
	{
		name = _name;
		clip = _animClip;
		wrapMode = clip.wrapMode;
		stopAction = clip.stopAction;
		speed = clip.speed;
		frameInfoFrames = clip.GetFrameInfoFrames();
		if (frameInfoFrames.Length != 0)
		{
			totalFrames = frameInfoFrames[frameInfoFrames.Length - 1];
		}
		else
		{
			totalFrames = 0;
		}
		length = (float)totalFrames / clip.frameRate;
		if (clip.eventInfos.Count >= 9)
		{
			frameToEventDict = clip.GetFrameToEventDict();
		}
	}

	public int GetCurrentIndex()
	{
		if (totalFrames > 1)
		{
			frame = (int)(time * clip.frameRate);
			if (frame < 0)
			{
				frame = -frame;
			}
			int num;
			if (wrapMode != WrapMode.PingPong)
			{
				num = exMath.Wrap(frame, totalFrames - 1, wrapMode);
			}
			else
			{
				num = frame;
				int num2 = num / totalFrames;
				num %= totalFrames;
				if ((num2 & 1) == 1)
				{
					num = totalFrames - 1 - num;
				}
			}
			if (cachedIndex - 1 >= 0 && num >= frameInfoFrames[cachedIndex - 1] && num < frameInfoFrames[cachedIndex])
			{
				return cachedIndex;
			}
			int num3 = Array.BinarySearch(frameInfoFrames, num + 1);
			if (num3 < 0)
			{
				num3 = ~num3;
			}
			cachedIndex = num3;
			return num3;
		}
		if (totalFrames == 1)
		{
			return 0;
		}
		return -1;
	}

	public void TriggerEvents(Component _target, int _start, float _end)
	{
		if (clip.eventInfos.Count == 0)
		{
			return;
		}
		for (int i = _start; (float)i <= _end; i++)
		{
			if (totalFrames == 0)
			{
				TriggerEvents(_target, 0, _reversed: false);
				continue;
			}
			bool flag = false;
			int num2;
			if (wrapMode == WrapMode.PingPong)
			{
				int num = i / totalFrames;
				num2 = i % totalFrames;
				flag = (num & 1) == 1;
				if (flag)
				{
					num2 = totalFrames - num2;
				}
			}
			else if (wrapMode == WrapMode.Loop)
			{
				num2 = exMath.Wrap(i, totalFrames - 1, wrapMode);
				if (i > 0 && num2 == 0)
				{
					TriggerEvents(_target, totalFrames, _reversed: false);
				}
			}
			else
			{
				num2 = i;
			}
			TriggerEvents(_target, num2, flag);
		}
	}

	private void TriggerEvents(Component _target, int _wrappedIndex, bool _reversed)
	{
		if (clip.eventInfos.Count == 0)
		{
			return;
		}
		List<exSpriteAnimationClip.EventInfo> value;
		if (frameToEventDict == null)
		{
			value = clip.eventInfos;
		}
		else if (!frameToEventDict.TryGetValue(_wrappedIndex, out value))
		{
			return;
		}
		if (_reversed)
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (value[num].frame == _wrappedIndex)
				{
					Trigger(_target, value[num]);
				}
			}
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].frame == _wrappedIndex)
			{
				Trigger(_target, value[i]);
			}
		}
	}

	public void Trigger(Component _target, exSpriteAnimationClip.EventInfo _event)
	{
		if (!(_event.methodName == ""))
		{
			switch (_event.paramType)
			{
			case exSpriteAnimationClip.EventInfo.ParamType.None:
				_target.SendMessage(_event.methodName, _event.msgOptions);
				break;
			case exSpriteAnimationClip.EventInfo.ParamType.String:
				_target.SendMessage(_event.methodName, _event.stringParam, _event.msgOptions);
				break;
			case exSpriteAnimationClip.EventInfo.ParamType.Float:
				_target.SendMessage(_event.methodName, _event.floatParam, _event.msgOptions);
				break;
			case exSpriteAnimationClip.EventInfo.ParamType.Int:
				_target.SendMessage(_event.methodName, _event.intParam, _event.msgOptions);
				break;
			case exSpriteAnimationClip.EventInfo.ParamType.Bool:
				_target.SendMessage(_event.methodName, _event.boolParam, _event.msgOptions);
				break;
			case exSpriteAnimationClip.EventInfo.ParamType.Object:
				_target.SendMessage(_event.methodName, _event.objectParam, _event.msgOptions);
				break;
			}
		}
	}
}
