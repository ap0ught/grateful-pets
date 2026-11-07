using System;
using System.Collections.ObjectModel;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.LayoutRenderers;

namespace AiUnity.NLog.Core.Layouts;

[Layout("SimpleLayout")]
[ThreadAgnostic]
[AppDomainFixedOutput]
public class SimpleLayout : Layout
{
	private const int MaxInitialRenderBufferLength = 16384;

	private int maxRenderedLength;

	private string fixedText;

	private string layoutText;

	private ConfigurationItemFactory configurationItemFactory;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public string Text
	{
		get
		{
			return layoutText;
		}
		set
		{
			LayoutRenderer[] renderers = LayoutParser.CompileLayout(configurationItemFactory, new SimpleStringReader(value), isNested: false, out var text);
			SetRenderers(renderers, text);
		}
	}

	public ReadOnlyCollection<LayoutRenderer> Renderers { get; private set; }

	public SimpleLayout()
		: this(string.Empty)
	{
	}

	public SimpleLayout(string txt)
		: this(txt, ConfigurationItemFactory.Default)
	{
	}

	public SimpleLayout(string txt, ConfigurationItemFactory configurationItemFactory)
	{
		this.configurationItemFactory = configurationItemFactory;
		Text = txt;
	}

	internal SimpleLayout(LayoutRenderer[] renderers, string text, ConfigurationItemFactory configurationItemFactory)
	{
		this.configurationItemFactory = configurationItemFactory;
		SetRenderers(renderers, text);
	}

	public static implicit operator SimpleLayout(string text)
	{
		return new SimpleLayout(text);
	}

	public static string Escape(string text)
	{
		return text.Replace("${", "${literal:text=${}");
	}

	public static string Evaluate(string text, LogEventInfo logEvent)
	{
		return new SimpleLayout(text).Render(logEvent);
	}

	public static string Evaluate(string text)
	{
		return Evaluate(text, LogEventInfo.CreateNullEvent());
	}

	public override string ToString()
	{
		return "'" + Text + "'";
	}

	internal void SetRenderers(LayoutRenderer[] renderers, string text)
	{
		Renderers = new ReadOnlyCollection<LayoutRenderer>(renderers);
		if (Renderers.Count == 1 && Renderers[0] is LiteralLayoutRenderer)
		{
			fixedText = ((LiteralLayoutRenderer)Renderers[0]).Text;
		}
		else
		{
			fixedText = null;
		}
		layoutText = text;
	}

	protected override string GetFormattedMessage(LogEventInfo logEvent)
	{
		if (fixedText != null)
		{
			return fixedText;
		}
		if (logEvent.TryGetCachedLayoutValue(this, out var value))
		{
			return value;
		}
		int num = maxRenderedLength;
		if (num > 16384)
		{
			num = 16384;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		foreach (LayoutRenderer renderer in Renderers)
		{
			try
			{
				renderer.Render(stringBuilder, logEvent);
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				if (Logger.IsWarnEnabled)
				{
					Logger.Warn("Exception in {0}.Append(): {1}.", renderer.GetType().FullName, ex);
				}
			}
		}
		if (stringBuilder.Length > maxRenderedLength)
		{
			maxRenderedLength = stringBuilder.Length;
		}
		string text = stringBuilder.ToString();
		logEvent.AddCachedLayoutValue(this, text);
		return text;
	}
}
