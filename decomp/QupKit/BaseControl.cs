using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QupKit;

public class BaseControl
{
	public BaseControl Parent;

	private ControlLayout _Layout;

	private QupControlHook Hook;

	private static EventSystem _EventSystem;

	public RectTransform _rectTransform;

	public GameObject _rootObject;

	public Dictionary<GameObject, BaseControl> ChildrenByControl = new Dictionary<GameObject, BaseControl>();

	public List<BaseControl> Children = new List<BaseControl>();

	public Dictionary<string, BaseControl> ChildrenByName = new Dictionary<string, BaseControl>();

	public Action<PointerEventData> OnClicked = (PointerEventData pointerEventData) =>
	{
	};

	public Action<PointerEventData> OnDrag = (PointerEventData pointerEventData) =>
	{
	};

	public EventSystem EventSystemManager
	{
		get
		{
			if (_EventSystem == null)
			{
				_EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
			}
			return _EventSystem;
		}
	}

	public string Sprite
	{
		get
		{
			return rootObject.GetComponent<Image>().sprite.name;
		}
		set
		{
			rootObject.GetComponent<Image>().sprite = (Sprite)Resources.Load(value, typeof(Sprite));
		}
	}

	public string Label
	{
		get
		{
			return rootObject.transform.Find("Label").GetComponent<Text>().text;
		}
		set
		{
			rootObject.transform.Find("Label").GetComponent<Text>().text = value;
		}
	}

	public string Text
	{
		get
		{
			return rootObject.transform.Find("Text").GetComponent<Text>().text;
		}
		set
		{
			rootObject.transform.Find("Text").GetComponent<Text>().text = value;
		}
	}

	public string Icon
	{
		get
		{
			return rootObject.transform.Find("Icon").GetComponent<Image>().sprite.name;
		}
		set
		{
			rootObject.transform.Find("Icon").GetComponent<Image>().sprite = (Sprite)Resources.Load(value, typeof(Sprite));
		}
	}

	public ControlLayout Layout
	{
		get
		{
			return _Layout;
		}
		set
		{
			if (value.Parent == null)
			{
				_Layout = value;
			}
			else
			{
				_Layout = new ControlLayout(_Layout);
			}
			_Layout.Parent = this;
		}
	}

	public bool bWaiting
	{
		get
		{
			ScheduleComponent component = rootObject.GetComponent<ScheduleComponent>();
			if (component == null)
			{
				return false;
			}
			return component.Entries.Count > 0;
		}
	}

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = _rootObject.GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
		set
		{
			_rectTransform = value;
		}
	}

	public GameObject rootObject
	{
		get
		{
			return _rootObject;
		}
		set
		{
			_rootObject = value;
			rectTransform = _rootObject.GetComponent<RectTransform>();
		}
	}

	public float Width
	{
		get
		{
			try
			{
				return rectTransform.sizeDelta.x;
			}
			catch (Exception)
			{
				Debug.Log("no recttransform on " + rootObject.name);
				throw;
			}
		}
		set
		{
		}
	}

	public float Height
	{
		get
		{
			return rectTransform.sizeDelta.y;
		}
		set
		{
		}
	}

	public Color MainColor
	{
		get
		{
			Image component = rootObject.GetComponent<Image>();
			if (component != null)
			{
				return component.color;
			}
			return new Color(1f, 0f, 1f, 1f);
		}
		set
		{
			Image component = rootObject.GetComponent<Image>();
			if (component != null)
			{
				component.color = value;
			}
		}
	}

	public string Name
	{
		get
		{
			if (rootObject == null)
			{
				return null;
			}
			return rootObject.name;
		}
		set
		{
			if (!(rootObject == null))
			{
				rootObject.name = value;
			}
		}
	}

	public void Select()
	{
		rootObject.AddComponent<MakeSelected>();
	}

	public void Select(GameObject go)
	{
		go.AddComponent<MakeSelected>();
	}

	public GameObject FindChild(string path)
	{
		return rootObject.transform.Find(path).gameObject;
	}

	public virtual void MoveBy(Vector3 Offset, float Time = 0f)
	{
		LeanTween.finish(Layout);
		if (Time > 0f)
		{
			LeanTween.move(Layout, Layout.Offset + Offset, Time);
			return;
		}
		Layout.Offset += Offset;
		ApplyLayout();
	}

	public virtual void MoveTo(Vector3 Offset, float Time = 0f)
	{
		LeanTween.finish(Layout);
		if (Time > 0f)
		{
			LeanTween.move(Layout, Offset, Time);
			return;
		}
		Layout.Offset = Offset;
		ApplyLayout();
	}

	public virtual void OnCustomLayout()
	{
	}

	public virtual void BeforeLayout()
	{
	}

	public void ApplyLayout()
	{
		BeforeLayout();
		if (Layout.Anchor == ControlAnchor.Custom)
		{
			OnCustomLayout();
			return;
		}
		if (Layout.Anchor == ControlAnchor.Fill)
		{
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
			rootObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			rootObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		}
		if (Layout.Anchor >= ControlAnchor.TopLeft && Layout.Anchor <= ControlAnchor.BottomRight)
		{
			rootObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Width, Height);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D = Layout.Offset;
		}
		switch (Layout.Anchor)
		{
		case ControlAnchor.TopLeft:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(Width / 2f + Layout.Margin.Left, (0f - Height) / 2f + Layout.Margin.Top, 0f);
			break;
		case ControlAnchor.Top:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0f + Layout.Margin.Left, (0f - Height) / 2f, 0f);
			break;
		case ControlAnchor.TopRight:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3((0f - Width) / 2f + Layout.Margin.Left, (0f - Height) / 2f + Layout.Margin.Top, 0f);
			break;
		case ControlAnchor.Left:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(Width / 2f + Layout.Margin.Left, 0f, 0f);
			break;
		case ControlAnchor.Middle:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0f, 0f, 0f);
			break;
		case ControlAnchor.Right:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3((0f - Width) / 2f - Layout.Margin.Right, 0f, 0f);
			break;
		case ControlAnchor.BottomLeft:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(Width / 2f + Layout.Margin.Left, Height / 2f - Layout.Margin.Bottom, 0f);
			break;
		case ControlAnchor.Bottom:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0f, Height / 2f - Layout.Margin.Bottom, 0f);
			break;
		case ControlAnchor.BottomRight:
			rootObject.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
			rootObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
			rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3((0f - Width) / 2f - Layout.Margin.Right, Height / 2f - Layout.Margin.Bottom, 0f);
			break;
		}
		rootObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0f, 0f, Layout.Offset.z);
		for (int i = 0; i < Children.Count; i++)
		{
			Children[i].ApplyLayout();
		}
	}

	public void After(float T, Action A)
	{
		if (rootObject.GetComponent<ScheduleComponent>() == null)
		{
			rootObject.AddComponent<ScheduleComponent>();
		}
		rootObject.GetComponent<ScheduleComponent>().After(T, A);
	}

	public T GetChildComponent<T>(string Child)
	{
		return rootObject.transform.Find(Child).GetComponent<T>();
	}

	public GameObject GetChildGameObject(string Name)
	{
		try
		{
			return rootObject.transform.Find(Name).gameObject;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception getting control name " + Name + " : " + ex);
			return null;
		}
	}

	public BaseControl GetChild(string Name)
	{
		try
		{
			return ChildrenByName[Name];
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception getting control name " + Name + " : " + ex);
			return null;
		}
	}

	public BaseControl AddChild(BaseControl NewChild)
	{
		return AddChild(NewChild, new ControlLayout(ControlAnchor.Custom));
	}

	public BaseControl AddChild(BaseControl NewChild, ControlLayout Layout)
	{
		NewChild.rootObject.transform.SetParent(rootObject.transform);
		NewChild.Hook = NewChild.rootObject.AddComponent<QupControlHook>();
		NewChild.Hook.Control = NewChild;
		NewChild.Layout = Layout;
		Children.Add(NewChild);
		ChildrenByName.Add(NewChild.Name, NewChild);
		ChildrenByControl.Add(NewChild.rootObject, NewChild);
		NewChild.AddOnClickHandler((BaseEventData D) =>
		{
			NewChild.OnClicked((PointerEventData)D);
		});
		NewChild.AddOnDragHandler((BaseEventData D) =>
		{
			NewChild.OnDrag((PointerEventData)D);
		});
		NewChild.Parent = this;
		return NewChild;
	}

	public virtual void AddPointerEnterHandler(Action<BaseEventData> A)
	{
		if (rootObject.GetComponent<EventTrigger>() == null)
		{
			rootObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(A.Invoke);
		rootObject.GetComponent<EventTrigger>().triggers.Add(entry);
	}

	public static void AddEventHandler(GameObject rootObject, EventTriggerType Type, Action<BaseEventData> A)
	{
		if (rootObject.GetComponent<EventTrigger>() == null)
		{
			rootObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = Type;
		entry.callback.AddListener(A.Invoke);
		rootObject.GetComponent<EventTrigger>().triggers.Add(entry);
	}

	public virtual void AddEventHandler(EventTriggerType Type, Action<BaseEventData> A)
	{
		if (rootObject.GetComponent<EventTrigger>() == null)
		{
			rootObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = Type;
		entry.callback.AddListener(A.Invoke);
		rootObject.GetComponent<EventTrigger>().triggers.Add(entry);
	}

	public virtual void AddPointerExitHandler(Action<BaseEventData> A)
	{
		if (rootObject.GetComponent<EventTrigger>() == null)
		{
			rootObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerExit;
		entry.callback.AddListener(A.Invoke);
		rootObject.GetComponent<EventTrigger>().triggers.Add(entry);
	}

	public virtual void AddOnClickHandler(Action<BaseEventData> A)
	{
		if (rootObject.GetComponent<EventTrigger>() == null)
		{
			rootObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener(A.Invoke);
		rootObject.GetComponent<EventTrigger>().triggers.Add(entry);
	}

	public virtual void AddOnDragHandler(Action<BaseEventData> A)
	{
		if (rootObject.GetComponent<EventTrigger>() == null)
		{
			rootObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Drag;
		entry.callback.AddListener(A.Invoke);
		rootObject.GetComponent<EventTrigger>().triggers.Add(entry);
	}
}
