using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls;

public class TreeViewItem : ItemContainer
{
	private TreeViewExpander m_expander;

	[SerializeField]
	private HorizontalLayoutGroup m_itemLayout;

	private Toggle m_toggle;

	private TreeView m_treeView;

	private int m_indent;

	private TreeViewItem m_parent;

	private bool m_canExpand;

	private bool m_isExpanded;

	private TreeView TreeView
	{
		get
		{
			if (m_treeView == null)
			{
				m_treeView = GetComponentInParent<TreeView>();
			}
			return m_treeView;
		}
	}

	public int Indent => m_indent;

	public TreeViewItem Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			if (!(m_parent == value))
			{
				TreeViewItem parent = m_parent;
				m_parent = value;
				if (m_parent != null && TreeView != null && m_itemLayout != null)
				{
					m_indent = m_parent.m_indent + TreeView.Indent;
					m_itemLayout.padding = new RectOffset(m_indent, m_itemLayout.padding.right, m_itemLayout.padding.top, m_itemLayout.padding.bottom);
					int siblingIndex = transform.GetSiblingIndex();
					SetIndent(this, ref siblingIndex);
				}
				else
				{
					ZeroIndent();
				}
				if (TreeView != null && ParentChanged != null)
				{
					ParentChanged(this, new ParentChangedEventArgs(parent, m_parent));
				}
			}
		}
	}

	public override bool IsSelected
	{
		get
		{
			return base.IsSelected;
		}
		set
		{
			if (base.IsSelected != value)
			{
				m_toggle.isOn = value;
				base.IsSelected = value;
			}
		}
	}

	public bool CanExpand
	{
		get
		{
			return m_canExpand;
		}
		set
		{
			if (m_canExpand != value)
			{
				m_canExpand = value;
				if (m_expander != null)
				{
					m_expander.CanExpand = m_canExpand;
				}
				if (!m_canExpand)
				{
					IsExpanded = false;
				}
			}
		}
	}

	public bool IsExpanded
	{
		get
		{
			return m_isExpanded;
		}
		set
		{
			if (m_isExpanded == value)
			{
				return;
			}
			m_isExpanded = value && m_canExpand;
			if (m_expander != null)
			{
				m_expander.IsOn = value && m_canExpand;
			}
			if (TreeView != null)
			{
				if (m_isExpanded)
				{
					TreeView.Expand(this);
				}
				else
				{
					TreeView.Collapse(this);
				}
			}
		}
	}

	public bool HasChildren
	{
		get
		{
			int siblingIndex = transform.GetSiblingIndex();
			if (TreeView == null)
			{
				return false;
			}
			TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(siblingIndex + 1);
			if (treeViewItem != null)
			{
				return treeViewItem.Parent == this;
			}
			return false;
		}
	}

	public static event EventHandler<ParentChangedEventArgs> ParentChanged;

	private void ZeroIndent()
	{
		m_indent = 0;
		if (m_itemLayout != null)
		{
			m_itemLayout.padding = new RectOffset(m_indent, m_itemLayout.padding.right, m_itemLayout.padding.top, m_itemLayout.padding.bottom);
		}
	}

	private void SetIndent(TreeViewItem parent, ref int siblingIndex)
	{
		while (true)
		{
			TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(siblingIndex + 1);
			if (treeViewItem == null || treeViewItem.Parent != parent)
			{
				break;
			}
			treeViewItem.m_indent = parent.m_indent + TreeView.Indent;
			treeViewItem.m_itemLayout.padding.left = treeViewItem.m_indent;
			siblingIndex++;
			SetIndent(treeViewItem, ref siblingIndex);
		}
	}

	public bool IsDescendantOf(TreeViewItem ancestor)
	{
		if (ancestor == null)
		{
			return true;
		}
		if (ancestor == this)
		{
			return false;
		}
		TreeViewItem treeViewItem = this;
		while (treeViewItem != null)
		{
			if (ancestor == treeViewItem)
			{
				return true;
			}
			treeViewItem = treeViewItem.Parent;
		}
		return false;
	}

	public TreeViewItem FirstChild()
	{
		if (!HasChildren)
		{
			return null;
		}
		int siblingIndex = transform.GetSiblingIndex();
		siblingIndex++;
		return (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
	}

	public TreeViewItem NextChild(TreeViewItem currentChild)
	{
		if (currentChild == null)
		{
			throw new ArgumentNullException("currentChild");
		}
		int siblingIndex = currentChild.transform.GetSiblingIndex();
		siblingIndex++;
		TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
		while (treeViewItem != null && treeViewItem.IsDescendantOf(this))
		{
			if (treeViewItem.Parent == this)
			{
				return treeViewItem;
			}
			siblingIndex++;
			treeViewItem = (TreeViewItem)TreeView.GetItemContainer(siblingIndex);
		}
		return null;
	}

	public TreeViewItem LastChild()
	{
		if (!HasChildren)
		{
			return null;
		}
		int num = transform.GetSiblingIndex();
		TreeViewItem result = null;
		while (true)
		{
			num++;
			TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(num);
			if (treeViewItem == null || !treeViewItem.IsDescendantOf(this))
			{
				break;
			}
			if (treeViewItem.Parent == this)
			{
				result = treeViewItem;
			}
		}
		return result;
	}

	public TreeViewItem LastDescendant()
	{
		if (!HasChildren)
		{
			return null;
		}
		int num = transform.GetSiblingIndex();
		TreeViewItem result = null;
		while (true)
		{
			num++;
			TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(num);
			if (treeViewItem == null || !treeViewItem.IsDescendantOf(this))
			{
				break;
			}
			result = treeViewItem;
		}
		return result;
	}

	protected override void AwakeOverride()
	{
		m_toggle = GetComponent<Toggle>();
		m_toggle.interactable = false;
		m_toggle.isOn = IsSelected;
		m_expander = GetComponentInChildren<TreeViewExpander>();
		if (m_expander != null)
		{
			m_expander.CanExpand = m_canExpand;
		}
	}

	protected override void StartOverride()
	{
		if (TreeView != null)
		{
			m_toggle.isOn = TreeView.IsItemSelected(Item);
			m_isSelected = m_toggle.isOn;
		}
		if (Parent != null)
		{
			m_indent = Parent.m_indent + TreeView.Indent;
			m_itemLayout.padding = new RectOffset(m_indent, m_itemLayout.padding.right, m_itemLayout.padding.top, m_itemLayout.padding.bottom);
		}
		if (CanExpand && TreeView.AutoExpand)
		{
			IsExpanded = true;
		}
	}

	public override void Clear()
	{
		base.Clear();
		m_parent = null;
		ZeroIndent();
		m_isSelected = false;
		m_toggle.isOn = m_isSelected;
		m_isExpanded = false;
		m_canExpand = false;
		m_expander.IsOn = false;
		m_expander.CanExpand = false;
	}
}
