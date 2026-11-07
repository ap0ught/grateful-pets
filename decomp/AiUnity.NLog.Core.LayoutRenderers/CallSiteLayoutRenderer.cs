using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("callsite", false)]
[ThreadAgnostic]
[Preserve]
public class CallSiteLayoutRenderer : LayoutRenderer, IUsesStackTrace
{
	[DefaultValue(true)]
	public bool ClassName { get; set; }

	[DefaultValue(true)]
	public bool MethodName { get; set; }

	[DefaultValue(false)]
	public bool CleanNamesOfAnonymousDelegates { get; set; }

	[DefaultValue(0)]
	public int SkipFrames { get; set; }

	[DefaultValue(false)]
	public bool FileName { get; set; }

	[DefaultValue(true)]
	public bool IncludeSourcePath { get; set; }

	StackTraceUsage IUsesStackTrace.StackTraceUsage
	{
		get
		{
			if (FileName)
			{
				return StackTraceUsage.WithSource;
			}
			return StackTraceUsage.WithoutSource;
		}
	}

	public CallSiteLayoutRenderer()
	{
		ClassName = true;
		MethodName = true;
		CleanNamesOfAnonymousDelegates = false;
		FileName = false;
		IncludeSourcePath = true;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		StackFrame stackFrame = ((logEvent.StackTrace != null) ? logEvent.StackTrace.GetFrame(logEvent.UserStackFrameNumber + SkipFrames) : null);
		if (stackFrame == null)
		{
			return;
		}
		MethodBase method = stackFrame.GetMethod();
		if (ClassName)
		{
			if (method.DeclaringType != null)
			{
				string text = method.DeclaringType.FullName;
				if (CleanNamesOfAnonymousDelegates && text.Contains("+<>"))
				{
					text = text[..text.IndexOf("+<>")];
				}
				builder.Append(text);
			}
			else
			{
				builder.Append("<no type>");
			}
		}
		if (MethodName)
		{
			if (ClassName)
			{
				builder.Append(".");
			}
			if (method != null)
			{
				string text2 = method.Name;
				if (CleanNamesOfAnonymousDelegates && text2.Contains("__") && text2.StartsWith("<") && text2.Contains(">"))
				{
					int num = text2.IndexOf('<') + 1;
					int num2 = text2.IndexOf('>');
					text2 = text2.Substring(num, num2 - num);
				}
				builder.Append(text2);
			}
			else
			{
				builder.Append("<no method>");
			}
		}
		if (!FileName)
		{
			return;
		}
		string fileName = stackFrame.GetFileName();
		if (fileName != null)
		{
			builder.Append("(");
			if (IncludeSourcePath)
			{
				builder.Append(fileName);
			}
			else
			{
				builder.Append(Path.GetFileName(fileName));
			}
			builder.Append(":");
			builder.Append(stackFrame.GetFileLineNumber());
			builder.Append(")");
		}
	}
}
