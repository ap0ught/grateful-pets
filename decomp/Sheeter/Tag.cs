using System;
using System.Linq.Expressions;
using XRL.World;

namespace Sheeter;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class Tag : BlueprintValueElement
{
	public override string GetFrom(GameObjectBlueprint Blueprint)
	{
		if (!Blueprint.Tags.TryGetValue(Key, out var value))
		{
			return null;
		}
		if (Value is string text && ((Operator == ExpressionType.Equal && value != text) || (Operator == ExpressionType.NotEqual && value == text)))
		{
			return null;
		}
		if (Output)
		{
			return value;
		}
		return "true";
	}
}
