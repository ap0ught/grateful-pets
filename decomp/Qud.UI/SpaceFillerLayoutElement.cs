using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

[ExecuteAlways]
public class SpaceFillerLayoutElement : MonoBehaviour, ILayoutElement
{
	private float _preferredWidth;

	public RectTransform leftSideMirrorTarget;

	public float minWidth => -1f;

	public float preferredWidth => _preferredWidth;

	public float flexibleWidth => -1f;

	public float minHeight => -1f;

	public float preferredHeight => -1f;

	public float flexibleHeight => -1f;

	public int layoutPriority => 10;

	private IEnumerable<RectTransform> OtherSiblings()
	{
		int count = base.gameObject.transform.parent.childCount;
		for (int index = 0; index < count; index++)
		{
			GameObject gameObject = base.gameObject.transform.parent.GetChild(index).gameObject;
			if (!(gameObject == leftSideMirrorTarget.gameObject) && !(gameObject == base.gameObject))
			{
				yield return gameObject.transform as RectTransform;
			}
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
		float num = LayoutUtility.GetPreferredWidth(leftSideMirrorTarget);
		RectTransform rectTransform = gameObject.transform.parent as RectTransform;
		float num2 = OtherSiblings().Sum((RectTransform s) =>
		{
			float num3 = LayoutUtility.GetMinWidth(s);
			return (num3 > 0f) ? num3 : LayoutUtility.GetPreferredWidth(s);
		});
		_preferredWidth = Math.Max(0f, Math.Min(num, rectTransform.rect.width - num2 - num));
	}

	public void CalculateLayoutInputVertical()
	{
	}
}
