using System;

namespace Battlehub.UIControls;

public class ItemDropArgs : EventArgs
{
	public object[] DragItems { get; private set; }

	public object DropTarget { get; private set; }

	public ItemDropAction Action { get; private set; }

	public bool IsExternal { get; private set; }

	public ItemDropArgs(object[] dragItems, object dropTarget, ItemDropAction action, bool isExternal)
	{
		DragItems = dragItems;
		DropTarget = dropTarget;
		Action = action;
		IsExternal = isExternal;
	}
}
