using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Conditions;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Layouts;

namespace AiUnity.NLog.Core.Internal;

internal static class PropertyHelper
{
	private static Dictionary<Type, Dictionary<string, PropertyInfo>> parameterInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	internal static void SetPropertyFromString(object o, string name, string value, ConfigurationItemFactory configurationItemFactory)
	{
		Logger.Debug("Setting '{0}.{1}' to '{2}'", o.GetType().Name, name, value);
		if (!TryGetPropertyInfo(o, name, out var result))
		{
			throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);
		}
		try
		{
			if (result.IsDefined(typeof(ArrayParameterAttribute), inherit: false))
			{
				throw new NotSupportedException("Parameter " + name + " of " + o.GetType().Name + " is an array and cannot be assigned a scalar value.");
			}
			Type propertyType = result.PropertyType;
			propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
			if (!TryNLogSpecificConversion(propertyType, value, out var newValue, configurationItemFactory) && !TryGetEnumValue(propertyType, value, out newValue) && !TrySpecialConversion(propertyType, value, out newValue) && !TryImplicitConversion(propertyType, value, out newValue))
			{
				newValue = Convert.ChangeType(value, propertyType);
			}
			result.SetValue(o, newValue, null);
		}
		catch (TargetInvocationException ex)
		{
			throw new NLogConfigurationException("Error when setting property '" + result.Name + "' on " + o, ex.InnerException);
		}
		catch (Exception ex2)
		{
			if (ex2.MustBeRethrown())
			{
				throw;
			}
			throw new NLogConfigurationException("Error when setting property '" + result.Name + "' on " + o, ex2);
		}
	}

	internal static bool IsArrayProperty(Type t, string name)
	{
		if (!TryGetPropertyInfo(t, name, out var result))
		{
			throw new NotSupportedException("Parameter " + name + " not supported on " + t.Name);
		}
		return result.IsDefined(typeof(ArrayParameterAttribute), inherit: false);
	}

	internal static bool TryGetPropertyInfo(object o, string propertyName, out PropertyInfo result)
	{
		PropertyInfo property = o.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
		if (property != null)
		{
			result = property;
			return true;
		}
		lock (parameterInfoCache)
		{
			Type type = o.GetType();
			if (!parameterInfoCache.TryGetValue(type, out var value))
			{
				value = BuildPropertyInfoDictionary(type);
				parameterInfoCache[type] = value;
			}
			return value.TryGetValue(propertyName, out result);
		}
	}

	internal static Type GetArrayItemType(PropertyInfo propInfo)
	{
		return ((ArrayParameterAttribute)Attribute.GetCustomAttribute(propInfo, typeof(ArrayParameterAttribute)))?.ItemType;
	}

	internal static IEnumerable<PropertyInfo> GetAllReadableProperties(Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
	}

	internal static void CheckRequiredParameters(object o)
	{
		foreach (PropertyInfo allReadableProperty in GetAllReadableProperties(o.GetType()))
		{
			if (allReadableProperty.IsDefined(typeof(RequiredParameterAttribute), inherit: false) && allReadableProperty.GetValue(o, null) == null)
			{
				throw new NLogConfigurationException("Required parameter '" + allReadableProperty.Name + "' on '" + o?.ToString() + "' was not specified.");
			}
		}
	}

	private static bool TryImplicitConversion(Type resultType, string value, out object result)
	{
		if (Type.GetTypeCode(resultType) != TypeCode.Object)
		{
			result = null;
			return false;
		}
		MethodInfo method = resultType.GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
		if (method == null)
		{
			result = null;
			return false;
		}
		result = method.Invoke(null, new object[1] { value });
		return true;
	}

	private static bool TryNLogSpecificConversion(Type propertyType, string value, out object newValue, ConfigurationItemFactory configurationItemFactory)
	{
		if (propertyType == typeof(Layout) || propertyType == typeof(SimpleLayout))
		{
			newValue = new SimpleLayout(value, configurationItemFactory);
			return true;
		}
		if (propertyType == typeof(ConditionExpression))
		{
			newValue = ConditionParser.ParseExpression(value, configurationItemFactory);
			return true;
		}
		newValue = null;
		return false;
	}

	private static bool TryGetEnumValue(Type resultType, string value, out object result)
	{
		if (!resultType.IsEnum)
		{
			result = null;
			return false;
		}
		if (resultType.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			long num = 0L;
			string[] array = value.Split(',');
			foreach (string text in array)
			{
				FieldInfo field = resultType.GetField(text.Trim(), BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if (field == null)
				{
					throw new NLogConfigurationException("Invalid enumeration value '" + value + "'.");
				}
				num |= Convert.ToInt64(field.GetValue(null));
			}
			result = Convert.ChangeType(num, Enum.GetUnderlyingType(resultType));
			result = Enum.ToObject(resultType, result);
			return true;
		}
		FieldInfo field2 = resultType.GetField(value, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (field2 == null)
		{
			throw new NLogConfigurationException("Invalid enumeration value '" + value + "'.");
		}
		result = field2.GetValue(null);
		return true;
	}

	private static bool TrySpecialConversion(Type type, string value, out object newValue)
	{
		if (type == typeof(Uri))
		{
			newValue = new Uri(value, UriKind.RelativeOrAbsolute);
			return true;
		}
		if (type == typeof(Encoding))
		{
			newValue = Encoding.GetEncoding(value);
			return true;
		}
		if (type == typeof(CultureInfo))
		{
			newValue = new CultureInfo(value);
			return true;
		}
		if (type == typeof(Type))
		{
			newValue = Type.GetType(value, throwOnError: true);
			return true;
		}
		newValue = null;
		return false;
	}

	private static bool TryGetPropertyInfo(Type targetType, string propertyName, out PropertyInfo result)
	{
		if (!string.IsNullOrEmpty(propertyName))
		{
			PropertyInfo property = targetType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				result = property;
				return true;
			}
		}
		lock (parameterInfoCache)
		{
			if (!parameterInfoCache.TryGetValue(targetType, out var value))
			{
				value = BuildPropertyInfoDictionary(targetType);
				parameterInfoCache[targetType] = value;
			}
			return value.TryGetValue(propertyName, out result);
		}
	}

	private static Dictionary<string, PropertyInfo> BuildPropertyInfoDictionary(Type t)
	{
		Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
		foreach (PropertyInfo allReadableProperty in GetAllReadableProperties(t))
		{
			ArrayParameterAttribute arrayParameterAttribute = (ArrayParameterAttribute)Attribute.GetCustomAttribute(allReadableProperty, typeof(ArrayParameterAttribute));
			if (arrayParameterAttribute != null)
			{
				dictionary[arrayParameterAttribute.ElementName] = allReadableProperty;
			}
			else
			{
				dictionary[allReadableProperty.Name] = allReadableProperty;
			}
			if (allReadableProperty.IsDefined(typeof(DefaultParameterAttribute), inherit: false))
			{
				dictionary[string.Empty] = allReadableProperty;
			}
		}
		return dictionary;
	}
}
