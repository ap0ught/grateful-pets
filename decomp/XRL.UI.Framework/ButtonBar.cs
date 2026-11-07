using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine.Events;

namespace XRL.UI.Framework;

public class ButtonBar : HorizontalScroller
{
	public class ButtonBarButtonData : PooledFrameworkDataElement<ButtonBarButtonData>
	{
		public enum HighlightState
		{
			Invalid = -1,
			Normal,
			Highlighted,
			NotHighlighted
		}

		public int n;

		public ButtonBarButton button;

		public IRenderable icon;

		public string label;

		public Action onSelect;

		public HighlightState Highlighted;

		public ButtonBarButtonData set(string label, IRenderable icon = null, HighlightState highlighted = HighlightState.Normal, Action onSelect = null)
		{
			this.label = label;
			this.icon = icon;
			this.onSelect = onSelect;
			Highlighted = highlighted;
			return this;
		}

		public override void free()
		{
			button = null;
			onSelect = null;
			label = null;
			button = null;
			icon = null;
			n = 0;
			Highlighted = HighlightState.Normal;
		}
	}

	public List<ButtonBarButtonData> categoryButtons;

	public UnityEvent<string> OnSearchTextChange = new UnityEvent<string>();

	public void SetupContext(InputAxisTypes axis = InputAxisTypes.NavigationXAxis)
	{
		scrollContext.SetAxis(axis);
		scrollContext.contexts.Clear();
		scrollContext.wraps = false;
		scrollContext.Setup();
	}

	public void SetButtons(IEnumerable<ButtonBarButtonData> buttons)
	{
		choices.Clear();
		choices.AddRange(buttons);
		for (int i = 0; i < choices.Count; i++)
		{
			((ButtonBarButtonData)choices[i]).n = i;
		}
		lastContexts.Clear();
		lastContexts.AddRange(scrollContext.contexts);
		scrollContext.contexts.Clear();
		LayoutChildren();
		scrollContext.Setup();
	}
}
