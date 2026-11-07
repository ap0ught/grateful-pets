using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XRL.Collections;

public class ScopeDisposedList<T> : PooledList<T>
{
	protected static ConcurrentBag<ScopeDisposedList<T>> UnusedPool = new ConcurrentBag<ScopeDisposedList<T>>();

	protected bool Pooled;

	public static ScopeDisposedList<T> GetFromPool()
	{
		if (UnusedPool.TryTake(out var result))
		{
			result.Reuse();
			return result;
		}
		return new ScopeDisposedList<T>();
	}

	public static ScopeDisposedList<T> GetFromPoolFilledWith(IEnumerable<T> items)
	{
		ScopeDisposedList<T> fromPool = GetFromPool();
		fromPool.AddRange(items);
		return fromPool;
	}

	public static ScopeDisposedList<T> GetFromPoolFilledWith(IReadOnlyCollection<T> items)
	{
		ScopeDisposedList<T> fromPool = GetFromPool();
		fromPool.AddRange(items);
		return fromPool;
	}

	public static ScopeDisposedList<T> GetFromPoolFilledWith(IReadOnlyList<T> items)
	{
		ScopeDisposedList<T> fromPool = GetFromPool();
		fromPool.AddRange(items);
		return fromPool;
	}

	protected void Reuse()
	{
		if (!Pooled)
		{
			throw new Exception("Attempt to reuse ScopeDisposedList not in pool");
		}
		Pooled = false;
	}

	public override void Dispose()
	{
		if (!Pooled)
		{
			base.Dispose();
			Pooled = true;
			UnusedPool.Add(this);
		}
	}
}
