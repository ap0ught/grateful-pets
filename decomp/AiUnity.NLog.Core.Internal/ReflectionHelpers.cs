using System;
using System.Collections.Generic;
using System.Reflection;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal;

internal static class ReflectionHelpers
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public static Type[] SafeGetTypes(this Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			Exception[] loaderExceptions = ex.LoaderExceptions;
			foreach (Exception ex2 in loaderExceptions)
			{
				Logger.Warn("Type load exception: {0}", ex2);
			}
			List<Type> list = new List<Type>();
			Type[] types = ex.Types;
			foreach (Type type in types)
			{
				if (type != null)
				{
					list.Add(type);
				}
			}
			return list.ToArray();
		}
	}
}
