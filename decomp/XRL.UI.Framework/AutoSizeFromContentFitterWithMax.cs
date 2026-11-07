using System;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

[ExecuteAlways]
public class AutoSizeFromContentFitterWithMax : MonoBehaviour, ILayoutElement
{
	public RectTransform Content;

	public float verticalSpaceReserved;

	public float extraWidthForScroll;

	public float contentHeight;

	public float contentWidth;

	public bool preferredMode;

	public float maxHeight => (float)((double)Screen.height / Options.StageScale) - verticalSpaceReserved;

	public bool isScrolling => maxHeight < contentHeight;

	public float minWidth => contentWidth + (isScrolling ? extraWidthForScroll : 0f);

	public float preferredWidth
	{
		get
		{
			if (!preferredMode)
			{
				return contentWidth;
			}
			return -1f;
		}
	}

	public float flexibleWidth => preferredMode ? 1 : 0;

	public float minHeight
	{
		get
		{
			if (!preferredMode)
			{
				return Math.Min(maxHeight, contentHeight);
			}
			return -1f;
		}
	}

	public float preferredHeight
	{
		get
		{
			if (!preferredMode)
			{
				return -1f;
			}
			return Math.Min(maxHeight, contentHeight);
		}
	}

	public float flexibleHeight => preferredMode ? 1 : 0;

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
