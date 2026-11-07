using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XRL.CharacterBuilds;

namespace XRL.UI.Framework;

public class CategoryMenusScroller : FrameworkScroller
{
	public UITextSkin selectedTitleText;

	public UIThreeColorProperties selectedIcon;

	public UITextSkin selectedDescriptionText;

	public RectTransform safeArea;

	public RectTransform OuterVLayout;

	public RectTransform DescriptionHLayout;

	public RectTransform DescriptionArea;

	public float lastSafeAreaWidth;

	public int descriptionBottomBreakpoint;

	public bool allowVerticalLayout = true;

	public LayoutElement iconLayoutElement;

	public UITextSkin categoryScrollerText;

	public bool hasShown;

	public bool breakDescription
	{
		get
		{
			if (allowVerticalLayout)
			{
				return lastSafeAreaWidth < (float)descriptionBottomBreakpoint;
			}
			return false;
		}
	}

	public void UpdateDescriptions(FrameworkDataElement dataElement)
	{
		if (dataElement is PrefixMenuOption prefixMenuOption)
		{
			selectedTitleText.SetText(prefixMenuOption.Description);
			selectedDescriptionText.SetText(prefixMenuOption.LongDescription);
			selectedIcon.FromRenderable(prefixMenuOption.Renderable);
		}
	}

	public NavigationContext ContextFor(int index, int subMenuIndex)
	{
		return GetPrefabForIndex(index).GetComponent<FrameworkScroller>().scrollContext.GetContextAt(subMenuIndex);
	}

	public override void ScrollSelectedIntoView()
	{
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections = null)
	{
		base.BeforeShow(descriptor, selections);
		if (!hasShown)
		{
			try
			{
				UpdateDescriptions((scrollContext.GetDataAt(0) as CategoryMenuData).menuOptions[0]);
				hasShown = true;
			}
			catch
			{
			}
		}
		onHighlight.RemoveAllListeners();
		onHighlight.AddListener(UpdateDescriptions);
	}

	public override ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		return new ScrollChildContext
		{
			proxyTo = GetPrefabForIndex(index).GetComponent<FrameworkScroller>().scrollContext
		};
	}

	public override void SetupPrefab(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
	{
		base.SetupPrefab(newChild, context, data, index);
		FrameworkScroller component = newChild.GetComponent<FrameworkScroller>();
		component.onSelected = onSelected;
		component.onHighlight = onHighlight;
	}

	public override void Update()
	{
		base.Update();
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			safeArea.anchoredPosition = new Vector2(0f, 0f);
			safeArea.sizeDelta = new Vector2(0f, -150f);
			centeringFrame.GetComponent<VerticalLayoutGroup>().padding.bottom = 25;
		}
		else
		{
			safeArea.anchoredPosition = new Vector2(0f, -25f);
			safeArea.sizeDelta = new Vector2(-300f, -250f);
			centeringFrame.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
		}
		if (safeArea.rect.width == lastSafeAreaWidth)
		{
			return;
		}
		if (iconLayoutElement != null)
		{
			if (Media.sizeClass < Media.SizeClass.Medium)
			{
				iconLayoutElement.minWidth = 40f;
				iconLayoutElement.minHeight = 60f;
			}
			else
			{
				iconLayoutElement.minWidth = 80f;
				iconLayoutElement.minHeight = 120f;
			}
		}
		lastSafeAreaWidth = (int)Math.Floor(safeArea.rect.width);
		if (breakDescription)
		{
			DescriptionArea.SetParent(OuterVLayout, worldPositionStays: false);
			DescriptionArea.SetSiblingIndex(DescriptionHLayout.GetSiblingIndex() + 1);
		}
		else
		{
			DescriptionArea.SetParent(DescriptionHLayout, worldPositionStays: false);
			DescriptionArea.SetSiblingIndex(2);
		}
	}
}
