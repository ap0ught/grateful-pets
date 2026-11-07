using System.Collections.Generic;

namespace XRL.UI.Framework;

public class PooledFrameworkDataElement<T> : FrameworkDataElement where T : FrameworkDataElement, new()
{
	private static Queue<T> pool = new Queue<T>();

	public static T next()
	{
		if (pool.Count > 0)
		{
			return pool.Dequeue();
		}
		return new T();
	}

	public virtual void free()
	{
		pool.Enqueue(this as T);
	}
}
