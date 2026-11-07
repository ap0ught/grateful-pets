using System.Collections.Generic;
using UnityEngine;

public class exUIButton : exUIControl
{
	public new static string[] eventNames = new string[3] { "onClick", "onButtonDown", "onButtonUp" };

	private List<exUIEventListener> onClick;

	private List<exUIEventListener> onButtonDown;

	private List<exUIEventListener> onButtonUp;

	public bool allowDrag = true;

	public float dragThreshold = 40f;

	private bool pressing;

	private int pressingID = -1;

	private Vector2 pressDownAt = Vector2.zero;

	public void OnClick(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onClick", onClick, _event);
	}

	public void OnButtonDown(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onButtonDown", onButtonDown, _event);
	}

	public void OnButtonUp(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onButtonUp", onButtonUp, _event);
	}

	public override void CacheEventListeners()
	{
		base.CacheEventListeners();
		onClick = eventListenerTable["onClick"];
		onButtonDown = eventListenerTable["onButtonDown"];
		onButtonUp = eventListenerTable["onButtonUp"];
	}

	public override string[] GetEventNames()
	{
		string[] array = base.GetEventNames();
		string[] array2 = new string[array.Length + eventNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i];
		}
		for (int j = 0; j < eventNames.Length; j++)
		{
			array2[j + array.Length] = eventNames[j];
		}
		return array2;
	}

	protected new void Awake()
	{
		base.Awake();
		AddEventListener("onPressDown", (exUIEvent _event) =>
		{
			if (!pressing)
			{
				exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
				if (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0))
				{
					pressing = true;
					pressDownAt = exUIPointEvent2.mainPoint.pos;
					pressingID = exUIPointEvent2.mainPoint.id;
					exUIMng.inst.SetFocus(this);
					OnButtonDown(new exUIEvent
					{
						bubbles = false
					});
					_event.StopPropagation();
				}
			}
		});
		AddEventListener("onPressUp", (exUIEvent _event) =>
		{
			exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
			if (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0))
			{
				OnButtonUp(new exUIEvent
				{
					bubbles = false
				});
				if (pressing)
				{
					pressing = false;
					OnClick(new exUIEvent
					{
						bubbles = false
					});
					_event.StopPropagation();
				}
			}
		});
		AddEventListener("onHoverOut", (exUIEvent _event) =>
		{
			if (pressing)
			{
				pressing = false;
				pressingID = -1;
				_event.StopPropagation();
			}
		});
		AddEventListener("onHoverMove", (exUIEvent _event) =>
		{
			if (pressing)
			{
				exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
				for (int i = 0; i < exUIPointEvent2.pointInfos.Length; i++)
				{
					exUIPointInfo exUIPointInfo2 = exUIPointEvent2.pointInfos[i];
					if (exUIPointInfo2.id == pressingID)
					{
						Vector2 vector = exUIPointEvent2.mainPoint.pos - pressDownAt;
						if (!allowDrag && vector.sqrMagnitude >= dragThreshold * dragThreshold)
						{
							exUIMng.inst.HoverOut(this, exUIPointInfo2.id);
						}
						else
						{
							_event.StopPropagation();
						}
					}
				}
			}
		});
	}
}
