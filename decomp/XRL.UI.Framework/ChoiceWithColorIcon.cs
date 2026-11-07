using System;
using UnityEngine;

namespace XRL.UI.Framework;

/// <summary>
///   Data Structure for various UI elements to use.
/// </summary>
[Serializable]
public class ChoiceWithColorIcon : FrameworkDataElement, IFrameworkDataHotkey
{
	public string Title;

	public string IconPath;

	public bool HFlip;

	public bool VFlip;

	[NonSerialized]
	public Predicate<ChoiceWithColorIcon> Chosen = (ChoiceWithColorIcon choice) => false;

	public Color IconForegroundColor;

	public Color IconDetailColor;

	public string Hotkey { get; set; }

	public bool IsChosen()
	{
		return Chosen(this);
	}
}
