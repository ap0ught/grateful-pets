using System.Collections.Generic;
using System.Threading;

namespace XRL.Collections;

public class LRUCache<KeyType, ValueType>
{
	protected class CacheEntry
	{
		public KeyType Key;

		public ValueType Value;

		public CacheEntry Next;

		public CacheEntry Prev;
	}

	public interface IGenerator
	{
		ValueType Generate(KeyType Key);
	}

	protected Dictionary<KeyType, CacheEntry> Cache;

	protected CacheEntry Head;

	protected CacheEntry Tail;

	protected int Capacity;

	protected IGenerator Generator;

	protected IEqualityComparer<KeyType> Comparer;

	protected readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

	public LRUCache(int capacity, IGenerator generator, IEqualityComparer<KeyType> equalityComparer = null)
	{
		Capacity = capacity;
		Generator = generator;
		Comparer = equalityComparer ?? EqualityComparer<KeyType>.Default;
		bool flag = true;
		CacheEntry cacheEntry = null;
		rwlock.EnterWriteLock();
		try
		{
			Cache = new Dictionary<KeyType, CacheEntry>(capacity, equalityComparer);
			for (uint num = 0u; num < capacity; num++)
			{
				CacheEntry cacheEntry2 = new CacheEntry();
				if (flag)
				{
					Head = cacheEntry2;
					flag = false;
				}
				if (cacheEntry != null)
				{
					cacheEntry.Prev = cacheEntry2;
					cacheEntry2.Next = cacheEntry;
				}
				cacheEntry = cacheEntry2;
			}
			Tail = cacheEntry;
		}
		finally
		{
			rwlock.ExitWriteLock();
		}
	}

	~LRUCache()
	{
		rwlock.Dispose();
	}

	public ValueType Get(KeyType Key)
	{
		rwlock.EnterUpgradeableReadLock();
		try
		{
			if (Comparer.Equals(Key, Head.Key))
			{
				return Head.Value;
			}
			if (Cache.TryGetValue(Key, out var value))
			{
				MoveToHead(value);
				return value.Value;
			}
			rwlock.EnterWriteLock();
			try
			{
				value = Tail;
				if (value.Key != null)
				{
					Cache.Remove(value.Key);
				}
				value.Key = Key;
				value.Value = Generator.Generate(Key);
				Cache.Add(Key, value);
				MoveToHead(value);
				return value.Value;
			}
			finally
			{
				rwlock.ExitWriteLock();
			}
		}
		finally
		{
			rwlock.ExitUpgradeableReadLock();
		}
	}

	protected void MoveToHead(CacheEntry entry)
	{
		if (entry == Head)
		{
			return;
		}
		bool isWriteLockHeld = rwlock.IsWriteLockHeld;
		if (!isWriteLockHeld)
		{
			rwlock.EnterWriteLock();
		}
		try
		{
			if (entry.Next != null)
			{
				entry.Next.Prev = entry.Prev;
			}
			if (entry.Prev == null)
			{
				Tail = entry.Next;
			}
			else
			{
				entry.Prev.Next = entry.Next;
			}
			entry.Next = null;
			entry.Prev = Head;
			Head.Next = entry;
			Head = entry;
		}
		finally
		{
			if (!isWriteLockHeld)
			{
				rwlock.ExitWriteLock();
			}
		}
	}
}
