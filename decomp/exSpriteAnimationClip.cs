using System;
using System.Collections.Generic;
using UnityEngine;

public class exSpriteAnimationClip : ScriptableObject
{
	public enum StopAction
	{
		DoNothing,
		DefaultSprite,
		Hide,
		Destroy
	}

	[Serializable]
	public class FrameInfo
	{
		public exTextureInfo textureInfo;

		public int frames = 1;

		public FrameInfo(exTextureInfo _textureInfo, int _frames)
		{
			textureInfo = _textureInfo;
			frames = _frames;
		}
	}

	[Serializable]
	public class EventInfo
	{
		public class SearchComparer : IComparer<EventInfo>
		{
			private static SearchComparer instance_;

			private static int frame;

			public static int BinarySearch(List<EventInfo> _list, int _frame)
			{
				frame = _frame;
				if (instance_ == null)
				{
					instance_ = new SearchComparer();
				}
				return _list.BinarySearch(null, instance_);
			}

			public int Compare(EventInfo _x, EventInfo _y)
			{
				if (_x == null && _y == null)
				{
					return 0;
				}
				if (_x != null)
				{
					if (_x.frame > frame)
					{
						return 1;
					}
					if (_x.frame < frame)
					{
						return -1;
					}
					return 0;
				}
				if (frame > _y.frame)
				{
					return 1;
				}
				if (frame < _y.frame)
				{
					return -1;
				}
				return 0;
			}
		}

		public enum ParamType
		{
			None,
			String,
			Float,
			Int,
			Bool,
			Object
		}

		public int frame;

		public string methodName = "";

		public ParamType paramType;

		public string stringParam = "";

		public float floatParam;

		public int intParam = -1;

		public UnityEngine.Object objectParam;

		public SendMessageOptions msgOptions;

		public bool boolParam
		{
			get
			{
				return intParam != 0;
			}
			set
			{
				intParam = (value ? 1 : 0);
			}
		}
	}

	public WrapMode wrapMode = WrapMode.Once;

	public StopAction stopAction;

	[SerializeField]
	protected float frameRate_ = 60f;

	public List<FrameInfo> frameInfos = new List<FrameInfo>();

	public List<EventInfo> eventInfos = new List<EventInfo>();

	[NonSerialized]
	private int[] frameInfoFrames;

	[NonSerialized]
	private Dictionary<int, List<EventInfo>> frameToEventDict;

	public float speed = 1f;

	public float frameRate
	{
		get
		{
			return frameRate_;
		}
		set
		{
			if (value != frameRate_)
			{
				frameRate_ = Mathf.RoundToInt(Mathf.Max(value, 1f));
			}
		}
	}

	public int GetTotalFrames()
	{
		int num = 0;
		for (int i = 0; i < frameInfos.Count; i++)
		{
			num += frameInfos[i].frames;
		}
		return num;
	}

	public int[] GetFrameInfoFrames()
	{
		if (frameInfoFrames == null)
		{
			frameInfoFrames = new int[frameInfos.Count];
			int num = 0;
			for (int i = 0; i < frameInfos.Count; i++)
			{
				num += frameInfos[i].frames;
				frameInfoFrames[i] = num;
			}
		}
		return frameInfoFrames;
	}

	public float GetLength()
	{
		return (float)GetTotalFrames() / frameRate_;
	}

	public void AddFrame(exTextureInfo _info, int _frames = 1)
	{
		InsertFrameInfo(frameInfos.Count, new FrameInfo(_info, _frames));
	}

	public void AddFrameAt(int _idx, exTextureInfo _info, int _frames = 1)
	{
		InsertFrameInfo(_idx, new FrameInfo(_info, _frames));
	}

	public void RemoveFrame(FrameInfo _frameInfo)
	{
		frameInfos.Remove(_frameInfo);
	}

	public void InsertFrameInfo(int _idx, FrameInfo _frameInfo)
	{
		frameInfos.Insert(_idx, _frameInfo);
	}

	public void AddEmptyEvent(int _frame)
	{
		EventInfo eventInfo = new EventInfo();
		eventInfo.frame = _frame;
		AddEvent(eventInfo);
	}

	public void AddEvent(EventInfo _eventInfo)
	{
		if (eventInfos.Count == 0)
		{
			eventInfos.Insert(0, _eventInfo);
			return;
		}
		if (eventInfos.Count == 1)
		{
			if (_eventInfo.frame >= eventInfos[0].frame)
			{
				eventInfos.Insert(1, _eventInfo);
			}
			else
			{
				eventInfos.Insert(0, _eventInfo);
			}
			return;
		}
		bool flag = false;
		EventInfo eventInfo = eventInfos[0];
		for (int i = 1; i < eventInfos.Count; i++)
		{
			EventInfo eventInfo2 = eventInfos[i];
			if (_eventInfo.frame >= eventInfo.frame && _eventInfo.frame < eventInfo2.frame)
			{
				eventInfos.Insert(i, _eventInfo);
				flag = true;
				break;
			}
			eventInfo = eventInfo2;
		}
		if (!flag)
		{
			eventInfos.Insert(eventInfos.Count, _eventInfo);
		}
	}

	public void RemoveEvent(EventInfo _eventInfo)
	{
		eventInfos.Remove(_eventInfo);
	}

	public Dictionary<int, List<EventInfo>> GetFrameToEventDict()
	{
		if (frameToEventDict == null)
		{
			frameToEventDict = new Dictionary<int, List<EventInfo>>();
			int num = -1;
			List<EventInfo> list = null;
			for (int i = 0; i < eventInfos.Count; i++)
			{
				EventInfo eventInfo = eventInfos[i];
				if (eventInfo.frame != num)
				{
					if (list != null)
					{
						frameToEventDict.Add(num, list);
					}
					list = new List<EventInfo>();
					num = eventInfo.frame;
				}
				list.Add(eventInfo);
			}
			if (list != null)
			{
				frameToEventDict.Add(num, list);
			}
		}
		return frameToEventDict;
	}
}
