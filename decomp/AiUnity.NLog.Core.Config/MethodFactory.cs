using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;

namespace AiUnity.NLog.Core.Config;

internal class MethodFactory<TClassAttributeType, TMethodAttributeType> : INamedItemFactory<MethodInfo, MethodInfo>, IFactory where TClassAttributeType : Attribute where TMethodAttributeType : DisplayNameAttribute
{
	private readonly Dictionary<string, MethodInfo> nameToMethodInfo = new Dictionary<string, MethodInfo>();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public IDictionary<string, MethodInfo> AllRegisteredItems => nameToMethodInfo;

	public void ScanTypes(Type[] types, string prefix)
	{
		foreach (Type type in types)
		{
			try
			{
				RegisterType(type, prefix);
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				Logger.Error("Failed to add type '" + type.FullName + "': {0}", ex);
			}
		}
	}

	public void RegisterType(Type type, string itemNamePrefix)
	{
		if (!type.IsDefined(typeof(TClassAttributeType), inherit: false))
		{
			return;
		}
		MethodInfo[] methods = type.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			TMethodAttributeType[] array = (TMethodAttributeType[])methodInfo.GetCustomAttributes(typeof(TMethodAttributeType), inherit: false);
			foreach (TMethodAttributeType val in array)
			{
				RegisterDefinition(itemNamePrefix + val.DisplayName, methodInfo);
			}
		}
	}

	public void Clear()
	{
		nameToMethodInfo.Clear();
	}

	public void RegisterDefinition(string name, MethodInfo methodInfo)
	{
		nameToMethodInfo[name] = methodInfo;
	}

	public bool TryCreateInstance(string name, out MethodInfo result)
	{
		return nameToMethodInfo.TryGetValue(name, out result);
	}

	public MethodInfo CreateInstance(string name)
	{
		if (TryCreateInstance(name, out var result))
		{
			return result;
		}
		throw new NLogConfigurationException("Unknown function: '" + name + "'");
	}

	public bool TryGetDefinition(string name, out MethodInfo result)
	{
		return nameToMethodInfo.TryGetValue(name, out result);
	}
}
