using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Wintellect.PowerCollections;

[Serializable]
[DebuggerDisplay("{DebuggerDisplayString()}")]
public abstract class ReadOnlyDictionaryBase<TKey, TValue> : ReadOnlyCollectionBase<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection
{
	[Serializable]
	private sealed class KeysCollection : ReadOnlyCollectionBase<TKey>
	{
		private ReadOnlyDictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.Count;

		public KeysCollection(ReadOnlyDictionaryBase<TKey, TValue> myDictionary)
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
		private ReadOnlyDictionaryBase<TKey, TValue> myDictionary;

		public override int Count => myDictionary.Count;

		public ValuesCollection(ReadOnlyDictionaryBase<TKey, TValue> myDictionary)
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
			MethodModifiesCollection();
		}
	}

	public virtual ICollection<TKey> Keys => new KeysCollection(this);

	public virtual ICollection<TValue> Values => new ValuesCollection(this);

	bool IDictionary.IsFixedSize => true;

	bool IDictionary.IsReadOnly => true;

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
			MethodModifiesCollection();
		}
	}

	private void MethodModifiesCollection()
	{
		throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, Util.SimpleClassName(GetType())));
	}

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		MethodModifiesCollection();
	}

	public virtual bool Remove(TKey key)
	{
		MethodModifiesCollection();
		return false;
	}

	public virtual bool ContainsKey(TKey key)
	{
		TValue value;
		return TryGetValue(key, out value);
	}

	public abstract bool TryGetValue(TKey key, out TValue value);

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

	public override bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (ContainsKey(item.Key))
		{
			return object.Equals(this[item.Key], item.Value);
		}
		return false;
	}

	void IDictionary.Add(object key, object value)
	{
		MethodModifiesCollection();
	}

	void IDictionary.Clear()
	{
		MethodModifiesCollection();
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
		MethodModifiesCollection();
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
