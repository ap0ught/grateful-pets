using System;
using UnityEngine;

namespace XRL.UI.Framework;

/// <summary>
///   Data Structure for various UI elements to use.
/// </summary>
[Serializable]
public class SummaryBlockData : FrameworkDataElement
{
	public string Title;

	public string IconPath;

	public Color IconForegroundColor;

	public Color IconDetailColor;

	public int SortOrder;
}
