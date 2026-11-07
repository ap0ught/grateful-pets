using System.Collections.Generic;
using UnityEngine;

public class exUIMng : MonoBehaviour
{
	protected static exUIMng inst_ = null;

	private static ControlSorterByPriority controlSorterByPrioirty = new ControlSorterByPriority();

	private static ControlSorterByLevel controlSorterByLevel = new ControlSorterByLevel();

	public bool simulateMouseAsTouch;

	public bool showDebugInfo;

	public bool showDebugInfoInGameView;

	private bool initialized;

	private bool hasMouse;

	private bool hasTouch;

	private List<exUIControl> controls = new List<exUIControl>();

	private exHotPoint[] mousePoints = new exHotPoint[3];

	private exHotPoint[] touchPoints = new exHotPoint[10];

	private exUIControl focus;

	public static exUIMng inst
	{
		get
		{
			if (inst_ == null)
			{
				inst_ = Object.FindFirstObjectByType(typeof(exUIMng)) as exUIMng;
			}
			return inst_;
		}
	}

	public static List<exUIControl> GetRoutine(exUIControl _ctrl)
	{
		List<exUIControl> list = new List<exUIControl>();
		exUIControl parent = _ctrl.parent;
		while (parent != null)
		{
			list.Add(parent);
			parent = parent.parent;
		}
		return list;
	}

	public static exUIControl FindRoot(exUIControl _ctrl)
	{
		exUIControl result = null;
		exUIControl exUIControl2 = _ctrl;
		while (exUIControl2 != null)
		{
			result = exUIControl2;
			exUIControl2 = exUIControl2.parent;
		}
		return result;
	}

	public static exUIControl FindParent(exUIControl _ctrl)
	{
		return _ctrl.FindParentComponent();
	}

	public static void FindAndAddChild(exUIControl _ctrl)
	{
		_ctrl.children.Clear();
		FindAndAddChildRecursively(_ctrl, _ctrl.transform);
	}

	private static void FindAndAddChildRecursively(exUIControl _ctrl, Transform _trans)
	{
		foreach (Transform _tran in _trans)
		{
			exUIControl component = _tran.GetComponent<exUIControl>();
			if ((bool)component)
			{
				_ctrl.AddChild(component);
				FindAndAddChild(component);
			}
			else
			{
				FindAndAddChildRecursively(_ctrl, _tran);
			}
		}
	}

	private void Awake()
	{
		Init();
	}

	private void Update()
	{
		HandleEvents();
	}

	private void OnGUI()
	{
		if (showDebugInfoInGameView)
		{
			ShowDebugInfo(new Rect(10f, 10f, 300f, 300f));
		}
	}

	public void AddControl(exUIControl _ctrl)
	{
		if (controls.IndexOf(_ctrl) == -1)
		{
			controls.Add(_ctrl);
			FindAndAddChild(_ctrl);
		}
	}

	public void SetFocus(exUIControl _ctrl)
	{
		if (focus != _ctrl)
		{
			exUIControl relatedTarget = focus;
			if (focus != null)
			{
				exUIFocusEvent exUIFocusEvent2 = new exUIFocusEvent();
				exUIFocusEvent2.relatedTarget = focus;
				focus.OnUnfocus(exUIFocusEvent2);
			}
			focus = _ctrl;
			if (focus != null)
			{
				exUIFocusEvent exUIFocusEvent3 = new exUIFocusEvent();
				exUIFocusEvent3.relatedTarget = relatedTarget;
				focus.OnFocus(exUIFocusEvent3);
			}
		}
	}

	public void DispatchEvent(exUIControl _sender, string _name, List<exUIEventListener> _listeners, exUIEvent _event)
	{
		_event.target = _sender;
		if (_event.bubbles)
		{
			List<exUIControl> routine = GetRoutine(_sender);
			if (_listeners != null)
			{
				for (int i = 0; i < _listeners.Count; i++)
				{
					if (!_listeners[i].capturePhase)
					{
						continue;
					}
					_event.eventPhase = exUIEventPhase.Capture;
					for (int num = routine.Count - 1; num >= 0; num--)
					{
						exUIControl exUIControl2 = routine[num];
						List<exUIEventListener> eventListeners = exUIControl2.GetEventListeners(_name);
						_event.currentTarget = exUIControl2;
						DoDispatchEvent(exUIControl2, eventListeners, _event);
						if (_event.isPropagationStopped)
						{
							return;
						}
					}
				}
			}
			if (_listeners != null)
			{
				_event.eventPhase = exUIEventPhase.Target;
				_event.currentTarget = _sender;
				DoDispatchEvent(_sender, _listeners, _event);
				if (_event.isPropagationStopped)
				{
					return;
				}
			}
			_event.eventPhase = exUIEventPhase.Bubble;
			for (int j = 0; j < routine.Count; j++)
			{
				exUIControl exUIControl3 = routine[j];
				List<exUIEventListener> eventListeners2 = exUIControl3.GetEventListeners(_name);
				_event.currentTarget = exUIControl3;
				DoDispatchEvent(exUIControl3, eventListeners2, _event);
				if (_event.isPropagationStopped)
				{
					break;
				}
			}
		}
		else
		{
			_event.eventPhase = exUIEventPhase.Target;
			_event.currentTarget = _sender;
			DoDispatchEvent(_sender, _listeners, _event);
		}
	}

	public void DispatchEvent(exUIControl _sender, string _name, exUIEvent _event)
	{
		List<exUIEventListener> eventListeners = _sender.GetEventListeners(_name);
		DispatchEvent(_sender, _name, eventListeners, _event);
	}

	private void DoDispatchEvent(exUIControl _sender, List<exUIEventListener> _listeners, exUIEvent _event)
	{
		for (int i = 0; i < _listeners.Count; i++)
		{
			exUIEventListener exUIEventListener2 = _listeners[i];
			if (_event.eventPhase == exUIEventPhase.Capture)
			{
				if (!exUIEventListener2.capturePhase)
				{
					continue;
				}
			}
			else if (_event.eventPhase == exUIEventPhase.Bubble && exUIEventListener2.capturePhase)
			{
				continue;
			}
			exUIEventListener2.func(_event);
		}
	}

	private void Init()
	{
		if (initialized)
		{
			return;
		}
		if (GetComponent<Camera>() == null)
		{
			Debug.LogError("The exUIMng should attach to a camera");
			return;
		}
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			hasMouse = false;
			hasTouch = true;
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			hasMouse = true;
			hasTouch = false;
		}
		for (int i = 0; i < 10; i++)
		{
			touchPoints[i] = new exHotPoint();
			touchPoints[i].Reset();
			touchPoints[i].id = i;
		}
		for (int j = 0; j < 3; j++)
		{
			mousePoints[j] = new exHotPoint();
			mousePoints[j].Reset();
			mousePoints[j].id = j;
			mousePoints[j].isMouse = true;
		}
		exUIControl[] array = Object.FindObjectsByType(typeof(exUIControl), FindObjectsSortMode.None) as exUIControl[];
		foreach (exUIControl ctrl in array)
		{
			if (FindParent(ctrl) == null)
			{
				AddControl(ctrl);
			}
		}
		initialized = true;
	}

	public void HoverOut(exUIControl _ctrl, int _id)
	{
		exHotPoint[] array = null;
		array = ((!hasTouch && !simulateMouseAsTouch) ? mousePoints : touchPoints);
		foreach (exHotPoint exHotPoint2 in array)
		{
			if (!exHotPoint2.active || exHotPoint2.id != _id)
			{
				continue;
			}
			exUIPointInfo exUIPointInfo2 = new exUIPointInfo
			{
				id = exHotPoint2.id,
				pos = exHotPoint2.pos,
				delta = exHotPoint2.delta,
				worldPos = exHotPoint2.worldPos,
				worldDelta = exHotPoint2.worldDelta
			};
			exUIPointEvent exUIPointEvent2 = new exUIPointEvent();
			exUIPointEvent2.isMouse = exHotPoint2.isMouse;
			exUIPointEvent2.pointInfos = new exUIPointInfo[1] { exUIPointInfo2 };
			if (_ctrl != null)
			{
				_ctrl.OnHoverOut(exUIPointEvent2);
			}
			exHotPoint2.hover = null;
			if (_ctrl.parent != null)
			{
				exUIPointEvent2.Reset();
				exHotPoint2.hover = _ctrl.parent;
				_ctrl.parent.OnHoverIn(exUIPointEvent2);
				if (exHotPoint2.isTouch)
				{
					exUIPointEvent2.Reset();
					exHotPoint2.pressed = _ctrl.parent;
					_ctrl.parent.OnPressDown(exUIPointEvent2);
				}
			}
		}
	}

	private void HandleEvents()
	{
		for (int i = 0; i < touchPoints.Length; i++)
		{
			exHotPoint obj = touchPoints[i];
			obj.active = false;
			obj.pressDown = false;
			obj.pressUp = false;
		}
		for (int j = 0; j < mousePoints.Length; j++)
		{
			exHotPoint obj2 = mousePoints[j];
			obj2.active = false;
			obj2.pressDown = false;
			obj2.pressUp = false;
		}
		if (hasMouse)
		{
			HandleMouse();
		}
		if (hasTouch)
		{
			HandleTouches();
		}
	}

	private void HandleHotPoints(exHotPoint[] _hotPoints, bool _isMouse)
	{
		int num = (_isMouse ? 1 : _hotPoints.Length);
		for (int i = 0; i < num; i++)
		{
			exHotPoint exHotPoint2 = _hotPoints[i];
			if (!exHotPoint2.active || (exHotPoint2.pressed != null && exHotPoint2.pressed.grabMouseOrTouch))
			{
				continue;
			}
			exUIControl hover = exHotPoint2.hover;
			exUIControl exUIControl2 = (exHotPoint2.hover = PickControl(exHotPoint2.pos));
			if (!(hover != exUIControl2))
			{
				continue;
			}
			exUIPointInfo exUIPointInfo2 = new exUIPointInfo
			{
				id = exHotPoint2.id,
				pos = exHotPoint2.pos,
				delta = exHotPoint2.delta,
				worldPos = exHotPoint2.worldPos,
				worldDelta = exHotPoint2.worldDelta
			};
			exUIPointEvent exUIPointEvent2 = new exUIPointEvent();
			exUIPointEvent2.isMouse = exHotPoint2.isMouse;
			exUIPointEvent2.pointInfos = new exUIPointInfo[1] { exUIPointInfo2 };
			if (hover != null)
			{
				hover.OnHoverOut(exUIPointEvent2);
			}
			if (exUIControl2 != null)
			{
				exUIPointEvent2.Reset();
				exUIControl2.OnHoverIn(exUIPointEvent2);
				if (exHotPoint2.isTouch)
				{
					exHotPoint2.pressDown = true;
				}
			}
		}
		if (_isMouse)
		{
			for (int j = 1; j < _hotPoints.Length; j++)
			{
				_hotPoints[j].hover = _hotPoints[0].hover;
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis != 0f && _hotPoints[0].hover != null)
			{
				exUIWheelEvent exUIWheelEvent2 = new exUIWheelEvent();
				exUIWheelEvent2.delta = axis;
				_hotPoints[0].hover.OnEXMouseWheel(exUIWheelEvent2);
			}
		}
		foreach (exHotPoint exHotPoint3 in _hotPoints)
		{
			if (exHotPoint3.active && exHotPoint3.pressDown)
			{
				exUIControl exUIControl3 = exHotPoint3.hover;
				if (exHotPoint3.pressed != null && exHotPoint3.pressed.grabMouseOrTouch)
				{
					exUIControl3 = exHotPoint3.pressed;
				}
				if (exUIControl3 != null)
				{
					exUIPointInfo exUIPointInfo3 = new exUIPointInfo
					{
						id = exHotPoint3.id,
						pos = exHotPoint3.pos,
						delta = exHotPoint3.delta,
						worldPos = exHotPoint3.worldPos,
						worldDelta = exHotPoint3.worldDelta
					};
					exUIPointEvent exUIPointEvent3 = new exUIPointEvent();
					exUIPointEvent3.isMouse = exHotPoint3.isMouse;
					exUIPointEvent3.pointInfos = new exUIPointInfo[1] { exUIPointInfo3 };
					exUIControl3.OnPressDown(exUIPointEvent3);
				}
				exHotPoint3.pressed = exUIControl3;
			}
		}
		Dictionary<exUIControl, List<exHotPoint>> dictionary = new Dictionary<exUIControl, List<exHotPoint>>();
		foreach (exHotPoint exHotPoint4 in _hotPoints)
		{
			if (!exHotPoint4.active || !(exHotPoint4.delta != Vector2.zero))
			{
				continue;
			}
			exUIControl exUIControl4 = exHotPoint4.hover;
			if (exHotPoint4.pressed != null && exHotPoint4.pressed.grabMouseOrTouch)
			{
				exUIControl4 = exHotPoint4.pressed;
			}
			if (exUIControl4 != null)
			{
				List<exHotPoint> list = null;
				if (dictionary.ContainsKey(exUIControl4))
				{
					list = dictionary[exUIControl4];
				}
				else
				{
					list = new List<exHotPoint>();
					dictionary.Add(exUIControl4, list);
				}
				list.Add(exHotPoint4);
			}
		}
		foreach (KeyValuePair<exUIControl, List<exHotPoint>> item in dictionary)
		{
			exUIPointEvent exUIPointEvent4 = new exUIPointEvent();
			exUIPointEvent4.pointInfos = new exUIPointInfo[item.Value.Count];
			for (int m = 0; m < item.Value.Count; m++)
			{
				exHotPoint exHotPoint5 = item.Value[m];
				exUIPointInfo exUIPointInfo4 = new exUIPointInfo
				{
					id = exHotPoint5.id,
					pos = exHotPoint5.pos,
					delta = exHotPoint5.delta,
					worldPos = exHotPoint5.worldPos,
					worldDelta = exHotPoint5.worldDelta
				};
				exUIPointEvent4.pointInfos[m] = exUIPointInfo4;
				exUIPointEvent4.isMouse = exHotPoint5.isMouse;
			}
			item.Key.OnHoverMove(exUIPointEvent4);
		}
		foreach (exHotPoint exHotPoint6 in _hotPoints)
		{
			if (!exHotPoint6.active || !exHotPoint6.pressUp)
			{
				continue;
			}
			exUIPointInfo exUIPointInfo5 = new exUIPointInfo
			{
				id = exHotPoint6.id,
				pos = exHotPoint6.pos,
				delta = exHotPoint6.delta,
				worldPos = exHotPoint6.worldPos,
				worldDelta = exHotPoint6.worldDelta
			};
			exUIPointEvent exUIPointEvent5 = new exUIPointEvent();
			exUIPointEvent5.isMouse = exHotPoint6.isMouse;
			exUIPointEvent5.pointInfos = new exUIPointInfo[1] { exUIPointInfo5 };
			exUIControl exUIControl5 = exHotPoint6.hover;
			if (exHotPoint6.pressed != null && exHotPoint6.pressed.grabMouseOrTouch)
			{
				exUIControl5 = exHotPoint6.pressed;
			}
			if (exUIControl5 != null)
			{
				exUIControl5.OnPressUp(exUIPointEvent5);
				if (exHotPoint6.isTouch)
				{
					exUIControl5.OnHoverOut(exUIPointEvent5);
				}
			}
			exHotPoint6.pressed = null;
		}
	}

	private void HandleTouches()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.fingerId < 10)
			{
				exHotPoint exHotPoint2 = touchPoints[touch.fingerId];
				exHotPoint2.active = true;
				if (!exHotPoint2.active)
				{
					exHotPoint2.Reset();
					continue;
				}
				exHotPoint2.pos = touch.position;
				exHotPoint2.delta = touch.deltaPosition;
				Vector3 vector = GetComponent<Camera>().ScreenToWorldPoint(touch.position - touch.deltaPosition);
				exHotPoint2.worldPos = GetComponent<Camera>().ScreenToWorldPoint(touch.position);
				exHotPoint2.worldDelta = exHotPoint2.worldPos - vector;
				exHotPoint2.pressDown = touch.phase == TouchPhase.Began;
				exHotPoint2.pressUp = touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended;
			}
		}
		HandleHotPoints(touchPoints, _isMouse: false);
	}

	private void HandleMouse()
	{
		if (simulateMouseAsTouch)
		{
			for (int i = 0; i < 3; i++)
			{
				exHotPoint exHotPoint2 = touchPoints[i];
				exHotPoint2.id = i;
				exHotPoint2.active = Input.GetMouseButtonDown(i) || Input.GetMouseButton(i) || Input.GetMouseButtonUp(i);
				if (!exHotPoint2.active)
				{
					exHotPoint2.Reset();
					continue;
				}
				Vector2 pos = exHotPoint2.pos;
				exHotPoint2.pos = Input.mousePosition;
				if (Input.GetMouseButtonDown(i))
				{
					exHotPoint2.delta = Vector2.zero;
				}
				else
				{
					exHotPoint2.delta = exHotPoint2.pos - pos;
				}
				Vector3 worldPos = exHotPoint2.worldPos;
				exHotPoint2.worldPos = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
				exHotPoint2.worldDelta = exHotPoint2.worldPos - worldPos;
				exHotPoint2.pressDown = Input.GetMouseButtonDown(i);
				exHotPoint2.pressUp = Input.GetMouseButtonUp(i);
			}
			HandleHotPoints(touchPoints, _isMouse: false);
			return;
		}
		for (int j = 0; j < 3; j++)
		{
			exHotPoint exHotPoint3 = mousePoints[j];
			exHotPoint3.active = true;
			if (!exHotPoint3.active)
			{
				exHotPoint3.Reset();
				continue;
			}
			Vector2 pos2 = exHotPoint3.pos;
			exHotPoint3.pos = Input.mousePosition;
			exHotPoint3.delta = exHotPoint3.pos - pos2;
			Vector3 worldPos2 = exHotPoint3.worldPos;
			exHotPoint3.worldPos = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
			exHotPoint3.worldDelta = exHotPoint3.worldPos - worldPos2;
			exHotPoint3.pressDown = Input.GetMouseButtonDown(j);
			exHotPoint3.pressUp = Input.GetMouseButtonUp(j);
		}
		HandleHotPoints(mousePoints, _isMouse: true);
	}

	private exUIControl PickControl(Vector2 _screenPos)
	{
		Vector3 vector = GetComponent<Camera>().ScreenToWorldPoint(_screenPos);
		controls.Sort(controlSorterByPrioirty);
		for (int i = 0; i < controls.Count; i++)
		{
			exUIControl ctrl = controls[i];
			exUIControl exUIControl2 = RecursivelyGetUIControl(ctrl, vector);
			if (exUIControl2 != null)
			{
				return exUIControl2;
			}
		}
		Ray ray = GetComponent<Camera>().ScreenPointToRay(_screenPos);
		ray.origin = new Vector3(ray.origin.x, ray.origin.y, GetComponent<Camera>().transform.position.z);
		RaycastHit[] array = Physics.RaycastAll(ray);
		List<exUIControl> list = new List<exUIControl>();
		foreach (RaycastHit raycastHit in array)
		{
			exUIControl component = raycastHit.collider.gameObject.GetComponent<exUIControl>();
			if ((bool)component && component.gameObject.activeInHierarchy && component.activeInHierarchy)
			{
				list.Add(component);
			}
		}
		if (list.Count > 0)
		{
			list.Sort(controlSorterByLevel);
			return list[list.Count - 1];
		}
		return null;
	}

	private exUIControl RecursivelyGetUIControl(exUIControl _ctrl, Vector2 _worldPos)
	{
		if (!_ctrl.gameObject.activeSelf || !_ctrl.activeSelf)
		{
			return null;
		}
		bool flag = false;
		if (_ctrl.useCollider)
		{
			flag = true;
		}
		else
		{
			Vector2 point = new Vector2(_worldPos.x - _ctrl.transform.position.x, _worldPos.y - _ctrl.transform.position.y);
			flag = _ctrl.GetLocalAABoundingRect().Contains(point);
		}
		if (flag)
		{
			for (int i = 0; i < _ctrl.children.Count; i++)
			{
				exUIControl ctrl = _ctrl.children[i];
				exUIControl exUIControl2 = RecursivelyGetUIControl(ctrl, _worldPos);
				if (exUIControl2 != null)
				{
					return exUIControl2;
				}
			}
			if (!_ctrl.useCollider)
			{
				return _ctrl;
			}
		}
		return null;
	}

	public void ShowDebugInfo(Rect _pos)
	{
		GUILayout.BeginArea(new Rect(_pos.x, _pos.y, _pos.width, _pos.height), "Debug", GUI.skin.window);
		GUILayout.Label("Keyboard Focus: " + (focus ? focus.name : "None"));
		if (hasTouch || simulateMouseAsTouch)
		{
			for (int i = 0; i < touchPoints.Length; i++)
			{
				exHotPoint exHotPoint2 = touchPoints[i];
				if (exHotPoint2.active)
				{
					GUILayout.Label("Touch[" + i + "]");
					GUILayout.BeginHorizontal();
					GUILayout.Space(15f);
					GUILayout.BeginVertical();
					GUILayout.Label("pos: " + exHotPoint2.pos.ToString());
					GUILayout.Label("hover: " + (exHotPoint2.hover ? exHotPoint2.hover.name : "None"));
					GUILayout.Label("pressed: " + (exHotPoint2.pressed ? exHotPoint2.pressed.name : "None"));
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
			}
		}
		if (hasMouse && !simulateMouseAsTouch)
		{
			GUILayout.Label("Mouse");
			GUILayout.BeginHorizontal();
			GUILayout.Space(15f);
			GUILayout.BeginVertical();
			GUILayout.Label("pos: " + mousePoints[0].pos.ToString());
			GUILayout.Label("hover: " + (mousePoints[0].hover ? mousePoints[0].hover.name : "None"));
			GUILayout.Label("left-pressed: " + (mousePoints[0].pressed ? mousePoints[0].pressed.name : "None"));
			GUILayout.Label("right-pressed: " + (mousePoints[1].pressed ? mousePoints[1].pressed.name : "None"));
			GUILayout.Label("middle-pressed: " + (mousePoints[2].pressed ? mousePoints[2].pressed.name : "None"));
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		GUILayout.Label("Root Controls");
		GUILayout.BeginHorizontal();
		GUILayout.Space(15f);
		GUILayout.BeginVertical();
		for (int j = 0; j < controls.Count; j++)
		{
			GUILayout.Label("[" + j + "] " + controls[j].name);
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
