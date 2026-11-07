using System;
using System.Collections.Generic;

namespace Battlehub.UIControls;

public class ItemsCancelArgs : EventArgs
{
	public List<object> Items { get; set; }

	public ItemsCancelArgs(List<object> items)
	{
		Items = items;
	}
}
