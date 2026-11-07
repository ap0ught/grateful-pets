using System.Collections.Generic;
using System.ComponentModel;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.LayoutRenderers;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("NLogViewer")]
[Preserve]
public class NLogViewerTarget : NetworkTarget
{
	private readonly Log4JXmlEventLayout layout = new Log4JXmlEventLayout();

	public bool IncludeNLogData
	{
		get
		{
			return Renderer.IncludeNLogData;
		}
		set
		{
			Renderer.IncludeNLogData = value;
		}
	}

	public string AppInfo
	{
		get
		{
			return Renderer.AppInfo;
		}
		set
		{
			Renderer.AppInfo = value;
		}
	}

	public bool IncludeCallSite
	{
		get
		{
			return Renderer.IncludeCallSite;
		}
		set
		{
			Renderer.IncludeCallSite = value;
		}
	}

	[Display("Source Info", "Send message source to NLogViewer.", false, 0)]
	[DefaultValue(true)]
	public bool IncludeSourceInfo
	{
		get
		{
			return Renderer.IncludeSourceInfo;
		}
		set
		{
			Renderer.IncludeSourceInfo = value;
		}
	}

	public bool IncludeMdc
	{
		get
		{
			return Renderer.IncludeMdc;
		}
		set
		{
			Renderer.IncludeMdc = value;
		}
	}

	public bool IncludeNdc
	{
		get
		{
			return Renderer.IncludeNdc;
		}
		set
		{
			Renderer.IncludeNdc = value;
		}
	}

	public string NdcItemSeparator
	{
		get
		{
			return Renderer.NdcItemSeparator;
		}
		set
		{
			Renderer.NdcItemSeparator = value;
		}
	}

	[ArrayParameter(typeof(NLogViewerParameterInfo), "parameter")]
	public IList<NLogViewerParameterInfo> Parameters { get; private set; }

	public Log4JXmlEventLayoutRenderer Renderer => layout.Renderer;

	public override Layout Layout
	{
		get
		{
			return layout;
		}
		set
		{
		}
	}

	public NLogViewerTarget()
	{
		Parameters = new List<NLogViewerParameterInfo>();
		Renderer.Parameters = Parameters;
		NewLine = false;
	}
}
