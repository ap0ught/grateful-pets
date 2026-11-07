using System;
using System.Linq.Expressions;
using XRL.World;

namespace Sheeter;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class IntProperty : BlueprintValueElement
{
	public override string GetFrom(GameObjectBlueprint Blueprint)
	{
		if (!Blueprint.IntProps.TryGetValue(Key, out var value))
		{
			return null;
		}
		if (Value is int num && ((Operator == ExpressionType.Equal && value != num) || (Operator == ExpressionType.NotEqual && value == num) || (Operator == ExpressionType.GreaterThan && value <= num) || (Operator == ExpressionType.GreaterThanOrEqual && value < num) || (Operator == ExpressionType.LessThan && value >= num) || (Operator == ExpressionType.LessThanOrEqual && value > num)))
		{
			return null;
		}
		if (Output)
		{
			return value.ToString();
		}
		return "true";
	}
}
