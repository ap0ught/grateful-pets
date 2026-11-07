using System.ComponentModel;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Preserve]
public abstract class TargetWithLayout : Target
{
	private const string LayoutDefault = "[${level}] ${callsite}${newline}${message}${exception}";

	[RequiredParameter]
	[Display("Layout", "Specifies the layout and content of log message.  The + icon will present a list of variables that can be added.  For the Unity Console all excepted xml formating can be used (http://docs.unity3d.com/Manual/StyledText.html)", false, -100)]
	[DefaultValue("[${level}] ${callsite}${newline}${message}${exception}")]
	public virtual Layout Layout { get; set; }

	protected TargetWithLayout()
	{
		Layout = "[${level}] ${callsite}${newline}${message}${exception}";
	}
}
