using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class VisibleWindowScroller : FrameworkScroller
{
	public LayoutElement TopPadder;

	public LayoutElement BottomPadder;

	public int ItemsRendered = 10;

	public Dictionary<int, float> HeightMemory = new Dictionary<int, float>();

	public bool StaticHeight;

	public float AverageHeight = 20f;

	public float MinimumHeight = 20f;

	public float lastScrollPosition;

	public int topRenderedIndex;

	public Dictionary<int, int> LastPrefabIndex = new Dictionary<int, int>();

	private float? _spacerHeight;

	private float? _spacing;

	private bool prefabNullWarned;

	public ScrollChildContext OffscreenSelectedContext;

	private int lastItemsAbove = -1;

	private Dictionary<int, RectTransform> HeightsToCheck = new Dictionary<int, RectTransform>();

	private List<GameObject> childrenToDelete = new List<GameObject>();

	public float spacerHeight
	{
		get
		{
			float valueOrDefault = _spacerHeight.GetValueOrDefault();
			if (!_spacerHeight.HasValue)
			{
				valueOrDefault = ((spacerPrefab == null) ? 0f : spacerPrefab.GetComponent<RectTransform>().rect.height);
				_spacerHeight = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public float spacing => _spacing ?? childRoot.GetComponent<VerticalLayoutGroup>().spacing;

	public VisibleWindowScroller()
	{
		scrollContext = new CyclicScrollContext<FrameworkDataElement, NavigationContext>
		{
			preSelect = ScrollIndexIntoView
		};
	}

	public float HeightOfItem(int index)
	{
		if (HeightMemory.TryGetValue(index, out var value))
		{
			return value;
		}
		return AverageHeight;
	}

	public int ItemsFullyAboveScrollTop()
	{
		int num = 0;
		float num2 = 0f;
		while (num2 < lastScrollPosition && num < choices.Count)
		{
			num2 += spacing * 2f + spacerHeight + HeightOfItem(num);
			if (num2 >= lastScrollPosition)
			{
				return num;
			}
			num++;
		}
		return num;
	}

	public float HeightOfFirstNItems(int n)
	{
		float num = (spacing * 2f + spacerHeight) * (float)n;
		for (int i = 0; i < n; i++)
		{
			num += HeightOfItem(i);
		}
		return num;
	}

	public float HeightOfItemsStartingAtIndexN(int n)
	{
		float num = (spacing * 2f + spacerHeight) * (float)(choices.Count - 1 - n);
		for (int i = n; i < choices.Count; i++)
		{
			num += HeightOfItem(i);
		}
		return num;
	}

	public float ItemsInView()
	{
		return Math.Max(1f, scrollRect.viewport.rect.height / (MinimumHeight + spacerHeight + spacing * 2f));
	}

	public override void Update()
	{
		if (!prefabNullWarned && selectionPrefab == null)
		{
			prefabNullWarned = true;
			MetricsManager.LogError(gameObject.name + " -> selectionPrefab became null!");
		}
		base.Update();
	}

	public override void Awake()
	{
		base.Awake();
		scrollRect.onValueChanged.AddListener(OnScrollChange);
	}

	public virtual void OnScrollChange(Vector2 value)
	{
		lastScrollPosition = (1f - value.y) * (scrollRect.content.rect.height - scrollRect.viewport.rect.height);
		ReflowScroller();
	}

	public override void SelectHighest()
	{
		if (selectionClones.Count > 0)
		{
			int index = (from a in selectionClones.FindAll((FrameworkUnityScrollChild c) => c.enabled && ScrollViewCalcs.GetScrollViewCalcs(c.GetComponent<RectTransform>(), scrollCalcs).isAnyInView)
				orderby (a.transform as RectTransform).GetSiblingIndex()
				select a).First().GetComponent<FrameworkUnityScrollChild>().scrollContext.index;
			scrollContext.SelectIndex(index);
			ScrollIndexIntoView(index);
		}
	}

	public override void SelectLowest()
	{
		if (selectionClones.Count > 0)
		{
			int index = (from a in selectionClones.FindAll((FrameworkUnityScrollChild c) => c.gameObject.activeInHierarchy && ScrollViewCalcs.GetScrollViewCalcs(c.GetComponent<RectTransform>(), scrollCalcs).isAnyInView)
				orderby (a.transform as RectTransform).GetSiblingIndex()
				select a).Last().GetComponent<FrameworkUnityScrollChild>().scrollContext.index;
			scrollContext.SelectIndex(index);
			ScrollIndexIntoView(index);
		}
	}

	public override void DoPageUp()
	{
		scrollRect.verticalNormalizedPosition += (scrollRect.viewport.rect.height - AverageHeight) / scrollRect.content.rect.height;
		scrollRect.verticalNormalizedPosition = Math.Clamp(scrollRect.verticalNormalizedPosition, 0f, 1f);
		OnScrollChange(scrollRect.normalizedPosition);
		SelectHighest();
	}

	public override void DoPageDown()
	{
		scrollRect.verticalNormalizedPosition -= (scrollRect.viewport.rect.height - AverageHeight) / scrollRect.content.rect.height;
		scrollRect.verticalNormalizedPosition = Math.Clamp(scrollRect.verticalNormalizedPosition, 0f, 1f);
		OnScrollChange(scrollRect.normalizedPosition);
		SelectLowest();
	}

	protected override void InvalidatePrefabs()
	{
		base.InvalidatePrefabs();
		scrollContext.contexts.Clear();
		LastPrefabIndex.Clear();
	}

	public void ReflowScroller(bool force = false)
	{
		CyclicScrollContext<FrameworkDataElement, NavigationContext> cyclicScrollContext = scrollContext as CyclicScrollContext<FrameworkDataElement, NavigationContext>;
		if (cyclicScrollContext != null)
		{
			cyclicScrollContext.intentional = true;
		}
		if (OffscreenSelectedContext != null && !OffscreenSelectedContext.IsActive())
		{
			OffscreenSelectedContext = null;
		}
		int num = ItemsFullyAboveScrollTop();
		int val = Math.Max(0, choices.Count - ItemsRendered);
		int val2 = Math.Max(0, num - (int)(((double)ItemsRendered - Math.Ceiling(ItemsInView())) / 2.0));
		if (force || Math.Min(val2, val) != topRenderedIndex)
		{
			topRenderedIndex = Math.Min(val2, val);
			TopPadder.minHeight = HeightOfFirstNItems(topRenderedIndex);
			BottomPadder.minHeight = HeightOfItemsStartingAtIndexN(topRenderedIndex + ItemsRendered);
			int num2 = 1;
			for (int i = 0; i < Math.Min(choices.Count, Math.Min(selectionClones.Count, ItemsRendered)); i++)
			{
				int num3 = topRenderedIndex + i;
				int num4 = num3 % ItemsRendered;
				selectionClones[num4].gameObject.SetActive(value: true);
				selectionClones[num4].transform.SetSiblingIndex(num2++);
				if (spacerPrefab != null)
				{
					spacerClones[num4].gameObject.SetActive(value: true);
					spacerClones[num4].transform.SetSiblingIndex(num2++);
				}
				if (!LastPrefabIndex.ContainsKey(num4) || LastPrefabIndex[num4] != num3)
				{
					ScrollChildContext scrollChildContext = scrollContext.GetContextAt(num3) as ScrollChildContext;
					ScrollChildContext offscreenSelectedContext = OffscreenSelectedContext;
					if (offscreenSelectedContext != null && offscreenSelectedContext.index == num3)
					{
						scrollChildContext = OffscreenSelectedContext;
						scrollContext.contexts[num4] = scrollChildContext;
						OffscreenSelectedContext = null;
					}
					else if (scrollChildContext.index != num3 && scrollChildContext.IsActive())
					{
						ScrollChildContext offscreenSelectedContext2 = scrollChildContext;
						ScrollChildContext offscreenSelectedContext3 = OffscreenSelectedContext;
						if (offscreenSelectedContext3 != null && offscreenSelectedContext3.index == num3)
						{
							scrollChildContext = OffscreenSelectedContext;
						}
						else
						{
							scrollChildContext = MakeContextFor(null, num3);
							scrollChildContext.index = num3;
							scrollChildContext.parentContext = scrollContext;
						}
						OffscreenSelectedContext = offscreenSelectedContext2;
						scrollContext.contexts[num4] = scrollChildContext;
					}
					SetupPrefab(selectionClones[num4], scrollChildContext, scrollContext.GetDataAt(num3), num3);
				}
				if (selectionClones[num4] is IIndexSensitiveScrollChild indexSensitiveScrollChild)
				{
					indexSensitiveScrollChild.SetIndex(num3 - num);
				}
			}
			lastItemsAbove = num;
		}
		if (num != lastItemsAbove)
		{
			for (int j = 0; j < Math.Min(choices.Count, Math.Min(selectionClones.Count, ItemsRendered)); j++)
			{
				int num5 = topRenderedIndex + j;
				int index = num5 % ItemsRendered;
				if (selectionClones[index] is IIndexSensitiveScrollChild indexSensitiveScrollChild2)
				{
					indexSensitiveScrollChild2.SetIndex(num5 - num);
				}
			}
			lastItemsAbove = num;
		}
		if (!StaticHeight && HeightsToCheck.Count > 0)
		{
			LayoutRebuilder.MarkLayoutForRebuild(scrollRect.content.transform as RectTransform);
			foreach (KeyValuePair<int, RectTransform> item in HeightsToCheck)
			{
				float num6 = (HeightMemory[item.Key] = item.Value.rect.height);
			}
			HeightsToCheck.Clear();
			AverageHeight = 0f;
			if (MinimumHeight < 20f)
			{
				MinimumHeight = 20f;
			}
			foreach (float value in HeightMemory.Values)
			{
				AverageHeight += value / (float)HeightMemory.Count;
				MinimumHeight = Math.Min(MinimumHeight, value);
			}
			if (MinimumHeight < 1f)
			{
				MinimumHeight = 1f;
			}
		}
		if (cyclicScrollContext != null)
		{
			cyclicScrollContext.intentional = false;
		}
	}

	public override void SetupPrefab(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
	{
		context.index = index;
		base.SetupPrefab(newChild, context, data, index);
		RectTransform value = newChild.transform as RectTransform;
		HeightsToCheck[index] = value;
		LastPrefabIndex.Set(selectionClones.IndexOf(newChild), index);
		context.Setup();
	}

	public override FrameworkUnityScrollChild GetPrefabForIndex(int x)
	{
		if (selectionPrefab == null)
		{
			MetricsManager.LogError("selectionPrefab was null");
			return null;
		}
		while (selectionClones.Count < x % ItemsRendered + 1)
		{
			selectionClones.Add(UnityEngine.Object.Instantiate(selectionPrefab));
		}
		_ = x % ItemsRendered;
		if (x % ItemsRendered >= selectionClones.Count || x % ItemsRendered < 0)
		{
			return selectionClones.FirstOrDefault();
		}
		return selectionClones[x % ItemsRendered];
	}

	public override void UpdateSelections(IEnumerable<FrameworkDataElement> selections = null)
	{
		base.UpdateSelections(selections);
		ReflowScroller(force: true);
	}

	public override void LayoutChildren()
	{
		_spacerHeight = null;
		LastPrefabIndex.Clear();
		childrenToDelete.Clear();
		for (int i = 0; i < childRoot.transform.childCount; i++)
		{
			Transform child = childRoot.transform.GetChild(i);
			bool flag = false;
			for (int j = 0; j < selectionClones.Count; j++)
			{
				if (selectionClones[j].gameObject == child.gameObject)
				{
					flag = true;
					break;
				}
			}
			if (!flag && !spacerClones.Contains(child.gameObject) && !(TopPadder?.transform == child) && !(BottomPadder?.transform == child))
			{
				childrenToDelete.Add(child.gameObject);
			}
		}
		for (int k = 0; k < childrenToDelete.Count; k++)
		{
			childrenToDelete[k].DestroyImmediate();
		}
		childrenToDelete.Clear();
		if ((object)TopPadder == null)
		{
			TopPadder = new GameObject("Top Padder", typeof(RectTransform), typeof(LayoutElement)).GetComponent<LayoutElement>();
			TopPadder.transform.SetParent(childRoot.transform, worldPositionStays: false);
		}
		HeightMemory.Clear();
		while (true)
		{
			if (lastContexts.Count > 0)
			{
				scrollContext.contexts.AddRange(lastContexts);
				lastContexts.Clear();
			}
			int num = Math.Min(ItemsRendered, choices.Count);
			for (int l = scrollContext.contexts.Count; l < num; l++)
			{
				scrollContext.contexts.Add(MakeContextFor(null, l));
			}
			if (spacerPrefab != null)
			{
				int num2 = Math.Min(ItemsRendered, choices.Count);
				for (int m = spacerClones.Count; m < num2; m++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(spacerPrefab);
					gameObject.transform.SetParent(childRoot.transform, worldPositionStays: false);
					spacerClones.Add(gameObject);
				}
			}
			int num3 = Math.Min(ItemsRendered, choices.Count);
			for (int n = selectionClones.Count; n < num3; n++)
			{
				FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(n);
				prefabForIndex.gameObject.name = $"#{n} {selectionPrefab.gameObject.name}";
				prefabForIndex.transform.SetParent(childRoot.transform, worldPositionStays: false);
			}
			if ((object)BottomPadder == null)
			{
				BottomPadder = new GameObject("Bottom Padder", typeof(RectTransform), typeof(LayoutElement)).GetComponent<LayoutElement>();
				BottomPadder.transform.SetParent(childRoot.transform, worldPositionStays: false);
			}
			ReflowScroller(force: true);
			if (!(ItemsInView() * 1.5f > (float)ItemsRendered))
			{
				break;
			}
			ItemsRendered = (int)Math.Ceiling(ItemsInView() * 2.5f);
		}
		if (selectionClones.Count <= choices.Count)
		{
			return;
		}
		for (int num4 = choices.Count; num4 < selectionClones.Count; num4++)
		{
			selectionClones[num4].gameObject.SetActive(value: false);
			if (spacerClones.Count > num4)
			{
				spacerClones[num4].gameObject.SetActive(value: false);
			}
		}
	}

	public override ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		return base.MakeContextFor(data, index);
	}

	public Vector2 ScrollOffsetForIndex(int index)
	{
		float num = HeightOfFirstNItems(index);
		return new Vector2(0f, 1f - num / (scrollRect.content.rect.height - scrollRect.viewport.rect.height));
	}

	public void ScrollIndexIntoView(int index)
	{
		if (index < topRenderedIndex || index >= topRenderedIndex + ItemsRendered)
		{
			OnScrollChange(ScrollOffsetForIndex(index));
			Canvas.ForceUpdateCanvases();
		}
		FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(index);
		if (prefabForIndex != null)
		{
			ScrollViewCalcs.ScrollIntoView(prefabForIndex.GetComponent<RectTransform>(), scrollViewCalcs);
			OnScrollChange(scrollRect.normalizedPosition);
			ScrollViewCalcs.ScrollIntoView(prefabForIndex.GetComponent<RectTransform>(), scrollViewCalcs);
		}
	}

	public override void ScrollSelectedToTop()
	{
		OnScrollChange(scrollRect.normalizedPosition);
		Canvas.ForceUpdateCanvases();
		base.ScrollSelectedToTop();
	}

	public override void ScrollSelectedIntoView()
	{
		if (ScrollOnSelection())
		{
			ScrollIndexIntoView(selectedPosition);
		}
	}
}
