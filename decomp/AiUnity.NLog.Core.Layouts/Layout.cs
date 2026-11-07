using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;

namespace AiUnity.NLog.Core.Layouts;

[NLogConfigurationItem]
public abstract class Layout : ISupportsInitialize, IRenderable
{
	private bool isInitialized;

	private bool threadAgnostic;

	internal bool IsThreadAgnostic => threadAgnostic;

	protected LoggingConfiguration LoggingConfiguration { get; private set; }

	public static implicit operator Layout(string text)
	{
		return FromString(text);
	}

	public static Layout FromString(string layoutText)
	{
		return FromString(layoutText, ConfigurationItemFactory.Default);
	}

	public static Layout FromString(string layoutText, ConfigurationItemFactory configurationItemFactory)
	{
		return new SimpleLayout(layoutText, configurationItemFactory);
	}

	public virtual void Precalculate(LogEventInfo logEvent)
	{
		if (!threadAgnostic)
		{
			Render(logEvent);
		}
	}

	public string Render(LogEventInfo logEvent)
	{
		if (!isInitialized)
		{
			isInitialized = true;
			InitializeLayout();
		}
		return GetFormattedMessage(logEvent);
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
		if (isInitialized)
		{
			return;
		}
		LoggingConfiguration = configuration;
		isInitialized = true;
		threadAgnostic = true;
		object[] array = ObjectGraphScanner.FindReachableObjects<object>(new object[1] { this });
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].GetType().IsDefined(typeof(ThreadAgnosticAttribute), inherit: true))
			{
				threadAgnostic = false;
				break;
			}
		}
		InitializeLayout();
	}

	internal void Close()
	{
		if (isInitialized)
		{
			LoggingConfiguration = null;
			isInitialized = false;
			CloseLayout();
		}
	}

	protected virtual void InitializeLayout()
	{
	}

	protected virtual void CloseLayout()
	{
	}

	protected abstract string GetFormattedMessage(LogEventInfo logEvent);
}
