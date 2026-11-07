using System;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

[ExecuteAlways]
public class ScrollWindowFitter : MonoBehaviour, ILayoutElement
{
	public RectTransform Content;

	public ScrollRect Scroller;

	public float verticalSpaceReserved;

	public float extraWidthForScroll;

	public float contentHeight;

	public float contentWidth;

	public float maxHeight => (float)((double)Screen.height / Options.StageScale) - verticalSpaceReserved;

	public bool isScrolling => Scroller.verticalScrollbar.enabled;

	public float minWidth => -1f;

	public float preferredWidth => contentWidth + (isScrolling ? extraWidthForScroll : 0f);

	public float flexibleWidth => -1f;

	public float minHeight => -1f;

	public float preferredHeight => Math.Min(maxHeight, contentHeight);

	public float flexibleHeight => -1f;

	public int layoutPriority => 1;

	public void CalculateLayoutInputHorizontal()
	{
		contentWidth = LayoutUtility.GetPreferredWidth(Content);
	}

	public void CalculateLayoutInputVertical()
	{
		contentHeight = LayoutUtility.GetPreferredHeight(Content);
	}
}
