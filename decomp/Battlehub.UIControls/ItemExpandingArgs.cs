using System;
using System.Collections;

namespace Battlehub.UIControls;

public class ItemExpandingArgs : EventArgs
{
	public object Item { get; private set; }

	public IEnumerable Children { get; set; }

	public ItemExpandingArgs(object item)
	{
		Item = item;
	}
}
