using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Wintellect.PowerCollections;

[Serializable]
[DebuggerDisplay("{DebuggerDisplayString()}")]
public abstract class ReadOnlyCollectionBase<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ICollection
{
	public abstract int Count { get; }

	bool ICollection<T>.IsReadOnly => true;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	private void MethodModifiesCollection()
	{
		throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, Util.SimpleClassName(GetType())));
	}

	public override string ToString()
	{
		return Algorithms.ToString(this);
	}

	public virtual bool Exists(Predicate<T> predicate)
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return Algorithms.Exists(this, predicate);
	}

	public virtual bool TrueForAll(Predicate<T> predicate)
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return Algorithms.TrueForAll(this, predicate);
	}

	public virtual int CountWhere(Predicate<T> predicate)
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return Algorithms.CountWhere(this, predicate);
	}

	public IEnumerable<T> FindAll(Predicate<T> predicate)
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return Algorithms.FindWhere(this, predicate);
	}

	public virtual void ForEach(Action<T> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		Algorithms.ForEach(this, action);
	}

	public virtual IEnumerable<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
	{
		if (converter == null)
		{
			throw new ArgumentNullException("converter");
		}
		return Algorithms.Convert(this, converter);
	}

	void ICollection<T>.Add(T item)
	{
		MethodModifiesCollection();
	}

	void ICollection<T>.Clear()
	{
		MethodModifiesCollection();
	}

	bool ICollection<T>.Remove(T item)
	{
		MethodModifiesCollection();
		return false;
	}

	public virtual bool Contains(T item)
	{
		IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (equalityComparer.Equals(current, item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void CopyTo(T[] array, int arrayIndex)
	{
		int count = Count;
		if (count == 0)
		{
			return;
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", count, Strings.ArgMustNotBeNegative);
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, Strings.ArgMustNotBeNegative);
		}
		if (arrayIndex >= array.Length || count > array.Length - arrayIndex)
		{
			throw new ArgumentException("arrayIndex", Strings.ArrayTooSmall);
		}
		int num = arrayIndex;
		int num2 = 0;
		foreach (T item in (IEnumerable<T>)this)
		{
			if (num2 >= count)
			{
				break;
			}
			array[num] = item;
			num++;
			num2++;
		}
	}

	public virtual T[] ToArray()
	{
		T[] array = new T[Count];
		CopyTo(array, 0);
		return array;
	}

	public abstract IEnumerator<T> GetEnumerator();

	void ICollection.CopyTo(Array array, int index)
	{
		int count = Count;
		if (count == 0)
		{
			return;
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", count, Strings.ArgMustNotBeNegative);
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", index, Strings.ArgMustNotBeNegative);
		}
		if (index >= array.Length || count > array.Length - index)
		{
			throw new ArgumentException("index", Strings.ArrayTooSmall);
		}
		int num = 0;
		foreach (object item in (IEnumerable)this)
		{
			if (num >= count)
			{
				break;
			}
			array.SetValue(item, index);
			index++;
			num++;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			yield return current;
		}
	}

	internal string DebuggerDisplayString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('{');
		bool flag = true;
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (stringBuilder.Length >= 250)
				{
					stringBuilder.Append(",...");
					break;
				}
				if (!flag)
				{
					stringBuilder.Append(',');
				}
				if (current == null)
				{
					stringBuilder.Append("null");
				}
				else
				{
					stringBuilder.Append(current.ToString());
				}
				flag = false;
			}
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}
}
