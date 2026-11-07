using System;
using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL.CharacterBuilds;

namespace XRL.UI.Framework;

public class PaperdollScroller : MonoBehaviour, IFrameworkContext
{
	public class PaperdollScrollerContext<T, U> : ScrollContext<T, U> where U : NavigationContext
	{
		public PaperdollLayoutGroup paperdollLayoutGroup;

		public override void OnEnter()
		{
			NavigationContext navigationContext = (NavigationContext)currentEvent.data["to"];
			Event triggeringEvent = NavigationController.instance.triggeringEvent;
			bool flag = false;
			try
			{
				flag = triggeringEvent?.data["axis"] as InputAxisTypes? == AxisType;
			}
			catch (Exception)
			{
			}
			if (navigationContext == this)
			{
				NavigationContext navigationContext2 = (NavigationContext)currentEvent.data["from"];
				IEnumerable<NavigationContext> enumerable = navigationContext2?.parents.Prepend(navigationContext2).TakeWhile((NavigationContext c) => !parents.Prepend(this).Contains(c));
				AbstractScrollContext abstractScrollContext = ((enumerable != null) ? enumerable.FirstOrDefault((NavigationContext c) => c is AbstractScrollContext) : null) as AbstractScrollContext;
				if (triggeringEvent != null && triggeringEvent.axisValue.HasValue && rowWidth > 0 && abstractScrollContext != null && abstractScrollContext.AxisType == AxisType)
				{
					if (flag)
					{
						if (triggeringEvent != null && triggeringEvent.axisValue > 0)
						{
							int val = (length - 1) / rowWidth;
							SelectIndex(Math.Min(length - 1, rowWidth * Math.Min(val, abstractScrollContext.selectedRow)));
						}
						else if (triggeringEvent != null && triggeringEvent.axisValue < 0)
						{
							int num = Math.Min((length - 1) / rowWidth, abstractScrollContext.selectedRow);
							SelectIndex(Math.Min(length - 1, rowWidth * (num + 1) - 1));
						}
					}
					else if (triggeringEvent != null && triggeringEvent.axisValue > 0)
					{
						int val2 = abstractScrollContext.selectedColumn;
						SelectIndex(Math.Min(length - 1, val2));
					}
					else if (triggeringEvent != null && triggeringEvent.axisValue < 0)
					{
						int num2;
						for (num2 = abstractScrollContext.selectedColumn; num2 + rowWidth < length; num2 += rowWidth)
						{
						}
						SelectIndex(Math.Min(length - 1, num2));
					}
				}
				else if (flag && triggeringEvent != null && triggeringEvent.axisValue == 1)
				{
					SelectIndex(paperdollLayoutGroup.Leftmost());
				}
				else if (flag && triggeringEvent != null && triggeringEvent.axisValue == -1)
				{
					SelectIndex(paperdollLayoutGroup.Rightmost());
				}
				else
				{
					SelectIndex(Math.Min(Math.Max(0, selectedPosition), length - 1));
				}
				currentEvent.Cancel();
			}
			else
			{
				int? num3 = ContextIndex(navigationContext);
				if (num3.HasValue)
				{
					int valueOrDefault = num3.GetValueOrDefault();
					selectedPosition = valueOrDefault;
					UpdateGridSiblings();
				}
			}
			if (enterHandler != null)
			{
				enterHandler();
			}
		}
	}

	public InputAxisTypes NavigationAxis = InputAxisTypes.NavigationYAxis;

	public PaperdollScrollerContext<FrameworkDataElement, NavigationContext> scrollContext = new PaperdollScrollerContext<FrameworkDataElement, NavigationContext>();

	public List<FrameworkDataElement> choices = new List<FrameworkDataElement>();

	public bool EnablePaging;

	public PaperdollLayoutGroup paperdollLayoutGroup;

	public GameObject childRoot;

	public FrameworkUnityScrollChild selectionPrefab;

	public UnityEvent<FrameworkDataElement> onSelected = new UnityEvent<FrameworkDataElement>();

	public UnityEvent<FrameworkDataElement> onHighlight = new UnityEvent<FrameworkDataElement>();

	public int selectedPosition;

	public bool autoHotkey;

	public Dictionary<KeyCode, FrameworkDataElement> hotkeyChoices = new Dictionary<KeyCode, FrameworkDataElement>();

	public ScrollRect scrollRect;

	public List<FrameworkUnityScrollChild> selectionClones = new List<FrameworkUnityScrollChild>();

	protected List<NavigationContext> lastContexts = new List<NavigationContext>();

	private List<PaperdollLayoutElement> paperdollElements = new List<PaperdollLayoutElement>();

	protected ScrollViewCalcs scrollCalcs;

	public UnityEvent<FrameworkUnityScrollChild, ScrollChildContext, FrameworkDataElement, int> PostSetup = new UnityEvent<FrameworkUnityScrollChild, ScrollChildContext, FrameworkDataElement, int>();

	public float lastWidth;

	private RectTransform _rtWidth;

	public bool wasActive = true;

	private bool FirstLateUpdate = true;

	public bool centeringEnabled = true;

	public RectTransform centeringFreeSpace;

	public RectTransform centeringFrame;

	public RectTransform centeringLowestObject;

	public float lastHeight = float.NaN;

	protected ScrollViewCalcs scrollViewCalcs = new ScrollViewCalcs();

	public Func<bool> ScrollOnSelection = () => true;

	public Event currentEvent => NavigationController.currentEvent;

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

	private PaperdollLayoutElement selectedElement => paperdollElements[scrollContext.selectedPosition];

	private RectTransform rtWidth => _rtWidth ?? (_rtWidth = scrollRect?.transform.parent?.GetComponent<RectTransform>());

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
		scrollContext.paperdollLayoutGroup = paperdollLayoutGroup;
	}

	public virtual FrameworkUnityScrollChild GetPrefabForIndex(int x)
	{
		while (selectionClones.Count <= x)
		{
			selectionClones.Add(UnityEngine.Object.Instantiate(selectionPrefab));
		}
		return selectionClones[x];
	}

	public virtual void LayoutChildren()
	{
		paperdollElements.ForEach((PaperdollLayoutElement e) =>
		{
			e.clearConnector();
		});
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in childRoot.transform)
		{
			if (!selectionClones.Contains(item.GetComponent<FrameworkUnityScrollChild>()))
			{
				list.Add(item.gameObject);
			}
		}
		if (selectionClones.Count > choices.Count)
		{
			list.AddRange(from clone in selectionClones.Skip(choices.Count)
				select clone.gameObject);
			selectionClones.RemoveRange(choices.Count, selectionClones.Count - choices.Count);
		}
		list.ForEach((GameObject child) =>
		{
			try
			{
				child.DestroyImmediate();
			}
			catch
			{
			}
		});
		paperdollElements.Clear();
		foreach (FrameworkDataElement choice in choices)
		{
			int count = scrollContext.contexts.Count;
			ScrollChildContext scrollChildContext = MakeContextFor(choice, count);
			scrollContext.contexts.Add(scrollChildContext);
			NavigationContext navigationContext = lastContexts.ElementAtOrDefault(count);
			if (navigationContext != null && navigationContext.IsActive())
			{
				scrollChildContext.Activate();
			}
			FrameworkUnityScrollChild prefabForIndex = GetPrefabForIndex(count);
			prefabForIndex.transform.SetParent(childRoot.transform, worldPositionStays: false);
			SetupPrefab(prefabForIndex, scrollChildContext, choice, count);
			scrollChildContext.Setup();
			PaperdollLayoutElement component = prefabForIndex.GetComponent<PaperdollLayoutElement>();
			component.context = scrollChildContext;
			component.scrollChild = prefabForIndex;
			component.data = choice as EquipmentLineData;
			paperdollElements.Add(component);
		}
	}

	public virtual void ChangePrefabs(FrameworkUnityScrollChild newSelectionPrefab)
	{
		selectionPrefab = newSelectionPrefab;
		InvalidatePrefabs();
	}

	protected virtual void InvalidatePrefabs()
	{
		foreach (Transform item in selectionClones.Select((FrameworkUnityScrollChild s) => s.transform))
		{
			UnityEngine.Object.Destroy(item);
		}
		selectionClones.Clear();
	}

	public virtual void UpdateSelections(IEnumerable<FrameworkDataElement> selections = null)
	{
		if (selections == null)
		{
			selections = choices;
		}
		choices = (scrollContext.data = new List<FrameworkDataElement>(selections));
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
		paperdollLayoutGroup.Setup(paperdollElements);
	}

	public void BeforeShow(IEnumerable<FrameworkDataElement> selections = null)
	{
		BeforeShow(null, selections);
	}

	public virtual void HandleXAxisEvent()
	{
		if (!currentEvent.axisValue.HasValue)
		{
			return;
		}
		if (currentEvent.axisValue > 0)
		{
			int num = paperdollLayoutGroup.RightOf(selectedElement);
			if (num >= 0)
			{
				paperdollElements[num].GetComponent<FrameworkContext>().context.Activate();
				currentEvent.Handle();
			}
		}
		else
		{
			int num2 = paperdollLayoutGroup.LeftOf(selectedElement);
			if (num2 >= 0)
			{
				paperdollElements[num2].GetComponent<FrameworkContext>().context.Activate();
				currentEvent.Handle();
			}
		}
	}

	public virtual void HandleYAxisEvent()
	{
		if (!currentEvent.axisValue.HasValue)
		{
			return;
		}
		if (currentEvent.axisValue < 0)
		{
			int num = paperdollLayoutGroup.AboveOf(selectedElement);
			if (num >= 0)
			{
				paperdollElements[num].GetComponent<FrameworkContext>().context.Activate();
				currentEvent.Handle();
			}
		}
		else
		{
			int num2 = paperdollLayoutGroup.BelowOf(selectedElement);
			if (num2 >= 0)
			{
				paperdollElements[num2].GetComponent<FrameworkContext>().context.Activate();
				currentEvent.Handle();
			}
		}
	}

	public virtual void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections = null)
	{
		hotkeyChoices.Clear();
		scrollContext.wraps = false;
		PaperdollScrollerContext<FrameworkDataElement, NavigationContext> paperdollScrollerContext = scrollContext;
		if (paperdollScrollerContext.axisHandlers == null)
		{
			paperdollScrollerContext.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		scrollContext.axisHandlers[InputAxisTypes.NavigationXAxis] = HandleXAxisEvent;
		scrollContext.axisHandlers[InputAxisTypes.NavigationYAxis] = HandleYAxisEvent;
		if (EnablePaging)
		{
			scrollContext.axisHandlers.Set(PageAxis, Event.Helpers.Handle(Event.Helpers.Axis(DoPageDown, DoPageUp)));
		}
		UpdateSelections(selections);
	}

	public void SelectHighest()
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

	public void SelectLowest()
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
				onSelected?.Invoke(choices[context.index]);
			})
		} };
		return context;
	}

	public virtual void UpdateSelection()
	{
		ScrollSelectedIntoView();
		onHighlight?.Invoke(scrollContext.data[selectedPosition]);
	}

	public virtual void UpdateWidth()
	{
	}

	public virtual void LateUpdate()
	{
		if (FirstLateUpdate)
		{
			Canvas.ForceUpdateCanvases();
			UpdateLayout();
			Canvas.ForceUpdateCanvases();
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
			Canvas.ForceUpdateCanvases();
			lastHeight = centeringFreeSpace.rect.y;
			float num = Math.Min(centeringFreeSpace.rect.height, Math.Max(0f, centeringFrame.rect.height / 2f - 140f));
			centeringFrame.anchoredPosition = new Vector2(centeringFrame.anchoredPosition.x, 0f - num);
			Canvas.ForceUpdateCanvases();
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
