using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class ScreenAwareTargetedContentFitter : UIBehaviour, ILayoutSelfController, ILayoutController
{
	internal static class SetPropertyUtility
	{
		public static bool SetColor(ref Color currentValue, Color newValue)
		{
			if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
			{
				return false;
			}
			currentValue = newValue;
			return true;
		}

		public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
		{
			if (currentValue.Equals(newValue))
			{
				return false;
			}
			currentValue = newValue;
			return true;
		}

		public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
		{
			if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
			{
				return false;
			}
			currentValue = newValue;
			return true;
		}
	}

	public enum FitMode
	{
		Unconstrained,
		MinSize,
		PreferredSize
	}

	[SerializeField]
	protected FitMode m_HorizontalFit;

	[SerializeField]
	protected FitMode m_VerticalFit;

	public float screenHeightMax = 100f;

	public float screenWidthMax = 100f;

	[NonSerialized]
	private RectTransform m_Rect;

	public RectTransform target;

	private DrivenRectTransformTracker m_Tracker;

	public FitMode horizontalFit
	{
		get
		{
			return m_HorizontalFit;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value))
			{
				SetDirty();
			}
		}
	}

	public FitMode verticalFit
	{
		get
		{
			return m_VerticalFit;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value))
			{
				SetDirty();
			}
		}
	}

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	protected ScreenAwareTargetedContentFitter()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		m_Tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	private void HandleSelfFittingAlongAxis(int axis)
	{
		FitMode fitMode = ((axis == 0) ? horizontalFit : verticalFit);
		if (fitMode != FitMode.Unconstrained)
		{
			m_Tracker.Add(this, rectTransform, (axis == 0) ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY);
			float a = ((axis == 0) ? (CanvasSize.width * screenWidthMax) : (CanvasSize.height * screenHeightMax));
			if (fitMode == FitMode.MinSize)
			{
				rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, Mathf.Min(a, LayoutUtility.GetMinSize(target, axis)));
			}
			else
			{
				rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, Mathf.Min(a, LayoutUtility.GetPreferredSize(target, axis)));
			}
		}
	}

	public virtual void SetLayoutHorizontal()
	{
		m_Tracker.Clear();
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
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}
}
