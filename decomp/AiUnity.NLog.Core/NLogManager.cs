using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Config;
using UnityEngine;

namespace AiUnity.NLog.Core;

public sealed class NLogManager : Singleton<NLogManager>, ILogManager
{
	public delegate CultureInfo GetCultureInfo();

	public const LogLevels GlobalLogLevelsDefault = LogLevels.Everything;

	private readonly LogFactory globalFactory = new LogFactory();

	public bool AssertException { get; set; }

	public GetCultureInfo DefaultCultureInfo { get; set; }

	public LogLevels GlobalLogLevel
	{
		get
		{
			return globalFactory.GlobalThreshold;
		}
		set
		{
			globalFactory.GlobalThreshold = value;
		}
	}

	public HashSet<Assembly> HiddenAssemblies { get; set; }

	public HashSet<string> HiddenNameSpaces { get; set; }

	public bool ShowUnityLog { get; set; }

	public bool ThrowExceptions
	{
		get
		{
			return globalFactory.ThrowExceptions;
		}
		set
		{
			globalFactory.ThrowExceptions = value;
		}
	}

	internal LoggingConfiguration Configuration
	{
		get
		{
			return globalFactory.Configuration;
		}
		set
		{
			globalFactory.Configuration = value;
		}
	}

	public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
	{
		add
		{
			globalFactory.ConfigurationChanged += value;
		}
		remove
		{
			globalFactory.ConfigurationChanged -= value;
		}
	}

	public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded
	{
		add
		{
			globalFactory.ConfigurationReloaded += value;
		}
		remove
		{
			globalFactory.ConfigurationReloaded -= value;
		}
	}

	public NLogManager()
	{
		HiddenAssemblies = new HashSet<Assembly>();
		HiddenNameSpaces = new HashSet<string>();
		DefaultCultureInfo = () => CultureInfo.CurrentCulture;
		AssertException = false;
	}

	public NLogger GetLogger(object context, IFormatProvider formatProvider = null)
	{
		UnityEngine.Object context2 = context as UnityEngine.Object;
		return globalFactory.GetLogger(context.GetType().FullName, context2, formatProvider);
	}

	public NLogger GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider = null)
	{
		return globalFactory.GetLogger(name, context, formatProvider);
	}

	public NLogger GetLogger(string name, Type loggerType, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
	{
		return globalFactory.GetLogger(name, loggerType, context, formatProvider);
	}

	public void ReconfigExistingLoggers()
	{
		globalFactory.ReconfigExistingLoggers();
	}

	public void ReloadConfig()
	{
		globalFactory.ReloadConfig();
	}

	AiUnity.Common.Log.ILogger ILogManager.GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
	{
		return GetLogger(name, context, formatProvider);
	}
}
