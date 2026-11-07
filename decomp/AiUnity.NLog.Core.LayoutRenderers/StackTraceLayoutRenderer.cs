using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("stacktrace", false)]
[ThreadAgnostic]
[Preserve]
public class StackTraceLayoutRenderer : LayoutRenderer, IUsesStackTrace
{
	[DefaultValue("Flat")]
	public StackTraceFormat Format { get; set; }

	[DefaultValue(3)]
	public int TopFrames { get; set; }

	[DefaultValue(" => ")]
	public string Separator { get; set; }

	StackTraceUsage IUsesStackTrace.StackTraceUsage => StackTraceUsage.WithoutSource;

	public StackTraceLayoutRenderer()
	{
		Separator = " => ";
		TopFrames = 3;
		Format = StackTraceFormat.Flat;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		bool flag = true;
		int num = logEvent.UserStackFrameNumber + TopFrames - 1;
		if (num >= logEvent.StackTrace.FrameCount)
		{
			num = logEvent.StackTrace.FrameCount - 1;
		}
		switch (Format)
		{
		case StackTraceFormat.Raw:
		{
			for (int num3 = num; num3 >= logEvent.UserStackFrameNumber; num3--)
			{
				StackFrame frame2 = logEvent.StackTrace.GetFrame(num3);
				builder.Append(frame2.ToString());
			}
			break;
		}
		case StackTraceFormat.Flat:
		{
			for (int num4 = num; num4 >= logEvent.UserStackFrameNumber; num4--)
			{
				StackFrame frame3 = logEvent.StackTrace.GetFrame(num4);
				if (!flag)
				{
					builder.Append(Separator);
				}
				Type declaringType = frame3.GetMethod().DeclaringType;
				if (declaringType != null)
				{
					builder.Append(declaringType.Name);
				}
				else
				{
					builder.Append("<no type>");
				}
				builder.Append(".");
				builder.Append(frame3.GetMethod().Name);
				flag = false;
			}
			break;
		}
		case StackTraceFormat.DetailedFlat:
		{
			for (int num2 = num; num2 >= logEvent.UserStackFrameNumber; num2--)
			{
				StackFrame frame = logEvent.StackTrace.GetFrame(num2);
				if (!flag)
				{
					builder.Append(Separator);
				}
				builder.Append("[");
				builder.Append(frame.GetMethod());
				builder.Append("]");
				flag = false;
			}
			break;
		}
		}
	}
}
