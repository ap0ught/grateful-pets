using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls;

public class ExternalDragItem : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IDropHandler, IEndDragHandler
{
	public TreeView TreeView;

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		TreeView.ExternalBeginDrag(eventData.position);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		TreeView.ExternalItemDrag(eventData.position);
	}

	void IDropHandler.OnDrop(PointerEventData eventData)
	{
		if (TreeView.DropTarget != null)
		{
			TreeView.AddChild(TreeView.DropTarget, new GameObject());
		}
		TreeView.ExternalItemDrop();
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (TreeView.DropTarget != null)
		{
			GameObject gameObject = (GameObject)TreeView.DropTarget;
			GameObject gameObject2 = new GameObject();
			TreeViewItem treeViewItem = (TreeViewItem)TreeView.GetItemContainer(TreeView.DropTarget);
			if (TreeView.DropAction == ItemDropAction.SetLastChild)
			{
				gameObject2.transform.SetParent(gameObject.transform);
				TreeView.AddChild(TreeView.DropTarget, gameObject2);
				treeViewItem.CanExpand = true;
				treeViewItem.IsExpanded = true;
			}
			else if (TreeView.DropAction != ItemDropAction.None)
			{
				int num = ((TreeView.DropAction != ItemDropAction.SetNextSibling) ? TreeView.IndexOf(gameObject) : (TreeView.IndexOf(gameObject) + 1));
				gameObject2.transform.SetParent(gameObject.transform.parent);
				gameObject2.transform.SetSiblingIndex(num);
				((TreeViewItem)TreeView.Insert(num, gameObject2)).Parent = treeViewItem.Parent;
			}
		}
		TreeView.ExternalItemDrop();
	}
}
