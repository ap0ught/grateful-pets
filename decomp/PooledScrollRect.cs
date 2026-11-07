using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public abstract class PooledScrollRect<T, Y> : MonoBehaviour, IPointerExitHandler, IEventSystemHandler, IPointerEnterHandler, ICollectionManagement<T> where Y : PooledScrollRectElement<T>
{
	private enum RepurposeMethod
	{
		TopGoesToBottom,
		BottomGoesToTop
	}

	public ScrollRect contentScrollRect;

	public VerticalLayoutGroup contentLayoutGroup;

	public ContentSizeFitter contentSizeFitter;

	public float ElementSize;

	public Y Template;

	[SerializeField]
	protected List<T> _scrollListData = new List<T>();

	protected List<float> _elementHeights = new List<float>();

	public Queue<Y> elements = new Queue<Y>();

	public bool Reflowing;

	private int forceLockCount = 5;

	public int MaxElements = 60;

	public int update = 2;

	private RectTransform _contentLayoutRect;

	public bool NeedsReflow;

	private bool clearNextUpdate;

	protected ScrollRect _scrollRect;

	private LayoutElement _spacer;

	private List<Y> _activeElements = new List<Y>();

	public bool doSpacer = true;

	public int forceLocked;

	public bool unlocked;

	private bool IsHovered;

	private RectTransform contentLayoutRect
	{
		get
		{
			if (_contentLayoutRect == null)
			{
				_contentLayoutRect = contentLayoutGroup.gameObject.GetComponent<RectTransform>();
			}
			return _contentLayoutRect;
		}
	}

	protected int _totalNumElements => _scrollListData.Count;

	public T Get(int index)
	{
		return _scrollListData[index];
	}

	public void Reflow()
	{
		if (!(_scrollRect == null) && !contentLayoutGroup.enabled)
		{
			if (!contentSizeFitter.enabled)
			{
				contentSizeFitter.enabled = true;
			}
			if (!contentLayoutGroup.enabled)
			{
				contentLayoutGroup.enabled = true;
			}
			update = 2;
			Reflowing = true;
		}
	}

	public void Add(T value)
	{
		if (clearNextUpdate)
		{
			_Clear();
			clearNextUpdate = false;
		}
		unlocked = false;
		forceLocked = forceLockCount;
		update = 2;
		if (_scrollRect == null)
		{
			return;
		}
		if (!contentLayoutGroup.enabled)
		{
			if (!contentSizeFitter.enabled)
			{
				contentSizeFitter.enabled = true;
			}
			if (!contentLayoutGroup.enabled)
			{
				contentLayoutGroup.enabled = true;
			}
			update = 2;
		}
		Y val = null;
		if (elements.Count < MaxElements)
		{
			val = UnityEngine.Object.Instantiate(Template);
			if (!val.gameObject.activeSelf)
			{
				val.gameObject.SetActive(value: true);
			}
			val.transform.SetParent(_scrollRect.content, worldPositionStays: false);
		}
		else
		{
			val = elements.Dequeue();
			val.transform.SetAsLastSibling();
		}
		val.GetComponent<Y>().Setup(0, new List<T> { value });
		elements.Enqueue(val);
		ScrollToBot();
	}

	public void Update()
	{
		if (clearNextUpdate)
		{
			_Clear();
			clearNextUpdate = false;
		}
		if (forceLocked > 0)
		{
			forceLocked--;
		}
		if (update > 0)
		{
			update--;
			if (update <= 0 && contentLayoutGroup != null && contentLayoutGroup.enabled)
			{
				contentSizeFitter.enabled = false;
				contentLayoutGroup.enabled = false;
			}
			if (Reflowing)
			{
				ScrollToBot();
			}
		}
		else
		{
			Reflowing = false;
		}
		if (!unlocked && !IsHovered)
		{
			ScrollToBot();
		}
	}

	public void RemoveAt(int index)
	{
		_scrollListData.RemoveAt(index);
		ScrollToBot();
	}

	public void RemoveRange(int index, int count)
	{
		_scrollListData.RemoveRange(index, Math.Max(count, _scrollListData.Count - index));
		ScrollToBot();
	}

	public void Clear()
	{
		if (Thread.CurrentThread == GameManager.Instance.uiQueue.threadContext)
		{
			_Clear();
		}
		else
		{
			clearNextUpdate = true;
		}
	}

	private void _Clear()
	{
		_scrollListData.Clear();
		while (elements.Count > 0)
		{
			Y val = elements.Dequeue();
			val.transform.parent = null;
			val.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(val.gameObject);
		}
	}

	public void ScrollToBot()
	{
		if (_scrollRect != null && _scrollRect.normalizedPosition != Vector2.zero)
		{
			_scrollRect.normalizedPosition = new Vector2(0f, 0f);
			unlocked = false;
		}
	}

	private void OnEnable()
	{
		if (_scrollRect == null)
		{
			_scrollRect = GetComponent<ScrollRect>();
		}
		if (_spacer == null && doSpacer)
		{
			_spacer = SpawnSpacer();
		}
		_scrollRect.onValueChanged.AddListener(ScrollMoved);
		ScrollMoved(Vector2.zero);
		unlocked = false;
		forceLocked = forceLockCount;
	}

	private void OnDisable()
	{
		_scrollRect.onValueChanged.RemoveListener(ScrollMoved);
	}

	private void ScrollMoved(Vector2 delta)
	{
		if (forceLocked <= 0)
		{
			unlocked = true;
		}
	}

	private void AdjustContentSize(float size)
	{
		Vector2 sizeDelta = _scrollRect.content.sizeDelta;
		if (_scrollRect.vertical)
		{
			sizeDelta.y = size;
		}
		else
		{
			sizeDelta.x = size;
		}
		_scrollRect.content.sizeDelta = sizeDelta;
	}

	private void AdjustSpacer(float size)
	{
		if (_scrollRect.vertical)
		{
			_spacer.minHeight = size;
		}
		else
		{
			_spacer.minWidth = size;
		}
	}

	private float GetScrollRectNormalizedPosition()
	{
		return Mathf.Clamp01(_scrollRect.vertical ? (1f - _scrollRect.verticalNormalizedPosition) : _scrollRect.horizontalNormalizedPosition);
	}

	private LayoutElement SpawnSpacer()
	{
		LayoutElement layoutElement = new GameObject("Spacer").AddComponent<LayoutElement>();
		if (_scrollRect.vertical)
		{
			layoutElement.minHeight = 100f;
		}
		else
		{
			layoutElement.minWidth = 100f;
		}
		layoutElement.transform.SetParent(_scrollRect.content.transform, worldPositionStays: false);
		return layoutElement;
	}

	private void InitializeElements(int requiredElementsInList, int numElementsCulledAbove)
	{
		for (int i = 0; i < _activeElements.Count; i++)
		{
			UnityEngine.Object.Destroy(_activeElements[i].gameObject);
		}
		_activeElements.Clear();
		for (int j = 0; j < requiredElementsInList && j + numElementsCulledAbove < _totalNumElements; j++)
		{
			Y val = UnityEngine.Object.Instantiate(Template);
			if (!val.gameObject.activeSelf)
			{
				val.gameObject.SetActive(value: true);
			}
			val.transform.SetParent(_scrollRect.content, worldPositionStays: false);
			val.Setup(j + numElementsCulledAbove, _scrollListData);
			_activeElements.Add(val);
		}
	}

	private void RepurposeElement(RepurposeMethod repurposeMethod, int numElementsCulledAbove)
	{
		if (repurposeMethod == RepurposeMethod.TopGoesToBottom)
		{
			Y val = _activeElements[0];
			_activeElements.RemoveAt(0);
			_activeElements.Add(val);
			val.transform.SetSiblingIndex(_activeElements[_activeElements.Count - 2].transform.GetSiblingIndex() + 1);
			val.Setup(numElementsCulledAbove + _activeElements.Count - 1, _scrollListData);
		}
		else
		{
			Y val2 = _activeElements[_activeElements.Count - 1];
			_activeElements.RemoveAt(_activeElements.Count - 1);
			_activeElements.Insert(0, val2);
			val2.transform.SetSiblingIndex(_activeElements[1].transform.GetSiblingIndex());
			val2.Setup(numElementsCulledAbove, _scrollListData);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsHovered = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsHovered = true;
	}
}
