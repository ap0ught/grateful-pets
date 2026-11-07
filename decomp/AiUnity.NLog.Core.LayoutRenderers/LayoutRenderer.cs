using System;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[NLogConfigurationItem]
[Preserve]
public abstract class LayoutRenderer : ISupportsInitialize, IRenderable, IDisposable
{
	private const int MaxInitialRenderBufferLength = 16384;

	private int maxRenderedLength;

	private bool isInitialized;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	protected LoggingConfiguration LoggingConfiguration { get; private set; }

	public override string ToString()
	{
		LayoutRendererAttribute layoutRendererAttribute = (LayoutRendererAttribute)Attribute.GetCustomAttribute(GetType(), typeof(LayoutRendererAttribute));
		if (layoutRendererAttribute != null)
		{
			return "Layout Renderer: ${" + layoutRendererAttribute.DisplayName + "}";
		}
		return GetType().Name;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public string Render(LogEventInfo logEvent)
	{
		int num = maxRenderedLength;
		if (num > 16384)
		{
			num = 16384;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		Render(stringBuilder, logEvent);
		if (stringBuilder.Length > maxRenderedLength)
		{
			maxRenderedLength = stringBuilder.Length;
		}
		return stringBuilder.ToString();
	}

	void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
	{
		Initialize(configuration);
	}

	void ISupportsInitialize.Close()
	{
		Close();
	}

	internal void Initialize(LoggingConfiguration configuration)
	{
		if (!isInitialized)
		{
			LoggingConfiguration = configuration;
			isInitialized = true;
			InitializeLayoutRenderer();
		}
	}

	internal void Close()
	{
		if (isInitialized)
		{
			LoggingConfiguration = null;
			isInitialized = false;
			CloseLayoutRenderer();
		}
	}

	internal void Render(StringBuilder builder, LogEventInfo logEvent)
	{
		if (!isInitialized)
		{
			isInitialized = true;
			InitializeLayoutRenderer();
		}
		try
		{
			Append(builder, logEvent);
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Warn("Exception in layout renderer: {0}", ex);
		}
	}

	protected abstract void Append(StringBuilder builder, LogEventInfo logEvent);

	protected virtual void InitializeLayoutRenderer()
	{
	}

	protected virtual void CloseLayoutRenderer()
	{
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
	}
}
