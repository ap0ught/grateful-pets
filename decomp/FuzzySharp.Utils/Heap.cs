using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FuzzySharp.Utils;

public abstract class Heap<T> : IEnumerable<T>, IEnumerable
{
	private const int InitialCapacity = 0;

	private const int GrowFactor = 2;

	private const int MinGrow = 1;

	private int _capacity;

	private T[] _heap = new T[0];

	private int _tail;

	public int Count => _tail;

	public int Capacity => _capacity;

	protected Comparer<T> Comparer { get; }

	protected abstract bool Dominates(T x, T y);

	protected Heap()
		: this(Comparer<T>.Default)
	{
	}

	protected Heap(Comparer<T> comparer)
		: this(Enumerable.Empty<T>(), comparer)
	{
	}

	protected Heap(IEnumerable<T> collection)
		: this(collection, Comparer<T>.Default)
	{
	}

	protected Heap(IEnumerable<T> collection, Comparer<T> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		Comparer = comparer ?? throw new ArgumentNullException("comparer");
		foreach (T item in collection)
		{
			if (Count == Capacity)
			{
				Grow();
			}
			_heap[_tail++] = item;
		}
		for (int num = Parent(_tail - 1); num >= 0; num--)
		{
			BubbleDown(num);
		}
	}

	public void Add(T item)
	{
		if (Count == Capacity)
		{
			Grow();
		}
		_heap[_tail++] = item;
		BubbleUp(_tail - 1);
	}

	private void BubbleUp(int i)
	{
		while (i != 0 && !Dominates(_heap[Parent(i)], _heap[i]))
		{
			Swap(i, Parent(i));
			i = Parent(i);
		}
	}

	public T GetMin()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Heap is empty");
		}
		return _heap[0];
	}

	public T ExtractDominating()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Heap is empty");
		}
		T result = _heap[0];
		_tail--;
		Swap(_tail, 0);
		BubbleDown(0);
		return result;
	}

	private void BubbleDown(int i)
	{
		while (true)
		{
			int num = Dominating(i);
			if (num == i)
			{
				break;
			}
			Swap(i, num);
			i = num;
		}
	}

	private int Dominating(int i)
	{
		int dominatingNode = i;
		dominatingNode = GetDominating(YoungChild(i), dominatingNode);
		return GetDominating(OldChild(i), dominatingNode);
	}

	private int GetDominating(int newNode, int dominatingNode)
	{
		if (newNode < _tail && !Dominates(_heap[dominatingNode], _heap[newNode]))
		{
			return newNode;
		}
		return dominatingNode;
	}

	private void Swap(int i, int j)
	{
		T val = _heap[i];
		_heap[i] = _heap[j];
		_heap[j] = val;
	}

	private static int Parent(int i)
	{
		return (i + 1) / 2 - 1;
	}

	private static int YoungChild(int i)
	{
		return (i + 1) * 2 - 1;
	}

	private static int OldChild(int i)
	{
		return YoungChild(i) + 1;
	}

	private void Grow()
	{
		int num = _capacity * 2 + 1;
		T[] array = new T[num];
		Array.Copy(_heap, array, _capacity);
		_heap = array;
		_capacity = num;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _heap.Take(Count).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
