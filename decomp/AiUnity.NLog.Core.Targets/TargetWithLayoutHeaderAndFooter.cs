using System.ComponentModel;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Preserve]
public abstract class TargetWithLayoutHeaderAndFooter : TargetWithLayout
{
	[RequiredParameter]
	[Display("Layout", "Specifies the layout and content of log message.  The + icon will present a list of variables that can be added.  For the Unity Console all excepted xml formating can be used (http://docs.unity3d.com/Manual/StyledText.html)", false, -100)]
	[DefaultValue("${message}")]
	public override Layout Layout
	{
		get
		{
			return LHF.Layout;
		}
		set
		{
			if (value is LayoutWithHeaderAndFooter)
			{
				base.Layout = value;
			}
			else if (LHF == null)
			{
				LHF = new LayoutWithHeaderAndFooter
				{
					Layout = value
				};
			}
			else
			{
				LHF.Layout = value;
			}
		}
	}

	public Layout Footer
	{
		get
		{
			return LHF.Footer;
		}
		set
		{
			LHF.Footer = value;
		}
	}

	public Layout Header
	{
		get
		{
			return LHF.Header;
		}
		set
		{
			LHF.Header = value;
		}
	}

	private LayoutWithHeaderAndFooter LHF
	{
		get
		{
			return (LayoutWithHeaderAndFooter)base.Layout;
		}
		set
		{
			base.Layout = value;
		}
	}
}
