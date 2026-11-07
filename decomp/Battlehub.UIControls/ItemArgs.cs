using System;

namespace Battlehub.UIControls;

public class ItemArgs : EventArgs
{
	public object[] Items { get; private set; }

	public ItemArgs(object[] item)
	{
		Items = item;
	}
}
