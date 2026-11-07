using System;
using System.IO;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("file-contents", false)]
[Preserve]
public class FileContentsLayoutRenderer : LayoutRenderer
{
	private string lastFileName;

	private string currentFileContents;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[DefaultParameter]
	public Layout FileName { get; set; }

	public Encoding Encoding { get; set; }

	public FileContentsLayoutRenderer()
	{
		Encoding = Encoding.Default;
		lastFileName = string.Empty;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		lock (this)
		{
			string text = FileName.Render(logEvent);
			if (text != lastFileName)
			{
				currentFileContents = ReadFileContents(text);
				lastFileName = text;
			}
		}
		builder.Append(currentFileContents);
	}

	private string ReadFileContents(string fileName)
	{
		try
		{
			using StreamReader streamReader = new StreamReader(fileName, Encoding);
			return streamReader.ReadToEnd();
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Error("Cannot read file contents: {0} {1}", fileName, ex);
			return string.Empty;
		}
	}
}
