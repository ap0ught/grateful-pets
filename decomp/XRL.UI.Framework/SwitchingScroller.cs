using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class SwitchingScroller : HorizontalScrollerScroller
{
	public enum SwitchDirection
	{
		None,
		Vertical,
		Horizontal
	}

	public GameObject switchingContainer;

	public SwitchDirection currentDirection;

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (!(switchingContainer != null))
		{
			return;
		}
		SwitchDirection switchDirection = SwitchDirection.Vertical;
		float num = 0f;
		foreach (Transform item in switchingContainer.transform)
		{
			num += item.GetComponent<RectTransform>().rect.width;
		}
		scrollRect.GetComponent<LayoutElement>().preferredHeight = childRoot.GetComponent<RectTransform>().rect.height + 50f;
		if (num < switchingContainer.GetComponent<RectTransform>().rect.width)
		{
			switchDirection = SwitchDirection.Horizontal;
		}
		if (switchDirection != currentDirection)
		{
			switchTo(switchDirection);
		}
	}

	public void switchTo(SwitchDirection direction)
	{
		currentDirection = direction;
		if (!(switchingContainer == null))
		{
			switchingContainer.GetComponent<VerticalLayoutGroup>()?.DestroyImmediate();
			switchingContainer.GetComponent<HorizontalLayoutGroup>()?.DestroyImmediate();
			switch (direction)
			{
			case SwitchDirection.Vertical:
			{
				NavigationAxis = InputAxisTypes.NavigationYAxis;
				scrollContext.SetAxis(InputAxisTypes.NavigationYAxis);
				VerticalLayoutGroup verticalLayoutGroup = switchingContainer.AddComponent<VerticalLayoutGroup>();
				verticalLayoutGroup.padding.left = 10;
				verticalLayoutGroup.childForceExpandHeight = false;
				verticalLayoutGroup.childForceExpandWidth = false;
				verticalLayoutGroup.childControlWidth = false;
				verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
				break;
			}
			case SwitchDirection.Horizontal:
			{
				NavigationAxis = InputAxisTypes.NavigationXAxis;
				scrollContext.SetAxis(InputAxisTypes.NavigationXAxis);
				HorizontalLayoutGroup horizontalLayoutGroup = switchingContainer.AddComponent<HorizontalLayoutGroup>();
				horizontalLayoutGroup.padding.left = 0;
				horizontalLayoutGroup.childForceExpandHeight = false;
				horizontalLayoutGroup.childForceExpandWidth = false;
				horizontalLayoutGroup.childControlWidth = false;
				horizontalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
				break;
			}
			}
		}
	}

	public override void SetupPrefab(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
	{
		base.SetupPrefab(newChild, context, data, index);
		FrameworkScroller component = newChild.GetComponent<FrameworkScroller>();
		if (component != null)
		{
			component.onSelected = onSelected;
			component.onHighlight = onHighlight;
		}
	}
}
