using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("exception", false)]
[ThreadAgnostic]
[Preserve]
public class ExceptionLayoutRenderer : LayoutRenderer
{
	private delegate void ExceptionDataTarget(StringBuilder sb, Exception ex);

	private string format;

	private string innerFormat = string.Empty;

	private ExceptionDataTarget[] exceptionDataTargets;

	private ExceptionDataTarget[] innerExceptionDataTargets;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[DefaultParameter]
	public string Format
	{
		get
		{
			return format;
		}
		set
		{
			format = value;
			exceptionDataTargets = CompileFormat(value);
		}
	}

	public string InnerFormat
	{
		get
		{
			return innerFormat;
		}
		set
		{
			innerFormat = value;
			innerExceptionDataTargets = CompileFormat(value);
		}
	}

	[DefaultValue("\n")]
	public string Separator { get; set; }

	[DefaultValue(0)]
	public int MaxInnerExceptionLevel { get; set; }

	public string InnerExceptionSeparator { get; set; }

	public ExceptionLayoutRenderer()
	{
		Format = (InnerFormat = "Header,Message,Method,Type,Stacktrace,Footer");
		Separator = Environment.NewLine;
		InnerExceptionSeparator = Environment.NewLine;
		MaxInnerExceptionLevel = 10;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		if (logEvent.Exception == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(128);
		string value = string.Empty;
		ExceptionDataTarget[] array = exceptionDataTargets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i](stringBuilder, logEvent.Exception);
			stringBuilder.Append(value);
			value = Separator;
		}
		Exception innerException = logEvent.Exception.InnerException;
		int num = 0;
		while (innerException != null && num < MaxInnerExceptionLevel)
		{
			value = string.Empty;
			array = innerExceptionDataTargets ?? exceptionDataTargets;
			for (int i = 0; i < array.Length; i++)
			{
				array[i](stringBuilder, innerException);
				stringBuilder.Append(value);
				value = Separator;
			}
			innerException = innerException.InnerException;
			num++;
		}
		builder.Append(stringBuilder.ToString());
	}

	private ExceptionDataTarget[] CompileFormat(string formatSpecifier)
	{
		string[] array = formatSpecifier.Replace(" ", string.Empty).Split(',');
		List<ExceptionDataTarget> list = new List<ExceptionDataTarget>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			switch (text.ToUpper(CultureInfo.InvariantCulture))
			{
			case "HEADER":
				list.Add(AppendHeader);
				break;
			case "MESSAGE":
				list.Add(AppendMessage);
				break;
			case "TYPE":
				list.Add(AppendType);
				break;
			case "SHORTTYPE":
				list.Add(AppendShortType);
				break;
			case "TOSTRING":
				list.Add(AppendToString);
				break;
			case "METHOD":
				list.Add(AppendMethod);
				break;
			case "STACKTRACE":
				list.Add(AppendStackTrace);
				break;
			case "DATA":
				list.Add(AppendData);
				break;
			case "FOOTER":
				list.Add(AppendFooter);
				break;
			default:
				Logger.Warn("Unknown exception data target: {0}", text);
				break;
			}
		}
		return list.ToArray();
	}

	private void AppendHeader(StringBuilder sb, Exception ex)
	{
		string text = $"Exception: {ex.GetType().FullName}{Environment.NewLine}";
		string text2 = new string('*', text.Length);
		sb.Append(Environment.NewLine + text2 + Environment.NewLine);
		sb.Append(text);
		sb.Append(text2 + Environment.NewLine);
	}

	private void AppendFooter(StringBuilder sb, Exception ex)
	{
		string text = $"Exception: {ex.GetType().FullName}{Environment.NewLine}";
		string value = new string('*', text.Length);
		sb.Append(value);
	}

	protected virtual void AppendMessage(StringBuilder sb, Exception ex)
	{
		try
		{
			sb.AppendFormat("{0,-11} {1}", "Message:", ex.Message);
		}
		catch (Exception ex2)
		{
			string text = $"Exception in {typeof(ExceptionLayoutRenderer).FullName}.AppendMessage(): {ex2.GetType().FullName}.";
			sb.Append("NLog message:" + text);
			if (Logger.IsWarnEnabled)
			{
				Logger.Warn(text);
			}
		}
	}

	protected virtual void AppendMethod(StringBuilder sb, Exception ex)
	{
		if (ex.TargetSite != null)
		{
			sb.AppendFormat("{0,-12} {1}", "Method:", ex.TargetSite.ToString());
		}
	}

	protected virtual void AppendStackTrace(StringBuilder sb, Exception ex)
	{
		sb.AppendFormat("{1}Stack Trace:{1}{0}", ex.StackTrace, Environment.NewLine);
	}

	protected virtual void AppendToString(StringBuilder sb, Exception ex)
	{
		sb.Append(ex.ToString());
	}

	protected virtual void AppendType(StringBuilder sb, Exception ex)
	{
		sb.AppendFormat("{0,-13} {1}", "Type:", ex.GetType().FullName);
	}

	protected virtual void AppendShortType(StringBuilder sb, Exception ex)
	{
		sb.Append(ex.GetType().Name);
	}

	protected virtual void AppendData(StringBuilder sb, Exception ex)
	{
		string value = string.Empty;
		foreach (object key in ex.Data.Keys)
		{
			sb.Append(value);
			sb.AppendFormat("{0}: {1}", key, ex.Data[key]);
			value = ";";
		}
	}
}
