using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class exUIControl : exPlane
{
	[Serializable]
	public class SlotInfo
	{
		public GameObject receiver;

		public string method = "";

		public bool capturePhase;
	}

	[Serializable]
	public class EventTrigger
	{
		public string name;

		public List<SlotInfo> slots;

		public EventTrigger(string _name)
		{
			name = _name;
			slots = new List<SlotInfo>();
		}
	}

	public static string[] eventNames = new string[10] { "onFocus", "onUnfocus", "onActive", "onDeactive", "onHoverIn", "onHoverOut", "onHoverMove", "onPressDown", "onPressUp", "onMouseWheel" };

	private List<exUIEventListener> onFocus;

	private List<exUIEventListener> onUnfocus;

	private List<exUIEventListener> onActive;

	private List<exUIEventListener> onDeactive;

	private List<exUIEventListener> onHoverIn;

	private List<exUIEventListener> onHoverOut;

	private List<exUIEventListener> onHoverMove;

	private List<exUIEventListener> onPressDown;

	private List<exUIEventListener> onPressUp;

	private List<exUIEventListener> onMouseWheel;

	[NonSerialized]
	public exUIControl parent;

	[NonSerialized]
	public List<exUIControl> children = new List<exUIControl>();

	[SerializeField]
	protected bool active_ = true;

	public int priority;

	public bool useCollider;

	public bool grabMouseOrTouch;

	public List<EventTrigger> events = new List<EventTrigger>();

	protected Dictionary<string, List<exUIEventListener>> eventListenerTable = new Dictionary<string, List<exUIEventListener>>();

	protected List<exUIEventListener>[] cachedEventListenerTable;

	public bool activeInHierarchy
	{
		get
		{
			if (!active_)
			{
				return false;
			}
			exUIControl exUIControl2 = parent;
			while (exUIControl2 != null)
			{
				if (!exUIControl2.active_)
				{
					return false;
				}
				exUIControl2 = exUIControl2.parent;
			}
			return true;
		}
		set
		{
			if (active_ != value)
			{
				active_ = value;
				exUIEvent exUIEvent2 = new exUIEvent();
				exUIEvent2.bubbles = false;
				if (active_)
				{
					OnActive(exUIEvent2);
				}
				else
				{
					OnDeactive(exUIEvent2);
				}
				for (int i = 0; i < children.Count; i++)
				{
					children[i].activeInHierarchy = value;
				}
			}
		}
	}

	public bool activeSelf
	{
		get
		{
			return active_;
		}
		set
		{
			if (active_ != value)
			{
				active_ = value;
				exUIEvent exUIEvent2 = new exUIEvent();
				exUIEvent2.bubbles = false;
				if (active_)
				{
					OnActive(exUIEvent2);
				}
				else
				{
					OnDeactive(exUIEvent2);
				}
			}
		}
	}

	public void OnFocus(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onFocus", onFocus, _event);
	}

	public void OnUnfocus(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onUnfocus", onUnfocus, _event);
	}

	public void OnActive(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onActive", onActive, _event);
	}

	public void OnDeactive(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onDeactive", onDeactive, _event);
	}

	public void OnHoverIn(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onHoverIn", onHoverIn, _event);
	}

	public void OnHoverOut(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onHoverOut", onHoverOut, _event);
	}

	public void OnHoverMove(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onHoverMove", onHoverMove, _event);
	}

	public void OnPressDown(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onPressDown", onPressDown, _event);
	}

	public void OnPressUp(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onPressUp", onPressUp, _event);
	}

	public void OnEXMouseWheel(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onMouseWheel", onMouseWheel, _event);
	}

	public virtual void CacheEventListeners()
	{
		onFocus = eventListenerTable["onFocus"];
		onUnfocus = eventListenerTable["onUnfocus"];
		onActive = eventListenerTable["onActive"];
		onDeactive = eventListenerTable["onDeactive"];
		onHoverIn = eventListenerTable["onHoverIn"];
		onHoverOut = eventListenerTable["onHoverOut"];
		onHoverMove = eventListenerTable["onHoverMove"];
		onPressDown = eventListenerTable["onPressDown"];
		onPressUp = eventListenerTable["onPressUp"];
		onMouseWheel = eventListenerTable["onMouseWheel"];
	}

	public virtual string[] GetEventNames()
	{
		string[] array = new string[eventNames.Length];
		for (int i = 0; i < eventNames.Length; i++)
		{
			array[i] = eventNames[i];
		}
		return array;
	}

	private static void AddEventListeners(exUIControl _ctrl, string _eventName, List<SlotInfo> _slots)
	{
		foreach (SlotInfo _slot in _slots)
		{
			bool flag = false;
			if (_slot.receiver == null)
			{
				continue;
			}
			MonoBehaviour[] components = _slot.receiver.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in components)
			{
				if (!(monoBehaviour is exUIControl))
				{
					MethodInfo method = monoBehaviour.GetType().GetMethod(_slot.method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(exUIEvent) }, null);
					if (method != null)
					{
						Action<exUIEvent> func = (Action<exUIEvent>)Delegate.CreateDelegate(typeof(Action<exUIEvent>), monoBehaviour, method);
						_ctrl.AddEventListener(_eventName, func, _slot.capturePhase);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Debug.LogWarning("Can not find method " + _slot.method + " in " + _slot.receiver.name);
			}
		}
	}

	protected void Awake()
	{
		string[] array = GetEventNames();
		foreach (string key in array)
		{
			if (!eventListenerTable.ContainsKey(key))
			{
				eventListenerTable.Add(key, new List<exUIEventListener>());
			}
		}
		for (int j = 0; j < events.Count; j++)
		{
			EventTrigger eventTrigger = events[j];
			AddEventListeners(this, eventTrigger.name, eventTrigger.slots);
		}
		CacheEventListeners();
	}

	private void OnDestroy()
	{
		if (parent != null)
		{
			parent.RemoveChild(this);
		}
	}

	public void AddEventListener(string _name, Action<exUIEvent> _func, bool _capturePhase = false)
	{
		List<exUIEventListener> list = null;
		if (eventListenerTable.ContainsKey(_name))
		{
			list = eventListenerTable[_name];
			for (int i = 0; i < list.Count; i++)
			{
				exUIEventListener exUIEventListener2 = list[i];
				if (exUIEventListener2.func == _func && exUIEventListener2.capturePhase == _capturePhase)
				{
					return;
				}
			}
		}
		if (list == null)
		{
			list = new List<exUIEventListener>();
			eventListenerTable.Add(_name, list);
		}
		exUIEventListener exUIEventListener3 = new exUIEventListener();
		exUIEventListener3.func = _func;
		exUIEventListener3.capturePhase = _capturePhase;
		list.Add(exUIEventListener3);
	}

	public void RemoveEventListener(string _name, Action<exUIEvent> _func, bool _capturePhase = false)
	{
		List<exUIEventListener> list = null;
		if (!eventListenerTable.ContainsKey(_name))
		{
			return;
		}
		list = eventListenerTable[_name];
		for (int i = 0; i < list.Count; i++)
		{
			exUIEventListener exUIEventListener2 = list[i];
			if (exUIEventListener2.func == _func && exUIEventListener2.capturePhase == _capturePhase)
			{
				list.RemoveAt(i);
				break;
			}
		}
	}

	public void DispatchEvent(string _name, exUIEvent _event)
	{
		if (eventListenerTable.ContainsKey(_name))
		{
			List<exUIEventListener> listeners = eventListenerTable[_name];
			exUIMng.inst.DispatchEvent(this, _name, listeners, _event);
		}
	}

	public List<exUIEventListener> GetEventListeners(string _name)
	{
		if (eventListenerTable.ContainsKey(_name))
		{
			return eventListenerTable[_name];
		}
		return null;
	}

	public bool IsSelfOrAncestorOf(exUIControl _ctrl)
	{
		if (_ctrl == null)
		{
			return false;
		}
		if (_ctrl == this)
		{
			return true;
		}
		exUIControl exUIControl2 = _ctrl.parent;
		while (exUIControl2 != null)
		{
			if (exUIControl2 == this)
			{
				return true;
			}
			exUIControl2 = exUIControl2.parent;
		}
		return false;
	}

	public void AddChild(exUIControl _ctrl)
	{
		if (!(_ctrl == null) && !(_ctrl.parent == this) && !_ctrl.IsSelfOrAncestorOf(this))
		{
			exUIControl exUIControl2 = _ctrl.parent;
			if (exUIControl2 != null)
			{
				exUIControl2.RemoveChild(_ctrl);
			}
			children.Add(_ctrl);
			_ctrl.parent = this;
		}
	}

	public void RemoveChild(exUIControl _ctrl)
	{
		if (!(_ctrl == null))
		{
			int num = children.IndexOf(_ctrl);
			if (num != -1)
			{
				children.RemoveAt(num);
				_ctrl.parent = null;
			}
		}
	}

	public void Internal_SetActive(bool _active)
	{
		active_ = _active;
	}
}
