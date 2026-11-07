using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;

namespace AiUnity.Common.Services;

public class VariableService : Singleton<VariableService>
{
	private static IInternalLogger Logger => Singleton<CommonInternalLogger>.Instance;

	public string CreateCamelVariable(params string[] names)
	{
		return CreatePascalVariable(names).LowercaseLetter();
	}

	public string CreatePascalVariable(params string[] names)
	{
		if (names == null)
		{
			return null;
		}
		IEnumerable<string> source = from u in names.Where((string s) => !string.IsNullOrEmpty(s)).SelectMany((string s) => s.Split(' ', '-', '.'))
			select u.UppercaseLetter();
		return string.Join(string.Empty, source.ToArray());
	}

	public string GetFormattedValue(string type, object value)
	{
		if (value is string && type != null && !type.After(".").Trim().Equals("string", StringComparison.OrdinalIgnoreCase))
		{
			return GetFormattedValue(typeof(object), value);
		}
		return GetFormattedValue(value.GetType(), value);
	}

	public string GetFormattedValue(Type type, object value)
	{
		if (type == null || value == null)
		{
			return null;
		}
		if (type.Equals(typeof(string)))
		{
			if (value.ToString() == "null" && type.IsValueType)
			{
				return null;
			}
			return $"\"{value}\"";
		}
		if (value is IConvertible && TypeDescriptor.GetConverter(type).IsValid(value))
		{
			try
			{
				string text = Convert.ChangeType(value, type).ToString();
				if (type.Equals(typeof(bool)))
				{
					return text.ToLower();
				}
				if (type.Equals(typeof(float)))
				{
					return text + "f";
				}
				return text;
			}
			catch
			{
			}
		}
		return value.ToString();
	}
}
