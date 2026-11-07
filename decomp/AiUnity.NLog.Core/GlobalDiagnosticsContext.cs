using System.Collections.Generic;
using AiUnity.Common.Log;

namespace AiUnity.NLog.Core;

public sealed class GlobalDiagnosticsContext : IVariablesContext
{
	private static Dictionary<string, string> contextVariables = new Dictionary<string, string>();

	public static void Set(string item, string value)
	{
		lock (contextVariables)
		{
			contextVariables[item] = value;
		}
	}

	void IVariablesContext.Set(string key, object value)
	{
		Set(key, value.ToString());
	}

	public static string Get(string item)
	{
		lock (contextVariables)
		{
			if (!contextVariables.TryGetValue(item, out var value))
			{
				return string.Empty;
			}
			return value;
		}
	}

	object IVariablesContext.Get(string key)
	{
		return Get(key);
	}

	public static bool Contains(string item)
	{
		lock (contextVariables)
		{
			return contextVariables.ContainsKey(item);
		}
	}

	bool IVariablesContext.Contains(string key)
	{
		return Contains(key);
	}

	public static void Remove(string item)
	{
		lock (contextVariables)
		{
			contextVariables.Remove(item);
		}
	}

	void IVariablesContext.Remove(string key)
	{
		Remove(key);
	}

	public static void Clear()
	{
		lock (contextVariables)
		{
			contextVariables.Clear();
		}
	}

	void IVariablesContext.Clear()
	{
		Clear();
	}
}
