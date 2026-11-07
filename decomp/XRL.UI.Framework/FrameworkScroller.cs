using System;
using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL.CharacterBuilds;

namespace XRL.UI.Framework;

public class FrameworkScroller : MonoBehaviour, IFrameworkContext
{
	public interface IIndexSensitiveScrollChild
	{
		void SetIndex(int windowIndex);
	}

	public InputAxisTypes NavigationAxis;

	public ScrollContext<FrameworkDataElement, NavigationContext> scrollContext = new ScrollContext<FrameworkDataElement, NavigationContext>();

	public List<FrameworkDataElement> choices = new List<FrameworkDataElement>();

	public bool EnablePaging;

	public GameObject childRoot;

	public FrameworkUnityScrollChild selectionPrefab;

	public LayoutElement contentFitterLayoutElement;

	public GameObject spacerPrefab;

	public UITextSkin titleText;

	public UnityEvent<FrameworkDataElement> onSelected = new UnityEvent<FrameworkDataElement>();

	public UnityEvent<FrameworkDataElement> onHighlight = new UnityEvent<FrameworkDataElement>();

	public int selectedPosition;

	public bool autoHotkey;

	public Dictionary<KeyCode, FrameworkDataElement> hotkeyChoices = new Dictionary<KeyCode, FrameworkDataElement>();

	public ScrollRect scrollRect;

	private bool _gridLayoutInitialized;

	private GridLayoutGroup _gridLayout;

	public List<FrameworkUnityScrollChild> selectionClones = new List<FrameworkUnityScrollChild>();

	public List<GameObject> spacerClones = new List<GameObject>();

	protected List<NavigationContext> lastContexts = new List<NavigationContext>();

	public bool putSpacersTopAndBottom;

	public bool basePrefabPoolingAllowed;

	private Queue<GameObject> prefabPool = new Queue<GameObject>();

	protected ScrollViewCalcs scrollCalcs;

	public UnityEvent<FrameworkUnityScrollChild, ScrollChildContext, FrameworkDataElement, int> PostSetup = new UnityEvent<FrameworkUnityScrollChild, ScrollChildContext, FrameworkDataElement, int>();

	public float lastWidth;

	private RectTransform _rtWidth;

	public bool wasActive = true;

	private bool FirstLateUpdate = true;

	private bool _DisableForcedUpdatesOnLayout;

	public bool centeringEnabled = true;

	public RectTransform centeringFreeSpace;

	public RectTransform centeringFrame;

	public RectTransform centeringLowestObject;

	public float lastHeight = float.NaN;

	private LayoutElement _layout;

	private RectTransform _contentFitterLayoutElementRectTransform;

	protected ScrollViewCalcs scrollViewCalcs = new ScrollViewCalcs();

	public Func<bool> ScrollOnSelection = () => true;

	public GridLayoutGroup gridLayout
	{
		get
		{
			if (!_gridLayoutInitialized)
			{
				_gridLayout = childRoot.GetComponent<GridLayoutGroup>();
				_gridLayoutInitialized = true;
			}
			return _gridLayout;
		}
	}

	public InputAxisTypes PageAxis
	{
		get
		{
			if (NavigationAxis == InputAxisTypes.NavigationXAxis)
			{
				return InputAxisTypes.NavigationPageXAxis;
			}
			if (NavigationAxis == InputAxisTypes.NavigationYAxis)
			{
				return InputAxisTypes.NavigationPageYAxis;
			}
			throw new Exception("Unhandled page axis");
		}
	}

	public virtual bool allowPrefabPoolingDuringLayoutChildren => basePrefabPoolingAllowed;

	private RectTransform rtWidth => _rtWidth ?? (_rtWidth = scrollRect?.transform.parent?.GetComponent<RectTransform>());

	private LayoutElement layout => _layout ?? (_layout = scrollRect.GetComponent<LayoutElement>());

	private RectTransform contentFitterLayoutElementRectTransform => _contentFitterLayoutElementRectTransform ?? (_contentFitterLayoutElementRectTransform = contentFitterLayoutElement.GetComponent<RectTransform>());

	public virtual NavigationContext GetNavigationContext()
	{
		return scrollContext;
	}

	public void Start()
	{
		selectedPosition = 0;
		if (scrollContext != null)
		{
			scrollContext.data = choices;
		}
	}

	public virtual FrameworkUnityScrollChild GetPrefabForIndex(int x)
	{
		if (selectionPrefab == null)
		{
			MetricsManager.LogError("selectionPrefab is null!");
			return null;
		}
		while (selectionClones.Count <= x)
		{
			if (allowPrefabPoolingDuringLayoutChildren && prefabPool.Count > 0)
			{
				FrameworkUnityScrollChild frameworkUnityScrollChild = prefabPool.Dequeue()?.GetComponent<FrameworkUnityScrollChild>();
				if (frameworkUnityScrollChild != null)
				{
					frameworkUnityScrollChild.gameObject?.SetActive(value: true);
					frameworkUnityScrollChild.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
					selectionClones.Add(frameworkUnityScrollChild);
				}
				else
				{
					selectionClones.Add(UnityEngine.Object.Instantiate(selectionPrefab));
				}
			}
			else
			{
				selectionClones.Add(UnityEngine.Object.Instantiate(selectionPrefab));
			}
		}
		return selectionClones[x];
	}

	public virtual void LayoutChildren()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in childRoot.transform)
		{
			if (!selectionClones.Contains(item.GetComponent<FrameworkUnityScrollChild>()) && !spacerClones.Contains(item.gameObject))
			{
				list.Add(item.gameObject);
			}
		}
		if (selectionClones.Count > choices.Count)
		{
			list.AddRange(from clone in selectionClones.Skip(choices.Count)
				select clone.gameObject);
			selectionClones.RemoveRange(choices.Count, selectionClones.Count - choices.Count);
			if (spacerPrefab != null)
			{
				List<GameObject> list2 = spacerClones.Skip(choices.Count + (putSpacersTopAndBottom ? 1 : (-1))).ToList();
				list.AddRange(list2);
				foreach (GameObject item2 in list2)
				{
					spacerClones.Remove(item2);
				}
			}
		}
		list.ForEach((GameObject child) =>
		{
			try
			{
				if (allowPrefabPoolingDuringLayoutChildren)
				{
					child.SetActive(value: false);
					child.transform.SetParent(null);
					prefabPool.Enqueue(child);
				}
				else
				{
					child.DestroyImmediate();
				}
			}
			catch
			{
			}
		});
		foreach (FrameworkDataElement choice in choices)
		{
			int count = scrollContext.contexts.Count;
			int num = (putSpacersTopAndBottom ? (-1) : 0);
			if (count > num && spacerPrefab != null && spacerClones.Count < count - num)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(spacerPrefab);
				gameObject.transform.SetParent(childRoot.transform, worldPositionStays: false);
				spacerClones.Add(gameObject);
			}
			ScrollChildContext scrollChildContext = MakeContextFor(choice, count);
			scrollContext.contexts.Add(scrollChildContext);
			NavigationContext navigationContext = lastContexts.ElementAtOrDefault(count);
			if (navigationContext != null && navigationContext.IsActive())
			{
				scrollChildContext.Activate();
			}
			FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(count);
			if (prefabForIndex.gameObject.GetComponent<ControlId>() == null)
			{
				prefabForIndex.gameObject.AddComponent<ControlId>();
			}
			prefabForIndex.gameObject.GetComponent<ControlId>().id = choice.Id;
			prefabForIndex.transform.SetParent(childRoot.transform, worldPositionStays: false);
			SetupPrefab(prefabForIndex, scrollChildContext, choice, count);
			scrollChildContext.Setup();
		}
		if (putSpacersTopAndBottom && spacerClones.Count < choices.Count + 1)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(spacerPrefab);
			gameObject2.transform.SetParent(childRoot.transform, worldPositionStays: false);
			spacerClones.Add(gameObject2);
		}
	}

	public virtual void ChangePrefabs(FrameworkUnityScrollChild newSelectionPrefab, GameObject newSpacerPrefab = null)
	{
		if (newSelectionPrefab == null)
		{
			MetricsManager.LogError("changing to a null selection prefab!");
		}
		selectionPrefab = newSelectionPrefab;
		spacerPrefab = newSpacerPrefab;
		InvalidatePrefabs();
	}

	protected virtual void InvalidatePrefabs()
	{
		foreach (Transform item in selectionClones.Select((FrameworkUnityScrollChild s) => s.transform).Concat(spacerClones.Select((GameObject s) => s.transform)))
		{
			UnityEngine.Object.Destroy(item);
		}
		selectionClones.Clear();
		spacerClones.Clear();
	}

	public virtual void UpdateSelections(IEnumerable<FrameworkDataElement> selections = null)
	{
		if (selections == null)
		{
			selections = choices;
		}
		choices?.Clear();
		choices?.AddRange(selections);
		scrollContext.data = choices;
		if (selectedPosition >= selections.Count() && selections.Count() > 0)
		{
			scrollContext.SelectIndex(selections.Count() - 1);
		}
		if (autoHotkey && CapabilityManager.AllowKeyboardHotkeys)
		{
			List<KeyCode>.Enumerator enumerator = ControlManager.GetHotkeySpread(new List<string> { "Menus", "UINav", "Chargen" }).GetEnumerator();
			foreach (IFrameworkDataHotkey item in from choice in choices.SelectMany(ChoiceAndChildren)
				select choice as IFrameworkDataHotkey)
			{
				if (item != null)
				{
					if (enumerator.MoveNext())
					{
						KeyCode current2 = enumerator.Current;
						item.Hotkey = "[{{W|" + current2.ToString() + "}}]";
						hotkeyChoices.Add(current2, item as FrameworkDataElement);
					}
					else
					{
						item.Hotkey = "";
					}
				}
			}
		}
		lastContexts.Clear();
		lastContexts.AddRange(scrollContext.contexts);
		scrollContext.contexts.Clear();
		LayoutChildren();
		scrollContext.Setup();
		if (selectedPosition < scrollContext.data.Count && scrollContext.IsActive() && selectedPosition > 0 && scrollContext.data != null && selectedPosition < scrollContext.data.Count)
		{
			onHighlight?.Invoke(scrollContext.data[selectedPosition]);
		}
	}

	public void BeforeShow(IEnumerable<FrameworkDataElement> selections = null)
	{
		BeforeShow(null, selections);
	}

	public virtual void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections = null)
	{
		lastHeight = -1f;
		hotkeyChoices.Clear();
		if (titleText != null && descriptor != null)
		{
			titleText.SetText(descriptor.title);
		}
		scrollContext.wraps = false;
		scrollContext.SetAxis(NavigationAxis);
		if (EnablePaging)
		{
			scrollContext.axisHandlers.Set(PageAxis, Event.Helpers.Handle(Event.Helpers.Axis(DoPageDown, DoPageUp)));
		}
		UpdateSelections(selections);
	}

	public virtual void SelectHighest()
	{
		if (selectionClones.Count > 0)
		{
			int index = selectionClones.IndexOf(selectionClones.FindAll((FrameworkUnityScrollChild c) => ScrollViewCalcs.GetScrollViewCalcs(c.GetComponent<RectTransform>(), scrollCalcs).isAnyInView).First());
			scrollContext.SelectIndex(index);
			CategoryMenuController categoryMenuController = selectionClones[index].FrameworkControl as CategoryMenuController;
			if (categoryMenuController != null && categoryMenuController.scroller != null)
			{
				categoryMenuController.scroller.SelectHighest();
			}
		}
	}

	public virtual void SelectLowest()
	{
		if (selectionClones.Count > 0)
		{
			int index = selectionClones.IndexOf(selectionClones.FindAll((FrameworkUnityScrollChild c) => ScrollViewCalcs.GetScrollViewCalcs(c.GetComponent<RectTransform>(), scrollCalcs).isAnyInView).Last());
			scrollContext.SelectIndex(index);
			CategoryMenuController categoryMenuController = selectionClones[index].FrameworkControl as CategoryMenuController;
			if (categoryMenuController != null && categoryMenuController.scroller != null)
			{
				categoryMenuController.scroller.SelectLowest();
			}
		}
	}

	public virtual void DoScrollUp()
	{
		scrollRect.verticalNormalizedPosition += scrollRect.viewport.rect.height / scrollRect.content.rect.height / 5f;
		SelectHighest();
	}

	public virtual void DoScrollDown()
	{
		scrollRect.verticalNormalizedPosition -= scrollRect.viewport.rect.height / scrollRect.content.rect.height / 5f;
		SelectLowest();
	}

	public virtual void DoPageUp()
	{
		scrollRect.verticalNormalizedPosition += scrollRect.viewport.rect.height / scrollRect.content.rect.height;
		SelectHighest();
	}

	public virtual void DoPageDown()
	{
		scrollRect.verticalNormalizedPosition -= scrollRect.viewport.rect.height / scrollRect.content.rect.height;
		SelectLowest();
	}

	public virtual void RefreshDatas()
	{
		for (int i = 0; i < choices.Count; i++)
		{
			FrameworkDataElement data = choices[i];
			FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(i);
			ScrollChildContext context = scrollContext.contexts[i] as ScrollChildContext;
			SetupPrefab(prefabForIndex, context, data, i);
		}
	}

	public virtual void SetupPrefab(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
	{
		newChild.Setup(data, context);
		PostSetup.Invoke(newChild, context, data, index);
	}

	public virtual ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		ScrollChildContext context = new ScrollChildContext
		{
			index = index
		};
		context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
		{
			InputButtonTypes.AcceptButton,
			Event.Helpers.Handle(() =>
			{
				if (TutorialManager.AllowOnSelected(choices[context.index]))
				{
					FrameworkDataElement frameworkDataElement = choices.ElementAtOrDefault(context.index);
					if (frameworkDataElement == null)
					{
						MetricsManager.LogError($"Scroll Child Context Activation handler can't find choices[{index}]");
					}
					else
					{
						onSelected?.Invoke(frameworkDataElement);
					}
				}
			})
		} };
		return context;
	}

	public virtual void UpdateSelection()
	{
		ScrollSelectedIntoView();
		if (selectedPosition < scrollContext.data.Count)
		{
			onHighlight?.Invoke(scrollContext.data[selectedPosition]);
		}
	}

	public virtual void UpdateWidth()
	{
	}

	public void DisableFirstLateUpdate()
	{
		FirstLateUpdate = false;
	}

	public void DisableForcedUpdatesOnLayout()
	{
		_DisableForcedUpdatesOnLayout = true;
	}

	public virtual void LateUpdate()
	{
		if (FirstLateUpdate)
		{
			if (!_DisableForcedUpdatesOnLayout)
			{
				Canvas.ForceUpdateCanvases();
			}
			UpdateLayout();
			if (!_DisableForcedUpdatesOnLayout)
			{
				Canvas.ForceUpdateCanvases();
			}
			FirstLateUpdate = false;
		}
	}

	private void UpdateLayout()
	{
		scrollContext.IsActive();
		if (rtWidth != null && rtWidth?.rect.width != lastWidth)
		{
			lastWidth = rtWidth?.rect.width ?? 0f;
			UpdateWidth();
		}
		if (scrollRect != null && gridLayout != null)
		{
			if (layout.preferredHeight != gridLayout.preferredHeight + 10f)
			{
				layout.preferredHeight = gridLayout.preferredHeight + 10f + (float)(autoHotkey ? 16 : 0);
			}
		}
		else if (scrollRect != null && contentFitterLayoutElement != null)
		{
			float num = contentFitterLayoutElementRectTransform.rect.height + 10f + (float)(autoHotkey ? 16 : 0);
			float num2 = contentFitterLayoutElementRectTransform.rect.width + 10f;
			if (layout.preferredHeight != num)
			{
				layout.preferredHeight = num;
			}
			if (layout.preferredWidth != num2)
			{
				layout.preferredWidth = num2;
			}
		}
		UpdateCentering();
	}

	public virtual void Update()
	{
		bool flag = scrollContext.IsActive();
		if (flag != wasActive || (flag && scrollContext.selectedPosition != selectedPosition))
		{
			wasActive = flag;
			selectedPosition = scrollContext.selectedPosition;
			if (flag)
			{
				UpdateSelection();
			}
		}
		UpdateLayout();
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<KeyCode, FrameworkDataElement> hotkeyChoice in hotkeyChoices)
		{
			if (ControlManager.isKeyDown(hotkeyChoice.Key))
			{
				if (The.CurrentContext == The.UiContext)
				{
					ControlManager.ResetInput();
				}
				onSelected?.Invoke(hotkeyChoice.Value);
				break;
			}
		}
	}

	public virtual void Awake()
	{
	}

	public void UpdateCentering()
	{
		if (centeringEnabled && centeringFreeSpace != null && centeringFrame != null && centeringFreeSpace.rect.y != lastHeight)
		{
			if (!_DisableForcedUpdatesOnLayout)
			{
				Canvas.ForceUpdateCanvases();
			}
			lastHeight = centeringFreeSpace.rect.y;
			float num = Math.Min(centeringFreeSpace.rect.height, Math.Max(0f, centeringFrame.rect.height / 2f - 140f));
			centeringFrame.anchoredPosition = new Vector2(centeringFrame.anchoredPosition.x, 0f - num);
			if (!_DisableForcedUpdatesOnLayout)
			{
				Canvas.ForceUpdateCanvases();
			}
			TutorialManager.RefreshHighlightPosition();
		}
	}

	public virtual void ScrollSelectedIntoView()
	{
		if (ScrollOnSelection())
		{
			ScrollViewCalcs.ScrollIntoView(GetPrefabForIndex(selectedPosition).GetComponent<RectTransform>(), scrollViewCalcs);
		}
	}

	public virtual void ScrollSelectedToTop()
	{
		ScrollViewCalcs.ScrollToTopOfRect(GetPrefabForIndex(selectedPosition).GetComponent<RectTransform>(), scrollViewCalcs);
	}

	public IEnumerable<FrameworkDataElement> ChoiceAndChildren(FrameworkDataElement choice)
	{
		yield return choice;
		IEnumerable<FrameworkDataElement> enumerable = (choice as IFrameworkDataList)?.getChildren().SelectMany(ChoiceAndChildren);
		if (enumerable == null)
		{
			yield break;
		}
		foreach (FrameworkDataElement item in enumerable)
		{
			yield return item;
		}
	}
}
