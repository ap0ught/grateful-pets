using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI;

public class ScrollViewCalcs
{
	public ScrollRect scroller;

	public float contentHeight;

	public float scrollHeight;

	public float contentTop;

	public float contentBottom;

	public float scrollTop;

	public float scrollBottom;

	public float scrollPercent;

	private static Vector3[] contentCorners = new Vector3[4];

	private static Vector3[] myCorners = new Vector3[4];

	public bool isAnyAboveView => scrollTop > contentTop;

	public bool isAnyBelowView => scrollBottom < contentBottom;

	public bool isAnyInView
	{
		get
		{
			if (contentBottom > scrollTop)
			{
				return contentTop < scrollBottom;
			}
			return false;
		}
	}

	public bool isInFullView
	{
		get
		{
			if (!isAnyAboveView)
			{
				return !isAnyBelowView;
			}
			return false;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("contentHeight: " + contentHeight + " ");
		stringBuilder.Append("scrollHeight: " + scrollHeight + " ");
		stringBuilder.Append("contentTop: " + contentTop + " ");
		stringBuilder.Append("contentBottom: " + contentBottom + " ");
		stringBuilder.Append("scrollTop: " + scrollTop + " ");
		stringBuilder.Append("scrollBottom: " + scrollBottom + " ");
		stringBuilder.Append("scrollPercent: " + scrollPercent + " ");
		return stringBuilder.ToString();
	}

	public void ScrollPageDown()
	{
		scroller.verticalNormalizedPosition = scrollPercent - scrollHeight / contentHeight / 2f;
	}

	public void ScrollPageUp()
	{
		scroller.verticalNormalizedPosition = scrollPercent + scrollHeight / contentHeight / 2f;
	}

	public void ScrollToContentTop(float contentTop)
	{
		scroller.verticalNormalizedPosition = 1f - contentTop / (contentHeight - scrollHeight);
	}

	public static ScrollViewCalcs GetScrollViewCalcs(RectTransform rt, ScrollViewCalcs reuse = null)
	{
		ScrollViewCalcs scrollViewCalcs = reuse ?? new ScrollViewCalcs();
		if (scrollViewCalcs.scroller == null)
		{
			scrollViewCalcs.scroller = rt.GetComponentInParent<ScrollRect>();
		}
		if (scrollViewCalcs.scroller != null && scrollViewCalcs.scroller.vertical)
		{
			scrollViewCalcs.scroller.content.GetWorldCorners(contentCorners);
			rt.GetWorldCorners(myCorners);
			scrollViewCalcs.contentHeight = scrollViewCalcs.scroller.content.rect.height * (float)Options.StageScale;
			scrollViewCalcs.scrollHeight = scrollViewCalcs.scroller.viewport.rect.height * (float)Options.StageScale;
			scrollViewCalcs.contentTop = contentCorners[2].y - myCorners[2].y;
			scrollViewCalcs.contentBottom = contentCorners[2].y - myCorners[0].y;
			scrollViewCalcs.scrollPercent = scrollViewCalcs.scroller.verticalNormalizedPosition;
			scrollViewCalcs.scrollTop = (1f - scrollViewCalcs.scrollPercent) * (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			scrollViewCalcs.scrollBottom = scrollViewCalcs.scrollTop + scrollViewCalcs.scrollHeight;
		}
		return scrollViewCalcs;
	}

	public static void ScrollIntoView(RectTransform rectTransform, ScrollViewCalcs reuse = null)
	{
		ScrollRect componentInParent = rectTransform.GetComponentInParent<ScrollRect>();
		if (!(componentInParent != null) || !componentInParent.vertical)
		{
			return;
		}
		ScrollViewCalcs scrollViewCalcs = GetScrollViewCalcs(rectTransform, reuse);
		if (scrollViewCalcs.scrollHeight < scrollViewCalcs.contentHeight)
		{
			if (scrollViewCalcs.isAnyAboveView)
			{
				componentInParent.verticalNormalizedPosition = 1f - scrollViewCalcs.contentTop / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
			else if (scrollViewCalcs.isAnyBelowView)
			{
				componentInParent.verticalNormalizedPosition = 1f - (scrollViewCalcs.contentBottom - scrollViewCalcs.scrollHeight) / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
		}
	}

	public static void ScrollToTopOfRect(RectTransform rectTransform, ScrollViewCalcs reuse = null)
	{
		ScrollViewCalcs scrollViewCalcs = GetScrollViewCalcs(rectTransform, reuse);
		if (scrollViewCalcs.scrollHeight < scrollViewCalcs.contentHeight)
		{
			scrollViewCalcs.scroller.verticalNormalizedPosition = 1f - scrollViewCalcs.contentTop / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
		}
	}

	public static void ScrollToBottomOfRect(RectTransform rectTransform, ScrollViewCalcs reuse = null)
	{
		ScrollRect componentInParent = rectTransform.GetComponentInParent<ScrollRect>();
		if (componentInParent != null && componentInParent.vertical)
		{
			ScrollViewCalcs scrollViewCalcs = GetScrollViewCalcs(rectTransform, reuse);
			if (scrollViewCalcs.scrollHeight < scrollViewCalcs.contentHeight)
			{
				componentInParent.verticalNormalizedPosition = 1f - (scrollViewCalcs.contentBottom - scrollViewCalcs.scrollHeight) / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
		}
	}
}
