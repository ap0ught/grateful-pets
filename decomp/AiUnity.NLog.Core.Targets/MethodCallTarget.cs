using System;
using System.Reflection;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("MethodCall")]
[Preserve]
public sealed class MethodCallTarget : MethodCallTargetBase
{
	public string ClassName { get; set; }

	public string MethodName { get; set; }

	private MethodInfo Method { get; set; }

	protected override void InitializeTarget()
	{
		base.InitializeTarget();
		if (ClassName != null && MethodName != null)
		{
			Type type = Type.GetType(ClassName);
			Method = type.GetMethod(MethodName);
		}
		else
		{
			Method = null;
		}
	}

	protected override void DoInvoke(object[] parameters)
	{
		if (Method != null)
		{
			Method.Invoke(null, parameters);
		}
	}
}
