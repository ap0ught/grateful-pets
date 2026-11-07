using System;

namespace Battlehub.UIControls;

public class SelectionChangedArgs : EventArgs
{
	public object[] OldItems { get; private set; }

	public object[] NewItems { get; private set; }

	public object OldItem
	{
		get
		{
			if (OldItems == null)
			{
				return null;
			}
			if (OldItems.Length == 0)
			{
				return null;
			}
			return OldItems[0];
		}
	}

	public object NewItem
	{
		get
		{
			if (NewItems == null)
			{
				return null;
			}
			if (NewItems.Length == 0)
			{
				return null;
			}
			return NewItems[0];
		}
	}

	public SelectionChangedArgs(object[] oldItems, object[] newItems)
	{
		OldItems = oldItems;
		NewItems = newItems;
	}

	public SelectionChangedArgs(object oldItem, object newItem)
	{
		OldItems = new object[1] { oldItem };
		NewItems = new object[1] { newItem };
	}
}
