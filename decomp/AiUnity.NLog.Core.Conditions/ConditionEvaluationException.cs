using System;

namespace AiUnity.NLog.Core.Conditions;

[Serializable]
public class ConditionEvaluationException : Exception
{
	public ConditionEvaluationException()
	{
	}

	public ConditionEvaluationException(string message)
		: base(message)
	{
	}

	public ConditionEvaluationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
