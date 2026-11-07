using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Qud.UI;

[AddComponentMenu("Layout/Content Size Fitter With Max", 141)]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ContentSizeWithMax : UIBehaviour, ILayoutSelfController, ILayoutController
{
	public enum FitMode
	{
		Unconstrained,
		MinSize,
		PreferredSize
	}

	[SerializeField]
	protected FitMode horizontalFit;

	[SerializeField]
	protected FitMode verticalFit;

	[Tooltip("Maximum Preferred size when using Preferred Size")]
	public Vector2 MaximumPreferredSize;

	[NonSerialized]
	private RectTransform rectTransform;

	private DrivenRectTransformTracker tracker;

	public FitMode HorizontalFit
	{
		get
		{
			return horizontalFit;
		}
		set
		{
			if (horizontalFit != value)
			{
				horizontalFit = value;
				SetDirty();
			}
		}
	}

	public FitMode VerticalFit
	{
		get
		{
			return verticalFit;
		}
		set
		{
			if (verticalFit != value)
			{
				verticalFit = value;
				SetDirty();
			}
		}
	}

	private RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	private void HandleSelfFittingAlongAxis(int axis)
	{
		FitMode fitMode = ((axis == 0) ? HorizontalFit : VerticalFit);
		if (fitMode == FitMode.Unconstrained)
		{
			tracker.Add(this, RectTransform, DrivenTransformProperties.None);
			return;
		}
		tracker.Add(this, RectTransform, (axis == 0) ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY);
		switch (fitMode)
		{
		case FitMode.MinSize:
			RectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(rectTransform, axis));
			break;
		case FitMode.PreferredSize:
			RectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, Mathf.Clamp(LayoutUtility.GetPreferredSize(rectTransform, axis), 0f, (axis == 0) ? MaximumPreferredSize.x : MaximumPreferredSize.y));
			break;
		}
	}

	public virtual void SetLayoutHorizontal()
	{
		tracker.Clear();
		HandleSelfFittingAlongAxis(0);
	}

	public virtual void SetLayoutVertical()
	{
		HandleSelfFittingAlongAxis(1);
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
		}
	}
}
