using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Wintellect.PowerCollections;

[Serializable]
[DebuggerDisplay("{DebuggerDisplayString()}")]
public abstract class ReadOnlyMultiDictionaryBase<TKey, TValue> : ReadOnlyCollectionBase<KeyValuePair<TKey, ICollection<TValue>>>, IDictionary<TKey, ICollection<TValue>>, ICollection<KeyValuePair<TKey, ICollection<TValue>>>, IEnumerable<KeyValuePair<TKey, ICollection<TValue>>>, IEnumerable
{
	[Serializable]
	private sealed class ValuesForKeyCollection : ReadOnlyCollectionBase<TValue>
	{
		private ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary;

		private TKey key;

		public override int Count => myDictionary.CountValues(key);

		public ValuesForKeyCollection(ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary, TKey key)
		{
			this.myDictionary = myDictionary;
			this.key = key;
		}

		private IEnumerator<TValue> NoValues()
		{
			yield break;
		}

		public override IEnumerator<TValue> GetEnumerator()
		{
			if (myDictionary.TryEnumerateValuesForKey(key, out var values))
			{
				return values;
			}
			return NoValues();
		}

		public override bool Contains(TValue item)
		{
			return myDictionary.Contains(key, item);
		}
	}

	[Serializable]
	private sealed class KeysCollection : ReadOnlyCollectionBase<TKey>
	{
		private ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.Count;

		public KeysCollection(ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<TKey> GetEnumerator()
		{
			return myDictionary.EnumerateKeys();
		}

		public override bool Contains(TKey key)
		{
			return myDictionary.ContainsKey(key);
		}
	}

	[Serializable]
	private sealed class ValuesCollection : ReadOnlyCollectionBase<TValue>
	{
		private ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.CountAllValues();

		public ValuesCollection(ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<TValue> GetEnumerator()
		{
			using IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys();
			while (enumKeys.MoveNext())
			{
				TKey current = enumKeys.Current;
				if (myDictionary.TryEnumerateValuesForKey(current, out var enumValues))
				{
					using (enumValues)
					{
						while (enumValues.MoveNext())
						{
							yield return enumValues.Current;
						}
					}
				}
				enumValues = null;
			}
		}

		public override bool Contains(TValue value)
		{
			using (IEnumerator<TValue> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TValue current = enumerator.Current;
					if (myDictionary.EqualValues(current, value))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[Serializable]
	private sealed class EnumerableValuesCollection : ReadOnlyCollectionBase<ICollection<TValue>>
	{
		private ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.Count;

		public EnumerableValuesCollection(ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<ICollection<TValue>> GetEnumerator()
		{
			using IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys();
			while (enumKeys.MoveNext())
			{
				TKey current = enumKeys.Current;
				yield return new ValuesForKeyCollection(myDictionary, current);
			}
		}

		public override bool Contains(ICollection<TValue> values)
		{
			if (values == null)
			{
				return false;
			}
			TValue[] array = Algorithms.ToArray(values);
			using (IEnumerator<ICollection<TValue>> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ICollection<TValue> current = enumerator.Current;
					if (current.Count != array.Length)
					{
						continue;
					}
					if (Algorithms.EqualCollections(current, values, myDictionary.EqualValues))
					{
						return true;
					}
					bool[] array2 = new bool[array.Length];
					foreach (TValue item in current)
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (!array2[i] && myDictionary.EqualValues(item, array[i]))
							{
								array2[i] = true;
							}
						}
					}
					if (Array.IndexOf(array2, value: false) < 0)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[Serializable]
	private sealed class KeyValuePairsCollection : ReadOnlyCollectionBase<KeyValuePair<TKey, TValue>>
	{
		private ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.CountAllValues();

		public KeyValuePairsCollection(ReadOnlyMultiDictionaryBase<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			using IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys();
			while (enumKeys.MoveNext())
			{
				TKey key = enumKeys.Current;
				if (myDictionary.TryEnumerateValuesForKey(key, out var enumValues))
				{
					using (enumValues)
					{
						while (enumValues.MoveNext())
						{
							yield return new KeyValuePair<TKey, TValue>(key, enumValues.Current);
						}
					}
				}
				enumValues = null;
			}
		}

		public override bool Contains(KeyValuePair<TKey, TValue> pair)
		{
			return myDictionary[pair.Key].Contains(pair.Value);
		}
	}

	private volatile IEqualityComparer<TValue> valueEqualityComparer;

	public abstract override int Count { get; }

	public virtual ICollection<TKey> Keys => new KeysCollection(this);

	public virtual ICollection<TValue> Values => new ValuesCollection(this);

	ICollection<ICollection<TValue>> IDictionary<TKey, ICollection<TValue>>.Values => new EnumerableValuesCollection(this);

	public virtual ICollection<KeyValuePair<TKey, TValue>> KeyValuePairs => new KeyValuePairsCollection(this);

	public virtual ICollection<TValue> this[TKey key] => new ValuesForKeyCollection(this, key);

	ICollection<TValue> IDictionary<TKey, ICollection<TValue>>.this[TKey key]
	{
		get
		{
			if (ContainsKey(key))
			{
				return new ValuesForKeyCollection(this, key);
			}
			throw new KeyNotFoundException(Strings.KeyNotFound);
		}
		set
		{
			MethodModifiesCollection();
		}
	}

	private void MethodModifiesCollection()
	{
		throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, Util.SimpleClassName(GetType())));
	}

	protected abstract IEnumerator<TKey> EnumerateKeys();

	protected abstract bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values);

	void IDictionary<TKey, ICollection<TValue>>.Add(TKey key, ICollection<TValue> values)
	{
		MethodModifiesCollection();
	}

	bool IDictionary<TKey, ICollection<TValue>>.Remove(TKey key)
	{
		MethodModifiesCollection();
		return false;
	}

	bool IDictionary<TKey, ICollection<TValue>>.TryGetValue(TKey key, out ICollection<TValue> values)
	{
		if (ContainsKey(key))
		{
			values = this[key];
			return true;
		}
		values = null;
		return false;
	}

	public virtual bool ContainsKey(TKey key)
	{
		IEnumerator<TValue> values;
		return TryEnumerateValuesForKey(key, out values);
	}

	public abstract bool Contains(TKey key, TValue value);

	public override bool Contains(KeyValuePair<TKey, ICollection<TValue>> pair)
	{
		foreach (TValue item in pair.Value)
		{
			if (!Contains(pair.Key, item))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool EqualValues(TValue value1, TValue value2)
	{
		if (valueEqualityComparer == null)
		{
			valueEqualityComparer = EqualityComparer<TValue>.Default;
		}
		return valueEqualityComparer.Equals(value1, value2);
	}

	protected virtual int CountValues(TKey key)
	{
		int num = 0;
		if (TryEnumerateValuesForKey(key, out var values))
		{
			using (values)
			{
				while (values.MoveNext())
				{
					num++;
				}
			}
		}
		return num;
	}

	protected virtual int CountAllValues()
	{
		int num = 0;
		using IEnumerator<TKey> enumerator = EnumerateKeys();
		while (enumerator.MoveNext())
		{
			TKey current = enumerator.Current;
			num += CountValues(current);
		}
		return num;
	}

	public override string ToString()
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		using (IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, ICollection<TValue>> current = enumerator.Current;
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				if (current.Key == null)
				{
					stringBuilder.Append("null");
				}
				else
				{
					stringBuilder.Append(current.Key.ToString());
				}
				stringBuilder.Append("->");
				stringBuilder.Append('(');
				bool flag2 = true;
				foreach (TValue item in current.Value)
				{
					if (!flag2)
					{
						stringBuilder.Append(",");
					}
					if (item == null)
					{
						stringBuilder.Append("null");
					}
					else
					{
						stringBuilder.Append(item.ToString());
					}
					flag2 = false;
				}
				stringBuilder.Append(')');
				flag = false;
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	internal new string DebuggerDisplayString()
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		using (IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, ICollection<TValue>> current = enumerator.Current;
				if (stringBuilder.Length >= 250)
				{
					stringBuilder.Append(", ...");
					break;
				}
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				if (current.Key == null)
				{
					stringBuilder.Append("null");
				}
				else
				{
					stringBuilder.Append(current.Key.ToString());
				}
				stringBuilder.Append("->");
				stringBuilder.Append('(');
				bool flag2 = true;
				foreach (TValue item in current.Value)
				{
					if (!flag2)
					{
						stringBuilder.Append(",");
					}
					if (item == null)
					{
						stringBuilder.Append("null");
					}
					else
					{
						stringBuilder.Append(item.ToString());
					}
					flag2 = false;
				}
				stringBuilder.Append(')');
				flag = false;
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	public override IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> GetEnumerator()
	{
		using IEnumerator<TKey> enumKeys = EnumerateKeys();
		while (enumKeys.MoveNext())
		{
			TKey current = enumKeys.Current;
			yield return new KeyValuePair<TKey, ICollection<TValue>>(current, new ValuesForKeyCollection(this, current));
		}
	}
}
