using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class MultiDictionary<TKey, TValue> : MultiDictionaryBase<TKey, TValue>, ICloneable
{
	[Serializable]
	private struct KeyAndValues(TKey key)
	{
		public TKey Key = key;

		public int Count = 0;

		public TValue[] Values = null;

		public static KeyAndValues Copy(KeyAndValues x)
		{
			KeyAndValues result = default;
			result.Key = x.Key;
			result.Count = x.Count;
			if (x.Values != null)
			{
				result.Values = (TValue[])x.Values.Clone();
			}
			else
			{
				result.Values = null;
			}
			return result;
		}
	}

	[Serializable]
	private class KeyAndValuesEqualityComparer : IEqualityComparer<KeyAndValues>
	{
		private IEqualityComparer<TKey> keyEqualityComparer;

		public KeyAndValuesEqualityComparer(IEqualityComparer<TKey> keyEqualityComparer)
		{
			this.keyEqualityComparer = keyEqualityComparer;
		}

		public bool Equals(KeyAndValues x, KeyAndValues y)
		{
			return keyEqualityComparer.Equals(x.Key, y.Key);
		}

		public int GetHashCode(KeyAndValues obj)
		{
			return Util.GetHashCode(obj.Key, keyEqualityComparer);
		}
	}

	private IEqualityComparer<TKey> keyEqualityComparer;

	private IEqualityComparer<TValue> valueEqualityComparer;

	private IEqualityComparer<KeyAndValues> equalityComparer;

	private Hash<KeyAndValues> hash;

	private bool allowDuplicateValues;

	public IEqualityComparer<TKey> KeyComparer => keyEqualityComparer;

	public IEqualityComparer<TValue> ValueComparer => valueEqualityComparer;

	public sealed override int Count => hash.ElementCount;

	public MultiDictionary(bool allowDuplicateValues)
		: this(allowDuplicateValues, (IEqualityComparer<TKey>)EqualityComparer<TKey>.Default, (IEqualityComparer<TValue>)EqualityComparer<TValue>.Default)
	{
	}

	public MultiDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer)
		: this(allowDuplicateValues, keyEqualityComparer, (IEqualityComparer<TValue>)EqualityComparer<TValue>.Default)
	{
	}

	public MultiDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer, IEqualityComparer<TValue> valueEqualityComparer)
	{
		if (keyEqualityComparer == null)
		{
			throw new ArgumentNullException("keyEqualityComparer");
		}
		if (valueEqualityComparer == null)
		{
			throw new ArgumentNullException("valueEqualityComparer");
		}
		this.allowDuplicateValues = allowDuplicateValues;
		this.keyEqualityComparer = keyEqualityComparer;
		this.valueEqualityComparer = valueEqualityComparer;
		equalityComparer = new KeyAndValuesEqualityComparer(keyEqualityComparer);
		hash = new Hash<KeyAndValues>(equalityComparer);
	}

	private MultiDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyEqualityComparer, IEqualityComparer<TValue> valueEqualityComparer, IEqualityComparer<KeyAndValues> equalityComparer, Hash<KeyAndValues> hash)
	{
		if (keyEqualityComparer == null)
		{
			throw new ArgumentNullException("keyEqualityComparer");
		}
		if (valueEqualityComparer == null)
		{
			throw new ArgumentNullException("valueEqualityComparer");
		}
		this.allowDuplicateValues = allowDuplicateValues;
		this.keyEqualityComparer = keyEqualityComparer;
		this.valueEqualityComparer = valueEqualityComparer;
		this.equalityComparer = equalityComparer;
		this.hash = hash;
	}

	public sealed override void Add(TKey key, TValue value)
	{
		KeyAndValues item = new KeyAndValues(key);
		if (hash.Find(item, replace: false, out var item2))
		{
			int count = item2.Count;
			if (!allowDuplicateValues)
			{
				int hashCode = Util.GetHashCode(value, valueEqualityComparer);
				for (int i = 0; i < count; i++)
				{
					if (Util.GetHashCode(item2.Values[i], valueEqualityComparer) == hashCode && valueEqualityComparer.Equals(item2.Values[i], value))
					{
						item2.Values[i] = value;
						return;
					}
				}
			}
			if (count == item2.Values.Length)
			{
				TValue[] array = new TValue[count * 2];
				Array.Copy(item2.Values, array, count);
				item2.Values = array;
			}
			item2.Values[count] = value;
			item2.Count = count + 1;
			hash.Find(item2, replace: true, out item);
		}
		else
		{
			item.Count = 1;
			item.Values = new TValue[1] { value };
			hash.Insert(item, replaceOnDuplicate: true, out item2);
		}
	}

	public sealed override bool Remove(TKey key, TValue value)
	{
		KeyAndValues item = new KeyAndValues(key);
		if (hash.Find(item, replace: false, out var item2))
		{
			int count = item2.Count;
			int hashCode = Util.GetHashCode(value, valueEqualityComparer);
			int num = -1;
			for (int i = 0; i < count; i++)
			{
				if (Util.GetHashCode(item2.Values[i], valueEqualityComparer) == hashCode && valueEqualityComparer.Equals(item2.Values[i], value))
				{
					num = i;
				}
			}
			if (count == 1)
			{
				hash.Delete(item2, out item);
				return true;
			}
			if (num >= 0)
			{
				if (num < count - 1)
				{
					Array.Copy(item2.Values, num + 1, item2.Values, num, count - num - 1);
				}
				item2.Count = count - 1;
				hash.Find(item2, replace: true, out item);
				return true;
			}
			return false;
		}
		return false;
	}

	public sealed override bool Remove(TKey key)
	{
		KeyAndValues itemDeleted;
		return hash.Delete(new KeyAndValues(key), out itemDeleted);
	}

	public sealed override void Clear()
	{
		hash.StopEnumerations();
		hash = new Hash<KeyAndValues>(equalityComparer);
	}

	protected sealed override bool EqualValues(TValue value1, TValue value2)
	{
		return valueEqualityComparer.Equals(value1, value2);
	}

	public sealed override bool Contains(TKey key, TValue value)
	{
		KeyAndValues find = new KeyAndValues(key);
		if (hash.Find(find, replace: false, out var item))
		{
			int count = item.Count;
			int hashCode = Util.GetHashCode(value, valueEqualityComparer);
			for (int i = 0; i < count; i++)
			{
				if (Util.GetHashCode(item.Values[i], valueEqualityComparer) == hashCode && valueEqualityComparer.Equals(item.Values[i], value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public sealed override bool ContainsKey(TKey key)
	{
		KeyAndValues find = new KeyAndValues(key);
		KeyAndValues item;
		return hash.Find(find, replace: false, out item);
	}

	protected sealed override IEnumerator<TKey> EnumerateKeys()
	{
		foreach (KeyAndValues item in hash)
		{
			yield return item.Key;
		}
	}

	private IEnumerator<TValue> EnumerateValues(KeyAndValues keyAndValues)
	{
		int count = keyAndValues.Count;
		int stamp = hash.GetEnumerationStamp();
		int i = 0;
		while (i < count)
		{
			yield return keyAndValues.Values[i];
			hash.CheckEnumerationStamp(stamp);
			int num = i + 1;
			i = num;
		}
	}

	protected sealed override bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
	{
		KeyAndValues find = new KeyAndValues(key);
		if (hash.Find(find, replace: false, out var item))
		{
			values = EnumerateValues(item);
			return true;
		}
		values = null;
		return false;
	}

	protected sealed override int CountValues(TKey key)
	{
		KeyAndValues find = new KeyAndValues(key);
		if (hash.Find(find, replace: false, out var item))
		{
			return item.Count;
		}
		return 0;
	}

	public MultiDictionary<TKey, TValue> Clone()
	{
		return new MultiDictionary<TKey, TValue>(allowDuplicateValues, keyEqualityComparer, valueEqualityComparer, equalityComparer, hash.Clone(KeyAndValues.Copy));
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	private void NonCloneableType(Type t)
	{
		throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, t.FullName));
	}

	public MultiDictionary<TKey, TValue> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(TKey), out var isValue))
		{
			NonCloneableType(typeof(TKey));
		}
		if (!Util.IsCloneableType(typeof(TValue), out var isValue2))
		{
			NonCloneableType(typeof(TValue));
		}
		MultiDictionary<TKey, TValue> multiDictionary = new MultiDictionary<TKey, TValue>(allowDuplicateValues, keyEqualityComparer, valueEqualityComparer);
		foreach (KeyAndValues item in hash)
		{
			TKey key;
			if (isValue)
			{
				key = item.Key;
			}
			else
			{
				key = ((item.Key != null) ? ((TKey)((ICloneable)(object)item.Key).Clone()) : default(TKey));
			}
			TValue[] array = new TValue[item.Count];
			if (isValue2)
			{
				Array.Copy(item.Values, array, item.Count);
			}
			else
			{
				for (int i = 0; i < item.Count; i++)
				{
					if (item.Values[i] == null)
					{
						array[i] = default;
					}
					else
					{
						array[i] = (TValue)((ICloneable)(object)item.Values[i]).Clone();
					}
				}
			}
			multiDictionary.AddMany(key, array);
		}
		return multiDictionary;
	}
}
