using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class ButtonBarButton : FrameworkUnityScrollChild, IFrameworkControl, IPointerClickHandler, IEventSystemHandler
{
	public UIThreeColorProperties icon;

	public UITextSkin text;

	public GameObject spacer;

	public StringBuilder SB = new StringBuilder();

	public static List<MenuOption> itemOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Select"
		}
	};

	public Image background;

	public bool wasSelected;

	public ButtonBar.ButtonBarButtonData.HighlightState lastHighlightState = ButtonBar.ButtonBarButtonData.HighlightState.Invalid;

	public bool selectionBackgroundColor = true;

	public bool selectionTextColor;

	public bool selectionIconColor = true;

	public ButtonBar.ButtonBarButtonData data;

	public bool selected => GetNavigationContext()?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void LateUpdate()
	{
		if (selectionBackgroundColor && (selected != wasSelected || (lastHighlightState != (data?.Highlighted ?? ButtonBar.ButtonBarButtonData.HighlightState.Normal) && background != null)))
		{
			Color color;
			if (data != null && data.Highlighted == ButtonBar.ButtonBarButtonData.HighlightState.Highlighted)
			{
				if (selected)
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#FFFFFF", out color);
				}
				else
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#FFFF00", out color);
				}
			}
			else if (data != null && data.Highlighted == ButtonBar.ButtonBarButtonData.HighlightState.NotHighlighted)
			{
				if (selected)
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#FFFFFF", out color);
				}
				else
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#4A757E", out color);
				}
			}
			else if (selected)
			{
				UnityEngine.ColorUtility.TryParseHtmlString("#FFFFFF", out color);
			}
			else
			{
				UnityEngine.ColorUtility.TryParseHtmlString("#4A757E", out color);
			}
			background.color = color;
		}
		if (selectionTextColor && (selected != wasSelected || (lastHighlightState != (data?.Highlighted ?? ButtonBar.ButtonBarButtonData.HighlightState.Normal) && text != null)))
		{
			Color color2;
			if (data != null && data.Highlighted == ButtonBar.ButtonBarButtonData.HighlightState.Highlighted)
			{
				if (selected)
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#FFFFFF", out color2);
				}
				else
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#DFFFDF", out color2);
				}
			}
			else if (data != null && data.Highlighted == ButtonBar.ButtonBarButtonData.HighlightState.NotHighlighted)
			{
				if (selected)
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#FFFFFF", out color2);
				}
				else
				{
					UnityEngine.ColorUtility.TryParseHtmlString("#4A757E", out color2);
				}
			}
			else if (selected)
			{
				UnityEngine.ColorUtility.TryParseHtmlString("#FFFFFF", out color2);
			}
			else
			{
				UnityEngine.ColorUtility.TryParseHtmlString("#4A757E", out color2);
			}
			text.color = color2;
			text.Apply();
		}
		if (selectionIconColor && (selected != wasSelected || lastHighlightState != (data?.Highlighted ?? ButtonBar.ButtonBarButtonData.HighlightState.Normal)) && icon != null && data != null && data.icon != null && icon != null)
		{
			if (selected || data.Highlighted == ButtonBar.ButtonBarButtonData.HighlightState.Highlighted)
			{
				icon.FromRenderable(data.icon);
			}
			else
			{
				Renderable renderable = new Renderable(data.icon);
				renderable.DetailColor = 'K';
				renderable.ColorString = "&K";
				icon.FromRenderable(renderable);
			}
		}
		wasSelected = selected;
		lastHighlightState = data?.Highlighted ?? ButtonBar.ButtonBarButtonData.HighlightState.Normal;
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is ButtonBar.ButtonBarButtonData buttonBarButtonData))
		{
			return;
		}
		if (spacer != null)
		{
			spacer.SetActive(buttonBarButtonData.n != 0);
		}
		this.data = buttonBarButtonData;
		if (buttonBarButtonData.icon != null)
		{
			if (buttonBarButtonData.Highlighted == ButtonBar.ButtonBarButtonData.HighlightState.Highlighted)
			{
				icon.FromRenderable(buttonBarButtonData.icon);
			}
			else
			{
				Renderable renderable = new Renderable(buttonBarButtonData.icon);
				renderable.DetailColor = 'K';
				renderable.ColorString = "&K";
				icon.FromRenderable(renderable);
			}
		}
		buttonBarButtonData.button = this;
		text.SetText(buttonBarButtonData.label);
	}

	public void HandleSelect()
	{
		data?.onSelect();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (GetNavigationContext().IsActive() && eventData.button == PointerEventData.InputButton.Left)
		{
			HandleSelect();
		}
	}
}
