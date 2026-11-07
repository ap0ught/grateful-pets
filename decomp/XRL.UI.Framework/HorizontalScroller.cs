using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL.CharacterBuilds;

namespace XRL.UI.Framework;

public class HorizontalScroller : FrameworkScroller
{
	public UITextSkin descriptionText;

	public Func<int, int> PreferredRows = (int input) => input;

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections = null)
	{
		if (descriptionText != null)
		{
			descriptionText.GetComponent<LayoutElement>().preferredHeight = 0f;
			descriptionText.GetComponent<LayoutElement>().preferredHeight = CalculateTallestDescription(selections);
		}
		base.BeforeShow(descriptor, selections);
		if (gridLayout != null)
		{
			scrollContext.SetAxis(NavigationAxis, dual: true);
			scrollContext.calculateGridWidth = CalculateGridColumns;
		}
		FastLayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
		int num = Math.Min(scrollContext.length - 1, scrollContext.selectedPosition);
		if (scrollContext.selectedPosition != num && scrollContext.length > 0)
		{
			scrollContext.SelectIndex(num);
		}
		descriptionText?.SetText(scrollContext.data[scrollContext.selectedPosition].Description);
		Update();
	}

	public override void UpdateWidth()
	{
		base.UpdateWidth();
		LayoutElement layoutElement = scrollRect?.GetComponent<LayoutElement>();
		if (layoutElement == null)
		{
			return;
		}
		layoutElement.preferredWidth = 9999f;
		Canvas.ForceUpdateCanvases();
		if (CalculateGridColumns() > 0)
		{
			int num = (int)Math.Ceiling((double)choices.Count / (double)CalculateGridColumns());
			if (num > 1)
			{
				layoutElement.preferredWidth = WidthPerGrid() * (float)PreferredRows((int)Math.Ceiling((double)choices.Count / (double)num)) + 10f;
			}
		}
		Canvas.ForceUpdateCanvases();
	}

	public IEnumerable<FrameworkDataElement> selectionAndChildren(FrameworkDataElement selection)
	{
		yield return selection;
		if (!(selection is IFrameworkDataList frameworkDataList))
		{
			yield break;
		}
		foreach (FrameworkDataElement child in frameworkDataList.getChildren())
		{
			yield return child;
		}
	}

	public virtual int CalculateTallestDescription(IEnumerable<FrameworkDataElement> selections)
	{
		int num = 100;
		foreach (FrameworkDataElement item in selections.SelectMany(selectionAndChildren))
		{
			descriptionText.SetText(item.Description);
			num = Math.Max(num, (int)descriptionText.preferredHeight);
		}
		return num;
	}

	public float WidthPerGrid()
	{
		if (gridLayout == null)
		{
			return 0f;
		}
		if (scrollContext.length < 2)
		{
			return 0f;
		}
		return Math.Abs(gridLayout.transform.GetChild(0).localPosition.x - gridLayout.transform.GetChild(1).localPosition.x);
	}

	public int CalculateGridColumns()
	{
		if (gridLayout == null)
		{
			return 0;
		}
		if (scrollContext.length == 0)
		{
			return 0;
		}
		int num = 0;
		Vector3 vector = gridLayout.transform.GetChild(0)?.localPosition ?? Vector3.zero;
		foreach (Transform item in gridLayout.transform)
		{
			if (Mathf.Approximately(vector.y, item.localPosition.y))
			{
				num++;
				continue;
			}
			break;
		}
		return num;
	}

	public override void UpdateSelection()
	{
		descriptionText?.SetText(scrollContext.data[scrollContext.selectedPosition].Description);
		base.UpdateSelection();
	}
}
