using System;
using System.Collections.Generic;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Preserve]
public abstract class MethodCallTargetBase : Target
{
	[ArrayParameter(typeof(MethodCallParameter), "parameter")]
	public IList<MethodCallParameter> Parameters { get; private set; }

	protected MethodCallTargetBase()
	{
		Parameters = new List<MethodCallParameter>();
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		object[] array = new object[Parameters.Count];
		int num = 0;
		foreach (MethodCallParameter parameter in Parameters)
		{
			array[num++] = parameter.GetValue(logEvent.LogEvent);
		}
		DoInvoke(array, logEvent.Continuation);
	}

	protected virtual void DoInvoke(object[] parameters, AsyncContinuation continuation)
	{
		try
		{
			DoInvoke(parameters);
			continuation(null);
		}
		catch (Exception exception)
		{
			if (exception.MustBeRethrown())
			{
				throw;
			}
			continuation(exception);
		}
	}

	protected abstract void DoInvoke(object[] parameters);
}
