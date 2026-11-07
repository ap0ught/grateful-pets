using System;
using System.Linq;

namespace AiUnity.Common.Extensions;

public static class SystemExtensions
{
	public static string GetCSharpName(this Type type, bool fullName = false)
	{
		if (type == null || type.Equals(typeof(void)))
		{
			return "void";
		}
		Type underlyingType = Nullable.GetUnderlyingType(type);
		Type type2 = ((underlyingType != null) ? underlyingType : type);
		string text = ((underlyingType != null) ? "?" : string.Empty);
		if (type2.IsGenericType)
		{
			return string.Format("{0}<{1}>{2}", type2.Name.Substring(0, type2.Name.IndexOf('`')), string.Join(", ", (from ga in type2.GetGenericArguments()
				select ga.GetCSharpName()).ToArray()), text);
		}
		return Type.GetTypeCode(type2) switch
		{
			TypeCode.Boolean => "bool", 
			TypeCode.Byte => "byte", 
			TypeCode.SByte => "sbyte", 
			TypeCode.Char => "char", 
			TypeCode.Decimal => "decimal" + text, 
			TypeCode.Double => "double" + text, 
			TypeCode.Single => "float" + text, 
			TypeCode.Int32 => "int" + text, 
			TypeCode.UInt32 => "uint" + text, 
			TypeCode.Int64 => "long" + text, 
			TypeCode.UInt64 => "ulong" + text, 
			TypeCode.Int16 => "short" + text, 
			TypeCode.UInt16 => "ushort" + text, 
			TypeCode.String => "string", 
			TypeCode.Object => (fullName ? type2.FullName : type2.Name) + text, 
			_ => null, 
		};
	}

	public static int GetInheritanceDepth(this Type type)
	{
		int num = 0;
		Type type2 = type;
		while (type2 != null)
		{
			num++;
			type2 = type2.BaseType;
		}
		return num;
	}
}
