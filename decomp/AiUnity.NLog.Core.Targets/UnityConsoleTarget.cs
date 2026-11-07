using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AiUnity.Common.Attributes;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Layouts;
using UnityEngine;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("UnityConsole")]
public class UnityConsoleTarget : TargetWithLayout
{
	private const string LayoutDefault = "<color=olive>[${level}] ${callsite}</color>${newline}<color=black>${message}</color>${newline}<color=red>${exception}</color>";

	[RequiredParameter]
	[DefaultValue("<color=olive>[${level}] ${callsite}</color>${newline}<color=black>${message}</color>${newline}<color=red>${exception}</color>")]
	[Display("Layout", "Specifies the layout and content of log message.  The + icon will present a list of variables that can be added.  For the Unity Console all excepted xml formating can be used (http://docs.unity3d.com/Manual/StyledText.html)", false, -100)]
	[Preserve]
	public override Layout Layout { get; set; }

	public UnityConsoleTarget()
	{
		Layout = "<color=olive>[${level}] ${callsite}</color>${newline}<color=black>${message}</color>${newline}<color=red>${exception}</color>";
	}

	protected override void Write(LogEventInfo logEvent)
	{
		string message = Layout.Render(logEvent);
		MonoBehaviour monoBehaviour = logEvent.Context as MonoBehaviour;
		GameObject context = ((monoBehaviour != null) ? monoBehaviour.gameObject : (logEvent.Context as GameObject));
		if (logEvent.Level.Has(LogLevels.Info) || logEvent.Level.Has(LogLevels.Debug) || logEvent.Level.Has(LogLevels.Trace))
		{
			Debug.Log(message, context);
			return;
		}
		if (logEvent.Level.Has(LogLevels.Warn))
		{
			Debug.LogWarning(message, context);
			return;
		}
		Debug.LogError(message, context);
		if (!logEvent.Level.Has(LogLevels.Assert) || !Singleton<NLogManager>.Instance.AssertException)
		{
			return;
		}
		Debug.Break();
		throw new AssertException(message, logEvent.Exception);
	}

	protected string FixUnityConsoleXML(string message)
	{
		Match match = Regex.Match(message.TrimEnd('\n'), "\\r?\\n").NextMatch();
		int index = match.Index;
		if (index > 0)
		{
			Stack<string> stack = new Stack<string>();
			string text = message.Substring(0, index);
			foreach (Match item in Regex.Matches(text, "(<(b|i|size|color)\\s*>)|(</(b|i|size|color)\\s*)"))
			{
				if (string.IsNullOrEmpty(item.Groups[1].Value))
				{
					if (Enumerable.Count(stack) == 0)
					{
						return message;
					}
					stack.Pop();
				}
				else
				{
					stack.Push(item.Groups[1].Value);
				}
			}
			if (stack.Count != 0)
			{
				StringBuilder stringBuilder = new StringBuilder(text);
				foreach (string item2 in stack)
				{
					Match match3 = Regex.Match(item2, "<(\\w+)[^<>]*>");
					stringBuilder.AppendFormat("</{0}>", match3.Groups[1].Value);
				}
				stringBuilder.AppendLine();
				stringBuilder.Append(string.Join("", stack.Reverse().ToArray()));
				stringBuilder.Append(message.Substring(index + match.Value.Length));
				return stringBuilder.ToString();
			}
		}
		return message;
	}
}
