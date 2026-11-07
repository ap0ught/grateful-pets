using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Targets;

namespace AiUnity.NLog.Core.Config;

public class LoggingConfiguration
{
	private readonly IDictionary<string, Target> targets = new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);

	private object[] configItems;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public ReadOnlyCollection<Target> ConfiguredNamedTargets => new List<Target>(targets.Values).AsReadOnly();

	public virtual IEnumerable<string> FileNamesToWatch => new string[0];

	public IList<LoggingRule> LoggingRules { get; private set; }

	public CultureInfo DefaultCultureInfo
	{
		get
		{
			return Singleton<NLogManager>.Instance.DefaultCultureInfo();
		}
		set
		{
			Singleton<NLogManager>.Instance.DefaultCultureInfo = () => value;
		}
	}

	public ReadOnlyCollection<Target> AllTargets => configItems.OfType<Target>().ToList().AsReadOnly();

	public LoggingConfiguration()
	{
		LoggingRules = new List<LoggingRule>();
	}

	public void AddTarget(string name, Target target)
	{
		if (name == null)
		{
			throw new ArgumentException("Target name cannot be null", "name");
		}
		Logger.Debug("Registering target {0}: {1}", name, target.GetType().FullName);
		targets[name] = target;
	}

	public Target FindTargetByName(string name)
	{
		if (!targets.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public virtual LoggingConfiguration Reload()
	{
		return this;
	}

	public void RemoveTarget(string name)
	{
		targets.Remove(name);
	}

	internal void Close()
	{
		Logger.Debug("Closing logging configuration...");
		foreach (ISupportsInitialize item in configItems.OfType<ISupportsInitialize>())
		{
			Logger.Trace("Closing {0}", item);
			try
			{
				item.Close();
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				Logger.Warn("Exception while closing {0}", ex);
			}
		}
		Logger.Debug("Finished closing logging configuration.");
	}

	internal void Dump()
	{
		List<string> list = new List<string>();
		list.Add("----- NLog configuration dump. -----");
		list.Add("Targets:");
		foreach (Target value in targets.Values)
		{
			list.Add(value.ToString());
		}
		list.Add("Rules:");
		foreach (LoggingRule loggingRule in LoggingRules)
		{
			list.Add(loggingRule.ToString());
		}
		list.Add("--- End of NLog configuration dump ---");
		Logger.Debug(string.Join(Environment.NewLine, list.ToArray()));
	}

	internal void FlushAllTargets(AsyncContinuation asyncContinuation)
	{
		List<Target> list = new List<Target>();
		foreach (LoggingRule loggingRule in LoggingRules)
		{
			foreach (Target target in loggingRule.Targets)
			{
				if (!list.Contains(target))
				{
					list.Add(target);
				}
			}
		}
		AsyncHelpers.ForEachItemInParallel(list, asyncContinuation, (Target target, AsyncContinuation cont) =>
		{
			target.Flush(cont);
		});
	}

	internal void ValidateConfig()
	{
		List<object> list = new List<object>();
		foreach (LoggingRule loggingRule in LoggingRules)
		{
			list.Add(loggingRule);
		}
		foreach (Target value in targets.Values)
		{
			list.Add(value);
		}
		if (Singleton<NLogManager>.Instance.ShowUnityLog)
		{
			list.Add(new UnityLogListener());
		}
		configItems = ObjectGraphScanner.FindReachableObjects<object>(list.ToArray());
		Logger.Trace("Found {0} configuration items", configItems.Length);
		object[] array = configItems;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyHelper.CheckRequiredParameters(array[i]);
		}
	}

	internal void InitializeAll()
	{
		ValidateConfig();
		foreach (ISupportsInitialize item in configItems.OfType<ISupportsInitialize>().Reverse())
		{
			Logger.Trace("Initializing {0}", item);
			try
			{
				item.Initialize(this);
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				if (Singleton<NLogManager>.Instance.ThrowExceptions)
				{
					throw new NLogConfigurationException("Error during initialization of " + item, ex);
				}
			}
		}
	}

	internal void EnsureInitialized()
	{
	}
}
