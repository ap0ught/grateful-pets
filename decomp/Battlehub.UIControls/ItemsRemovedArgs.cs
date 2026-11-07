using System;

namespace Battlehub.UIControls;

public class ItemsRemovedArgs : EventArgs
{
	public object[] Items { get; private set; }

	public ItemsRemovedArgs(object[] items)
	{
		Items = items;
	}
}
