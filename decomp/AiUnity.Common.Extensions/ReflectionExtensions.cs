using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AiUnity.Common.Extensions;

public static class ReflectionExtensions
{
	public static T GetAttribute<T>(this PropertyInfo propertyInfo)
	{
		return propertyInfo.GetAttributes<T>().FirstOrDefault();
	}

	public static T GetAttribute<T>(this FieldInfo fieldInfo)
	{
		return fieldInfo.GetAttributes<T>().FirstOrDefault();
	}

	public static IEnumerable<T> GetAttributes<T>(this PropertyInfo propertyInfo)
	{
		return propertyInfo.GetCustomAttributes(typeof(T), inherit: true).Cast<T>();
	}

	public static IEnumerable<T> GetAttributes<T>(this FieldInfo propertyInfo)
	{
		return propertyInfo.GetCustomAttributes(typeof(T), inherit: true).Cast<T>();
	}

	public static bool IsOverriding(this MethodInfo methodInfo)
	{
		if (methodInfo == null)
		{
			throw new ArgumentNullException();
		}
		return methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType;
	}
}
