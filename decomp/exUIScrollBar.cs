using UnityEngine;

public class exUIScrollBar : exUIControl
{
	public enum Direction
	{
		Vertical,
		Horizontal
	}

	public Direction direction;

	public exUIScrollView scrollView;

	public float cooldown = 0.5f;

	protected exSprite bar;

	protected exSprite background;

	protected float ratio = 1f;

	protected float scrollOffset;

	protected Vector3 scrollStart = Vector3.zero;

	protected bool dragging;

	protected int draggingID = -1;

	protected float cooldownTimer;

	protected bool isCoolingDown;

	protected new void Awake()
	{
		base.Awake();
		Transform transform = base.transform.Find("__bar");
		if ((bool)transform)
		{
			bar = transform.GetComponent<exSprite>();
			if ((bool)bar)
			{
				bar.customSize = true;
				bar.anchor = Anchor.TopLeft;
			}
			exUIButton component = transform.GetComponent<exUIButton>();
			if ((bool)component)
			{
				component.grabMouseOrTouch = true;
				component.AddEventListener("onPressDown", (exUIEvent _event) =>
				{
					if (!dragging)
					{
						exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
						if (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0))
						{
							dragging = true;
							draggingID = exUIPointEvent2.mainPoint.id;
							exUIMng.inst.SetFocus(this);
						}
					}
				});
				component.AddEventListener("onPressUp", (exUIEvent _event) =>
				{
					exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
					if ((exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0)) && exUIPointEvent2.pointInfos[0].id == draggingID && dragging)
					{
						dragging = false;
						draggingID = -1;
					}
				});
				component.AddEventListener("onHoverMove", (exUIEvent _event) =>
				{
					if ((bool)scrollView)
					{
						exUIPointEvent exUIPointEvent2 = _event as exUIPointEvent;
						for (int i = 0; i < exUIPointEvent2.pointInfos.Length; i++)
						{
							exUIPointInfo exUIPointInfo2 = exUIPointEvent2.pointInfos[i];
							if (dragging && (exUIPointEvent2.isTouch || exUIPointEvent2.GetMouseButton(0)) && exUIPointInfo2.id == draggingID)
							{
								Vector2 vector = exUIPointInfo2.worldDelta;
								vector.y = 0f - vector.y;
								scrollView.Scroll(vector / ratio);
							}
						}
					}
				});
			}
		}
		background = GetComponent<exSprite>();
		if ((bool)background)
		{
			scrollStart = (transform ? transform.localPosition : Vector3.zero);
			if (background.spriteType == exSpriteType.Sliced)
			{
				if (direction == Direction.Horizontal)
				{
					scrollStart.x = background.leftBorderSize;
				}
				else
				{
					scrollStart.y = background.topBorderSize;
				}
			}
		}
		if (!scrollView)
		{
			return;
		}
		scrollView.AddEventListener("onContentResized", (exUIEvent _event) =>
		{
			UpdateScrollBarRatio();
			UpdateScrollBar();
		});
		scrollView.AddEventListener("onScroll", (exUIEvent _event) =>
		{
			if (direction == Direction.Horizontal)
			{
				scrollOffset = scrollView.scrollOffset.x * ratio;
			}
			else
			{
				scrollOffset = scrollView.scrollOffset.y * ratio;
			}
			UpdateScrollBar();
			if (scrollView.showCondition == exUIScrollView.ShowCondition.WhenDragging)
			{
				activeSelf = true;
			}
		});
		scrollView.AddEventListener("onScrollFinished", (exUIEvent _event) =>
		{
			if (scrollView.showCondition == exUIScrollView.ShowCondition.WhenDragging)
			{
				cooldownTimer = cooldown;
				isCoolingDown = true;
			}
		});
		UpdateScrollBarRatio();
		UpdateScrollBar();
		if (scrollView.showCondition != exUIScrollView.ShowCondition.WhenDragging)
		{
			return;
		}
		exUIEffect component2 = GetComponent<exUIEffect>();
		if (component2 == null)
		{
			component2 = gameObject.AddComponent<exUIEffect>();
			if (background != null)
			{
				component2.AddEffect_Color(background, EffectEventType.Deactive, exEase.Type.Linear, new Color(background.color.r, background.color.g, background.color.b, 0f), 0.5f);
			}
			if (bar != null)
			{
				component2.AddEffect_Color(bar, EffectEventType.Deactive, exEase.Type.Linear, new Color(bar.color.r, bar.color.g, bar.color.b, 0f), 0.5f);
			}
		}
		if ((bool)background)
		{
			Color color = background.color;
			color.a = 0f;
			background.color = color;
		}
		if ((bool)bar)
		{
			Color color2 = bar.color;
			color2.a = 0f;
			bar.color = color2;
		}
		active_ = false;
	}

	private void Update()
	{
		if (isCoolingDown)
		{
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				isCoolingDown = false;
				activeSelf = false;
			}
		}
	}

	private void UpdateScrollBarRatio()
	{
		float num = 0f;
		float num2 = 0f;
		if (direction == Direction.Horizontal)
		{
			num = scrollView.contentSize.x;
			num2 = width;
			if (background != null && background.spriteType == exSpriteType.Sliced)
			{
				num2 = num2 - background.leftBorderSize - background.rightBorderSize;
			}
		}
		else
		{
			num = scrollView.contentSize.y;
			num2 = height;
			if (background != null && background.spriteType == exSpriteType.Sliced)
			{
				num2 = num2 - background.topBorderSize - background.bottomBorderSize;
			}
		}
		ratio = Mathf.Min(num2 / num, 1f);
	}

	private void UpdateScrollBar()
	{
		if (direction == Direction.Horizontal)
		{
			float num = ratio * scrollView.width;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = width;
			float num5 = scrollOffset;
			bar.width = ratio * scrollView.width;
			if (bar.spriteType == exSpriteType.Sliced)
			{
				bar.width = bar.width + bar.leftBorderSize + bar.rightBorderSize;
				num2 = bar.leftBorderSize;
			}
			if (background != null && background.spriteType == exSpriteType.Sliced)
			{
				num4 = num4 - background.leftBorderSize - background.rightBorderSize;
			}
			float num6 = 0f;
			float num7 = num4 - num;
			if (num5 < num6)
			{
				num3 = num6 - num5;
				num5 = num6;
			}
			else if (num5 > num7)
			{
				num3 = num5 - num7;
				num5 = num7 + num3;
			}
			bar.width -= num3;
			bar.transform.localPosition = new Vector3(scrollStart.x + num5 - num2, 0f - scrollStart.y, scrollStart.z);
		}
		else
		{
			float num8 = ratio * scrollView.height;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = height;
			float num12 = scrollOffset;
			bar.height = num8;
			if (bar.spriteType == exSpriteType.Sliced)
			{
				bar.height = bar.height + bar.topBorderSize + bar.bottomBorderSize;
				num9 = bar.topBorderSize;
			}
			if (background != null && background.spriteType == exSpriteType.Sliced)
			{
				num11 = num11 - background.topBorderSize - background.bottomBorderSize;
			}
			float num13 = 0f;
			float num14 = num11 - num8;
			if (num12 < num13)
			{
				num10 = num13 - num12;
				num12 = num13;
			}
			else if (num12 > num14)
			{
				num10 = num12 - num14;
				num12 = num14 + num10;
			}
			bar.height -= num10;
			bar.transform.localPosition = new Vector3(scrollStart.x, 0f - (scrollStart.y + num12 - num9), scrollStart.z);
		}
	}
}
