using System;
using System.Collections.Generic;

namespace XRL.UI.Framework;

/// <summary>
///   Data Structure for various UI elements to use.
/// </summary>
[Serializable]
public class CategoryMenuData : FrameworkDataElement
{
	public string Title;

	public List<PrefixMenuOption> menuOptions;
}
