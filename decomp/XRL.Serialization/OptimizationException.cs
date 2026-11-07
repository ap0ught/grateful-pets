using System;

namespace XRL.Serialization;

/// <summary>
/// Exception thrown when a value being optimized does not meet the required criteria for optimization.
/// </summary>
public class OptimizationException : Exception
{
	public OptimizationException(string message)
		: base(message)
	{
	}
}
