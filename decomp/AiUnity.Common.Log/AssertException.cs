using System;

namespace AiUnity.Common.Log;

public class AssertException : Exception
{
	public AssertException(string message, Exception innerException = null)
		: base(message, innerException)
	{
	}
}
