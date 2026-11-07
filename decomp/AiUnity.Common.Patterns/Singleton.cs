using System;

namespace AiUnity.Common.Patterns;

public abstract class Singleton<T> where T : new()
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				try
				{
					instance = new T();
				}
				catch (Exception innerException)
				{
					throw new Exception("Failed to create Singleton instance.", innerException);
				}
			}
			return instance;
		}
	}

	public static bool InstanceExists => instance != null;
}
