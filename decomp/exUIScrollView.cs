using System.Collections.Generic;
using UnityEngine;

public class exUIScrollView : exUIControl
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring
	}

	public enum ShowCondition
	{
		Always,
		OnlyIfNeeded,
		WhenDragging
	}

	public enum ContentDirection
	{
		TopToBottom,
		BottomToTop,
		LeftToRight,
		RightToLeft
	}

	public new static string[] eventNames = new string[3] { "onScroll", "onScrollFinished", "onContentResized" };

	private List<exUIEventListener> onScroll;

	private List<exUIEventListener> onScrollFinished;

	private List<exUIEventListener> onContentResized;

	[SerializeField]
	protected Vector2 contentSize_ = Vector2.zero;

	public bool draggable = true;

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	public ShowCondition showCondition;

	public bool allowHorizontalScroll = true;

	public ContentDirection horizontalContentDir = ContentDirection.LeftToRight;

	public bool allowVerticalScroll = true;

	public ContentDirection verticalContentDir;

	public Transform contentAnchor;

	public float scrollSpeed = 0.5f;

	private Vector2 scrollOffset_ = Vector2.zero;

	private bool dragging;

	private int draggingID = -1;

	private Vector3 originalAnchorPos = Vector3.zero;

	private bool damping;

	private Vector2 velocity = Vector2.zero;

	private bool spring;

	private Vector3 scrollDest = Vector3.zero;

	public Vector2 contentSize
	{
		get
		{
			return contentSize_;
		}
		set
		{
			if (contentSize_ != value)
			{
				contentSize_ = value;
				exUIEvent exUIEvent2 = new exUIEvent();
				exUIEvent2.bubbles = false;
				OnContentResized(exUIEvent2);
			}
		}
	}

	public Vector2 scrollOffset => scrollOffset_;

	public void OnScroll(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onScroll", onScroll, _event);
	}

	public void OnScrollFinished(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onScrollFinished", onScrollFinished, _event);
	}

	public void OnContentResized(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onContentResized", onContentResized, _event);
	}

	public override void CacheEventListeners()
	{
		base.CacheEventListeners();
		onScroll = eventListenerTable["onScroll"];
		onScrollFinished = eventListenerTable["onScrollFinished"];
		onContentResized = eventListenerTable["onContentResized"];
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
		grabMouseOrTouch = true;
		if (contentAnchor != null)
		{
			originalAnchorPos = contentAnchor.localPosition;
		}
		AddEventListener("onPressDown", (exUIEvent _event) =>
		{
			if (dragging)
			{
				_event.StopPropagation();
			}
			else
			{
				StartDrag(_event);
			}
		});
		AddEventListener("onPressUp", (exUIEvent _event) =>
		{
			exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
			if (draggable && (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0)) && exUIPointEvent2.pointInfos[0].id == draggingID)
			{
				if (dragging)
				{
					dragging = false;
					draggingID = -1;
					StartScroll();
				}
				_event.StopPropagation();
			}
		});
		AddEventListener("onHoverIn", (exUIEvent _event) =>
		{
			if (dragging)
			{
				_event.StopPropagation();
			}
			else
			{
				StartDrag(_event);
			}
		});
		AddEventListener("onHoverMove", (exUIEvent _event) =>
		{
			if (!dragging)
			{
				StartDrag(_event);
			}
			else
			{
				float x = ((horizontalContentDir == ContentDirection.LeftToRight) ? 0f : (0f - (contentSize_.x - width)));
				float y = ((verticalContentDir == ContentDirection.TopToBottom) ? 0f : (0f - (contentSize_.y - height)));
				Rect rect = new Rect(scrollOffset_.x, scrollOffset_.y, width, height);
				Rect bound = new Rect(x, y, Mathf.Max(contentSize_.x, width), Mathf.Max(contentSize_.y, height));
				Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset(rect, bound);
				exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
				for (int i = 0; i < exUIPointEvent2.pointInfos.Length; i++)
				{
					exUIPointInfo exUIPointInfo2 = exUIPointEvent2.pointInfos[i];
					if (draggable && (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0)) && exUIPointInfo2.id == draggingID)
					{
						Vector2 vector = exUIPointInfo2.worldDelta;
						vector.x = 0f - vector.x;
						if (Mathf.Abs(constrainOffset.x) > 0.001f)
						{
							vector.x *= 0.5f;
						}
						if (Mathf.Abs(constrainOffset.y) > 0.001f)
						{
							vector.y *= 0.5f;
						}
						velocity = Vector2.Lerp(velocity, velocity + vector / Time.deltaTime * scrollSpeed, 0.67f);
						if (Mathf.Sign(velocity.x) != Mathf.Sign(vector.x))
						{
							velocity.x = 0f;
						}
						if (Mathf.Sign(velocity.y) != Mathf.Sign(vector.y))
						{
							velocity.y = 0f;
						}
						_event.StopPropagation();
						Scroll(vector);
						break;
					}
				}
			}
		});
		AddEventListener("onMouseWheel", (exUIEvent _event) =>
		{
			float x = ((horizontalContentDir == ContentDirection.LeftToRight) ? 0f : (0f - (contentSize_.x - width)));
			float y = ((verticalContentDir == ContentDirection.TopToBottom) ? 0f : (0f - (contentSize_.y - height)));
			exUIWheelEvent exUIWheelEvent2 = _event as exUIWheelEvent;
			Vector2 vector = new Vector2(0f, (0f - exUIWheelEvent2.delta) * 100f);
			if (Mathf.Abs(exGeometryUtility.GetConstrainOffset(new Rect(scrollOffset_.x, scrollOffset_.y, width, height), new Rect(x, y, contentSize_.x, contentSize_.y)).y) > 0.001f)
			{
				vector.y *= 0.5f;
			}
			velocity = Vector2.Lerp(velocity, velocity + vector / Time.deltaTime * scrollSpeed, 0.67f);
			if (Mathf.Sign(velocity.x) != Mathf.Sign(vector.x))
			{
				velocity.x = 0f;
			}
			if (Mathf.Sign(velocity.y) != Mathf.Sign(vector.y))
			{
				velocity.y = 0f;
			}
			_event.StopPropagation();
			Scroll(vector);
			StartScroll();
		});
	}

	private void LateUpdate()
	{
		Vector2 vector = Vector2.zero;
		Vector2 delta = Vector2.zero;
		bool num = damping || spring;
		float x = ((horizontalContentDir == ContentDirection.LeftToRight) ? 0f : (0f - (contentSize_.x - width)));
		float y = ((verticalContentDir == ContentDirection.TopToBottom) ? 0f : (0f - (contentSize_.y - height)));
		Rect rect = new Rect(scrollOffset_.x, scrollOffset_.y, width, height);
		Rect bound = new Rect(x, y, Mathf.Max(contentSize_.x, width), Mathf.Max(contentSize_.y, height));
		if (damping || spring)
		{
			vector = exGeometryUtility.GetConstrainOffset(rect, bound);
		}
		velocity.x *= 0.9f;
		velocity.y *= 0.9f;
		if (damping)
		{
			if (Mathf.Abs(vector.x) > 0.001f)
			{
				if (dragEffect != DragEffect.MomentumAndSpring)
				{
					velocity.x = 0f;
				}
				else
				{
					spring = true;
				}
			}
			if (Mathf.Abs(vector.y) > 0.001f)
			{
				if (dragEffect != DragEffect.MomentumAndSpring)
				{
					velocity.y = 0f;
				}
				else
				{
					spring = true;
				}
			}
			if (velocity.sqrMagnitude < 1f)
			{
				damping = false;
				velocity = Vector2.zero;
			}
			else
			{
				delta = velocity * Time.deltaTime;
			}
		}
		if (spring)
		{
			Vector2 vector2 = contentAnchor.localPosition;
			Vector2 vector3 = exMath.SpringLerp(vector2, vector2 - vector, 15f, Time.deltaTime) - vector2;
			if (vector3.sqrMagnitude < 0.001f)
			{
				delta = -vector;
				spring = false;
			}
			else
			{
				delta += vector3;
			}
		}
		if (num)
		{
			Scroll(delta);
			contentAnchor.localPosition = scrollDest;
			if (damping || spring)
			{
				exUIEvent exUIEvent2 = new exUIEvent();
				exUIEvent2.bubbles = false;
				OnScrollFinished(exUIEvent2);
			}
		}
		else
		{
			contentAnchor.localPosition = Vector3.Lerp(contentAnchor.localPosition, scrollDest, 0.6f);
		}
	}

	private void StartDrag(exUIEvent _event)
	{
		exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
		if (draggable && (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0)))
		{
			dragging = true;
			draggingID = exUIPointEvent2.mainPoint.id;
			damping = false;
			spring = false;
			velocity = Vector2.zero;
			exUIMng.inst.SetFocus(this);
			_event.StopPropagation();
		}
	}

	public void StartScroll()
	{
		if (dragEffect != DragEffect.None)
		{
			damping = true;
			float x = ((horizontalContentDir == ContentDirection.LeftToRight) ? 0f : (0f - (contentSize_.x - width)));
			float y = ((verticalContentDir == ContentDirection.TopToBottom) ? 0f : (0f - (contentSize_.y - height)));
			Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset(new Rect(scrollOffset_.x, scrollOffset_.y, width, height), new Rect(x, y, contentSize_.x, contentSize_.y));
			if (Mathf.Abs(constrainOffset.x) > 0.001f && dragEffect == DragEffect.MomentumAndSpring)
			{
				velocity.x *= 0.5f;
			}
			if (Mathf.Abs(constrainOffset.y) > 0.001f && dragEffect == DragEffect.MomentumAndSpring)
			{
				velocity.y *= 0.5f;
			}
		}
		else
		{
			velocity = Vector2.zero;
			damping = false;
			exUIEvent exUIEvent2 = new exUIEvent();
			exUIEvent2.bubbles = false;
			OnScrollFinished(exUIEvent2);
		}
	}

	public void Scroll(Vector2 _delta)
	{
		if (!allowHorizontalScroll)
		{
			_delta.x = 0f;
		}
		if (!allowVerticalScroll)
		{
			_delta.y = 0f;
		}
		scrollOffset_ += _delta;
		if (dragEffect != DragEffect.MomentumAndSpring)
		{
			scrollOffset_.x = Mathf.Clamp(scrollOffset_.x, 0f, contentSize_.x - width);
			scrollOffset_.y = Mathf.Clamp(scrollOffset_.y, 0f, contentSize_.y - height);
		}
		if (contentAnchor != null)
		{
			scrollDest = new Vector3(originalAnchorPos.x - scrollOffset_.x, originalAnchorPos.y + scrollOffset_.y, originalAnchorPos.z);
		}
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnScroll(exUIEvent2);
	}
}
