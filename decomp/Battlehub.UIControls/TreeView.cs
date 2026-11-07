using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.UIControls;

public class TreeView : ItemsControl<TreeViewItemDataBindingArgs>
{
	public int Indent = 20;

	public bool CanReparent = true;

	public bool AutoExpand;

	private bool m_expandSilently;

	protected override bool CanScroll
	{
		get
		{
			if (!base.CanScroll)
			{
				return CanReparent;
			}
			return true;
		}
	}

	public event EventHandler<ItemExpandingArgs> ItemExpanding;

	protected override void OnEnableOverride()
	{
		base.OnEnableOverride();
		TreeViewItem.ParentChanged += OnTreeViewItemParentChanged;
	}

	protected override void OnDisableOverride()
	{
		base.OnDisableOverride();
		TreeViewItem.ParentChanged -= OnTreeViewItemParentChanged;
	}

	public TreeViewItem GetTreeViewItem(int siblingIndex)
	{
		return (TreeViewItem)GetItemContainer(siblingIndex);
	}

	public TreeViewItem GetTreeViewItem(object obj)
	{
		return (TreeViewItem)GetItemContainer(obj);
	}

	public void AddChild(object parent, object item)
	{
		if (parent == null)
		{
			Add(item);
			return;
		}
		TreeViewItem treeViewItem = (TreeViewItem)GetItemContainer(parent);
		if (treeViewItem == null)
		{
			return;
		}
		int num = -1;
		if (treeViewItem.IsExpanded)
		{
			if (treeViewItem.HasChildren)
			{
				TreeViewItem treeViewItem2 = treeViewItem.LastDescendant();
				num = IndexOf(treeViewItem2.Item) + 1;
			}
			else
			{
				num = IndexOf(treeViewItem.Item) + 1;
			}
		}
		else
		{
			treeViewItem.CanExpand = true;
		}
		if (num > -1)
		{
			((TreeViewItem)Insert(num, item)).Parent = treeViewItem;
		}
	}

	public override void Remove(object item)
	{
		throw new NotSupportedException("This method is not supported for TreeView use RemoveChild instead");
	}

	public void RemoveChild(object parent, object item, bool isLastChild)
	{
		if (parent == null)
		{
			base.Remove(item);
		}
		else if (GetItemContainer(item) != null)
		{
			base.Remove(item);
		}
		else if (isLastChild)
		{
			TreeViewItem treeViewItem = (TreeViewItem)GetItemContainer(parent);
			if ((bool)treeViewItem)
			{
				treeViewItem.CanExpand = false;
			}
		}
	}

	public void ChangeParent(object parent, object item)
	{
		if (IsDropInProgress)
		{
			return;
		}
		ItemContainer itemContainer = GetItemContainer(item);
		if (!(itemContainer == null))
		{
			ItemContainer itemContainer2 = GetItemContainer(parent);
			ItemContainer[] dragItems = new ItemContainer[1] { itemContainer };
			if (CanDrop(dragItems, itemContainer2))
			{
				Drop(dragItems, itemContainer2, ItemDropAction.SetLastChild);
			}
		}
	}

	public bool IsExpanded(object item)
	{
		TreeViewItem treeViewItem = (TreeViewItem)GetItemContainer(item);
		if (treeViewItem == null)
		{
			return false;
		}
		return treeViewItem.IsExpanded;
	}

	public void Expand(TreeViewItem item)
	{
		if (m_expandSilently || ItemExpanding == null)
		{
			return;
		}
		ItemExpandingArgs itemExpandingArgs = new ItemExpandingArgs(item.Item);
		ItemExpanding(this, itemExpandingArgs);
		IEnumerable children = itemExpandingArgs.Children;
		int num = item.transform.GetSiblingIndex();
		int num2 = IndexOf(item.Item);
		item.CanExpand = children != null;
		if (!item.CanExpand)
		{
			return;
		}
		foreach (object item2 in children)
		{
			num++;
			num2++;
			InsertItem(num2, item2);
			TreeViewItem treeViewItem = (TreeViewItem)InstantiateItemContainer(num);
			treeViewItem.Item = item2;
			treeViewItem.Parent = item;
			DataBindItem(item2, treeViewItem);
		}
		UpdateSelectedItemIndex();
	}

	public void Collapse(TreeViewItem item)
	{
		int siblingIndex = item.transform.GetSiblingIndex();
		int num = IndexOf(item.Item);
		Collapse(item, siblingIndex + 1, num + 1);
	}

	private void Unselect(List<object> selectedItems, TreeViewItem item, ref int containerIndex, ref int itemIndex)
	{
		while (true)
		{
			TreeViewItem treeViewItem = (TreeViewItem)GetItemContainer(containerIndex);
			if (!(treeViewItem == null) && !(treeViewItem.Parent != item))
			{
				containerIndex++;
				itemIndex++;
				selectedItems.Remove(treeViewItem.Item);
				Unselect(selectedItems, treeViewItem, ref containerIndex, ref itemIndex);
				continue;
			}
			break;
		}
	}

	private void Collapse(TreeViewItem item, int containerIndex, int itemIndex)
	{
		while (true)
		{
			TreeViewItem treeViewItem = (TreeViewItem)GetItemContainer(containerIndex);
			if (!(treeViewItem == null) && !(treeViewItem.Parent != item))
			{
				Collapse(treeViewItem, containerIndex + 1, itemIndex + 1);
				RemoveItemAt(itemIndex);
				DestroyItemContainer(containerIndex);
				continue;
			}
			break;
		}
	}

	protected override ItemContainer InstantiateItemContainerOverride(GameObject container)
	{
		TreeViewItem treeViewItem = container.GetComponent<TreeViewItem>();
		if (treeViewItem == null)
		{
			treeViewItem = container.AddComponent<TreeViewItem>();
			treeViewItem.gameObject.name = "TreeViewItem";
		}
		return treeViewItem;
	}

	protected override void DestroyItem(object item)
	{
		TreeViewItem treeViewItem = (TreeViewItem)GetItemContainer(item);
		if (treeViewItem != null)
		{
			Collapse(treeViewItem);
			base.DestroyItem(item);
			if (treeViewItem.Parent != null && !treeViewItem.Parent.HasChildren)
			{
				treeViewItem.Parent.CanExpand = false;
			}
		}
	}

	public override void DataBindItem(object item, ItemContainer itemContainer)
	{
		TreeViewItemDataBindingArgs treeViewItemDataBindingArgs = new TreeViewItemDataBindingArgs();
		treeViewItemDataBindingArgs.Item = item;
		treeViewItemDataBindingArgs.ItemPresenter = ((itemContainer.ItemPresenter == null) ? gameObject : itemContainer.ItemPresenter);
		treeViewItemDataBindingArgs.EditorPresenter = ((itemContainer.EditorPresenter == null) ? gameObject : itemContainer.EditorPresenter);
		RaiseItemDataBinding(treeViewItemDataBindingArgs);
		TreeViewItem treeViewItem = (TreeViewItem)itemContainer;
		treeViewItem.CanExpand = treeViewItemDataBindingArgs.HasChildren;
		treeViewItem.CanEdit = treeViewItemDataBindingArgs.CanEdit;
		treeViewItem.CanDrag = treeViewItemDataBindingArgs.CanDrag;
		treeViewItem.CanDrop = treeViewItemDataBindingArgs.CanDrop;
	}

	protected override bool CanDrop(ItemContainer[] dragItems, ItemContainer dropTarget)
	{
		if (!base.CanDrop(dragItems, dropTarget))
		{
			return false;
		}
		TreeViewItem treeViewItem = (TreeViewItem)dropTarget;
		if (treeViewItem == null)
		{
			return true;
		}
		for (int i = 0; i < dragItems.Length; i++)
		{
			TreeViewItem ancestor = (TreeViewItem)dragItems[i];
			if (treeViewItem.IsDescendantOf(ancestor))
			{
				return false;
			}
		}
		return true;
	}

	private void OnTreeViewItemParentChanged(object sender, ParentChangedEventArgs e)
	{
		TreeViewItem treeViewItem = (TreeViewItem)sender;
		if (!CanHandleEvent(treeViewItem))
		{
			return;
		}
		TreeViewItem oldParent = e.OldParent;
		if (DropMarker.Action != ItemDropAction.SetLastChild && DropMarker.Action != ItemDropAction.None)
		{
			if (oldParent != null && !oldParent.HasChildren)
			{
				oldParent.CanExpand = false;
			}
			return;
		}
		TreeViewItem newParent = e.NewParent;
		if (newParent != null)
		{
			if (newParent.CanExpand)
			{
				newParent.IsExpanded = true;
			}
			else
			{
				newParent.CanExpand = true;
				try
				{
					m_expandSilently = true;
					newParent.IsExpanded = true;
				}
				finally
				{
					m_expandSilently = false;
				}
			}
		}
		TreeViewItem treeViewItem2 = treeViewItem.FirstChild();
		TreeViewItem treeViewItem3 = null;
		if (newParent != null)
		{
			treeViewItem3 = newParent.LastChild();
			if (treeViewItem3 == null)
			{
				treeViewItem3 = newParent;
			}
		}
		else
		{
			treeViewItem3 = (TreeViewItem)LastItemContainer();
		}
		if (treeViewItem3 != treeViewItem)
		{
			TreeViewItem treeViewItem4 = treeViewItem3.LastDescendant();
			if (treeViewItem4 != null)
			{
				treeViewItem3 = treeViewItem4;
			}
			if (!treeViewItem3.IsDescendantOf(treeViewItem))
			{
				base.SetNextSibling(treeViewItem3, treeViewItem);
			}
		}
		if (treeViewItem2 != null)
		{
			MoveSubtree(treeViewItem, treeViewItem2);
		}
		if (oldParent != null && !oldParent.HasChildren)
		{
			oldParent.CanExpand = false;
		}
	}

	private void MoveSubtree(TreeViewItem parent, TreeViewItem child)
	{
		int siblingIndex = parent.transform.GetSiblingIndex();
		int num = child.transform.GetSiblingIndex();
		bool flag = false;
		if (siblingIndex < num)
		{
			flag = true;
		}
		TreeViewItem treeViewItem = parent;
		while (child != null && child.IsDescendantOf(parent) && !(treeViewItem == child))
		{
			base.SetNextSibling(treeViewItem, child);
			treeViewItem = child;
			if (flag)
			{
				num++;
			}
			child = (TreeViewItem)GetItemContainer(num);
		}
	}

	protected override void Drop(ItemContainer[] dragItems, ItemContainer dropTarget, ItemDropAction action)
	{
		TreeViewItem treeViewItem = (TreeViewItem)dropTarget;
		switch (action)
		{
		case ItemDropAction.SetLastChild:
		{
			for (int i = 0; i < dragItems.Length; i++)
			{
				TreeViewItem treeViewItem2 = (TreeViewItem)dragItems[i];
				if (treeViewItem != treeViewItem2)
				{
					treeViewItem2.Parent = treeViewItem;
				}
			}
			break;
		}
		case ItemDropAction.SetPrevSibling:
		{
			for (int j = 0; j < dragItems.Length; j++)
			{
				SetPrevSibling(treeViewItem, dragItems[j]);
			}
			break;
		}
		case ItemDropAction.SetNextSibling:
		{
			for (int num = dragItems.Length - 1; num >= 0; num--)
			{
				SetNextSibling(treeViewItem, dragItems[num]);
			}
			break;
		}
		}
		UpdateSelectedItemIndex();
	}

	protected override void SetNextSibling(ItemContainer sibling, ItemContainer nextSibling)
	{
		TreeViewItem treeViewItem = (TreeViewItem)sibling;
		TreeViewItem treeViewItem2 = treeViewItem.LastDescendant();
		if (treeViewItem2 == null)
		{
			treeViewItem2 = treeViewItem;
		}
		TreeViewItem treeViewItem3 = (TreeViewItem)nextSibling;
		TreeViewItem treeViewItem4 = treeViewItem3.FirstChild();
		base.SetNextSibling(treeViewItem2, nextSibling);
		if (treeViewItem4 != null)
		{
			MoveSubtree(treeViewItem3, treeViewItem4);
		}
		treeViewItem3.Parent = treeViewItem.Parent;
	}

	protected override void SetPrevSibling(ItemContainer sibling, ItemContainer prevSibling)
	{
		TreeViewItem treeViewItem = (TreeViewItem)sibling;
		TreeViewItem treeViewItem2 = (TreeViewItem)prevSibling;
		TreeViewItem treeViewItem3 = treeViewItem2.FirstChild();
		base.SetPrevSibling(sibling, prevSibling);
		if (treeViewItem3 != null)
		{
			MoveSubtree(treeViewItem2, treeViewItem3);
		}
		treeViewItem2.Parent = treeViewItem.Parent;
	}

	public void FixScrollRect()
	{
		Canvas.ForceUpdateCanvases();
		RectTransform component = Panel.GetComponent<RectTransform>();
		component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, component.rect.height - 0.01f);
	}
}
