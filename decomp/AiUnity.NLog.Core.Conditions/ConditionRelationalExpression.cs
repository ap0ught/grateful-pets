using System;

namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionRelationalExpression : ConditionExpression
{
	public ConditionExpression LeftExpression { get; private set; }

	public ConditionExpression RightExpression { get; private set; }

	public ConditionRelationalOperator RelationalOperator { get; private set; }

	public ConditionRelationalExpression(ConditionExpression leftExpression, ConditionExpression rightExpression, ConditionRelationalOperator relationalOperator)
	{
		LeftExpression = leftExpression;
		RightExpression = rightExpression;
		RelationalOperator = relationalOperator;
	}

	public override string ToString()
	{
		return "(" + LeftExpression?.ToString() + " " + GetOperatorString() + " " + RightExpression?.ToString() + ")";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		object leftValue = LeftExpression.Evaluate(context);
		object rightValue = RightExpression.Evaluate(context);
		return Compare(leftValue, rightValue, RelationalOperator);
	}

	private static object Compare(object leftValue, object rightValue, ConditionRelationalOperator relationalOperator)
	{
		StringComparer invariantCulture = StringComparer.InvariantCulture;
		PromoteTypes(ref leftValue, ref rightValue);
		return relationalOperator switch
		{
			ConditionRelationalOperator.Equal => (object)(invariantCulture.Compare(leftValue, rightValue) == 0), 
			ConditionRelationalOperator.NotEqual => invariantCulture.Compare(leftValue, rightValue) != 0, 
			ConditionRelationalOperator.Greater => invariantCulture.Compare(leftValue, rightValue) > 0, 
			ConditionRelationalOperator.GreaterOrEqual => invariantCulture.Compare(leftValue, rightValue) >= 0, 
			ConditionRelationalOperator.LessOrEqual => invariantCulture.Compare(leftValue, rightValue) <= 0, 
			ConditionRelationalOperator.Less => invariantCulture.Compare(leftValue, rightValue) < 0, 
			_ => throw new NotSupportedException("Relational operator " + relationalOperator.ToString() + " is not supported."), 
		};
	}

	private static void PromoteTypes(ref object val1, ref object val2)
	{
		if (val1 == null || val2 == null || val1.GetType() == val2.GetType())
		{
			return;
		}
		if (val1 is DateTime || val2 is DateTime)
		{
			val1 = Convert.ToDateTime(val1);
			val2 = Convert.ToDateTime(val2);
			return;
		}
		if (val1 is string || val2 is string)
		{
			val1 = Convert.ToString(val1);
			val2 = Convert.ToString(val2);
			return;
		}
		if (val1 is double || val2 is double)
		{
			val1 = Convert.ToDouble(val1);
			val2 = Convert.ToDouble(val2);
			return;
		}
		if (val1 is float || val2 is float)
		{
			val1 = Convert.ToSingle(val1);
			val2 = Convert.ToSingle(val2);
			return;
		}
		if (val1 is decimal || val2 is decimal)
		{
			val1 = Convert.ToDecimal(val1);
			val2 = Convert.ToDecimal(val2);
			return;
		}
		if (val1 is long || val2 is long)
		{
			val1 = Convert.ToInt64(val1);
			val2 = Convert.ToInt64(val2);
			return;
		}
		if (val1 is int || val2 is int)
		{
			val1 = Convert.ToInt32(val1);
			val2 = Convert.ToInt32(val2);
			return;
		}
		if (val1 is bool || val2 is bool)
		{
			val1 = Convert.ToBoolean(val1);
			val2 = Convert.ToBoolean(val2);
			return;
		}
		throw new ConditionEvaluationException("Cannot find common type for '" + val1.GetType().Name + "' and '" + val2.GetType().Name + "'.");
	}

	private string GetOperatorString()
	{
		return RelationalOperator switch
		{
			ConditionRelationalOperator.Equal => "==", 
			ConditionRelationalOperator.NotEqual => "!=", 
			ConditionRelationalOperator.Greater => ">", 
			ConditionRelationalOperator.Less => "<", 
			ConditionRelationalOperator.GreaterOrEqual => ">=", 
			ConditionRelationalOperator.LessOrEqual => "<=", 
			_ => throw new NotSupportedException("Relational operator " + RelationalOperator.ToString() + " is not supported."), 
		};
	}
}
