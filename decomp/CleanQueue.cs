using System;
using System.Collections.Generic;

[Serializable]
public class CleanQueue<T>
{
	public List<T> Items = new List<T>(1000);

	public int Count => Items.Count;

	public void Clear()
	{
		Items.Clear();
	}

	public bool Contains(T Item)
	{
		if (Item == null)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < Items.Count; j++)
			{
				if (Items[j] != null && Items[j].Equals(Item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public T Peek()
	{
		if (Items.Count == 0)
		{
			return default;
		}
		return Items[0];
	}

	public void Enqueue(T Item)
	{
		Items.Add(Item);
	}

	public T Dequeue()
	{
		T result = Items[0];
		Items.RemoveAt(0);
		return result;
	}
}
