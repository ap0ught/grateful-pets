using System.Collections.Generic;

namespace QupKit;

public class ObjectPool<T> where T : new()
{
	private static Queue<T> Pool = new Queue<T>();

	public static T Checkout()
	{
		lock (Pool)
		{
			if (!Pool.TryDequeue(out var result))
			{
				return new T();
			}
			return result;
		}
	}

	public static bool TryCheckout(out T Object)
	{
		lock (Pool)
		{
			return Pool.TryDequeue(out Object);
		}
	}

	public static void Return(T item)
	{
		lock (Pool)
		{
			Pool.Enqueue(item);
		}
	}
}
