using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Genkit;
using XRL.World;

namespace XRL.Collections;

/// <summary>String-keyed dictionary that also accepts <see cref="T:System.ReadOnlySpan`1" /> of type <see cref="T:System.Char" /> for keys.</summary>
/// <remarks>Stable key hashing with <see cref="M:Genkit.Hash.FNV1A64(System.String,System.UInt64)" />.</remarks>
[Serializable]
public class StringMap<T> : IDictionary<string, T>, ICollection<KeyValuePair<string, T>>, IEnumerable<KeyValuePair<string, T>>, IEnumerable, IReadOnlyDictionary<string, T>, IReadOnlyCollection<KeyValuePair<string, T>>, IComposite, IDictionary, ICollection
{
	protected struct Slot
	{
		public ulong Hash;

		public int Next;

		public string Key;

		public T Value;

		public bool Strict;
	}

	[Serializable]
	public struct Enumerator : IEnumerator<KeyValuePair<string, T>>, IEnumerator, IDisposable, IDictionaryEnumerator
	{
		private StringMap<T> Map;

		private int Version;

		private int Index;

		public KeyValuePair<string, T> Current => new KeyValuePair<string, T>(Map.Slots[Index].Key, Map.Slots[Index].Value);

		object IEnumerator.Current => new KeyValuePair<string, T>(Map.Slots[Index].Key, Map.Slots[Index].Value);

		DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Map.Slots[Index].Key, Map.Slots[Index].Value);

		object IDictionaryEnumerator.Key => Map.Slots[Index].Key;

		object IDictionaryEnumerator.Value => Map.Slots[Index].Value;

		public Enumerator(StringMap<T> Map)
		{
			this.Map = Map;
			Version = Map.Version;
			Index = -1;
		}

		public bool MoveNext()
		{
			if (Version != Map.Version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			while (++Index < Map.Length)
			{
				if (Map.Slots[Index].Key != null)
				{
					return true;
				}
			}
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			if (Version != Map.Version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			Index = -1;
		}
	}

	[Serializable]
	public struct ValueEnumerator(StringMap<T> Map) : IEnumerator<T>, IEnumerator, IDisposable
	{
		private StringMap<T> Map = Map;

		private int Version = Map.Version;

		private int Index = -1;

		public T Current => Map.Slots[Index].Value;

		object IEnumerator.Current => Map.Slots[Index].Value;

		public ValueEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (Version != Map.Version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			while (++Index < Map.Length)
			{
				if (Map.Slots[Index].Key != null)
				{
					return true;
				}
			}
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			if (Version != Map.Version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			Index = -1;
		}
	}

	protected int[] Buckets = Array.Empty<int>();

	protected Slot[] Slots = Array.Empty<Slot>();

	protected int Size;

	protected int Length;

	protected int Amount;

	protected int Next = -1;

	protected int Version;

	protected ulong _Seed;

	public ValueEnumerator Values => new ValueEnumerator(this);

	public int Capacity => Size;

	public int Count => Amount;

	public virtual bool WantFieldReflection => false;

	object ICollection.SyncRoot => this;

	bool ICollection.IsSynchronized => false;

	bool IDictionary.IsFixedSize => false;

	bool IDictionary.IsReadOnly => false;

	bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

	ICollection IDictionary.Keys
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	ICollection<string> IDictionary<string, T>.Keys
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	IEnumerable<string> IReadOnlyDictionary<string, T>.Keys
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	ICollection<T> IDictionary<string, T>.Values
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	IEnumerable<T> IReadOnlyDictionary<string, T>.Values
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public T this[[AllowNull] string Key]
	{
		get
		{
			int num = IndexOf(Key);
			if (num < 0)
			{
				return default;
			}
			return Slots[num].Value;
		}
		set
		{
			InsertInternal(Key, value);
		}
	}

	public T this[ReadOnlySpan<char> Key]
	{
		get
		{
			int num = IndexOf(Key);
			if (num < 0)
			{
				return default;
			}
			return Slots[num].Value;
		}
		set
		{
			InsertInternal(Key, value);
		}
	}

	object IDictionary.this[object Key]
	{
		get
		{
			int num = IndexOf((string)Key);
			return (num >= 0) ? Slots[num].Value : default(T);
		}
		set
		{
			InsertInternal((string)Key, (T)value);
		}
	}

	public T this[int Index] => Slots[Index].Value;

	public StringMap()
	{
		_Seed = 14695981039346656037uL;
	}

	public StringMap(int Capacity = 0, ulong Seed = 14695981039346656037uL)
	{
		_Seed = Seed;
		if (Capacity > 0)
		{
			InitCapacity(Capacity);
		}
	}

	public StringMap([DisallowNull] IDictionary<string, T> Dictionary, ulong Seed = 14695981039346656037uL)
		: this(Dictionary.Count, Seed)
	{
		foreach (KeyValuePair<string, T> item in Dictionary)
		{
			Add(item.Key, item.Value);
		}
	}

	protected void InitCapacity(int Capacity)
	{
		Capacity = Hash.GetCapacity(Capacity);
		Buckets = new int[Capacity];
		Slots = new Slot[Capacity];
	}

	public void Add([AllowNull] string Key, T Value)
	{
		InsertInternal(Key, Value, ReturnOnDuplicate: false, ThrowOnDuplicate: true);
	}

	public void Add(ReadOnlySpan<char> Key, T Value)
	{
		InsertInternal(Key, Value, ReturnOnDuplicate: false, ThrowOnDuplicate: true);
	}

	public bool TryAdd([AllowNull] string Key, T Value)
	{
		return InsertInternal(Key, Value, ReturnOnDuplicate: true);
	}

	public bool TryAdd(ReadOnlySpan<char> Key, T Value)
	{
		return InsertInternal(Key, Value, ReturnOnDuplicate: true);
	}

	public virtual void Clear()
	{
		if (Length > 0)
		{
			Array.Clear(Buckets, 0, Size);
			Array.Clear(Slots, 0, Length);
			Next = -1;
			Length = 0;
			Amount = 0;
			Version++;
		}
	}

	public void ClearValues()
	{
		for (int i = 0; i < Length; i++)
		{
			Slots[i].Value = default;
		}
	}

	public bool ContainsKey([AllowNull] string Key)
	{
		return IndexOf(Key) >= 0;
	}

	public bool ContainsKey(ReadOnlySpan<char> Key)
	{
		return IndexOf(Key) >= 0;
	}

	public bool ContainsValue([AllowNull] T Value)
	{
		if (Value == null)
		{
			for (int i = 0; i < Length; i++)
			{
				if (Slots[i].Key != null && Slots[i].Value == null)
				{
					return true;
				}
			}
		}
		else
		{
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int j = 0; j < Length; j++)
			{
				if (Slots[j].Key != null && equalityComparer.Equals(Slots[j].Value, Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected void CopyTo(KeyValuePair<string, T>[] Array, int Index)
	{
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				Array[Index++] = new KeyValuePair<string, T>(Slots[i].Key, Slots[i].Value);
			}
		}
	}

	protected int IndexOf([AllowNull] string Key)
	{
		if (Key != null && Size > 0)
		{
			ulong num = Hash.FNV1A64(Key, _Seed);
			for (int num2 = Buckets[num % (ulong)Size] - 1; num2 >= 0; num2 = Slots[num2].Next)
			{
				if (Slots[num2].Hash == num && (!Slots[num2].Strict || Key.Equals(Slots[num2].Key, StringComparison.Ordinal)))
				{
					return num2;
				}
			}
		}
		return -1;
	}

	protected int IndexOf([AllowNull] StringBuilder Text, int Start, int Length)
	{
		if (Text != null && Size > 0)
		{
			Span<char> span = stackalloc char[Length];
			for (int i = 0; i < Length; i++)
			{
				span[i] = Text[i + Start];
			}
			return IndexOf(span);
		}
		return -1;
	}

	protected int IndexOf(ReadOnlySpan<char> Key)
	{
		if (Size > 0)
		{
			ulong num = Hash.FNV1A64(Key, _Seed);
			for (int num2 = Buckets[num % (ulong)Size] - 1; num2 >= 0; num2 = Slots[num2].Next)
			{
				if (Slots[num2].Hash == num && (!Slots[num2].Strict || MemoryExtensions.Equals(Key, Slots[num2].Key, StringComparison.Ordinal)))
				{
					return num2;
				}
			}
		}
		return -1;
	}

	protected bool InsertInternal(string Key, T Value, bool ReturnOnDuplicate = false, bool ThrowOnDuplicate = false)
	{
		if (Key == null)
		{
			return false;
		}
		ulong num = Hash.FNV1A64(Key, _Seed);
		ulong num2 = 0uL;
		int num3 = -1;
		bool strict = false;
		if (Size > 0)
		{
			num2 = num % (ulong)Size;
			for (num3 = Buckets[num2] - 1; num3 >= 0; num3 = Slots[num3].Next)
			{
				if (Slots[num3].Hash == num)
				{
					if (Key.Equals(Slots[num3].Key, StringComparison.Ordinal))
					{
						if (ThrowOnDuplicate)
						{
							throw new ArgumentException("Element by key '" + Key + "' already exists.");
						}
						if (ReturnOnDuplicate)
						{
							return true;
						}
						Slots[num3].Value = Value;
						Version++;
						return true;
					}
					strict = (Slots[num3].Strict = true);
				}
			}
		}
		if (Next >= 0)
		{
			Next = Slots[num3 = Next].Next;
		}
		else
		{
			if (Length == Size)
			{
				Resize(Length * 2);
				num2 = num % (ulong)Size;
			}
			num3 = Length++;
		}
		Slots[num3].Hash = num;
		Slots[num3].Next = Buckets[num2] - 1;
		Slots[num3].Key = Key;
		Slots[num3].Value = Value;
		Slots[num3].Strict = strict;
		Buckets[num2] = num3 + 1;
		Version++;
		Amount++;
		return true;
	}

	protected bool InsertInternal(ReadOnlySpan<char> Key, T Value, bool ReturnOnDuplicate = false, bool ThrowOnDuplicate = false)
	{
		ulong num = Hash.FNV1A64(Key, _Seed);
		ulong num2 = 0uL;
		int num3 = -1;
		bool strict = false;
		if (Size > 0)
		{
			num2 = num % (ulong)Size;
			for (num3 = Buckets[num2] - 1; num3 >= 0; num3 = Slots[num3].Next)
			{
				if (Slots[num3].Hash == num)
				{
					if (MemoryExtensions.Equals(Key, Slots[num3].Key, StringComparison.Ordinal))
					{
						if (ThrowOnDuplicate)
						{
							throw new ArgumentException("Element by key '" + Slots[num3].Key + "' already exists.");
						}
						if (ReturnOnDuplicate)
						{
							return true;
						}
						Slots[num3].Value = Value;
						Version++;
						return true;
					}
					strict = (Slots[num3].Strict = true);
				}
			}
		}
		if (Next >= 0)
		{
			Next = Slots[num3 = Next].Next;
		}
		else
		{
			if (Length == Size)
			{
				Resize(Length * 2);
				num2 = num % (ulong)Size;
			}
			num3 = Length++;
		}
		Slots[num3].Hash = num;
		Slots[num3].Next = Buckets[num2] - 1;
		Slots[num3].Key = new string(Key);
		Slots[num3].Value = Value;
		Slots[num3].Strict = strict;
		Buckets[num2] = num3 + 1;
		Version++;
		Amount++;
		return true;
	}

	protected void Resize(int Capacity)
	{
		Capacity = Hash.GetCapacity(Capacity);
		if (Capacity == Size)
		{
			return;
		}
		int[] array = new int[Capacity];
		Slot[] array2 = new Slot[Capacity];
		Array.Copy(Slots, 0, array2, 0, Size);
		for (int i = 0; i < Size; i++)
		{
			if (Slots[i].Key != null)
			{
				ulong num = array2[i].Hash % (ulong)Capacity;
				array2[i].Next = array[num] - 1;
				array[num] = i + 1;
			}
		}
		Buckets = array;
		Slots = array2;
		Size = Capacity;
	}

	public void EnsureCapacity(int Capacity)
	{
		if (Size < Capacity)
		{
			Resize(Capacity);
		}
	}

	public bool Remove([AllowNull] string Key)
	{
		if (Key == null || Size == 0)
		{
			return false;
		}
		ulong num = Hash.FNV1A64(Key, _Seed);
		ulong num2 = num % (ulong)Size;
		int num3 = 0;
		int num4 = Buckets[num2] - 1;
		for (int num5 = -1; num4 >= 0; num5 = num4, num4 = Slots[num4].Next)
		{
			if (Slots[num4].Hash != num)
			{
				continue;
			}
			if (Slots[num4].Strict)
			{
				num3++;
				if (!Key.Equals(Slots[num4].Key, StringComparison.Ordinal))
				{
					continue;
				}
			}
			if (num5 < 0)
			{
				Buckets[num2] = Slots[num4].Next + 1;
			}
			else
			{
				Slots[num5].Next = Slots[num4].Next;
			}
			Slots[num4].Hash = 0uL;
			Slots[num4].Next = Next;
			Slots[num4].Key = null;
			Slots[num4].Value = default;
			Slots[num4].Strict = true;
			Next = num4;
			Amount--;
			Version++;
			if (num3 > 0 && num3 <= 2)
			{
				ValidateCollisions(Buckets[num2] - 1, num);
			}
			return true;
		}
		return false;
	}

	public bool Remove(ReadOnlySpan<char> Key)
	{
		if (Size == 0)
		{
			return false;
		}
		ulong num = Hash.FNV1A64(Key, _Seed);
		ulong num2 = num % (ulong)Size;
		int num3 = 0;
		int num4 = Buckets[num2] - 1;
		for (int num5 = -1; num4 >= 0; num5 = num4, num4 = Slots[num4].Next)
		{
			if (Slots[num4].Hash != num)
			{
				continue;
			}
			if (Slots[num4].Strict)
			{
				num3++;
				if (!MemoryExtensions.Equals(Key, Slots[num4].Key, StringComparison.Ordinal))
				{
					continue;
				}
			}
			if (num5 < 0)
			{
				Buckets[num2] = Slots[num4].Next + 1;
			}
			else
			{
				Slots[num5].Next = Slots[num4].Next;
			}
			Slots[num4].Hash = 0uL;
			Slots[num4].Next = Next;
			Slots[num4].Key = null;
			Slots[num4].Value = default;
			Slots[num4].Strict = true;
			Next = num4;
			Amount--;
			Version++;
			if (num3 > 0 && num3 <= 2)
			{
				ValidateCollisions(Buckets[num2] - 1, num);
			}
			return true;
		}
		return false;
	}

	protected void ValidateCollisions(int Slot, ulong Hash)
	{
		int num = -1;
		for (int num2 = Slot; num2 >= 0; num2 = Slots[num2].Next)
		{
			if (Slots[num2].Hash == Hash)
			{
				if (num != -1)
				{
					return;
				}
				num = num2;
			}
		}
		Slots[num].Strict = false;
	}

	public T GetValue(string Key)
	{
		int num = IndexOf(Key);
		if (num < 0)
		{
			return default;
		}
		return Slots[num].Value;
	}

	public T GetValue(ReadOnlySpan<char> Key)
	{
		int num = IndexOf(Key);
		if (num < 0)
		{
			return default;
		}
		return Slots[num].Value;
	}

	public T GetValue(ReadOnlySpan<char> Key, T Default)
	{
		int num = IndexOf(Key);
		if (num < 0)
		{
			return Default;
		}
		return Slots[num].Value;
	}

	public bool TryGetValue(string Key, out T Value)
	{
		int num = IndexOf(Key);
		if (num >= 0)
		{
			Value = Slots[num].Value;
			return true;
		}
		Value = default;
		return false;
	}

	public bool TryGetValue(ReadOnlySpan<char> Key, out T Value)
	{
		int num = IndexOf(Key);
		if (num >= 0)
		{
			Value = Slots[num].Value;
			return true;
		}
		Value = default;
		return false;
	}

	public bool TryGetValue(StringBuilder Builder, int Start, int Length, out T Value)
	{
		int num = IndexOf(Builder, Start, Length);
		if (num >= 0)
		{
			Value = Slots[num].Value;
			return true;
		}
		Value = default;
		return false;
	}

	public bool TryGetPair(string Key, out KeyValuePair<string, T> Pair)
	{
		int num = IndexOf(Key);
		if (num >= 0)
		{
			Pair = new KeyValuePair<string, T>(Slots[num].Key, Slots[num].Value);
			return true;
		}
		Pair = default;
		return false;
	}

	public bool TryGetPair(ReadOnlySpan<char> Key, out KeyValuePair<string, T> Pair)
	{
		int num = IndexOf(Key);
		if (num >= 0)
		{
			Pair = new KeyValuePair<string, T>(Slots[num].Key, Slots[num].Value);
			return true;
		}
		Pair = default;
		return false;
	}

	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Amount);
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				Writer.WriteOptimized(Slots[i].Key);
				Writer.WriteObject(Slots[i].Value);
			}
		}
	}

	public virtual void Read(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		Resize(num);
		for (int i = 0; i < num; i++)
		{
			InsertInternal(Reader.ReadOptimizedString(), (T)Reader.ReadObject());
		}
	}

	void IDictionary.Add(object Key, object Value)
	{
		Add((string)Key, (T)Value);
	}

	void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> Pair)
	{
		Add(Pair.Key, Pair.Value);
	}

	bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> Pair)
	{
		int num = IndexOf(Pair.Key);
		if (num >= 0 && EqualityComparer<T>.Default.Equals(Slots[num].Value, Pair.Value))
		{
			return true;
		}
		return false;
	}

	bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> Pair)
	{
		int num = IndexOf(Pair.Key);
		if (num >= 0 && EqualityComparer<T>.Default.Equals(Slots[num].Value, Pair.Value))
		{
			Remove(Pair.Key);
			return true;
		}
		return false;
	}

	void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] Array, int Index)
	{
		CopyTo(Array, Index);
	}

	void ICollection.CopyTo(Array Array, int Index)
	{
		CopyTo((KeyValuePair<string, T>[])Array, Index);
	}

	bool IDictionary.Contains(object Key)
	{
		if (Key is string key)
		{
			return ContainsKey(key);
		}
		return false;
	}

	void IDictionary.Remove(object Key)
	{
		if (Key is string key)
		{
			Remove(key);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
