using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;

namespace AiUnity.NLog.Core.Internal;

internal class ObjectGraphScanner
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public static T[] FindReachableObjects<T>(params object[] rootObjects) where T : class
	{
		Logger.Trace("FindReachableObject<{0}>:", typeof(T));
		List<T> list = new List<T>();
		Dictionary<object, int> visitedObjects = new Dictionary<object, int>();
		foreach (object o in rootObjects)
		{
			ScanProperties(list, o, 0, visitedObjects);
		}
		return list.ToArray();
	}

	private static void ScanProperties<T>(List<T> result, object o, int level, Dictionary<object, int> visitedObjects) where T : class
	{
		if (o == null || !o.GetType().IsDefined(typeof(NLogConfigurationItemAttribute), inherit: true) || visitedObjects.ContainsKey(o))
		{
			return;
		}
		visitedObjects.Add(o, 0);
		if (o is T item)
		{
			result.Add(item);
		}
		if (Logger.IsTraceEnabled)
		{
			Logger.Trace("{0}Scanning {1} '{2}'", new string(' ', level), o.GetType().Name, o);
		}
		foreach (PropertyInfo allReadableProperty in PropertyHelper.GetAllReadableProperties(o.GetType()))
		{
			if (allReadableProperty.PropertyType.IsPrimitive || allReadableProperty.PropertyType.IsEnum || allReadableProperty.PropertyType == typeof(string) || allReadableProperty.IsDefined(typeof(NLogConfigurationIgnorePropertyAttribute), inherit: true))
			{
				continue;
			}
			object value = allReadableProperty.GetValue(o, null);
			if (value == null)
			{
				continue;
			}
			if (value is IEnumerable enumerable)
			{
				foreach (object item2 in enumerable)
				{
					ScanProperties(result, item2, level + 1, visitedObjects);
				}
			}
			else
			{
				ScanProperties(result, value, level + 1, visitedObjects);
			}
		}
	}
}
