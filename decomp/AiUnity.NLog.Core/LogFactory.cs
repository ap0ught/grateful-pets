using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Targets;
using UnityEngine;

namespace AiUnity.NLog.Core;

public class LogFactory
{
	internal class LoggerCacheKey
	{
		internal Type ConcreteType { get; private set; }

		internal string Name { get; private set; }

		internal LoggerCacheKey(Type loggerConcreteType, string name)
		{
			ConcreteType = loggerConcreteType;
			Name = name;
		}

		public override bool Equals(object o)
		{
			if (!(o is LoggerCacheKey loggerCacheKey))
			{
				return false;
			}
			if (ConcreteType == loggerCacheKey.ConcreteType)
			{
				return loggerCacheKey.Name == Name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ConcreteType.GetHashCode() ^ Name.GetHashCode();
		}
	}

	private static TimeSpan defaultFlushTimeout = TimeSpan.FromSeconds(15.0);

	private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache = new Dictionary<LoggerCacheKey, WeakReference>();

	private LoggingConfiguration config;

	private bool configLoaded;

	private LogLevels globalThreshold = LogLevels.Everything;

	private int logsEnabled;

	private const string DefaultConfig = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n  <nlog buildLevels=\"\">\r\n\t<targets>\r\n\t\t<target name=\"UnityConsole\" type=\"UnityConsole\"/>\r\n\t</targets>\r\n\t<rules>\r\n\t\t<logger name=\"*\" namespace=\"*\" target=\"UnityConsole\" levels=\"Fatal, Error, Warn\"/>\r\n\t</rules>\r\n  </nlog>";

	public LoggingConfiguration Configuration
	{
		get
		{
			lock (this)
			{
				if (configLoaded && config != null)
				{
					return config;
				}
				configLoaded = true;
				if (config == null)
				{
					string configText = Singleton<NLogConfigFile>.Instance.GetConfigText();
					if (!string.IsNullOrEmpty(configText))
					{
						config = new XmlLoggingConfiguration(configText, Singleton<NLogConfigFile>.Instance.FileInfo.FullName);
					}
					else
					{
						Logger.Info("Using default configuration because unable to locate {0}.", Singleton<NLogConfigFile>.Instance.RelativeName);
						config = new XmlLoggingConfiguration("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n  <nlog buildLevels=\"\">\r\n\t<targets>\r\n\t\t<target name=\"UnityConsole\" type=\"UnityConsole\"/>\r\n\t</targets>\r\n\t<rules>\r\n\t\t<logger name=\"*\" namespace=\"*\" target=\"UnityConsole\" levels=\"Fatal, Error, Warn\"/>\r\n\t</rules>\r\n  </nlog>", Singleton<NLogConfigFile>.Instance.FileInfo.FullName);
					}
				}
				if (config != null)
				{
					config.InitializeAll();
				}
				return config;
			}
		}
		set
		{
			lock (this)
			{
				LoggingConfiguration loggingConfiguration = config;
				if (loggingConfiguration != null)
				{
					Logger.Debug("Closing old configuration.");
					Flush();
					loggingConfiguration.Close();
				}
				config = value;
				if (config != null)
				{
					Logger.Debug("Establish new logging configuration.");
					configLoaded = true;
					Dump(config);
					config.InitializeAll();
					ReconfigExistingLoggers(config);
				}
				else
				{
					Logger.Debug("Logging Configuration deleted.");
					configLoaded = false;
				}
				ConfigurationChanged?.Invoke(this, new LoggingConfigurationChangedEventArgs(loggingConfiguration, value));
			}
		}
	}

	public LogLevels GlobalThreshold
	{
		get
		{
			return globalThreshold;
		}
		set
		{
			if (globalThreshold != value)
			{
				lock (this)
				{
					globalThreshold = value;
					ReconfigExistingLoggers();
				}
			}
		}
	}

	public bool ThrowExceptions { get; set; }

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged;

	public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded;

	public LogFactory()
	{
	}

	public LogFactory(LoggingConfiguration config)
		: this()
	{
		Configuration = config;
	}

	public NLogger CreateNullLogger()
	{
		Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel = new Dictionary<LogLevels, TargetWithFilterChain>();
		NLogger nLogger = new NLogger();
		nLogger.Initialize(string.Empty, new LoggerConfiguration(targetsByLevel), this);
		return nLogger;
	}

	public void EnableLogging()
	{
		lock (this)
		{
			logsEnabled++;
			if (logsEnabled == 0)
			{
				ReconfigExistingLoggers();
			}
		}
	}

	public void Flush()
	{
		Flush(defaultFlushTimeout);
	}

	public void Flush(TimeSpan timeout)
	{
		try
		{
			AsyncHelpers.RunSynchronously((AsyncContinuation cb) =>
			{
				Flush(cb, timeout);
			});
		}
		catch (Exception ex)
		{
			if (ThrowExceptions)
			{
				throw;
			}
			Logger.Error(ex.ToString());
		}
	}

	public void Flush(int timeoutMilliseconds)
	{
		Flush(TimeSpan.FromMilliseconds(timeoutMilliseconds));
	}

	public void Flush(AsyncContinuation asyncContinuation)
	{
		Flush(asyncContinuation, TimeSpan.MaxValue);
	}

	public void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
	{
		Flush(asyncContinuation, TimeSpan.FromMilliseconds(timeoutMilliseconds));
	}

	public void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
	{
		try
		{
			Logger.Trace("LogFactory.Flush({0})", timeout);
			LoggingConfiguration configuration = Configuration;
			if (configuration != null)
			{
				Logger.Trace("Flushing all targets...");
				configuration.FlushAllTargets(AsyncHelpers.WithTimeout(asyncContinuation, timeout));
			}
			else
			{
				asyncContinuation(null);
			}
		}
		catch (Exception ex)
		{
			if (ThrowExceptions)
			{
				throw;
			}
			Logger.Error(ex.ToString());
		}
	}

	public NLogger GetLogger(string name, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
	{
		return GetLogger(name, typeof(NLogger), context, formatProvider);
	}

	public NLogger GetLogger(string name, Type loggerType, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
	{
		return GetLogger(new LoggerCacheKey(loggerType, name), context, formatProvider);
	}

	public bool IsLoggingEnabled()
	{
		return logsEnabled >= 0;
	}

	public void ReconfigExistingLoggers()
	{
		ReconfigExistingLoggers(config);
	}

	internal LoggerConfiguration GetConfigurationForLogger(string name, LoggingConfiguration configuration)
	{
		Dictionary<LogLevels, TargetWithFilterChain> dictionary = new Dictionary<LogLevels, TargetWithFilterChain>();
		Dictionary<LogLevels, TargetWithFilterChain> lastTargetsByLevel = new Dictionary<LogLevels, TargetWithFilterChain>();
		Logger.Debug("Getting targets for {0} by level.", name);
		if (configuration != null && IsLoggingEnabled())
		{
			GetTargetsByLevelForLogger(name, configuration.LoggingRules, dictionary, lastTargetsByLevel);
		}
		foreach (TargetWithFilterChain value in dictionary.Values)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} =>", value);
			for (TargetWithFilterChain targetWithFilterChain = value; targetWithFilterChain != null; targetWithFilterChain = targetWithFilterChain.NextInChain)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0}", targetWithFilterChain.Target.Name);
				if (targetWithFilterChain.FilterChain.Count > 0)
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ({0} filters)", targetWithFilterChain.FilterChain.Count);
				}
			}
			Logger.Trace("GetConfigurationForLogger: Chain = {0}", stringBuilder.ToString());
		}
		return new LoggerConfiguration(dictionary);
	}

	internal void GetTargetsByLevelForLogger(string name, IList<LoggingRule> rules, Dictionary<LogLevels, TargetWithFilterChain> targetsByLevel, Dictionary<LogLevels, TargetWithFilterChain> lastTargetsByLevel)
	{
		foreach (LoggingRule rule in rules)
		{
			if (!rule.NameMatches(name) || !rule.isPlatformMatch())
			{
				continue;
			}
			foreach (LogLevels flag in rule.logLevels.GetFlags())
			{
				foreach (Target target in rule.Targets)
				{
					TargetWithFilterChain targetWithFilterChain = new TargetWithFilterChain(target, rule.Filters);
					if (lastTargetsByLevel.TryGetValue(flag, out var value))
					{
						value.NextInChain = targetWithFilterChain;
					}
					else
					{
						targetsByLevel[flag] = targetWithFilterChain;
					}
					lastTargetsByLevel[flag] = targetWithFilterChain;
				}
			}
			GetTargetsByLevelForLogger(name, rule.ChildRules, targetsByLevel, lastTargetsByLevel);
			if (rule.Final)
			{
				break;
			}
		}
		foreach (TargetWithFilterChain value2 in targetsByLevel.Values)
		{
			value2?.PrecalculateStackTraceUsage();
		}
	}

	internal void ReconfigExistingLoggers(LoggingConfiguration configuration)
	{
		configuration?.EnsureInitialized();
		foreach (WeakReference item in loggerCache.Values.ToList())
		{
			if (item.Target is NLogger nLogger)
			{
				nLogger.SetConfiguration(GetConfigurationForLogger(nLogger.Name, configuration));
			}
		}
	}

	internal void ReloadConfig()
	{
		if (Configuration != null)
		{
			Configuration = Configuration.Reload();
			if (ConfigurationReloaded != null)
			{
				ConfigurationReloaded(this, new LoggingConfigurationReloadedEventArgs(succeeded: true, null));
			}
		}
		else
		{
			Logger.Error("Unable to reload configuration {0}.", Singleton<NLogConfigFile>.Instance.FileInfo.FullName);
		}
	}

	private static void Dump(LoggingConfiguration config)
	{
		if (Logger.IsDebugEnabled)
		{
			config.Dump();
		}
	}

	private NLogger GetLogger(LoggerCacheKey cacheKey, UnityEngine.Object context, IFormatProvider formatProvider)
	{
		lock (this)
		{
			if (loggerCache.TryGetValue(cacheKey, out var value) && value.Target is NLogger result)
			{
				return result;
			}
			NLogger nLogger;
			if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(NLogger))
			{
				try
				{
					nLogger = (NLogger)FactoryHelper.CreateInstance(cacheKey.ConcreteType);
				}
				catch (Exception ex)
				{
					if (ex.MustBeRethrown())
					{
						throw;
					}
					if (ThrowExceptions)
					{
						throw;
					}
					Logger.Error("Cannot create instance of specified type. Proceeding with default type instance. Exception : {0}", ex);
					cacheKey = new LoggerCacheKey(typeof(NLogger), cacheKey.Name);
					nLogger = new NLogger(context, formatProvider);
				}
			}
			else
			{
				nLogger = new NLogger(context, formatProvider);
			}
			if (cacheKey.ConcreteType != null)
			{
				nLogger.Initialize(cacheKey.Name, GetConfigurationForLogger(cacheKey.Name, Configuration), this);
			}
			loggerCache[cacheKey] = new WeakReference(nLogger);
			return nLogger;
		}
	}
}
