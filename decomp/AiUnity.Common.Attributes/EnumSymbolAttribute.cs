using System;

namespace AiUnity.Common.Attributes;

public class EnumSymbolAttribute : Attribute
{
	public readonly string EnumSymbol;

	public EnumSymbolAttribute(string enumSymbol)
	{
		EnumSymbol = enumSymbol;
	}

	public override string ToString()
	{
		return EnumSymbol;
	}
}
