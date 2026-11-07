using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Wintellect.PowerCollections;

[Serializable]
[DebuggerDisplay("{DebuggerDisplayString()}")]
public abstract class DictionaryBase<TKey, TValue> : CollectionBase<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection
{
	[Serializable]
	private sealed class KeysCollection : ReadOnlyCollectionBase<TKey>
	{
		private DictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.Count;

		public KeysCollection(DictionaryBase<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<TKey> GetEnumerator()
		{
			foreach (KeyValuePair<TKey, TValue> item in myDictionary)
			{
				yield return item.Key;
			}
		}

		public override bool Contains(TKey key)
		{
			return myDictionary.ContainsKey(key);
		}
	}

	[Serializable]
	private sealed class ValuesCollection : ReadOnlyCollectionBase<TValue>
	{
		private DictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.Count;

		public ValuesCollection(DictionaryBase<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<TValue> GetEnumerator()
		{
			foreach (KeyValuePair<TKey, TValue> item in myDictionary)
			{
				yield return item.Value;
			}
		}
	}

	[Serializable]
	private class DictionaryEnumeratorWrapper : IDictionaryEnumerator, IEnumerator
	{
		private IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

		public DictionaryEntry Entry
		{
			get
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				DictionaryEntry result = default;
				if (current.Key != null)
				{
					result.Key = current.Key;
				}
				result.Value = current.Value;
				return result;
			}
		}

		public object Key => enumerator.Current.Key;

		public object Value => enumerator.Current.Value;

		public object Current => Entry;

		public DictionaryEnumeratorWrapper(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
		{
			this.enumerator = enumerator;
		}

		public void Reset()
		{
			throw new NotSupportedException(Strings.ResetNotSupported);
		}

		public bool MoveNext()
		{
			return enumerator.MoveNext();
		}
	}

	public virtual TValue this[TKey key]
	{
		get
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			throw new KeyNotFoundException(Strings.KeyNotFound);
		}
		set
		{
			throw new NotImplementedException(Strings.MustOverrideIndexerSet);
		}
	}

	public virtual ICollection<TKey> Keys => new KeysCollection(this);

	public virtual ICollection<TValue> Values => new ValuesCollection(this);

	bool IDictionary.IsFixedSize => false;

	bool IDictionary.IsReadOnly => false;

	ICollection IDictionary.Keys => new KeysCollection(this);

	ICollection IDictionary.Values => new ValuesCollection(this);

	object IDictionary.this[object key]
	{
		get
		{
			if (key is TKey || key == null)
			{
				TKey key2 = (TKey)key;
				if (TryGetValue(key2, out var value))
				{
					return value;
				}
				return null;
			}
			return null;
		}
		set
		{
			CheckGenericType<TKey>("key", key);
			CheckGenericType<TValue>("value", value);
			this[(TKey)key] = (TValue)value;
		}
	}

	public abstract override void Clear();

	public abstract bool Remove(TKey key);

	public abstract bool TryGetValue(TKey key, out TValue value);

	public virtual void Add(TKey key, TValue value)
	{
		if (ContainsKey(key))
		{
			throw new ArgumentException(Strings.KeyAlreadyPresent, "key");
		}
		this[key] = value;
	}

	public virtual bool ContainsKey(TKey key)
	{
		TValue value;
		return TryGetValue(key, out value);
	}

	public override string ToString()
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
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
				if (current.Value == null)
				{
					stringBuilder.Append("null");
				}
				else
				{
					stringBuilder.Append(current.Value.ToString());
				}
				flag = false;
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	public new virtual IDictionary<TKey, TValue> AsReadOnly()
	{
		return Algorithms.ReadOnly(this);
	}

	public override void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public override bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (ContainsKey(item.Key))
		{
			return object.Equals(this[item.Key], item.Value);
		}
		return false;
	}

	public override bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (((ICollection<KeyValuePair<TKey, TValue>>)this).Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	private void CheckGenericType<ExpectedType>(string name, object value)
	{
		if (!(value is ExpectedType))
		{
			throw new ArgumentException(string.Format(Strings.WrongType, value, typeof(ExpectedType)), name);
		}
	}

	void IDictionary.Add(object key, object value)
	{
		CheckGenericType<TKey>("key", key);
		CheckGenericType<TValue>("value", value);
		Add((TKey)key, (TValue)value);
	}

	void IDictionary.Clear()
	{
		Clear();
	}

	bool IDictionary.Contains(object key)
	{
		if (key is TKey || key == null)
		{
			return ContainsKey((TKey)key);
		}
		return false;
	}

	void IDictionary.Remove(object key)
	{
		if (key is TKey || key == null)
		{
			Remove((TKey)key);
		}
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new DictionaryEnumeratorWrapper(GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IDictionary)this).GetEnumerator();
	}

	internal new string DebuggerDisplayString()
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
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
				if (current.Value == null)
				{
					stringBuilder.Append("null");
				}
				else
				{
					stringBuilder.Append(current.Value.ToString());
				}
				flag = false;
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
