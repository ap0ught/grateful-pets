using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Targets;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[Preserve]
public class Log4JXmlEventLayoutRenderer : LayoutRenderer, IUsesStackTrace
{
	private static readonly DateTime log4jDateBase = new DateTime(1970, 1, 1);

	private static readonly string dummyNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid().ToString();

	private static readonly string dummyNLogNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid().ToString();

	[DefaultValue(true)]
	public bool IncludeNLogData { get; set; }

	public bool IndentXml { get; set; }

	public string AppInfo { get; set; }

	public bool IncludeCallSite { get; set; }

	public bool IncludeSourceInfo { get; set; }

	public bool IncludeMdc { get; set; }

	public bool IncludeNdc { get; set; }

	[DefaultValue(" ")]
	public string NdcItemSeparator { get; set; }

	StackTraceUsage IUsesStackTrace.StackTraceUsage
	{
		get
		{
			if (IncludeSourceInfo)
			{
				return StackTraceUsage.WithSource;
			}
			if (IncludeCallSite)
			{
				return StackTraceUsage.WithoutSource;
			}
			return StackTraceUsage.None;
		}
	}

	internal IList<NLogViewerParameterInfo> Parameters { get; set; }

	public Log4JXmlEventLayoutRenderer()
	{
		IncludeNLogData = true;
		NdcItemSeparator = " ";
		AppInfo = "Unity3D Application";
		Parameters = new List<NLogViewerParameterInfo>();
	}

	internal void AppendToStringBuilder(StringBuilder sb, LogEventInfo logEvent)
	{
		Append(sb, logEvent);
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = IndentXml,
			ConformanceLevel = ConformanceLevel.Fragment,
			IndentChars = "  "
		};
		StringBuilder stringBuilder = new StringBuilder();
		using XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings);
		xmlWriter.WriteStartElement("log4j", "event", dummyNamespace);
		xmlWriter.WriteAttributeSafeString("xmlns", "nlog", "http://www.w3.org/2000/xmlns/", dummyNLogNamespace);
		xmlWriter.WriteAttributeSafeString("logger", logEvent.LoggerName);
		xmlWriter.WriteAttributeSafeString("level", logEvent.Level.ToString().ToUpper(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeSafeString("timestamp", Convert.ToString((long)(logEvent.TimeStamp.ToUniversalTime() - log4jDateBase).TotalMilliseconds));
		xmlWriter.WriteAttributeSafeString("thread", Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteElementSafeString("log4j", "message", dummyNamespace, logEvent.FormattedMessage);
		if (logEvent.Exception != null)
		{
			xmlWriter.WriteElementSafeString("log4j", "throwable", dummyNamespace, logEvent.Exception.ToString());
		}
		if (logEvent.Exception != null)
		{
			xmlWriter.WriteStartElement("log4j", "throwable", dummyNamespace);
			xmlWriter.WriteSafeCData(logEvent.Exception.ToString());
			xmlWriter.WriteEndElement();
		}
		if (IncludeCallSite || IncludeSourceInfo)
		{
			StackFrame userStackFrame = logEvent.UserStackFrame;
			if (userStackFrame != null)
			{
				MethodBase method = userStackFrame.GetMethod();
				Type declaringType = method.DeclaringType;
				xmlWriter.WriteStartElement("log4j", "locationInfo", dummyNamespace);
				if (declaringType != null)
				{
					xmlWriter.WriteAttributeSafeString("class", declaringType.FullName);
				}
				xmlWriter.WriteAttributeSafeString("method", method.ToString());
				if (IncludeSourceInfo)
				{
					xmlWriter.WriteAttributeSafeString("file", userStackFrame.GetFileName());
					xmlWriter.WriteAttributeSafeString("line", userStackFrame.GetFileLineNumber().ToString(CultureInfo.InvariantCulture));
				}
				xmlWriter.WriteEndElement();
				if (IncludeNLogData)
				{
					xmlWriter.WriteElementSafeString("nlog", "eventSequenceNumber", dummyNLogNamespace, logEvent.SequenceID.ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteStartElement("nlog", "locationInfo", dummyNLogNamespace);
					if (declaringType != null)
					{
						xmlWriter.WriteAttributeSafeString("assembly", declaringType.Assembly.FullName);
					}
					xmlWriter.WriteEndElement();
					xmlWriter.WriteStartElement("nlog", "properties", dummyNLogNamespace);
					foreach (KeyValuePair<object, object> property in logEvent.Properties)
					{
						xmlWriter.WriteStartElement("nlog", "data", dummyNLogNamespace);
						xmlWriter.WriteAttributeSafeString("name", Convert.ToString(property.Key));
						xmlWriter.WriteAttributeSafeString("value", Convert.ToString(property.Value));
						xmlWriter.WriteEndElement();
					}
					xmlWriter.WriteEndElement();
				}
			}
		}
		xmlWriter.WriteStartElement("log4j", "properties", dummyNamespace);
		foreach (NLogViewerParameterInfo parameter in Parameters)
		{
			xmlWriter.WriteStartElement("log4j", "data", dummyNamespace);
			xmlWriter.WriteAttributeSafeString("name", parameter.Name);
			xmlWriter.WriteAttributeSafeString("value", parameter.Layout.Render(logEvent));
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteStartElement("log4j", "data", dummyNamespace);
		xmlWriter.WriteAttributeSafeString("name", "log4japp");
		xmlWriter.WriteAttributeSafeString("value", AppInfo);
		xmlWriter.WriteEndElement();
		xmlWriter.WriteStartElement("log4j", "data", dummyNamespace);
		xmlWriter.WriteAttributeSafeString("name", "log4jmachinename");
		xmlWriter.WriteAttributeSafeString("value", Environment.MachineName);
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		stringBuilder.Replace(" xmlns:log4j=\"" + dummyNamespace + "\"", string.Empty);
		stringBuilder.Replace(" xmlns:nlog=\"" + dummyNLogNamespace + "\"", string.Empty);
		builder.Append(stringBuilder.ToString());
	}
}
