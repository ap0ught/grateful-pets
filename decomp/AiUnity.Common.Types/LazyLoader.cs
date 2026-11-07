using System;

namespace AiUnity.Common.Types;

public class LazyLoader<T> where T : class
{
	private readonly Func<T> function;

	private readonly object padlock;

	private bool hasRun;

	private T instance;

	public T Value
	{
		get
		{
			lock (padlock)
			{
				if (!hasRun)
				{
					instance = function();
					hasRun = true;
				}
			}
			return instance;
		}
	}

	public LazyLoader(Func<T> function)
	{
		hasRun = false;
		padlock = new object();
		this.function = function;
	}
}
