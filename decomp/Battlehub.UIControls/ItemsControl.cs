using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls;

public class ItemsControl<TDataBindingArgs> : ItemsControl where TDataBindingArgs : ItemDataBindingArgs, new()
{
	public event EventHandler<TDataBindingArgs> ItemDataBinding;

	public event EventHandler<TDataBindingArgs> ItemBeginEdit;

	public event EventHandler<TDataBindingArgs> ItemEndEdit;

	protected override void OnItemBeginEdit(object sender, EventArgs e)
	{
		if (CanHandleEvent(sender))
		{
			ItemContainer itemContainer = (ItemContainer)sender;
			if (ItemBeginEdit != null)
			{
				TDataBindingArgs e2 = new TDataBindingArgs
				{
					Item = itemContainer.Item,
					ItemPresenter = ((itemContainer.ItemPresenter == null) ? gameObject : itemContainer.ItemPresenter),
					EditorPresenter = ((itemContainer.EditorPresenter == null) ? gameObject : itemContainer.EditorPresenter)
				};
				ItemBeginEdit(this, e2);
			}
		}
	}

	protected override void OnItemEndEdit(object sender, EventArgs e)
	{
		if (CanHandleEvent(sender))
		{
			ItemContainer itemContainer = (ItemContainer)sender;
			if (ItemBeginEdit != null)
			{
				TDataBindingArgs e2 = new TDataBindingArgs
				{
					Item = itemContainer.Item,
					ItemPresenter = ((itemContainer.ItemPresenter == null) ? gameObject : itemContainer.ItemPresenter),
					EditorPresenter = ((itemContainer.EditorPresenter == null) ? gameObject : itemContainer.EditorPresenter)
				};
				ItemEndEdit(this, e2);
			}
		}
	}

	public override void DataBindItem(object item, ItemContainer itemContainer)
	{
		TDataBindingArgs val = new TDataBindingArgs
		{
			Item = item,
			ItemPresenter = ((itemContainer.ItemPresenter == null) ? gameObject : itemContainer.ItemPresenter),
			EditorPresenter = ((itemContainer.EditorPresenter == null) ? gameObject : itemContainer.EditorPresenter)
		};
		RaiseItemDataBinding(val);
		itemContainer.CanEdit = val.CanEdit;
		itemContainer.CanDrag = val.CanDrag;
		itemContainer.CanDrop = val.CanDrop;
	}

	protected void RaiseItemDataBinding(TDataBindingArgs args)
	{
		if (ItemDataBinding != null)
		{
			ItemDataBinding(this, args);
		}
	}
}
public abstract class ItemsControl : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDropHandler
{
	private enum ScrollDir
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	public KeyCode MultiselectKey = KeyCode.LeftControl;

	public KeyCode RangeselectKey = KeyCode.LeftShift;

	public KeyCode SelectAllKey = KeyCode.A;

	public KeyCode RemoveKey = KeyCode.Delete;

	public bool SelectOnPointerUp;

	public bool CanUnselectAll = true;

	public bool CanEdit = true;

	private bool m_prevCanDrag;

	public bool CanDrag = true;

	public bool CanReorder = true;

	public bool ExpandChildrenWidth = true;

	public bool ExpandChildrenHeight;

	private bool m_isDropInProgress;

	[SerializeField]
	private GameObject ItemContainerPrefab;

	[SerializeField]
	protected Transform Panel;

	private Canvas m_canvas;

	public Camera Camera;

	public float ScrollSpeed = 100f;

	private ScrollDir m_scrollDir;

	private ScrollRect m_scrollRect;

	private RectTransformChangeListener m_rtcListener;

	private float m_width;

	private float m_height;

	private List<ItemContainer> m_itemContainers;

	private ItemDropMarker m_dropMarker;

	private bool m_externalDragOperation;

	private ItemContainer m_dropTarget;

	private ItemContainer[] m_dragItems;

	private IList<object> m_items;

	private bool m_selectionLocked;

	private List<object> m_selectedItems;

	private HashSet<object> m_selectedItemsHS;

	private ItemContainer m_selectedItemContainer;

	private int m_selectedIndex = -1;

	protected virtual bool CanScroll => CanReorder;

	protected bool IsDropInProgress => m_isDropInProgress;

	public object DropTarget
	{
		get
		{
			if (m_dropTarget == null)
			{
				return null;
			}
			return m_dropTarget.Item;
		}
	}

	public ItemDropAction DropAction
	{
		get
		{
			if (m_dropMarker == null)
			{
				return ItemDropAction.None;
			}
			return m_dropMarker.Action;
		}
	}

	protected ItemDropMarker DropMarker => m_dropMarker;

	public IEnumerable Items
	{
		get
		{
			return m_items;
		}
		set
		{
			if (value == null)
			{
				m_items = null;
				m_scrollRect.verticalNormalizedPosition = 1f;
				m_scrollRect.horizontalNormalizedPosition = 0f;
			}
			else
			{
				m_items = value.OfType<object>().ToList();
			}
			DataBind();
		}
	}

	public int ItemsCount
	{
		get
		{
			if (m_items == null)
			{
				return 0;
			}
			return m_items.Count;
		}
	}

	public int SelectedItemsCount
	{
		get
		{
			if (m_selectedItems == null)
			{
				return 0;
			}
			return m_selectedItems.Count;
		}
	}

	public virtual IEnumerable SelectedItems
	{
		get
		{
			return m_selectedItems;
		}
		set
		{
			if (m_selectionLocked)
			{
				return;
			}
			m_selectionLocked = true;
			IList selectedItems = m_selectedItems;
			if (value != null)
			{
				m_selectedItems = value.OfType<object>().ToList();
				m_selectedItemsHS = new HashSet<object>(m_selectedItems);
				for (int num = m_selectedItems.Count - 1; num >= 0; num--)
				{
					object obj = m_selectedItems[num];
					ItemContainer itemContainer = GetItemContainer(obj);
					if (itemContainer != null)
					{
						itemContainer.IsSelected = true;
					}
				}
				if (m_selectedItems.Count == 0)
				{
					m_selectedItemContainer = null;
					m_selectedIndex = -1;
				}
				else
				{
					m_selectedItemContainer = GetItemContainer(m_selectedItems[0]);
					m_selectedIndex = IndexOf(m_selectedItems[0]);
				}
			}
			else
			{
				m_selectedItems = null;
				m_selectedItemsHS = null;
				m_selectedItemContainer = null;
				m_selectedIndex = -1;
			}
			List<object> list = new List<object>();
			if (selectedItems != null)
			{
				for (int i = 0; i < selectedItems.Count; i++)
				{
					object obj2 = selectedItems[i];
					if (m_selectedItemsHS == null || !m_selectedItemsHS.Contains(obj2))
					{
						list.Add(obj2);
						ItemContainer itemContainer2 = GetItemContainer(obj2);
						if (itemContainer2 != null)
						{
							itemContainer2.IsSelected = false;
						}
					}
				}
			}
			if (SelectionChanged != null)
			{
				object[] newItems = ((m_selectedItems == null) ? new object[0] : m_selectedItems.ToArray());
				SelectionChanged(this, new SelectionChangedArgs(list.ToArray(), newItems));
			}
			m_selectionLocked = false;
		}
	}

	public object SelectedItem
	{
		get
		{
			if (m_selectedItems == null || m_selectedItems.Count == 0)
			{
				return null;
			}
			return m_selectedItems[0];
		}
		set
		{
			SelectedIndex = IndexOf(value);
		}
	}

	public int SelectedIndex
	{
		get
		{
			if (SelectedItem == null)
			{
				return -1;
			}
			return m_selectedIndex;
		}
		set
		{
			if (m_selectedIndex == value || m_selectionLocked)
			{
				return;
			}
			m_selectionLocked = true;
			ItemContainer selectedItemContainer = m_selectedItemContainer;
			if (selectedItemContainer != null)
			{
				selectedItemContainer.IsSelected = false;
			}
			m_selectedIndex = value;
			object obj = null;
			if (m_selectedIndex >= 0 && m_selectedIndex < m_items.Count)
			{
				obj = m_items[m_selectedIndex];
				m_selectedItemContainer = GetItemContainer(obj);
				if (m_selectedItemContainer != null)
				{
					m_selectedItemContainer.IsSelected = true;
				}
			}
			object[] array = ((obj == null) ? new object[0] : new object[1] { obj });
			object[] array2 = ((m_selectedItems == null) ? new object[0] : m_selectedItems.Except(array).ToArray());
			foreach (object obj2 in array2)
			{
				ItemContainer itemContainer = GetItemContainer(obj2);
				if (itemContainer != null)
				{
					itemContainer.IsSelected = false;
				}
			}
			m_selectedItems = array.ToList();
			m_selectedItemsHS = new HashSet<object>(m_selectedItems);
			if (SelectionChanged != null)
			{
				SelectionChanged(this, new SelectionChangedArgs(array2, array));
			}
			m_selectionLocked = false;
		}
	}

	public event EventHandler<ItemArgs> ItemBeginDrag;

	public event EventHandler<ItemDropCancelArgs> ItemBeginDrop;

	public event EventHandler<ItemDropArgs> ItemDrop;

	public event EventHandler<ItemArgs> ItemEndDrag;

	public event EventHandler<SelectionChangedArgs> SelectionChanged;

	public event EventHandler<ItemArgs> ItemDoubleClick;

	public event EventHandler<ItemsCancelArgs> ItemsRemoving;

	public event EventHandler<ItemsRemovedArgs> ItemsRemoved;

	protected void RemoveItemAt(int index)
	{
		m_items.RemoveAt(index);
	}

	protected void RemoveItemContainerAt(int index)
	{
		m_itemContainers.RemoveAt(index);
	}

	protected void InsertItem(int index, object value)
	{
		m_items.Insert(index, value);
	}

	protected void InsertItemContainerAt(int index, ItemContainer container)
	{
		m_itemContainers.Insert(index, container);
	}

	public bool IsItemSelected(object obj)
	{
		if (m_selectedItemsHS == null)
		{
			return false;
		}
		return m_selectedItemsHS.Contains(obj);
	}

	public int IndexOf(object obj)
	{
		if (m_items == null)
		{
			return -1;
		}
		if (obj == null)
		{
			return -1;
		}
		return m_items.IndexOf(obj);
	}

	public ItemContainer GetItemContainer(object obj)
	{
		return m_itemContainers.Where((ItemContainer ic) => ic.Item == obj).FirstOrDefault();
	}

	public ItemContainer LastItemContainer()
	{
		if (m_itemContainers == null || m_itemContainers.Count == 0)
		{
			return null;
		}
		return m_itemContainers[m_itemContainers.Count - 1];
	}

	public ItemContainer GetItemContainer(int siblingIndex)
	{
		if (siblingIndex < 0 || siblingIndex >= m_itemContainers.Count)
		{
			return null;
		}
		return m_itemContainers[siblingIndex];
	}

	public void ExternalBeginDrag(Vector3 position)
	{
		if (CanDrag)
		{
			m_externalDragOperation = true;
			if (!(m_dropTarget == null) && (m_dragItems != null || m_externalDragOperation) && m_scrollDir == ScrollDir.None)
			{
				m_dropMarker.SetTraget(m_dropTarget);
			}
		}
	}

	public void ExternalItemDrag(Vector3 position)
	{
		if (CanDrag && m_dropTarget != null)
		{
			m_dropMarker.SetPosition(position);
		}
	}

	public void ExternalItemDrop()
	{
		if (CanDrag)
		{
			m_externalDragOperation = false;
			m_dropMarker.SetTraget(null);
		}
	}

	public ItemContainer Add(object item)
	{
		if (m_items == null)
		{
			m_items = new List<object>();
			m_itemContainers = new List<ItemContainer>();
		}
		return Insert(m_items.Count, item);
	}

	public virtual ItemContainer Insert(int index, object item)
	{
		if (m_items == null)
		{
			m_items = new List<object>();
			m_itemContainers = new List<ItemContainer>();
		}
		object obj = m_items.ElementAtOrDefault(index);
		ItemContainer itemContainer = GetItemContainer(obj);
		int siblingIndex = ((!(itemContainer != null)) ? m_itemContainers.Count : m_itemContainers.IndexOf(itemContainer));
		m_items.Insert(index, item);
		itemContainer = InstantiateItemContainer(siblingIndex);
		if (itemContainer != null)
		{
			itemContainer.Item = item;
			DataBindItem(item, itemContainer);
		}
		return itemContainer;
	}

	public void SetNextSibling(object sibling, object nextSibling)
	{
		ItemContainer itemContainer = GetItemContainer(sibling);
		if (!(itemContainer == null))
		{
			ItemContainer itemContainer2 = GetItemContainer(nextSibling);
			if (!(itemContainer2 == null))
			{
				Drop(new ItemContainer[1] { itemContainer2 }, itemContainer, ItemDropAction.SetNextSibling);
			}
		}
	}

	public void SetPrevSibling(object sibling, object prevSibling)
	{
		ItemContainer itemContainer = GetItemContainer(sibling);
		if (!(itemContainer == null))
		{
			ItemContainer itemContainer2 = GetItemContainer(prevSibling);
			if (!(itemContainer2 == null))
			{
				Drop(new ItemContainer[1] { itemContainer2 }, itemContainer, ItemDropAction.SetPrevSibling);
			}
		}
	}

	public virtual void Remove(object item)
	{
		if (item != null && m_items != null && m_items.Contains(item))
		{
			DestroyItem(item);
		}
	}

	public void RemoveAt(int index)
	{
		if (m_items != null)
		{
			if (index >= m_items.Count || index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			Remove(m_items[index]);
		}
	}

	private void Awake()
	{
		if (Panel == null)
		{
			Panel = transform;
		}
		m_itemContainers = GetComponentsInChildren<ItemContainer>().ToList();
		m_rtcListener = GetComponentInChildren<RectTransformChangeListener>();
		if (m_rtcListener != null)
		{
			m_rtcListener.RectTransformChanged += OnViewportRectTransformChanged;
		}
		m_dropMarker = GetComponentInChildren<ItemDropMarker>(includeInactive: true);
		m_scrollRect = GetComponent<ScrollRect>();
		if (Camera == null)
		{
			Camera = Camera.main;
		}
		m_prevCanDrag = CanDrag;
		OnCanDragChanged();
		AwakeOverride();
	}

	private void Start()
	{
		m_canvas = GetComponentInParent<Canvas>();
		StartOverride();
	}

	private void Update()
	{
		if (m_scrollDir != ScrollDir.None)
		{
			float num = m_scrollRect.content.rect.height - m_scrollRect.viewport.rect.height;
			float num2 = 0f;
			if (num > 0f)
			{
				num2 = ScrollSpeed / 10f * (1f / num);
			}
			float num3 = m_scrollRect.content.rect.width - m_scrollRect.viewport.rect.width;
			float num4 = 0f;
			if (num3 > 0f)
			{
				num4 = ScrollSpeed / 10f * (1f / num3);
			}
			if (m_scrollDir == ScrollDir.Up)
			{
				m_scrollRect.verticalNormalizedPosition += num2;
				if (m_scrollRect.verticalNormalizedPosition > 1f)
				{
					m_scrollRect.verticalNormalizedPosition = 1f;
					m_scrollDir = ScrollDir.None;
				}
			}
			else if (m_scrollDir == ScrollDir.Down)
			{
				m_scrollRect.verticalNormalizedPosition -= num2;
				if (m_scrollRect.verticalNormalizedPosition < 0f)
				{
					m_scrollRect.verticalNormalizedPosition = 0f;
					m_scrollDir = ScrollDir.None;
				}
			}
			else if (m_scrollDir == ScrollDir.Left)
			{
				m_scrollRect.horizontalNormalizedPosition -= num4;
				if (m_scrollRect.horizontalNormalizedPosition < 0f)
				{
					m_scrollRect.horizontalNormalizedPosition = 0f;
					m_scrollDir = ScrollDir.None;
				}
			}
			if (m_scrollDir == ScrollDir.Right)
			{
				m_scrollRect.horizontalNormalizedPosition += num4;
				if (m_scrollRect.horizontalNormalizedPosition > 1f)
				{
					m_scrollRect.horizontalNormalizedPosition = 1f;
					m_scrollDir = ScrollDir.None;
				}
			}
		}
		if (Input.GetKeyDown(RemoveKey))
		{
			RemoveSelectedItems();
		}
		if (Input.GetKeyDown(SelectAllKey) && Input.GetKey(RangeselectKey))
		{
			SelectedItems = m_items;
		}
		if (m_prevCanDrag != CanDrag)
		{
			OnCanDragChanged();
			m_prevCanDrag = CanDrag;
		}
		UpdateOverride();
	}

	private void OnEnable()
	{
		ItemContainer.Selected += OnItemSelected;
		ItemContainer.Unselected += OnItemUnselected;
		ItemContainer.PointerUp += OnItemPointerUp;
		ItemContainer.PointerDown += OnItemPointerDown;
		ItemContainer.PointerEnter += OnItemPointerEnter;
		ItemContainer.PointerExit += OnItemPointerExit;
		ItemContainer.DoubleClick += OnItemDoubleClick;
		ItemContainer.BeginEdit += OnItemBeginEdit;
		ItemContainer.EndEdit += OnItemEndEdit;
		ItemContainer.BeginDrag += OnItemBeginDrag;
		ItemContainer.Drag += OnItemDrag;
		ItemContainer.Drop += OnItemDrop;
		ItemContainer.EndDrag += OnItemEndDrag;
		OnEnableOverride();
	}

	private void OnDisable()
	{
		ItemContainer.Selected -= OnItemSelected;
		ItemContainer.Unselected -= OnItemUnselected;
		ItemContainer.PointerUp -= OnItemPointerUp;
		ItemContainer.PointerDown -= OnItemPointerDown;
		ItemContainer.PointerEnter -= OnItemPointerEnter;
		ItemContainer.PointerExit -= OnItemPointerExit;
		ItemContainer.DoubleClick -= OnItemDoubleClick;
		ItemContainer.BeginEdit -= OnItemBeginEdit;
		ItemContainer.EndEdit -= OnItemEndEdit;
		ItemContainer.BeginDrag -= OnItemBeginDrag;
		ItemContainer.Drag -= OnItemDrag;
		ItemContainer.Drop -= OnItemDrop;
		ItemContainer.EndDrag -= OnItemEndDrag;
		OnDisableOverride();
	}

	private void OnDestroy()
	{
		if (m_rtcListener != null)
		{
			m_rtcListener.RectTransformChanged -= OnViewportRectTransformChanged;
		}
		OnDestroyOverride();
	}

	protected virtual void AwakeOverride()
	{
	}

	protected virtual void StartOverride()
	{
	}

	protected virtual void UpdateOverride()
	{
	}

	protected virtual void OnEnableOverride()
	{
	}

	protected virtual void OnDisableOverride()
	{
	}

	protected virtual void OnDestroyOverride()
	{
	}

	private void OnViewportRectTransformChanged()
	{
		if (!ExpandChildrenHeight && !ExpandChildrenWidth)
		{
			return;
		}
		Rect rect = m_scrollRect.viewport.rect;
		if (rect.width == m_width && rect.height == m_height)
		{
			return;
		}
		m_width = rect.width;
		m_height = rect.height;
		if (m_itemContainers == null)
		{
			return;
		}
		for (int i = 0; i < m_itemContainers.Count; i++)
		{
			ItemContainer itemContainer = m_itemContainers[i];
			if (itemContainer != null)
			{
				if (ExpandChildrenWidth)
				{
					itemContainer.LayoutElement.minWidth = m_width;
				}
				if (ExpandChildrenHeight)
				{
					itemContainer.LayoutElement.minHeight = m_height;
				}
			}
		}
	}

	private void OnCanDragChanged()
	{
		for (int i = 0; i < m_itemContainers.Count; i++)
		{
			ItemContainer itemContainer = m_itemContainers[i];
			if (itemContainer != null)
			{
				itemContainer.CanDrag = CanDrag;
			}
		}
	}

	protected bool CanHandleEvent(object sender)
	{
		ItemContainer itemContainer = sender as ItemContainer;
		if (!itemContainer)
		{
			return false;
		}
		return itemContainer.transform.IsChildOf(Panel);
	}

	private void OnItemSelected(object sender, EventArgs e)
	{
		if (!m_selectionLocked && CanHandleEvent(sender))
		{
			ItemContainer.Unselected -= OnItemUnselected;
			if (Input.GetKey(MultiselectKey))
			{
				IList list = ((m_selectedItems != null) ? m_selectedItems.ToList() : new List<object>());
				list.Add(((ItemContainer)sender).Item);
				SelectedItems = list;
			}
			else if (Input.GetKey(RangeselectKey))
			{
				SelectRange((ItemContainer)sender);
			}
			else
			{
				SelectedIndex = IndexOf(((ItemContainer)sender).Item);
			}
			ItemContainer.Unselected += OnItemUnselected;
		}
	}

	private void SelectRange(ItemContainer itemContainer)
	{
		if (m_selectedItems != null && m_selectedItems.Count > 0)
		{
			List<object> list = new List<object>();
			int num = IndexOf(m_selectedItems[0]);
			object item = itemContainer.Item;
			int num2 = IndexOf(item);
			int num3 = Mathf.Min(num, num2);
			int num4 = Math.Max(num, num2);
			list.Add(m_selectedItems[0]);
			for (int i = num3; i < num; i++)
			{
				list.Add(m_items[i]);
			}
			for (int j = num + 1; j <= num4; j++)
			{
				list.Add(m_items[j]);
			}
			SelectedItems = list;
		}
		else
		{
			SelectedIndex = IndexOf(itemContainer.Item);
		}
	}

	private void OnItemUnselected(object sender, EventArgs e)
	{
		if (!m_selectionLocked && CanHandleEvent(sender))
		{
			IList list = ((m_selectedItems != null) ? m_selectedItems.ToList() : new List<object>());
			list.Remove(((ItemContainer)sender).Item);
			SelectedItems = list;
		}
	}

	private void OnItemPointerDown(ItemContainer sender, PointerEventData e)
	{
		if (!CanHandleEvent(sender) || m_externalDragOperation)
		{
			return;
		}
		m_dropMarker.SetTraget(null);
		m_dragItems = null;
		m_isDropInProgress = false;
		if (!SelectOnPointerUp)
		{
			if (Input.GetKey(RangeselectKey))
			{
				SelectRange(sender);
			}
			else if (Input.GetKey(MultiselectKey))
			{
				sender.IsSelected = !sender.IsSelected;
			}
			else
			{
				sender.IsSelected = true;
			}
		}
	}

	private void OnItemPointerUp(ItemContainer sender, PointerEventData e)
	{
		if (!CanHandleEvent(sender) || m_externalDragOperation || m_dragItems != null)
		{
			return;
		}
		if (SelectOnPointerUp)
		{
			if (!m_isDropInProgress)
			{
				if (Input.GetKey(RangeselectKey))
				{
					SelectRange(sender);
				}
				else if (Input.GetKey(MultiselectKey))
				{
					sender.IsSelected = !sender.IsSelected;
				}
				else
				{
					sender.IsSelected = true;
				}
			}
		}
		else if (!Input.GetKey(MultiselectKey) && !Input.GetKey(RangeselectKey) && m_selectedItems != null && m_selectedItems.Count > 1)
		{
			if (SelectedItem == sender.Item)
			{
				SelectedItem = null;
			}
			SelectedItem = sender.Item;
		}
	}

	private void OnItemPointerEnter(ItemContainer sender, PointerEventData eventData)
	{
		if (CanHandleEvent(sender))
		{
			m_dropTarget = sender;
			if ((m_dragItems != null || m_externalDragOperation) && m_scrollDir == ScrollDir.None)
			{
				m_dropMarker.SetTraget(m_dropTarget);
			}
		}
	}

	private void OnItemPointerExit(ItemContainer sender, PointerEventData eventData)
	{
		if (CanHandleEvent(sender))
		{
			m_dropTarget = null;
			if (m_dragItems != null || m_externalDragOperation)
			{
				m_dropMarker.SetTraget(null);
			}
		}
	}

	private void OnItemDoubleClick(ItemContainer sender, PointerEventData eventData)
	{
		if (CanHandleEvent(sender) && ItemDoubleClick != null)
		{
			ItemDoubleClick(this, new ItemArgs(new object[1] { sender.Item }));
		}
	}

	protected virtual void OnItemBeginEdit(object sender, EventArgs e)
	{
	}

	protected virtual void OnItemEndEdit(object sender, EventArgs e)
	{
	}

	private void OnItemBeginDrag(ItemContainer sender, PointerEventData eventData)
	{
		if (!CanHandleEvent(sender))
		{
			return;
		}
		if (m_dropTarget != null)
		{
			m_dropMarker.SetTraget(m_dropTarget);
			m_dropMarker.SetPosition(eventData.position);
		}
		if (m_selectedItems != null && m_selectedItems.Contains(sender.Item))
		{
			m_dragItems = GetDragItems();
		}
		else
		{
			m_dragItems = new ItemContainer[1] { sender };
		}
		if (ItemBeginDrag != null)
		{
			ItemBeginDrag(this, new ItemArgs(m_dragItems.Select((ItemContainer di) => di.Item).ToArray()));
		}
	}

	private void OnItemDrag(ItemContainer sender, PointerEventData eventData)
	{
		if (!CanHandleEvent(sender))
		{
			return;
		}
		ExternalItemDrag(eventData.position);
		float height = m_scrollRect.viewport.rect.height;
		float width = m_scrollRect.viewport.rect.width;
		Camera cam = null;
		if (m_canvas.renderMode == RenderMode.WorldSpace || m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			cam = Camera;
		}
		if (CanScroll)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollRect.viewport, eventData.position, cam, out var localPoint))
			{
				if (localPoint.y >= 0f)
				{
					m_scrollDir = ScrollDir.Up;
					m_dropMarker.SetTraget(null);
				}
				else if (localPoint.y < 0f - height)
				{
					m_scrollDir = ScrollDir.Down;
					m_dropMarker.SetTraget(null);
				}
				else if (localPoint.x <= 0f)
				{
					m_scrollDir = ScrollDir.Left;
				}
				else if (localPoint.x >= width)
				{
					m_scrollDir = ScrollDir.Right;
				}
				else
				{
					m_scrollDir = ScrollDir.None;
				}
			}
		}
		else
		{
			m_scrollDir = ScrollDir.None;
		}
	}

	private void OnItemDrop(ItemContainer sender, PointerEventData eventData)
	{
		if (!CanHandleEvent(sender))
		{
			return;
		}
		m_isDropInProgress = true;
		try
		{
			if (CanDrop(m_dragItems, m_dropTarget))
			{
				bool flag = false;
				if (ItemBeginDrop != null)
				{
					ItemDropCancelArgs itemDropCancelArgs = new ItemDropCancelArgs(m_dragItems.Select((ItemContainer di) => di.Item).ToArray(), m_dropTarget.Item, m_dropMarker.Action, isExternal: false);
					if (ItemBeginDrop != null)
					{
						ItemBeginDrop(this, itemDropCancelArgs);
						flag = itemDropCancelArgs.Cancel;
					}
				}
				if (!flag)
				{
					Drop(m_dragItems, m_dropTarget, m_dropMarker.Action);
					if (ItemDrop != null)
					{
						if (m_dragItems == null)
						{
							Debug.LogWarning("m_dragItems");
						}
						if (m_dropTarget == null)
						{
							Debug.LogWarning("m_dropTarget");
						}
						if (m_dropMarker == null)
						{
							Debug.LogWarning("m_dropMarker");
						}
						if (m_dragItems != null && m_dropTarget != null && m_dropMarker != null)
						{
							ItemDrop(this, new ItemDropArgs(m_dragItems.Select((ItemContainer di) => di.Item).ToArray(), m_dropTarget.Item, m_dropMarker.Action, isExternal: false));
						}
					}
				}
			}
			RaiseEndDrag();
		}
		finally
		{
			m_isDropInProgress = false;
		}
	}

	private void OnItemEndDrag(ItemContainer sender, PointerEventData eventData)
	{
		if (CanHandleEvent(sender))
		{
			RaiseEndDrag();
		}
	}

	private void RaiseEndDrag()
	{
		if (m_dragItems == null)
		{
			return;
		}
		if (ItemEndDrag != null)
		{
			ItemEndDrag(this, new ItemArgs(m_dragItems.Select((ItemContainer di) => di.Item).ToArray()));
		}
		m_dropMarker.SetTraget(null);
		m_dragItems = null;
		m_scrollDir = ScrollDir.None;
	}

	void IDropHandler.OnDrop(PointerEventData eventData)
	{
		if (!CanReorder)
		{
			return;
		}
		if (m_dragItems == null)
		{
			GameObject pointerDrag = eventData.pointerDrag;
			if (!(pointerDrag != null))
			{
				return;
			}
			ItemContainer component = pointerDrag.GetComponent<ItemContainer>();
			if (component != null && component.Item != null)
			{
				object item = component.Item;
				if (ItemDrop != null)
				{
					ItemDrop(this, new ItemDropArgs(new object[1] { item }, null, ItemDropAction.SetLastChild, isExternal: true));
				}
			}
			return;
		}
		if (m_itemContainers != null && m_itemContainers.Count > 0)
		{
			m_dropTarget = m_itemContainers.Last();
			m_dropMarker.Action = ItemDropAction.SetNextSibling;
		}
		m_isDropInProgress = true;
		try
		{
			if (CanDrop(m_dragItems, m_dropTarget))
			{
				Drop(m_dragItems, m_dropTarget, m_dropMarker.Action);
				if (ItemDrop != null)
				{
					ItemDrop(this, new ItemDropArgs(m_dragItems.Select((ItemContainer di) => di.Item).ToArray(), m_dropTarget.Item, m_dropMarker.Action, isExternal: false));
				}
			}
			m_dropMarker.SetTraget(null);
			m_dragItems = null;
		}
		finally
		{
			m_isDropInProgress = false;
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (CanUnselectAll)
		{
			SelectedIndex = -1;
		}
	}

	protected virtual bool CanDrop(ItemContainer[] dragItems, ItemContainer dropTarget)
	{
		if (dropTarget == null)
		{
			return true;
		}
		if (dragItems == null)
		{
			return false;
		}
		if (dragItems.Contains(dropTarget.Item))
		{
			return false;
		}
		return true;
	}

	protected ItemContainer[] GetDragItems()
	{
		ItemContainer[] array = new ItemContainer[m_selectedItems.Count];
		if (m_selectedItems != null)
		{
			for (int i = 0; i < m_selectedItems.Count; i++)
			{
				array[i] = GetItemContainer(m_selectedItems[i]);
			}
		}
		return array.OrderBy((ItemContainer di) => di.transform.GetSiblingIndex()).ToArray();
	}

	protected virtual void SetNextSibling(ItemContainer sibling, ItemContainer nextSibling)
	{
		int num = sibling.transform.GetSiblingIndex();
		int siblingIndex = nextSibling.transform.GetSiblingIndex();
		RemoveItemContainerAt(siblingIndex);
		RemoveItemAt(siblingIndex);
		if (siblingIndex > num)
		{
			num++;
		}
		nextSibling.transform.SetSiblingIndex(num);
		InsertItemContainerAt(num, nextSibling);
		InsertItem(num, nextSibling.Item);
	}

	protected virtual void SetPrevSibling(ItemContainer sibling, ItemContainer prevSibling)
	{
		int num = sibling.transform.GetSiblingIndex();
		int siblingIndex = prevSibling.transform.GetSiblingIndex();
		RemoveItemContainerAt(siblingIndex);
		RemoveItemAt(siblingIndex);
		if (siblingIndex < num)
		{
			num--;
		}
		prevSibling.transform.SetSiblingIndex(num);
		InsertItemContainerAt(num, prevSibling);
		InsertItem(num, prevSibling.Item);
	}

	protected virtual void Drop(ItemContainer[] dragItems, ItemContainer dropTarget, ItemDropAction action)
	{
		switch (action)
		{
		case ItemDropAction.SetPrevSibling:
			foreach (ItemContainer prevSibling in dragItems)
			{
				SetPrevSibling(dropTarget, prevSibling);
			}
			break;
		case ItemDropAction.SetNextSibling:
			foreach (ItemContainer nextSibling in dragItems)
			{
				SetNextSibling(dropTarget, nextSibling);
			}
			break;
		}
		UpdateSelectedItemIndex();
	}

	protected void UpdateSelectedItemIndex()
	{
		m_selectedIndex = IndexOf(SelectedItem);
	}

	protected virtual void DataBind()
	{
		m_itemContainers = GetComponentsInChildren<ItemContainer>().ToList();
		if (m_items == null)
		{
			for (int i = 0; i < m_itemContainers.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(m_itemContainers[i].gameObject);
			}
		}
		else
		{
			int num = m_items.Count - m_itemContainers.Count;
			if (num > 0)
			{
				for (int j = 0; j < num; j++)
				{
					InstantiateItemContainer(m_itemContainers.Count);
				}
			}
			else
			{
				int num2 = m_itemContainers.Count + num;
				for (int num3 = m_itemContainers.Count - 1; num3 >= num2; num3--)
				{
					DestroyItemContainer(num3);
				}
			}
		}
		for (int k = 0; k < m_itemContainers.Count; k++)
		{
			ItemContainer itemContainer = m_itemContainers[k];
			if (itemContainer != null)
			{
				itemContainer.Clear();
			}
		}
		if (m_items == null)
		{
			return;
		}
		for (int l = 0; l < m_items.Count; l++)
		{
			object item = m_items[l];
			ItemContainer itemContainer2 = m_itemContainers[l];
			itemContainer2.CanDrag = CanDrag;
			if (itemContainer2 != null)
			{
				itemContainer2.Item = item;
				DataBindItem(item, itemContainer2);
			}
		}
	}

	public virtual void DataBindItem(object item, ItemContainer itemContainer)
	{
	}

	protected ItemContainer InstantiateItemContainer(int siblingIndex)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(ItemContainerPrefab);
		gameObject.name = "ItemContainer";
		gameObject.transform.SetParent(Panel, worldPositionStays: false);
		gameObject.transform.SetSiblingIndex(siblingIndex);
		ItemContainer itemContainer = InstantiateItemContainerOverride(gameObject);
		itemContainer.CanDrag = CanDrag;
		if (ExpandChildrenWidth)
		{
			itemContainer.LayoutElement.minWidth = m_width;
		}
		if (ExpandChildrenHeight)
		{
			itemContainer.LayoutElement.minHeight = m_height;
		}
		m_itemContainers.Insert(siblingIndex, itemContainer);
		return itemContainer;
	}

	protected void DestroyItemContainer(int siblingIndex)
	{
		if (m_itemContainers != null && siblingIndex >= 0 && siblingIndex < m_itemContainers.Count)
		{
			UnityEngine.Object.DestroyImmediate(m_itemContainers[siblingIndex].gameObject);
			m_itemContainers.RemoveAt(siblingIndex);
		}
	}

	protected virtual ItemContainer InstantiateItemContainerOverride(GameObject container)
	{
		ItemContainer itemContainer = container.GetComponent<ItemContainer>();
		if (itemContainer == null)
		{
			itemContainer = container.AddComponent<ItemContainer>();
		}
		return itemContainer;
	}

	public void RemoveSelectedItems()
	{
		if (m_selectedItems == null)
		{
			return;
		}
		object[] array;
		if (ItemsRemoving != null)
		{
			ItemsCancelArgs itemsCancelArgs = new ItemsCancelArgs(m_selectedItems.ToList());
			ItemsRemoving(this, itemsCancelArgs);
			array = ((itemsCancelArgs.Items != null) ? itemsCancelArgs.Items.ToArray() : new object[0]);
		}
		else
		{
			array = m_selectedItems.ToArray();
		}
		if (array.Length != 0)
		{
			SelectedItems = null;
			foreach (object item in array)
			{
				DestroyItem(item);
			}
			if (ItemsRemoved != null)
			{
				ItemsRemoved(this, new ItemsRemovedArgs(array));
			}
		}
	}

	protected virtual void DestroyItem(object item)
	{
		if (m_selectedItems != null && m_selectedItems.Contains(item))
		{
			m_selectedItems.Remove(item);
			m_selectedItemsHS.Remove(item);
			if (m_selectedItems.Count == 0)
			{
				m_selectedItemContainer = null;
				m_selectedIndex = -1;
			}
			else
			{
				m_selectedItemContainer = GetItemContainer(m_selectedItems[0]);
				m_selectedIndex = IndexOf(m_selectedItemContainer.Item);
			}
		}
		ItemContainer itemContainer = GetItemContainer(item);
		if (itemContainer != null)
		{
			int siblingIndex = itemContainer.transform.GetSiblingIndex();
			DestroyItemContainer(siblingIndex);
			m_items.Remove(item);
		}
	}
}
