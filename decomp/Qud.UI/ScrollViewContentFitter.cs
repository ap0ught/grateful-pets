using System;
using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

[ExecuteAlways]
[RequireComponent(typeof(ScrollRect))]
public class ScrollViewContentFitter : LayoutElement
{
	public int MaxHeight = 600;

	public int MaxWidth = 800;

	private ScrollRect _sr;

	public void Update()
	{
		_sr = GetComponent<ScrollRect>();
		preferredWidth = Math.Min(MaxWidth, _sr.content.rect.width + _sr.verticalScrollbarSpacing);
		preferredHeight = Math.Min(MaxHeight, _sr.content.rect.height) + _sr.horizontalScrollbarSpacing;
	}
}
