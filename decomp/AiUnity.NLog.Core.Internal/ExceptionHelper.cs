using System;
using System.Threading;
using AiUnity.Common.Log;

namespace AiUnity.NLog.Core.Internal;

internal static class ExceptionHelper
{
	public static bool MustBeRethrown(this Exception exception)
	{
		if (exception is AssertException)
		{
			return true;
		}
		if (exception is StackOverflowException)
		{
			return true;
		}
		if (exception is ThreadAbortException)
		{
			return true;
		}
		if (exception is OutOfMemoryException)
		{
			return true;
		}
		if (exception is NLogConfigurationException)
		{
			return true;
		}
		if (exception.GetType().IsSubclassOf(typeof(NLogConfigurationException)))
		{
			return true;
		}
		return false;
	}
}
