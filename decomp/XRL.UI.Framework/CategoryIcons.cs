using System.Collections.Generic;
using UnityEngine;

namespace XRL.UI.Framework;

public class CategoryIcons : FrameworkDataElement, IFrameworkDataList
{
	public string Title;

	public Color TitleColor;

	public List<ChoiceWithColorIcon> Choices;

	public IEnumerable<FrameworkDataElement> getChildren()
	{
		return Choices;
	}
}
