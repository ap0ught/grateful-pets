using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Genkit;

namespace Wintellect.PowerCollections;

public static class Algorithms
{
	[Serializable]
	private class ListRange<T> : ListBase<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private IList<T> wrappedList;

		private int start;

		private int count;

		public override int Count => Math.Min(count, wrappedList.Count - start);

		public override T this[int index]
		{
			get
			{
				if (index < 0 || index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return wrappedList[start + index];
			}
			set
			{
				if (index < 0 || index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				wrappedList[start + index] = value;
			}
		}

		bool ICollection<T>.IsReadOnly => wrappedList.IsReadOnly;

		public ListRange(IList<T> wrappedList, int start, int count)
		{
			this.wrappedList = wrappedList;
			this.start = start;
			this.count = count;
		}

		public override void Clear()
		{
			if (wrappedList.Count - start < count)
			{
				count = wrappedList.Count - start;
			}
			while (count > 0)
			{
				wrappedList.RemoveAt(start + count - 1);
				count--;
			}
		}

		public override void Insert(int index, T item)
		{
			if (index < 0 || index > count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			wrappedList.Insert(start + index, item);
			count++;
		}

		public override void RemoveAt(int index)
		{
			if (index < 0 || index >= count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			wrappedList.RemoveAt(start + index);
			count--;
		}

		public override bool Remove(T item)
		{
			if (wrappedList.IsReadOnly)
			{
				throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "Range"));
			}
			return base.Remove(item);
		}
	}

	[Serializable]
	private class ArrayRange<T> : ListBase<T>
	{
		private T[] wrappedArray;

		private int start;

		private int count;

		public override int Count => count;

		public override T this[int index]
		{
			get
			{
				if (index < 0 || index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return wrappedArray[start + index];
			}
			set
			{
				if (index < 0 || index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				wrappedArray[start + index] = value;
			}
		}

		public ArrayRange(T[] wrappedArray, int start, int count)
		{
			this.wrappedArray = wrappedArray;
			this.start = start;
			this.count = count;
		}

		public override void Clear()
		{
			Array.Copy(wrappedArray, start + count, wrappedArray, start, wrappedArray.Length - (start + count));
			FillRange(wrappedArray, wrappedArray.Length - count, count, default);
			count = 0;
		}

		public override void Insert(int index, T item)
		{
			if (index < 0 || index > count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			int num = start + index;
			if (num + 1 < wrappedArray.Length)
			{
				Array.Copy(wrappedArray, num, wrappedArray, num + 1, wrappedArray.Length - num - 1);
			}
			if (num < wrappedArray.Length)
			{
				wrappedArray[num] = item;
			}
			if (start + count < wrappedArray.Length)
			{
				count++;
			}
		}

		public override void RemoveAt(int index)
		{
			if (index < 0 || index >= count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			int num = start + index;
			if (num < wrappedArray.Length - 1)
			{
				Array.Copy(wrappedArray, num + 1, wrappedArray, num, wrappedArray.Length - num - 1);
			}
			wrappedArray[wrappedArray.Length - 1] = default;
			count--;
		}
	}

	[Serializable]
	private class ReadOnlyCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private ICollection<T> wrappedCollection;

		public int Count => wrappedCollection.Count;

		public bool IsReadOnly => true;

		public ReadOnlyCollection(ICollection<T> wrappedCollection)
		{
			this.wrappedCollection = wrappedCollection;
		}

		private void MethodModifiesCollection()
		{
			throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "read-only collection"));
		}

		public IEnumerator<T> GetEnumerator()
		{
			return wrappedCollection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)wrappedCollection).GetEnumerator();
		}

		public bool Contains(T item)
		{
			return wrappedCollection.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			wrappedCollection.CopyTo(array, arrayIndex);
		}

		public void Add(T item)
		{
			MethodModifiesCollection();
		}

		public void Clear()
		{
			MethodModifiesCollection();
		}

		public bool Remove(T item)
		{
			MethodModifiesCollection();
			return false;
		}
	}

	[Serializable]
	private class ReadOnlyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private IList<T> wrappedList;

		public int Count => wrappedList.Count;

		public bool IsReadOnly => true;

		public T this[int index]
		{
			get
			{
				return wrappedList[index];
			}
			set
			{
				MethodModifiesCollection();
			}
		}

		public ReadOnlyList(IList<T> wrappedList)
		{
			this.wrappedList = wrappedList;
		}

		private void MethodModifiesCollection()
		{
			throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "read-only list"));
		}

		public IEnumerator<T> GetEnumerator()
		{
			return wrappedList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)wrappedList).GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return wrappedList.IndexOf(item);
		}

		public bool Contains(T item)
		{
			return wrappedList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			wrappedList.CopyTo(array, arrayIndex);
		}

		public void Add(T item)
		{
			MethodModifiesCollection();
		}

		public void Clear()
		{
			MethodModifiesCollection();
		}

		public void Insert(int index, T item)
		{
			MethodModifiesCollection();
		}

		public void RemoveAt(int index)
		{
			MethodModifiesCollection();
		}

		public bool Remove(T item)
		{
			MethodModifiesCollection();
			return false;
		}
	}

	[Serializable]
	private class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		private IDictionary<TKey, TValue> wrappedDictionary;

		public ICollection<TKey> Keys => ReadOnly(wrappedDictionary.Keys);

		public ICollection<TValue> Values => ReadOnly(wrappedDictionary.Values);

		public TValue this[TKey key]
		{
			get
			{
				return wrappedDictionary[key];
			}
			set
			{
				MethodModifiesCollection();
			}
		}

		public int Count => wrappedDictionary.Count;

		public bool IsReadOnly => true;

		public ReadOnlyDictionary(IDictionary<TKey, TValue> wrappedDictionary)
		{
			this.wrappedDictionary = wrappedDictionary;
		}

		private void MethodModifiesCollection()
		{
			throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "read-only dictionary"));
		}

		public void Add(TKey key, TValue value)
		{
			MethodModifiesCollection();
		}

		public bool ContainsKey(TKey key)
		{
			return wrappedDictionary.ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			MethodModifiesCollection();
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return wrappedDictionary.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			MethodModifiesCollection();
		}

		public void Clear()
		{
			MethodModifiesCollection();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return wrappedDictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			wrappedDictionary.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			MethodModifiesCollection();
			return false;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return wrappedDictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)wrappedDictionary).GetEnumerator();
		}
	}

	[Serializable]
	private class TypedEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
	{
		private IEnumerator wrappedEnumerator;

		T IEnumerator<T>.Current => (T)wrappedEnumerator.Current;

		object IEnumerator.Current => wrappedEnumerator.Current;

		public TypedEnumerator(IEnumerator wrappedEnumerator)
		{
			this.wrappedEnumerator = wrappedEnumerator;
		}

		void IDisposable.Dispose()
		{
			if (wrappedEnumerator is IDisposable)
			{
				((IDisposable)wrappedEnumerator).Dispose();
			}
		}

		bool IEnumerator.MoveNext()
		{
			return wrappedEnumerator.MoveNext();
		}

		void IEnumerator.Reset()
		{
			wrappedEnumerator.Reset();
		}
	}

	[Serializable]
	private class TypedEnumerable<T> : IEnumerable<T>, IEnumerable
	{
		private IEnumerable wrappedEnumerable;

		public TypedEnumerable(IEnumerable wrappedEnumerable)
		{
			this.wrappedEnumerable = wrappedEnumerable;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new TypedEnumerator<T>(wrappedEnumerable.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return wrappedEnumerable.GetEnumerator();
		}
	}

	[Serializable]
	private class TypedCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private ICollection wrappedCollection;

		public int Count => wrappedCollection.Count;

		public bool IsReadOnly => true;

		public TypedCollection(ICollection wrappedCollection)
		{
			this.wrappedCollection = wrappedCollection;
		}

		private void MethodModifiesCollection()
		{
			throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "strongly-typed Collection"));
		}

		public void Add(T item)
		{
			MethodModifiesCollection();
		}

		public void Clear()
		{
			MethodModifiesCollection();
		}

		public bool Remove(T item)
		{
			MethodModifiesCollection();
			return false;
		}

		public bool Contains(T item)
		{
			IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			foreach (object item2 in wrappedCollection)
			{
				if (item2 is T && equalityComparer.Equals(item, (T)item2))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			wrappedCollection.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new TypedEnumerator<T>(wrappedCollection.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return wrappedCollection.GetEnumerator();
		}
	}

	[Serializable]
	private class TypedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private IList wrappedList;

		public T this[int index]
		{
			get
			{
				return (T)wrappedList[index];
			}
			set
			{
				wrappedList[index] = value;
			}
		}

		public int Count => wrappedList.Count;

		public bool IsReadOnly => wrappedList.IsReadOnly;

		public TypedList(IList wrappedList)
		{
			this.wrappedList = wrappedList;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new TypedEnumerator<T>(wrappedList.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return wrappedList.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return wrappedList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			wrappedList.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			wrappedList.RemoveAt(index);
		}

		public void Add(T item)
		{
			wrappedList.Add(item);
		}

		public void Clear()
		{
			wrappedList.Clear();
		}

		public bool Contains(T item)
		{
			return wrappedList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			wrappedList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			if (wrappedList.Contains(item))
			{
				wrappedList.Remove(item);
				return true;
			}
			return false;
		}
	}

	[Serializable]
	private class UntypedCollection<T> : ICollection, IEnumerable
	{
		private ICollection<T> wrappedCollection;

		public int Count => wrappedCollection.Count;

		public bool IsSynchronized => false;

		public object SyncRoot => this;

		public UntypedCollection(ICollection<T> wrappedCollection)
		{
			this.wrappedCollection = wrappedCollection;
		}

		public void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int num = 0;
			int count = wrappedCollection.Count;
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, Strings.ArgMustNotBeNegative);
			}
			if (index >= array.Length || count > array.Length - index)
			{
				throw new ArgumentException("index", Strings.ArrayTooSmall);
			}
			foreach (T item in wrappedCollection)
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

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable)wrappedCollection).GetEnumerator();
		}
	}

	[Serializable]
	private class UntypedList<T> : IList, ICollection, IEnumerable
	{
		private IList<T> wrappedList;

		public bool IsFixedSize => false;

		public bool IsReadOnly => wrappedList.IsReadOnly;

		public object this[int index]
		{
			get
			{
				return wrappedList[index];
			}
			set
			{
				wrappedList[index] = ConvertToItemType("value", value);
			}
		}

		public int Count => wrappedList.Count;

		public bool IsSynchronized => false;

		public object SyncRoot => this;

		public UntypedList(IList<T> wrappedList)
		{
			this.wrappedList = wrappedList;
		}

		private T ConvertToItemType(string name, object value)
		{
			try
			{
				return (T)value;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(string.Format(Strings.WrongType, value, typeof(T)), name);
			}
		}

		public int Add(object value)
		{
			wrappedList.Add(ConvertToItemType("value", value));
			return wrappedList.Count - 1;
		}

		public void Clear()
		{
			wrappedList.Clear();
		}

		public bool Contains(object value)
		{
			if (value is T)
			{
				return wrappedList.Contains((T)value);
			}
			return false;
		}

		public int IndexOf(object value)
		{
			if (value is T)
			{
				return wrappedList.IndexOf((T)value);
			}
			return -1;
		}

		public void Insert(int index, object value)
		{
			wrappedList.Insert(index, ConvertToItemType("value", value));
		}

		public void Remove(object value)
		{
			if (value is T)
			{
				wrappedList.Remove((T)value);
			}
		}

		public void RemoveAt(int index)
		{
			wrappedList.RemoveAt(index);
		}

		public void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int num = 0;
			int count = wrappedList.Count;
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, Strings.ArgMustNotBeNegative);
			}
			if (index >= array.Length || count > array.Length - index)
			{
				throw new ArgumentException("index", Strings.ArrayTooSmall);
			}
			foreach (T wrapped in wrappedList)
			{
				if (num >= count)
				{
					break;
				}
				array.SetValue(wrapped, index);
				index++;
				num++;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable)wrappedList).GetEnumerator();
		}
	}

	[Serializable]
	private class ArrayWrapper<T> : ListBase<T>, IList, ICollection, IEnumerable
	{
		private T[] wrappedArray;

		public override int Count => wrappedArray.Length;

		public override T this[int index]
		{
			get
			{
				if (index < 0 || index >= wrappedArray.Length)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return wrappedArray[index];
			}
			set
			{
				if (index < 0 || index >= wrappedArray.Length)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				wrappedArray[index] = value;
			}
		}

		bool IList.IsFixedSize => true;

		public ArrayWrapper(T[] wrappedArray)
		{
			this.wrappedArray = wrappedArray;
		}

		public override void Clear()
		{
			int num = wrappedArray.Length;
			for (int i = 0; i < num; i++)
			{
				wrappedArray[i] = default;
			}
		}

		public override void Insert(int index, T item)
		{
			if (index < 0 || index > wrappedArray.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (index + 1 < wrappedArray.Length)
			{
				Array.Copy(wrappedArray, index, wrappedArray, index + 1, wrappedArray.Length - index - 1);
			}
			if (index < wrappedArray.Length)
			{
				wrappedArray[index] = item;
			}
		}

		public override void RemoveAt(int index)
		{
			if (index < 0 || index >= wrappedArray.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (index < wrappedArray.Length - 1)
			{
				Array.Copy(wrappedArray, index + 1, wrappedArray, index, wrappedArray.Length - index - 1);
			}
			wrappedArray[wrappedArray.Length - 1] = default;
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Length < wrappedArray.Length)
			{
				throw new ArgumentException("array is too short", "array");
			}
			if (arrayIndex < 0 || arrayIndex >= array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (array.Length + arrayIndex < wrappedArray.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			Array.Copy(wrappedArray, 0, array, arrayIndex, wrappedArray.Length);
		}

		public override IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)wrappedArray).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)wrappedArray).GetEnumerator();
		}
	}

	[Serializable]
	private class LexicographicalComparerClass<T> : IComparer<IEnumerable<T>>
	{
		private IComparer<T> itemComparer;

		public LexicographicalComparerClass(IComparer<T> itemComparer)
		{
			this.itemComparer = itemComparer;
		}

		public int Compare(IEnumerable<T> x, IEnumerable<T> y)
		{
			return LexicographicalCompare(x, y, itemComparer);
		}

		public override bool Equals(object obj)
		{
			if (obj is LexicographicalComparerClass<T>)
			{
				return itemComparer.Equals(((LexicographicalComparerClass<T>)obj).itemComparer);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return itemComparer.GetHashCode();
		}
	}

	[Serializable]
	private class ReverseComparerClass<T> : IComparer<T>
	{
		private IComparer<T> comparer;

		public ReverseComparerClass(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public int Compare(T x, T y)
		{
			return -comparer.Compare(x, y);
		}

		public override bool Equals(object obj)
		{
			if (obj is ReverseComparerClass<T>)
			{
				return comparer.Equals(((ReverseComparerClass<T>)obj).comparer);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return comparer.GetHashCode();
		}
	}

	[Serializable]
	private class IdentityComparer<T> : IEqualityComparer<T> where T : class
	{
		public bool Equals(T x, T y)
		{
			return x == y;
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}

		public override bool Equals(object obj)
		{
			if (obj != null)
			{
				return obj is IdentityComparer<T>;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 1900273135;
		}
	}

	[Serializable]
	private class CollectionEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
	{
		private IEqualityComparer<T> equalityComparer;

		public CollectionEqualityComparer(IEqualityComparer<T> equalityComparer)
		{
			this.equalityComparer = equalityComparer;
		}

		public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
		{
			return EqualCollections(x, y, equalityComparer);
		}

		public int GetHashCode(IEnumerable<T> obj)
		{
			int num = 927934782;
			foreach (T item in obj)
			{
				int hashCode = Util.GetHashCode(item, equalityComparer);
				num += hashCode;
				num = (num << 9) | (num >> 23);
			}
			return num & 0x7FFFFFFF;
		}
	}

	[Serializable]
	private class SetEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
	{
		private IEqualityComparer<T> equalityComparer;

		public SetEqualityComparer(IEqualityComparer<T> equalityComparer)
		{
			this.equalityComparer = equalityComparer;
		}

		public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
		{
			return EqualSets(x, y, equalityComparer);
		}

		public int GetHashCode(IEnumerable<T> obj)
		{
			int num = 1649354556;
			foreach (T item in obj)
			{
				int hashCode = Util.GetHashCode(item, equalityComparer);
				num += hashCode;
			}
			return num & 0x7FFFFFFF;
		}
	}

	public static IList<T> Range<T>(IList<T> list, int start, int count)
	{
		if (list == null)
		{
			throw new ArgumentOutOfRangeException("list");
		}
		if (start < 0 || start > list.Count || (start == list.Count && count != 0))
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (count < 0 || count > list.Count || count + start > list.Count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return new ListRange<T>(list, start, count);
	}

	public static IList<T> Range<T>(T[] array, int start, int count)
	{
		if (array == null)
		{
			throw new ArgumentOutOfRangeException("array");
		}
		if (start < 0 || start > array.Length || (start == array.Length && count != 0))
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (count < 0 || count > array.Length || count + start > array.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return new ArrayRange<T>(array, start, count);
	}

	public static ICollection<T> ReadOnly<T>(ICollection<T> collection)
	{
		if (collection == null)
		{
			return null;
		}
		return new ReadOnlyCollection<T>(collection);
	}

	public static IList<T> ReadOnly<T>(IList<T> list)
	{
		if (list == null)
		{
			return null;
		}
		if (list.IsReadOnly)
		{
			return list;
		}
		return new ReadOnlyList<T>(list);
	}

	public static IDictionary<TKey, TValue> ReadOnly<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
	{
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.IsReadOnly)
		{
			return dictionary;
		}
		return new ReadOnlyDictionary<TKey, TValue>(dictionary);
	}

	public static IEnumerable<T> TypedAs<T>(IEnumerable untypedCollection)
	{
		if (untypedCollection == null)
		{
			return null;
		}
		if (untypedCollection is IEnumerable<T>)
		{
			return (IEnumerable<T>)untypedCollection;
		}
		return new TypedEnumerable<T>(untypedCollection);
	}

	public static ICollection<T> TypedAs<T>(ICollection untypedCollection)
	{
		if (untypedCollection == null)
		{
			return null;
		}
		if (untypedCollection is ICollection<T>)
		{
			return (ICollection<T>)untypedCollection;
		}
		return new TypedCollection<T>(untypedCollection);
	}

	public static IList<T> TypedAs<T>(IList untypedList)
	{
		if (untypedList == null)
		{
			return null;
		}
		if (untypedList is IList<T>)
		{
			return (IList<T>)untypedList;
		}
		return new TypedList<T>(untypedList);
	}

	public static ICollection Untyped<T>(ICollection<T> typedCollection)
	{
		if (typedCollection == null)
		{
			return null;
		}
		if (typedCollection is ICollection)
		{
			return (ICollection)typedCollection;
		}
		return new UntypedCollection<T>(typedCollection);
	}

	public static IList Untyped<T>(IList<T> typedList)
	{
		if (typedList == null)
		{
			return null;
		}
		if (typedList is IList)
		{
			return (IList)typedList;
		}
		return new UntypedList<T>(typedList);
	}

	public static IList<T> ReadWriteList<T>(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return new ArrayWrapper<T>(array);
	}

	public static IEnumerable<T> Replace<T>(IEnumerable<T> collection, T itemFind, T replaceWith)
	{
		return Replace(collection, itemFind, replaceWith, EqualityComparer<T>.Default);
	}

	public static IEnumerable<T> Replace<T>(IEnumerable<T> collection, T itemFind, T replaceWith, IEqualityComparer<T> equalityComparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		foreach (T item in collection)
		{
			if (equalityComparer.Equals(item, itemFind))
			{
				yield return replaceWith;
			}
			else
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> Replace<T>(IEnumerable<T> collection, Predicate<T> predicate, T replaceWith)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				yield return replaceWith;
			}
			else
			{
				yield return item;
			}
		}
	}

	public static void ReplaceInPlace<T>(IList<T> list, T itemFind, T replaceWith)
	{
		ReplaceInPlace(list, itemFind, replaceWith, EqualityComparer<T>.Default);
	}

	public static void ReplaceInPlace<T>(IList<T> list, T itemFind, T replaceWith, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (equalityComparer.Equals(list[i], itemFind))
			{
				list[i] = replaceWith;
			}
		}
	}

	public static void ReplaceInPlace<T>(IList<T> list, Predicate<T> predicate, T replaceWith)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (predicate(list[i]))
			{
				list[i] = replaceWith;
			}
		}
	}

	public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> collection)
	{
		return RemoveDuplicates(collection, EqualityComparer<T>.Default);
	}

	public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		return RemoveDuplicates(collection, equalityComparer.Equals);
	}

	public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> collection, BinaryPredicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		T current = default;
		bool flag = true;
		foreach (T item in collection)
		{
			if (flag || !predicate(current, item))
			{
				current = item;
				yield return item;
			}
			flag = false;
		}
	}

	public static void RemoveDuplicatesInPlace<T>(IList<T> list)
	{
		RemoveDuplicatesInPlace(list, EqualityComparer<T>.Default);
	}

	public static void RemoveDuplicatesInPlace<T>(IList<T> list, IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		RemoveDuplicatesInPlace(list, equalityComparer.Equals);
	}

	public static void RemoveDuplicatesInPlace<T>(IList<T> list, BinaryPredicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		T val = default;
		int num = -1;
		int i = 0;
		int num2;
		for (num2 = list.Count; i < num2; i++)
		{
			T val2 = list[i];
			if (num < 0 || !predicate(val, val2))
			{
				val = val2;
				num++;
				if (num != i)
				{
					list[num] = val;
				}
			}
		}
		num++;
		if (num >= num2)
		{
			return;
		}
		if (list is ArrayWrapper<T> || (list is IList && ((IList)list).IsFixedSize))
		{
			while (num < num2)
			{
				list[num++] = default;
			}
			return;
		}
		while (num < num2)
		{
			list.RemoveAt(num2 - 1);
			num2--;
		}
	}

	public static int FirstConsecutiveEqual<T>(IList<T> list, int count)
	{
		return FirstConsecutiveEqual(list, count, EqualityComparer<T>.Default);
	}

	public static int FirstConsecutiveEqual<T>(IList<T> list, int count, IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		return FirstConsecutiveEqual(list, count, equalityComparer.Equals);
	}

	public static int FirstConsecutiveEqual<T>(IList<T> list, int count, BinaryPredicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (count < 1)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (list.Count < count)
		{
			return -1;
		}
		if (count == 1)
		{
			return 0;
		}
		int result = 0;
		int num = 0;
		T item = default;
		int num2 = 0;
		foreach (T item2 in list)
		{
			if (num > 0 && predicate(item, item2))
			{
				num2++;
				if (num2 >= count)
				{
					return result;
				}
			}
			else
			{
				item = item2;
				result = num;
				num2 = 1;
			}
			num++;
		}
		return -1;
	}

	public static int FirstConsecutiveWhere<T>(IList<T> list, int count, Predicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (count < 1)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		int count2 = list.Count;
		if (count > count2)
		{
			return -1;
		}
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		foreach (T item in list)
		{
			if (predicate(item))
			{
				if (num2 < 0)
				{
					num2 = num;
				}
				num3++;
				if (num3 >= count)
				{
					return num2;
				}
			}
			else
			{
				num3 = 0;
				num2 = -1;
			}
			num++;
		}
		return -1;
	}

	public static T FindFirstWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (TryFindFirstWhere(collection, predicate, out var foundItem))
		{
			return foundItem;
		}
		return default;
	}

	public static bool TryFindFirstWhere<T>(IEnumerable<T> collection, Predicate<T> predicate, out T foundItem)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				foundItem = item;
				return true;
			}
		}
		foundItem = default;
		return false;
	}

	public static T FindLastWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (TryFindLastWhere(collection, predicate, out var foundItem))
		{
			return foundItem;
		}
		return default;
	}

	public static bool TryFindLastWhere<T>(IEnumerable<T> collection, Predicate<T> predicate, out T foundItem)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (collection is IList<T> list)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				T val = list[num];
				if (predicate(val))
				{
					foundItem = val;
					return true;
				}
			}
			foundItem = default;
			return false;
		}
		bool result = false;
		foundItem = default;
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				foundItem = item;
				result = true;
			}
		}
		return result;
	}

	public static IEnumerable<T> FindWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				yield return item;
			}
		}
	}

	public static int FindFirstIndexWhere<T>(IList<T> list, Predicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in list)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static int FindLastIndexWhere<T>(IList<T> list, Predicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (predicate(list[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static IEnumerable<int> FindIndicesWhere<T>(IList<T> list, Predicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int index = 0;
		foreach (T item in list)
		{
			if (predicate(item))
			{
				yield return index;
			}
			int num = index + 1;
			index = num;
		}
	}

	public static int FirstIndexOf<T>(IList<T> list, T item)
	{
		return FirstIndexOf(list, item, EqualityComparer<T>.Default);
	}

	public static int FirstIndexOf<T>(IList<T> list, T item, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		int num = 0;
		foreach (T item2 in list)
		{
			if (equalityComparer.Equals(item2, item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static int LastIndexOf<T>(IList<T> list, T item)
	{
		return LastIndexOf(list, item, EqualityComparer<T>.Default);
	}

	public static int LastIndexOf<T>(IList<T> list, T item, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (equalityComparer.Equals(list[num], item))
			{
				return num;
			}
		}
		return -1;
	}

	public static IEnumerable<int> IndicesOf<T>(IList<T> list, T item)
	{
		return IndicesOf(list, item, EqualityComparer<T>.Default);
	}

	public static IEnumerable<int> IndicesOf<T>(IList<T> list, T item, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		int index = 0;
		foreach (T item2 in list)
		{
			if (equalityComparer.Equals(item2, item))
			{
				yield return index;
			}
			int num = index + 1;
			index = num;
		}
	}

	public static int FirstIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor)
	{
		return FirstIndexOfMany(list, itemsToLookFor, EqualityComparer<T>.Default);
	}

	public static int FirstIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (itemsToLookFor == null)
		{
			throw new ArgumentNullException("itemsToLookFor");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		Set<T> set = new Set<T>(itemsToLookFor, equalityComparer);
		int num = 0;
		foreach (T item in list)
		{
			if (set.Contains(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static int FirstIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, BinaryPredicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (itemsToLookFor == null)
		{
			throw new ArgumentNullException("itemsToLookFor");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in list)
		{
			foreach (T item2 in itemsToLookFor)
			{
				if (predicate(item, item2))
				{
					return num;
				}
			}
			num++;
		}
		return -1;
	}

	public static int LastIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor)
	{
		return LastIndexOfMany(list, itemsToLookFor, EqualityComparer<T>.Default);
	}

	public static int LastIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (itemsToLookFor == null)
		{
			throw new ArgumentNullException("itemsToLookFor");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		Set<T> set = new Set<T>(itemsToLookFor, equalityComparer);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (set.Contains(list[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static int LastIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, BinaryPredicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (itemsToLookFor == null)
		{
			throw new ArgumentNullException("itemsToLookFor");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			foreach (T item in itemsToLookFor)
			{
				if (predicate(list[num], item))
				{
					return num;
				}
			}
		}
		return -1;
	}

	public static IEnumerable<int> IndicesOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor)
	{
		return IndicesOfMany(list, itemsToLookFor, EqualityComparer<T>.Default);
	}

	public static IEnumerable<int> IndicesOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, IEqualityComparer<T> equalityComparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (itemsToLookFor == null)
		{
			throw new ArgumentNullException("itemsToLookFor");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		Set<T> setToLookFor = new Set<T>(itemsToLookFor, equalityComparer);
		int index = 0;
		foreach (T item in list)
		{
			if (setToLookFor.Contains(item))
			{
				yield return index;
			}
			int num = index + 1;
			index = num;
		}
	}

	public static IEnumerable<int> IndicesOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, BinaryPredicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (itemsToLookFor == null)
		{
			throw new ArgumentNullException("itemsToLookFor");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int index = 0;
		foreach (T x in list)
		{
			foreach (T item in itemsToLookFor)
			{
				if (predicate(x, item))
				{
					yield return index;
				}
			}
			int num = index + 1;
			index = num;
		}
	}

	public static int SearchForSubsequence<T>(IList<T> list, IEnumerable<T> pattern)
	{
		return SearchForSubsequence(list, pattern, EqualityComparer<T>.Default);
	}

	public static int SearchForSubsequence<T>(IList<T> list, IEnumerable<T> pattern, BinaryPredicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (pattern == null)
		{
			throw new ArgumentNullException("pattern");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		T[] array = ToArray(pattern);
		int count = list.Count;
		int num = array.Length;
		if (num == 0)
		{
			return 0;
		}
		if (count == 0)
		{
			return -1;
		}
		for (int i = 0; i <= count - num; i++)
		{
			int num2 = 0;
			while (true)
			{
				if (num2 < num)
				{
					if (!predicate(list[i + num2], array[num2]))
					{
						break;
					}
					num2++;
					continue;
				}
				return i;
			}
		}
		return -1;
	}

	public static int SearchForSubsequence<T>(IList<T> list, IEnumerable<T> pattern, IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		return SearchForSubsequence(list, pattern, equalityComparer.Equals);
	}

	public static bool IsSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return IsSubsetOf(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static bool IsSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> otherBag = new Bag<T>(collection1, equalityComparer);
		return new Bag<T>(collection2, equalityComparer).IsSupersetOf(otherBag);
	}

	public static bool IsProperSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return IsProperSubsetOf(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static bool IsProperSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> otherBag = new Bag<T>(collection1, equalityComparer);
		return new Bag<T>(collection2, equalityComparer).IsProperSupersetOf(otherBag);
	}

	public static bool DisjointSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return DisjointSets(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static bool DisjointSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Set<T> set = new Set<T>(collection1, equalityComparer);
		foreach (T item in collection2)
		{
			if (set.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public static bool EqualSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return EqualSets(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static bool EqualSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> otherBag = new Bag<T>(collection1, equalityComparer);
		return new Bag<T>(collection2, equalityComparer).IsEqualTo(otherBag);
	}

	public static IEnumerable<T> SetIntersection<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return SetIntersection(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static IEnumerable<T> SetIntersection<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> bag = new Bag<T>(collection1, equalityComparer);
		Bag<T> otherBag = new Bag<T>(collection2, equalityComparer);
		return Util.CreateEnumerableWrapper(bag.Intersection(otherBag));
	}

	public static IEnumerable<T> SetUnion<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return SetUnion(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static IEnumerable<T> SetUnion<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> bag = new Bag<T>(collection1, equalityComparer);
		Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
		if (bag.Count > bag2.Count)
		{
			bag.UnionWith(bag2);
			return Util.CreateEnumerableWrapper(bag);
		}
		bag2.UnionWith(bag);
		return Util.CreateEnumerableWrapper(bag2);
	}

	public static IEnumerable<T> SetDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return SetDifference(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static IEnumerable<T> SetDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> bag = new Bag<T>(collection1, equalityComparer);
		Bag<T> otherBag = new Bag<T>(collection2, equalityComparer);
		bag.DifferenceWith(otherBag);
		return Util.CreateEnumerableWrapper(bag);
	}

	public static IEnumerable<T> SetSymmetricDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return SetSymmetricDifference(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static IEnumerable<T> SetSymmetricDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentException("equalityComparer");
		}
		Bag<T> bag = new Bag<T>(collection1, equalityComparer);
		Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
		if (bag.Count > bag2.Count)
		{
			bag.SymmetricDifferenceWith(bag2);
			return Util.CreateEnumerableWrapper(bag);
		}
		bag2.SymmetricDifferenceWith(bag);
		return Util.CreateEnumerableWrapper(bag2);
	}

	public static IEnumerable<Pair<TFirst, TSecond>> CartesianProduct<TFirst, TSecond>(IEnumerable<TFirst> first, IEnumerable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		foreach (TFirst itemFirst in first)
		{
			foreach (TSecond item in second)
			{
				yield return new Pair<TFirst, TSecond>(itemFirst, item);
			}
		}
	}

	public static string ToString<T>(IEnumerable<T> collection)
	{
		return ToString(collection, recursive: true, "{", ",", "}");
	}

	public static string ToString<T>(IEnumerable<T> collection, bool recursive, string start, string separator, string end)
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		if (separator == null)
		{
			throw new ArgumentNullException("separator");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		if (collection == null)
		{
			return "null";
		}
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(start);
		foreach (T item in collection)
		{
			if (!flag)
			{
				stringBuilder.Append(separator);
			}
			if (item == null)
			{
				stringBuilder.Append("null");
			}
			else if (recursive && item is IEnumerable && !(item is string))
			{
				stringBuilder.Append(ToString(TypedAs<object>((IEnumerable)(object)item), recursive, start, separator, end));
			}
			else
			{
				stringBuilder.Append(item.ToString());
			}
			flag = false;
		}
		stringBuilder.Append(end);
		return stringBuilder.ToString();
	}

	public static string ToString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
	{
		bool flag = true;
		if (dictionary == null)
		{
			return "null";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			if (item.Key == null)
			{
				stringBuilder.Append("null");
			}
			else if (item.Key is IEnumerable && !(item.Key is string))
			{
				stringBuilder.Append(ToString(TypedAs<object>((IEnumerable)(object)item.Key), recursive: true, "{", ",", "}"));
			}
			else
			{
				stringBuilder.Append(item.Key.ToString());
			}
			stringBuilder.Append("->");
			if (item.Value == null)
			{
				stringBuilder.Append("null");
			}
			else if (item.Value is IEnumerable && !(item.Value is string))
			{
				stringBuilder.Append(ToString(TypedAs<object>((IEnumerable)(object)item.Value), recursive: true, "{", ",", "}"));
			}
			else
			{
				stringBuilder.Append(item.Value.ToString());
			}
			flag = false;
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	private static Random GetRandomGenerator()
	{
		return Calc.R;
	}

	public static T[] RandomShuffle<T>(IEnumerable<T> collection)
	{
		return RandomShuffle(collection, GetRandomGenerator());
	}

	public static T[] RandomShuffle<T>(IEnumerable<T> collection, Random randomGenerator)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (randomGenerator == null)
		{
			throw new ArgumentNullException("randomGenerator");
		}
		T[] array = ToArray(collection);
		for (int num = array.Length - 1; num >= 1; num--)
		{
			int num2 = randomGenerator.Next(num + 1);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
		return array;
	}

	public static void RandomShuffleInPlace<T>(IList<T> list)
	{
		RandomShuffleInPlace(list, GetRandomGenerator());
	}

	public static void RandomShuffleInPlace<T>(IList<T> list, Random randomGenerator)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (randomGenerator == null)
		{
			throw new ArgumentNullException("randomGenerator");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		for (int num = list.Count - 1; num >= 1; num--)
		{
			int index = randomGenerator.Next(num + 1);
			T value = list[num];
			list[num] = list[index];
			list[index] = value;
		}
	}

	public static T[] RandomSubset<T>(IEnumerable<T> collection, int count)
	{
		return RandomSubset(collection, count, GetRandomGenerator());
	}

	public static T[] RandomSubset<T>(IEnumerable<T> collection, int count, Random randomGenerator)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (randomGenerator == null)
		{
			throw new ArgumentNullException("randomGenerator");
		}
		IList<T> list = collection as IList<T>;
		if (list == null)
		{
			list = new List<T>(collection);
		}
		int count2 = list.Count;
		if (count < 0 || count > count2)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		T[] array = new T[count];
		Dictionary<int, T> dictionary = new Dictionary<int, T>(count);
		for (int i = 0; i < count; i++)
		{
			int num = randomGenerator.Next(count2 - i) + i;
			if (!dictionary.TryGetValue(num, out var value))
			{
				value = list[num];
			}
			array[i] = value;
			if (i != num)
			{
				if (dictionary.TryGetValue(i, out value))
				{
					dictionary[num] = value;
				}
				else
				{
					dictionary[num] = list[i];
				}
			}
		}
		return array;
	}

	public static IEnumerable<T[]> GeneratePermutations<T>(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		T[] array = ToArray(collection);
		if (array.Length == 0)
		{
			yield break;
		}
		int[] state = new int[array.Length - 1];
		int maxLength = state.Length;
		yield return array;
		if (array.Length == 1)
		{
			yield break;
		}
		int i = 0;
		while (true)
		{
			if (state[i] < i + 1)
			{
				T val;
				if (state[i] > 0)
				{
					val = array[i + 1];
					array[i + 1] = array[state[i] - 1];
					array[state[i] - 1] = val;
				}
				val = array[i + 1];
				array[i + 1] = array[state[i]];
				array[state[i]] = val;
				yield return array;
				state[i]++;
				i = 0;
			}
			else
			{
				T val = array[i + 1];
				array[i + 1] = array[i];
				array[i] = val;
				state[i] = 0;
				int num = i + 1;
				i = num;
				if (i >= maxLength)
				{
					break;
				}
			}
		}
	}

	public static IEnumerable<T[]> GenerateSortedPermutations<T>(IEnumerable<T> collection) where T : IComparable<T>
	{
		return GenerateSortedPermutations(collection, Comparer<T>.Default);
	}

	public static IEnumerable<T[]> GenerateSortedPermutations<T>(IEnumerable<T> collection, IComparer<T> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T[] array = ToArray(collection);
		int length = array.Length;
		if (length == 0)
		{
			yield break;
		}
		Array.Sort(array, comparer);
		yield return array;
		if (length == 1)
		{
			yield break;
		}
		while (true)
		{
			int num = length - 2;
			while (comparer.Compare(array[num], array[num + 1]) >= 0)
			{
				num--;
				if (num < 0)
				{
					yield break;
				}
			}
			int num2 = length - 1;
			while (comparer.Compare(array[num2], array[num]) <= 0)
			{
				num2--;
			}
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
			int num3 = num + 1;
			int num4 = length - 1;
			while (num3 < num4)
			{
				val = array[num3];
				array[num3] = array[num4];
				array[num4] = val;
				num3++;
				num4--;
			}
			yield return array;
		}
	}

	public static IEnumerable<T[]> GenerateSortedPermutations<T>(IEnumerable<T> collection, Comparison<T> comparison)
	{
		return GenerateSortedPermutations(collection, Comparers.ComparerFromComparison(comparison));
	}

	public static T Maximum<T>(IEnumerable<T> collection) where T : IComparable<T>
	{
		return Maximum(collection, Comparer<T>.Default);
	}

	public static T Maximum<T>(IEnumerable<T> collection, IComparer<T> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T val = default;
		bool flag = false;
		foreach (T item in collection)
		{
			if (!flag || comparer.Compare(val, item) < 0)
			{
				val = item;
			}
			flag = true;
		}
		if (!flag)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
		return val;
	}

	public static T Maximum<T>(IEnumerable<T> collection, Comparison<T> comparison)
	{
		return Maximum(collection, Comparers.ComparerFromComparison(comparison));
	}

	public static T Minimum<T>(IEnumerable<T> collection) where T : IComparable<T>
	{
		return Minimum(collection, Comparer<T>.Default);
	}

	public static T Minimum<T>(IEnumerable<T> collection, IComparer<T> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T val = default;
		bool flag = false;
		foreach (T item in collection)
		{
			if (!flag || comparer.Compare(val, item) > 0)
			{
				val = item;
			}
			flag = true;
		}
		if (!flag)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
		return val;
	}

	public static T Minimum<T>(IEnumerable<T> collection, Comparison<T> comparison)
	{
		return Minimum(collection, Comparers.ComparerFromComparison(comparison));
	}

	public static int IndexOfMaximum<T>(IList<T> list) where T : IComparable<T>
	{
		return IndexOfMaximum(list, Comparer<T>.Default);
	}

	public static int IndexOfMaximum<T>(IList<T> list, IComparer<T> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T x = default;
		int num = -1;
		int num2 = 0;
		foreach (T item in list)
		{
			if (num < 0 || comparer.Compare(x, item) < 0)
			{
				x = item;
				num = num2;
			}
			num2++;
		}
		return num;
	}

	public static int IndexOfMaximum<T>(IList<T> list, Comparison<T> comparison)
	{
		return IndexOfMaximum(list, Comparers.ComparerFromComparison(comparison));
	}

	public static int IndexOfMinimum<T>(IList<T> list) where T : IComparable<T>
	{
		return IndexOfMinimum(list, Comparer<T>.Default);
	}

	public static int IndexOfMinimum<T>(IList<T> list, IComparer<T> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T x = default;
		int num = -1;
		int num2 = 0;
		foreach (T item in list)
		{
			if (num < 0 || comparer.Compare(x, item) > 0)
			{
				x = item;
				num = num2;
			}
			num2++;
		}
		return num;
	}

	public static int IndexOfMinimum<T>(IList<T> list, Comparison<T> comparison)
	{
		return IndexOfMinimum(list, Comparers.ComparerFromComparison(comparison));
	}

	public static T[] Sort<T>(IEnumerable<T> collection) where T : IComparable<T>
	{
		return Sort(collection, Comparer<T>.Default);
	}

	public static T[] Sort<T>(IEnumerable<T> collection, IComparer<T> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T[] array = ToArray(collection);
		Array.Sort(array, comparer);
		return array;
	}

	public static T[] Sort<T>(IEnumerable<T> collection, Comparison<T> comparison)
	{
		return Sort(collection, Comparers.ComparerFromComparison(comparison));
	}

	public static void SortInPlace<T>(IList<T> list) where T : IComparable<T>
	{
		SortInPlace(list, Comparer<T>.Default);
	}

	public static void SortInPlace<T>(IList<T> list, IComparer<T> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		if (list is T[])
		{
			Array.Sort((T[])list, comparer);
			return;
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int[] array = new int[32];
		int[] array2 = new int[32];
		int num = 0;
		int num2 = 0;
		int num3 = list.Count - 1;
		while (true)
		{
			if (num2 == num3 - 1)
			{
				T val = list[num2];
				T val2 = list[num3];
				if (comparer.Compare(val, val2) > 0)
				{
					list[num3] = val;
					list[num2] = val2;
				}
				num2 = num3;
			}
			else if (num2 < num3)
			{
				int index = num2 + (num3 - num2) / 2;
				T val3 = list[num2];
				T val4 = list[index];
				T val5 = list[num3];
				if (comparer.Compare(val3, val4) > 0)
				{
					T val6 = val3;
					val3 = val4;
					val4 = val6;
				}
				if (comparer.Compare(val3, val5) > 0)
				{
					T val7 = val5;
					val5 = val4;
					val4 = val3;
					val3 = val7;
				}
				else if (comparer.Compare(val4, val5) > 0)
				{
					T val8 = val4;
					val4 = val5;
					val5 = val8;
				}
				if (num2 == num3 - 2)
				{
					list[num2] = val3;
					list[index] = val4;
					list[num3] = val5;
					num2 = num3;
					continue;
				}
				list[num2] = val3;
				list[index] = val5;
				T val9 = (list[num3] = val4);
				int num4 = num2;
				int num5 = num3;
				T val11;
				while (true)
				{
					num4++;
					val11 = list[num4];
					if (comparer.Compare(val11, val9) >= 0)
					{
						T val12;
						do
						{
							num5--;
							val12 = list[num5];
						}
						while (comparer.Compare(val12, val9) > 0);
						if (num5 < num4)
						{
							break;
						}
						list[num4] = val12;
						list[num5] = val11;
					}
				}
				list[num3] = val11;
				list[num4] = val9;
				num4++;
				if (num5 - num2 > num3 - num4)
				{
					array[num] = num2;
					array2[num] = num5;
					num2 = num4;
				}
				else
				{
					array[num] = num4;
					array2[num] = num3;
					num3 = num5;
				}
				num++;
			}
			else
			{
				if (num <= 0)
				{
					break;
				}
				num--;
				num2 = array[num];
				num3 = array2[num];
			}
		}
	}

	public static void SortInPlace<T>(IList<T> list, Comparison<T> comparison)
	{
		SortInPlace(list, Comparers.ComparerFromComparison(comparison));
	}

	public static T[] StableSort<T>(IEnumerable<T> collection) where T : IComparable<T>
	{
		return StableSort(collection, Comparer<T>.Default);
	}

	public static T[] StableSort<T>(IEnumerable<T> collection, IComparer<T> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		T[] array = ToArray(collection);
		StableSortInPlace(ReadWriteList(array), comparer);
		return array;
	}

	public static T[] StableSort<T>(IEnumerable<T> collection, Comparison<T> comparison)
	{
		return StableSort(collection, Comparers.ComparerFromComparison(comparison));
	}

	public static void StableSortInPlace<T>(IList<T> list) where T : IComparable<T>
	{
		StableSortInPlace(list, Comparer<T>.Default);
	}

	public static void StableSortInPlace<T>(IList<T> list, IComparer<T> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int[] array = new int[list.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i;
		}
		int[] array2 = new int[32];
		int[] array3 = new int[32];
		int num = 0;
		int num2 = 0;
		int num3 = list.Count - 1;
		while (true)
		{
			if (num2 == num3 - 1)
			{
				T val = list[num2];
				int num4 = array[num2];
				T val2 = list[num3];
				int num5 = array[num3];
				int num6;
				if ((num6 = comparer.Compare(val, val2)) > 0 || (num6 == 0 && num4 > num5))
				{
					list[num3] = val;
					array[num3] = num4;
					list[num2] = val2;
					array[num2] = num5;
				}
				num2 = num3;
			}
			else if (num2 < num3)
			{
				int num7 = num2 + (num3 - num2) / 2;
				T val3 = list[num2];
				T val4 = list[num7];
				T val5 = list[num3];
				int num8 = array[num2];
				int num9 = array[num7];
				int num10 = array[num3];
				int num6;
				if ((num6 = comparer.Compare(val3, val4)) > 0 || (num6 == 0 && num8 > num9))
				{
					T val6 = val3;
					val3 = val4;
					val4 = val6;
					int num11 = num8;
					num8 = num9;
					num9 = num11;
				}
				if ((num6 = comparer.Compare(val3, val5)) > 0 || (num6 == 0 && num8 > num10))
				{
					T val7 = val5;
					val5 = val4;
					val4 = val3;
					val3 = val7;
					int num12 = num10;
					num10 = num9;
					num9 = num8;
					num8 = num12;
				}
				else if ((num6 = comparer.Compare(val4, val5)) > 0 || (num6 == 0 && num9 > num10))
				{
					T val8 = val4;
					val4 = val5;
					val5 = val8;
					int num13 = num9;
					num9 = num10;
					num10 = num13;
				}
				if (num2 == num3 - 2)
				{
					list[num2] = val3;
					list[num7] = val4;
					list[num3] = val5;
					array[num2] = num8;
					array[num7] = num9;
					array[num3] = num10;
					num2 = num3;
					continue;
				}
				list[num2] = val3;
				array[num2] = num8;
				list[num7] = val5;
				array[num7] = num10;
				T val9 = (list[num3] = val4);
				int num14 = (array[num3] = num9);
				int num15 = num2;
				int num16 = num3;
				T val11;
				int num17;
				while (true)
				{
					num15++;
					val11 = list[num15];
					num17 = array[num15];
					if ((num6 = comparer.Compare(val11, val9)) >= 0 && (num6 != 0 || num17 >= num14))
					{
						T val12;
						int num18;
						do
						{
							num16--;
							val12 = list[num16];
							num18 = array[num16];
						}
						while ((num6 = comparer.Compare(val12, val9)) > 0 || (num6 == 0 && num18 > num14));
						if (num16 < num15)
						{
							break;
						}
						list[num15] = val12;
						list[num16] = val11;
						array[num15] = num18;
						array[num16] = num17;
					}
				}
				list[num3] = val11;
				array[num3] = num17;
				list[num15] = val9;
				array[num15] = num14;
				num15++;
				if (num16 - num2 > num3 - num15)
				{
					array2[num] = num2;
					array3[num] = num16;
					num2 = num15;
				}
				else
				{
					array2[num] = num15;
					array3[num] = num3;
					num3 = num16;
				}
				num++;
			}
			else
			{
				if (num <= 0)
				{
					break;
				}
				num--;
				num2 = array2[num];
				num3 = array3[num];
			}
		}
	}

	public static void StableSortInPlace<T>(IList<T> list, Comparison<T> comparison)
	{
		StableSortInPlace(list, Comparers.ComparerFromComparison(comparison));
	}

	public static int BinarySearch<T>(IList<T> list, T item, out int index) where T : IComparable<T>
	{
		return BinarySearch(list, item, Comparer<T>.Default, out index);
	}

	public static int BinarySearch<T>(IList<T> list, T item, IComparer<T> comparer, out int index)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		int num = 0;
		int num2 = list.Count;
		while (num2 > num)
		{
			int num3 = num + (num2 - num) / 2;
			T x = list[num3];
			int num4 = comparer.Compare(x, item);
			if (num4 < 0)
			{
				num = num3 + 1;
				continue;
			}
			if (num4 > 0)
			{
				num2 = num3;
				continue;
			}
			int num5 = num;
			int num6 = num2;
			int num7 = num3;
			num = num5;
			num2 = num7;
			while (num2 > num)
			{
				num3 = num + (num2 - num) / 2;
				x = list[num3];
				num4 = comparer.Compare(x, item);
				if (num4 < 0)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3;
				}
			}
			index = num;
			num = num7;
			num2 = num6;
			while (num2 > num)
			{
				num3 = num + (num2 - num) / 2;
				x = list[num3];
				num4 = comparer.Compare(x, item);
				if (num4 <= 0)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3;
				}
			}
			return num - index;
		}
		index = num;
		return 0;
	}

	public static int BinarySearch<T>(IList<T> list, T item, Comparison<T> comparison, out int index)
	{
		return BinarySearch(list, item, Comparers.ComparerFromComparison(comparison), out index);
	}

	public static IEnumerable<T> MergeSorted<T>(params IEnumerable<T>[] collections) where T : IComparable<T>
	{
		return MergeSorted(Comparer<T>.Default, collections);
	}

	public static IEnumerable<T> MergeSorted<T>(IComparer<T> comparer, params IEnumerable<T>[] collections)
	{
		if (collections == null)
		{
			throw new ArgumentNullException("collections");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		IEnumerator<T>[] enumerators = new IEnumerator<T>[collections.Length];
		bool[] more = new bool[collections.Length];
		T smallestItem = default;
		try
		{
			for (int i = 0; i < collections.Length; i++)
			{
				if (collections[i] != null)
				{
					enumerators[i] = collections[i].GetEnumerator();
					more[i] = enumerators[i].MoveNext();
				}
			}
			while (true)
			{
				int smallestItemIndex = -1;
				for (int j = 0; j < enumerators.Length; j++)
				{
					if (more[j])
					{
						T current = enumerators[j].Current;
						if (smallestItemIndex < 0 || comparer.Compare(smallestItem, current) > 0)
						{
							smallestItemIndex = j;
							smallestItem = current;
						}
					}
				}
				if (smallestItemIndex != -1)
				{
					yield return smallestItem;
					more[smallestItemIndex] = enumerators[smallestItemIndex].MoveNext();
					continue;
				}
				break;
			}
		}
		finally
		{
			IEnumerator<T>[] array = enumerators;
			for (int k = 0; k < array.Length; k++)
			{
				array[k]?.Dispose();
			}
		}
	}

	public static IEnumerable<T> MergeSorted<T>(Comparison<T> comparison, params IEnumerable<T>[] collections)
	{
		return MergeSorted(Comparers.ComparerFromComparison(comparison), collections);
	}

	public static int LexicographicalCompare<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2) where T : IComparable<T>
	{
		return LexicographicalCompare(sequence1, sequence2, Comparer<T>.Default);
	}

	public static int LexicographicalCompare<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2, Comparison<T> comparison)
	{
		return LexicographicalCompare(sequence1, sequence2, Comparers.ComparerFromComparison(comparison));
	}

	public static int LexicographicalCompare<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2, IComparer<T> comparer)
	{
		if (sequence1 == null)
		{
			throw new ArgumentNullException("sequence1");
		}
		if (sequence2 == null)
		{
			throw new ArgumentNullException("sequence2");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		using IEnumerator<T> enumerator = sequence1.GetEnumerator();
		using IEnumerator<T> enumerator2 = sequence2.GetEnumerator();
		bool flag;
		bool flag2;
		while (true)
		{
			flag = enumerator.MoveNext();
			flag2 = enumerator2.MoveNext();
			if (!flag || !flag2)
			{
				break;
			}
			int num = comparer.Compare(enumerator.Current, enumerator2.Current);
			if (num != 0)
			{
				return num;
			}
		}
		if (flag == flag2)
		{
			return 0;
		}
		if (flag)
		{
			return 1;
		}
		return -1;
	}

	public static IComparer<IEnumerable<T>> GetLexicographicalComparer<T>() where T : IComparable<T>
	{
		return GetLexicographicalComparer(Comparer<T>.Default);
	}

	public static IComparer<IEnumerable<T>> GetLexicographicalComparer<T>(IComparer<T> comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return new LexicographicalComparerClass<T>(comparer);
	}

	public static IComparer<IEnumerable<T>> GetLexicographicalComparer<T>(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		return new LexicographicalComparerClass<T>(Comparers.ComparerFromComparison(comparison));
	}

	public static IComparer<T> GetReverseComparer<T>(IComparer<T> comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return new ReverseComparerClass<T>(comparer);
	}

	public static IEqualityComparer<T> GetIdentityComparer<T>() where T : class
	{
		return new IdentityComparer<T>();
	}

	public static Comparison<T> GetReverseComparison<T>(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		return (T x, T y) => -comparison(x, y);
	}

	public static IComparer<T> GetComparerFromComparison<T>(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		return Comparers.ComparerFromComparison(comparison);
	}

	public static Comparison<T> GetComparisonFromComparer<T>(IComparer<T> comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return comparer.Compare;
	}

	public static IEqualityComparer<IEnumerable<T>> GetCollectionEqualityComparer<T>()
	{
		return GetCollectionEqualityComparer(EqualityComparer<T>.Default);
	}

	public static IEqualityComparer<IEnumerable<T>> GetCollectionEqualityComparer<T>(IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		return new CollectionEqualityComparer<T>(equalityComparer);
	}

	public static IEqualityComparer<IEnumerable<T>> GetSetEqualityComparer<T>()
	{
		return GetSetEqualityComparer(EqualityComparer<T>.Default);
	}

	public static IEqualityComparer<IEnumerable<T>> GetSetEqualityComparer<T>(IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		return new SetEqualityComparer<T>(equalityComparer);
	}

	public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool TrueForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static int CountWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				num++;
			}
		}
		return num;
	}

	public static ICollection<T> RemoveWhere<T>(ICollection<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (collection is T[])
		{
			collection = new ArrayWrapper<T>((T[])collection);
		}
		if (collection.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "collection");
		}
		if (collection is IList<T> list)
		{
			int num = -1;
			int i = 0;
			int num2 = list.Count;
			List<T> list2 = new List<T>();
			for (; i < num2; i++)
			{
				T val = list[i];
				if (predicate(val))
				{
					list2.Add(val);
					continue;
				}
				num++;
				if (num != i)
				{
					list[num] = val;
				}
			}
			num++;
			if (num < num2)
			{
				if (list is IList && ((IList)list).IsFixedSize)
				{
					while (num < num2)
					{
						list[num++] = default;
					}
				}
				else
				{
					while (num < num2)
					{
						list.RemoveAt(num2 - 1);
						num2--;
					}
				}
			}
			return list2;
		}
		List<T> list3 = new List<T>();
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				list3.Add(item);
			}
		}
		foreach (T item2 in list3)
		{
			collection.Remove(item2);
		}
		return list3;
	}

	public static IEnumerable<TDest> Convert<TSource, TDest>(IEnumerable<TSource> sourceCollection, Converter<TSource, TDest> converter)
	{
		if (sourceCollection == null)
		{
			throw new ArgumentNullException("sourceCollection");
		}
		if (converter == null)
		{
			throw new ArgumentNullException("converter");
		}
		foreach (TSource item in sourceCollection)
		{
			yield return converter(item);
		}
	}

	public static Converter<TKey, TValue> GetDictionaryConverter<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
	{
		return GetDictionaryConverter(dictionary, default);
	}

	public static Converter<TKey, TValue> GetDictionaryConverter<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TValue defaultValue)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		return (TKey key) => dictionary.TryGetValue(key, out var value) ? value : defaultValue;
	}

	public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		foreach (T item in collection)
		{
			action(item);
		}
	}

	public static int Partition<T>(IList<T> list, Predicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int num = 0;
		int num2 = list.Count - 1;
		while (true)
		{
			if (num <= num2 && predicate(list[num]))
			{
				num++;
				continue;
			}
			while (num <= num2 && !predicate(list[num2]))
			{
				num2--;
			}
			if (num > num2)
			{
				break;
			}
			T value = list[num];
			list[num] = list[num2];
			list[num2] = value;
			num++;
			num2--;
		}
		return num;
	}

	public static int StablePartition<T>(IList<T> list, Predicate<T> predicate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int count = list.Count;
		if (count == 0)
		{
			return 0;
		}
		T[] array = new T[count];
		int num = 0;
		int num2 = count - 1;
		foreach (T item in list)
		{
			if (predicate(item))
			{
				array[num++] = item;
			}
			else
			{
				array[num2--] = item;
			}
		}
		int i;
		for (i = 0; i < num; i++)
		{
			list[i] = array[i];
		}
		num2 = count - 1;
		while (i < count)
		{
			list[i++] = array[num2--];
		}
		return num;
	}

	public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] collections)
	{
		if (collections == null)
		{
			throw new ArgumentNullException("collections");
		}
		foreach (IEnumerable<T> enumerable in collections)
		{
			foreach (T item in enumerable)
			{
				yield return item;
			}
		}
	}

	public static bool EqualCollections<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
	{
		return EqualCollections(collection1, collection2, EqualityComparer<T>.Default);
	}

	public static bool EqualCollections<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		using IEnumerator<T> enumerator = collection1.GetEnumerator();
		using IEnumerator<T> enumerator2 = collection2.GetEnumerator();
		bool flag;
		bool flag2;
		while (true)
		{
			flag = enumerator.MoveNext();
			flag2 = enumerator2.MoveNext();
			if (!flag || !flag2)
			{
				break;
			}
			if (!equalityComparer.Equals(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
		}
		return flag == flag2;
	}

	public static bool EqualCollections<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, BinaryPredicate<T> predicate)
	{
		if (collection1 == null)
		{
			throw new ArgumentNullException("collection1");
		}
		if (collection2 == null)
		{
			throw new ArgumentNullException("collection2");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		using IEnumerator<T> enumerator = collection1.GetEnumerator();
		using IEnumerator<T> enumerator2 = collection2.GetEnumerator();
		bool flag;
		bool flag2;
		while (true)
		{
			flag = enumerator.MoveNext();
			flag2 = enumerator2.MoveNext();
			if (!flag || !flag2)
			{
				break;
			}
			if (!predicate(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
		}
		return flag == flag2;
	}

	public static T[] ToArray<T>(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection is ICollection<T> collection2)
		{
			T[] array = new T[collection2.Count];
			collection2.CopyTo(array, 0);
			return array;
		}
		return new List<T>(collection).ToArray();
	}

	public static int Count<T>(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection is ICollection<T>)
		{
			return ((ICollection<T>)collection).Count;
		}
		int num = 0;
		foreach (T item in collection)
		{
			_ = item;
			num++;
		}
		return num;
	}

	public static int CountEqual<T>(IEnumerable<T> collection, T find)
	{
		return CountEqual(collection, find, EqualityComparer<T>.Default);
	}

	public static int CountEqual<T>(IEnumerable<T> collection, T find, IEqualityComparer<T> equalityComparer)
	{
		if (collection == null)
		{
			throw new ArgumentException("collection");
		}
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		int num = 0;
		foreach (T item in collection)
		{
			if (equalityComparer.Equals(item, find))
			{
				num++;
			}
		}
		return num;
	}

	public static IEnumerable<T> NCopiesOf<T>(int n, T item)
	{
		if (n < 0)
		{
			throw new ArgumentOutOfRangeException("n", n, Strings.ArgMustNotBeNegative);
		}
		while (n-- > 0)
		{
			yield return item;
		}
	}

	public static void Fill<T>(IList<T> list, T value)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i] = value;
		}
	}

	public static void Fill<T>(T[] array, T value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = value;
		}
	}

	public static void FillRange<T>(IList<T> list, int start, int count, T value)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		if (count != 0)
		{
			if (start < 0 || start >= list.Count)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (count < 0 || count > list.Count || start > list.Count - count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			for (int i = start; i < count + start; i++)
			{
				list[i] = value;
			}
		}
	}

	public static void FillRange<T>(T[] array, int start, int count, T value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (count != 0)
		{
			if (start < 0 || start >= array.Length)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (count < 0 || count > array.Length || start > array.Length - count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			for (int i = start; i < count + start; i++)
			{
				array[i] = value;
			}
		}
	}

	public static void Copy<T>(IEnumerable<T> source, IList<T> dest, int destIndex)
	{
		Copy(source, dest, destIndex, int.MaxValue);
	}

	public static void Copy<T>(IEnumerable<T> source, T[] dest, int destIndex)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		if (destIndex < 0 || destIndex > dest.Length)
		{
			throw new ArgumentOutOfRangeException("destIndex");
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (destIndex >= dest.Length)
			{
				throw new ArgumentException(Strings.ArrayTooSmall, "array");
			}
			dest[destIndex++] = enumerator.Current;
		}
	}

	public static void Copy<T>(IEnumerable<T> source, IList<T> dest, int destIndex, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		if (dest.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "dest");
		}
		int count2 = dest.Count;
		if (destIndex < 0 || destIndex > count2)
		{
			throw new ArgumentOutOfRangeException("destIndex");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		while (destIndex < count2 && count > 0 && enumerator.MoveNext())
		{
			dest[destIndex++] = enumerator.Current;
			count--;
		}
		while (count > 0 && enumerator.MoveNext())
		{
			dest.Insert(count2++, enumerator.Current);
			count--;
		}
	}

	public static void Copy<T>(IEnumerable<T> source, T[] dest, int destIndex, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		int num = dest.Length;
		if (destIndex < 0 || destIndex > num)
		{
			throw new ArgumentOutOfRangeException("destIndex");
		}
		if (count < 0 || destIndex + count > num)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		while (destIndex < num && count > 0 && enumerator.MoveNext())
		{
			dest[destIndex++] = enumerator.Current;
			count--;
		}
	}

	public static void Copy<T>(IList<T> source, int sourceIndex, IList<T> dest, int destIndex, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		if (dest.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "dest");
		}
		int count2 = source.Count;
		int count3 = dest.Count;
		if (sourceIndex < 0 || sourceIndex >= count2)
		{
			throw new ArgumentOutOfRangeException("sourceIndex");
		}
		if (destIndex < 0 || destIndex > count3)
		{
			throw new ArgumentOutOfRangeException("destIndex");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count > count2 - sourceIndex)
		{
			count = count2 - sourceIndex;
		}
		if (source == dest && sourceIndex > destIndex)
		{
			while (count > 0)
			{
				dest[destIndex++] = source[sourceIndex++];
				count--;
			}
			return;
		}
		int num2;
		int num3;
		if (destIndex + count > count3)
		{
			int num = destIndex + count - count3;
			num2 = sourceIndex + (count - num);
			num3 = count3;
			count -= num;
			while (num > 0)
			{
				dest.Insert(num3++, source[num2++]);
				num--;
			}
		}
		num2 = sourceIndex + count - 1;
		num3 = destIndex + count - 1;
		while (count > 0)
		{
			dest[num3--] = source[num2--];
			count--;
		}
	}

	public static void Copy<T>(IList<T> source, int sourceIndex, T[] dest, int destIndex, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		int count2 = source.Count;
		int num = dest.Length;
		if (sourceIndex < 0 || sourceIndex >= count2)
		{
			throw new ArgumentOutOfRangeException("sourceIndex");
		}
		if (destIndex < 0 || destIndex > num)
		{
			throw new ArgumentOutOfRangeException("destIndex");
		}
		if (count < 0 || destIndex + count > num)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count > count2 - sourceIndex)
		{
			count = count2 - sourceIndex;
		}
		if (source is T[])
		{
			Array.Copy((T[])source, sourceIndex, dest, destIndex, count);
			return;
		}
		int num2 = sourceIndex;
		int num3 = destIndex;
		while (count > 0)
		{
			dest[num3++] = source[num2++];
			count--;
		}
	}

	public static IEnumerable<T> Reverse<T>(IList<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int i = source.Count - 1;
		while (i >= 0)
		{
			yield return source[i];
			int num = i - 1;
			i = num;
		}
	}

	public static void ReverseInPlace<T>(IList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int num = 0;
		int num2 = list.Count - 1;
		while (num < num2)
		{
			T value = list[num];
			list[num] = list[num2];
			list[num2] = value;
			num++;
			num2--;
		}
	}

	public static IEnumerable<T> Rotate<T>(IList<T> source, int amountToRotate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int count = source.Count;
		if (count != 0)
		{
			amountToRotate %= count;
			if (amountToRotate < 0)
			{
				amountToRotate += count;
			}
			int i = amountToRotate;
			while (i < count)
			{
				yield return source[i];
				int num = i + 1;
				i = num;
			}
			i = 0;
			while (i < amountToRotate)
			{
				yield return source[i];
				int num = i + 1;
				i = num;
			}
		}
	}

	public static void RotateInPlace<T>(IList<T> list, int amountToRotate)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list is T[])
		{
			list = new ArrayWrapper<T>((T[])list);
		}
		if (list.IsReadOnly)
		{
			throw new ArgumentException(Strings.ListIsReadOnly, "list");
		}
		int count = list.Count;
		if (count == 0)
		{
			return;
		}
		amountToRotate %= count;
		if (amountToRotate < 0)
		{
			amountToRotate += count;
		}
		int num = count;
		int num2 = 0;
		while (num > 0)
		{
			int num3 = num2;
			T value = list[num2];
			while (true)
			{
				num--;
				int num4 = num3 + amountToRotate;
				if (num4 >= count)
				{
					num4 -= count;
				}
				if (num4 == num2)
				{
					break;
				}
				list[num3] = list[num4];
				num3 = num4;
			}
			list[num3] = value;
			num2++;
		}
	}
}
