using System;
using System.Collections.Generic;

[Serializable]
public class CleanStack<T>
{
	public List<T> Items = new List<T>(64);

	public int Count => Items.Count;

	public void Clear()
	{
		Items.Clear();
	}

	public bool Contains(T Item)
	{
		if (Item == null)
		{
			int i = 0;
			for (int count = Items.Count; i < count; i++)
			{
				if (Items[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			int j = 0;
			for (int count2 = Items.Count; j < count2; j++)
			{
				T val = Items[j];
				if (val != null && val.Equals(Item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int IndexOf(T Item)
	{
		if (Item == null)
		{
			int i = 0;
			for (int count = Items.Count; i < count; i++)
			{
				if (Items[i] == null)
				{
					return i;
				}
			}
		}
		else
		{
			int j = 0;
			for (int count2 = Items.Count; j < count2; j++)
			{
				T val = Items[j];
				if (val != null && val.Equals(Item))
				{
					return j;
				}
			}
		}
		return -1;
	}

	public T Peek()
	{
		if (Items.Count == 0)
		{
			return default;
		}
		return Items[Items.Count - 1];
	}

	public void Push(T Item)
	{
		Items.Add(Item);
	}

	public T Pop()
	{
		T result = Items[Items.Count - 1];
		Items.RemoveAt(Items.Count - 1);
		return result;
	}

	public void Insert(int Index, T Item)
	{
		Items.Insert(Index, Item);
	}

	public bool InsertUnder(T Under, T Item)
	{
		int num = IndexOf(Under);
		if (num == -1)
		{
			Push(Item);
			return false;
		}
		Items.Insert(num, Item);
		return true;
	}
}
