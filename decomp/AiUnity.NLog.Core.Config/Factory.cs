using System;
using System.Collections.Generic;
using System.ComponentModel;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;

namespace AiUnity.NLog.Core.Config;

internal class Factory<TBaseType, TAttributeType> : INamedItemFactory<TBaseType, Type>, IFactory where TBaseType : class where TAttributeType : DisplayNameAttribute
{
	private delegate Type GetTypeDelegate();

	private readonly Dictionary<string, GetTypeDelegate> items = new Dictionary<string, GetTypeDelegate>(StringComparer.OrdinalIgnoreCase);

	private ConfigurationItemFactory parentFactory;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	internal Factory(ConfigurationItemFactory parentFactory)
	{
		this.parentFactory = parentFactory;
	}

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
		TAttributeType[] array = (TAttributeType[])type.GetCustomAttributes(typeof(TAttributeType), inherit: false);
		if (array != null)
		{
			TAttributeType[] array2 = array;
			foreach (TAttributeType val in array2)
			{
				RegisterDefinition(itemNamePrefix + val.DisplayName, type);
			}
		}
	}

	public void RegisterNamedType(string itemName, string typeName)
	{
		items[itemName] = () => Type.GetType(typeName, throwOnError: false);
	}

	public void Clear()
	{
		items.Clear();
	}

	public void RegisterDefinition(string name, Type type)
	{
		items[name] = () => type;
	}

	public bool TryGetDefinition(string itemName, out Type result)
	{
		if (!items.TryGetValue(itemName, out var value))
		{
			result = null;
			return false;
		}
		try
		{
			result = value();
			return result != null;
		}
		catch (Exception exception)
		{
			if (exception.MustBeRethrown())
			{
				throw;
			}
			result = null;
			return false;
		}
	}

	public bool TryCreateInstance(string itemName, out TBaseType result)
	{
		if (!TryGetDefinition(itemName, out var result2))
		{
			result = null;
			return false;
		}
		result = (TBaseType)parentFactory.CreateInstance(result2);
		return true;
	}

	public TBaseType CreateInstance(string name)
	{
		if (TryCreateInstance(name, out var result))
		{
			return result;
		}
		throw new ArgumentException(typeof(TBaseType).Name + " cannot be found: '" + name + "'");
	}
}
