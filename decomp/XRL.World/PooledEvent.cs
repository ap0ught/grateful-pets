using System;
using System.Collections.Generic;

namespace XRL.World;

public abstract class PooledEvent<T> : CachedEvent, IDisposable where T : PooledEvent<T>, new()
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(T), null, CountPool, ResetPool);

	protected static readonly List<T> Pool = new List<T>();

	protected static int PoolCounter;

	protected bool Disposed;

	public PooledEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		return Pool.Count;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref T E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		while (PoolCounter > 0)
		{
			T val = Pool[--PoolCounter];
			val.Reset();
			val.Disposed = true;
			if (this == val)
			{
				break;
			}
		}
	}

	public static T FromPool()
	{
		T val = MinEvent.FromPool(Pool, ref PoolCounter);
		val.Disposed = false;
		return val;
	}
}
