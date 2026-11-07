using System;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
public class SelectableTextMenuItem : ControlledSelectable
{
	public RectTransform progressBar;

	public UnityEngine.UI.Text cursor;

	public UITextSkin item;

	public UIThreeColorProperties iconImage;

	public int maxTextWidth = 78;

	[NonSerialized]
	private string oldItemText;

	public string itemText => (data as QudMenuItem?)?.text;

	public IRenderable getIcon()
	{
		return (data as QudMenuItem?)?.icon;
	}

	public override void Update()
	{
		if (oldItemText != itemText)
		{
			oldItemText = itemText;
			SelectChanged(selected);
		}
		base.Update();
	}

	public override void SelectChanged(bool newState)
	{
		base.SelectChanged(newState);
		IRenderable icon = getIcon();
		iconImage.gameObject.SetActive(icon != null);
		if (icon != null)
		{
			iconImage.FromRenderable(icon);
		}
		if (newState)
		{
			cursor.text = RTF.FormatToRTF("&W>");
			item.useBlockWrap = true;
			item.blockWrap = maxTextWidth;
			item.SetText("{{W|" + itemText + "}}");
		}
		else
		{
			cursor.text = " ";
			item.useBlockWrap = true;
			item.blockWrap = maxTextWidth;
			item.SetText("{{c|" + itemText + "}}");
		}
		if (TutorialManager.currentStep != null)
		{
			ControlId.Assign(gameObject, "QudTextMenuItem:" + (data as QudMenuItem?)?.simpleText);
		}
	}
}
