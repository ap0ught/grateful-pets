using System;
using System.Collections.Generic;

[Serializable]
public class CleanDictionary<TKey, TValue>
{
	public List<TKey> Keys = new List<TKey>();

	public List<TValue> Values = new List<TValue>();

	public int Count => Keys.Count;

	public TValue this[TKey Key]
	{
		get
		{
			int num = LocateValue(Key);
			if (num == -1)
			{
				throw new KeyNotFoundException("Couldn't find " + Key.ToString());
			}
			return Values[num];
		}
		set
		{
			int num = LocateValue(Key);
			if (num == -1)
			{
				throw new KeyNotFoundException("Couldn't find " + Key.ToString());
			}
			Values[num] = value;
		}
	}

	public void Clear()
	{
		if (Keys.Count > 0)
		{
			Keys.Clear();
		}
		if (Values.Count > 0)
		{
			Values.Clear();
		}
	}

	public void Add(TKey K, TValue V)
	{
		if (LocateValue(K) == -1)
		{
			Keys.Add(K);
			Values.Add(V);
			return;
		}
		throw new ArgumentException("An element with the same key " + K.ToString() + " already exists in the dictionary.");
	}

	public bool ContainsKey(TKey Item)
	{
		if (Item == null)
		{
			for (int i = 0; i < Keys.Count; i++)
			{
				if (Keys[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < Keys.Count; j++)
			{
				if (Keys[j] != null && Keys[j].Equals(Item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ContainsValue(TValue Item)
	{
		if (Item == null)
		{
			for (int i = 0; i < Keys.Count; i++)
			{
				if (Keys[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < Keys.Count; j++)
			{
				if (Keys[j] != null && Keys[j].Equals(Item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool TryGetValue(TKey Key, out TValue Value)
	{
		int num = LocateValue(Key);
		if (num == -1)
		{
			Value = default;
			return false;
		}
		Value = Values[num];
		return true;
	}

	public int LocateValue(TKey Key)
	{
		if (Key == null)
		{
			for (int i = 0; i < Keys.Count; i++)
			{
				if (Keys[i] == null)
				{
					return i;
				}
			}
		}
		else
		{
			for (int j = 0; j < Keys.Count; j++)
			{
				if (Keys[j] != null && Keys[j].Equals(Key))
				{
					return j;
				}
			}
		}
		return -1;
	}
}
