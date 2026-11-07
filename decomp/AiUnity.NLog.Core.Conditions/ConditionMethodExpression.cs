using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionMethodExpression : ConditionExpression
{
	private readonly bool acceptsLogEvent;

	private readonly string conditionMethodName;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public MethodInfo MethodInfo { get; private set; }

	public IList<ConditionExpression> MethodParameters { get; private set; }

	public ConditionMethodExpression(string conditionMethodName, MethodInfo methodInfo, IEnumerable<ConditionExpression> methodParameters)
	{
		MethodInfo = methodInfo;
		this.conditionMethodName = conditionMethodName;
		MethodParameters = new List<ConditionExpression>(methodParameters).AsReadOnly();
		ParameterInfo[] parameters = MethodInfo.GetParameters();
		if (parameters.Length != 0 && parameters[0].ParameterType == typeof(LogEventInfo))
		{
			acceptsLogEvent = true;
		}
		int num = MethodParameters.Count;
		if (acceptsLogEvent)
		{
			num++;
		}
		int num2 = 0;
		int num3 = 0;
		ParameterInfo[] array = parameters;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsOptional)
			{
				num3++;
			}
			else
			{
				num2++;
			}
		}
		if (num < num2 || num > parameters.Length)
		{
			string message = ((num3 <= 0) ? string.Format(CultureInfo.InvariantCulture, "Condition method '{0}' requires {1} parameters, but passed {2}.", conditionMethodName, num2, num) : string.Format(CultureInfo.InvariantCulture, "Condition method '{0}' requires between {1} and {2} parameters, but passed {3}.", conditionMethodName, num2, parameters.Length, num));
			Logger.Error(message);
			throw new ConditionParseException(message);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(conditionMethodName);
		stringBuilder.Append("(");
		string value = string.Empty;
		foreach (ConditionExpression methodParameter in MethodParameters)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(methodParameter);
			value = ", ";
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		int num = (acceptsLogEvent ? 1 : 0);
		object[] array = new object[MethodParameters.Count + num];
		int num2 = 0;
		foreach (ConditionExpression methodParameter in MethodParameters)
		{
			array[num2++ + num] = methodParameter.Evaluate(context);
		}
		if (acceptsLogEvent)
		{
			array[0] = context;
		}
		return MethodInfo.DeclaringType.InvokeMember(MethodInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding, null, null, array, CultureInfo.InvariantCulture);
	}
}
