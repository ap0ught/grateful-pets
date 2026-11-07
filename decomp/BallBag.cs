using System;
using System.Collections;
using System.Collections.Generic;
using XRL.Rules;

public class BallBag<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	public List<T> Items;

	public List<int> TotalWeights;

	public int TotalWeight;

	public Random Random;

	public int Count => Items.Count;

	public T this[int Index] => Items[Index];

	public int this[T Item]
	{
		get
		{
			int num = Items.IndexOf(Item);
			if (num == -1)
			{
				throw new KeyNotFoundException();
			}
			if (num > 0)
			{
				return TotalWeights[num] - TotalWeights[num - 1];
			}
			return TotalWeights[0];
		}
		set
		{
			Add(Item, value);
		}
	}

	public BallBag(int Capacity = 0)
	{
		Random = Stat.Rnd;
		Items = new List<T>(Capacity);
		TotalWeights = new List<int>(Capacity);
	}

	public BallBag(Random Random, int Capacity = 0)
	{
		this.Random = Random;
		Items = new List<T>(Capacity);
		TotalWeights = new List<int>(Capacity);
	}

	public void Add(T Item, int Weight)
	{
		if (Weight > 0)
		{
			Items.Add(Item);
			TotalWeight += Weight;
			TotalWeights.Add(TotalWeight);
		}
	}

	public void AddRange(IEnumerable<T> Items, Func<T, int> Weight)
	{
		foreach (T Item in Items)
		{
			Add(Item, Weight(Item));
		}
	}

	public bool Remove(T Item)
	{
		int num = Items.IndexOf(Item);
		if (num < 0)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public T RemoveAt(int Index)
	{
		int weight = GetWeight(Index);
		T result = Items[Index];
		Items.RemoveAt(Index);
		TotalWeights.RemoveAt(Index);
		for (int i = Index; i < Items.Count; i++)
		{
			TotalWeights[i] -= weight;
		}
		TotalWeight -= weight;
		return result;
	}

	public int GetWeight(int Index)
	{
		if (Index <= 0)
		{
			return TotalWeights[Index];
		}
		return TotalWeights[Index] - TotalWeights[Index - 1];
	}

	private int Roll()
	{
		int num = Random.Next(0, TotalWeight + 1);
		int num2 = 0;
		int num3 = Items.Count - 1;
		while (num3 > num2)
		{
			int num4 = (num2 + num3) / 2;
			if (TotalWeights[num4] < num)
			{
				num2 = num4 + 1;
				continue;
			}
			if (num4 > 0 && TotalWeights[num4 - 1] > num)
			{
				num3 = num4 - 1;
				continue;
			}
			return num4;
		}
		return (num2 + num3) / 2;
	}

	public void Clear()
	{
		Items.Clear();
		TotalWeights.Clear();
		TotalWeight = 0;
	}

	public T PeekOne()
	{
		if (Items.Count != 0)
		{
			return Items[Roll()];
		}
		return default;
	}

	public List<T> Peek(int Amount)
	{
		List<T> list = new List<T>(Amount);
		for (int i = 0; i < Amount; i++)
		{
			list.Add(PeekOne());
		}
		return list;
	}

	public T PluckOne()
	{
		int num = Roll();
		T result = Items[num];
		int num2 = num;
		while (num2 < Items.Count)
		{
			int num3 = TotalWeights[num2]--;
			if (num2 > 0)
			{
				num3 -= TotalWeights[num2 - 1];
			}
			if (num3 == 0)
			{
				Items.RemoveAt(num2);
				TotalWeights.RemoveAt(num2);
			}
			else
			{
				num2++;
			}
		}
		TotalWeight--;
		return result;
	}

	public List<T> Pluck(int Amount)
	{
		List<T> list = new List<T>(Amount);
		for (int i = 0; i < Amount; i++)
		{
			list.Add(PluckOne());
		}
		return list;
	}

	public T PickOne()
	{
		return RemoveAt(Roll());
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Items.GetEnumerator();
	}
}
