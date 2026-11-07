using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Filters;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Layouts;
using AiUnity.NLog.Core.Targets;
using AiUnity.NLog.Core.Targets.Wrappers;
using AiUnity.NLog.Core.Time;

namespace AiUnity.NLog.Core.Config;

public class XmlLoggingConfiguration : LoggingConfiguration
{
	private readonly ConfigurationItemFactory configurationItemFactory = ConfigurationItemFactory.Default;

	private readonly Dictionary<string, bool> visitedFile = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string> variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public Dictionary<string, string> Variables => variables;

	public bool AutoReload { get; set; }

	public override IEnumerable<string> FileNamesToWatch
	{
		get
		{
			if (AutoReload)
			{
				return visitedFile.Keys;
			}
			return new string[0];
		}
	}

	public XmlLoggingConfiguration(string fileName)
	{
		using XmlReader reader = XmlReader.Create(fileName);
		Initialize(reader, fileName, ignoreErrors: false);
	}

	public XmlLoggingConfiguration(string fileName, bool ignoreErrors)
	{
		using XmlReader reader = XmlReader.Create(fileName);
		Initialize(reader, fileName, ignoreErrors);
	}

	public XmlLoggingConfiguration(XmlReader reader, string fileName)
	{
		Initialize(reader, fileName, ignoreErrors: false);
	}

	public XmlLoggingConfiguration(XmlReader reader, string fileName, bool ignoreErrors)
	{
		Initialize(reader, fileName, ignoreErrors);
	}

	public XmlLoggingConfiguration(string text, string fileName)
	{
		using XmlReader reader = XmlReader.Create(new StringReader(text));
		Initialize(reader, fileName, ignoreErrors: false);
	}

	public XmlLoggingConfiguration(string text, string fileName, bool ignoreErrors)
	{
		using XmlReader reader = XmlReader.Create(new StringReader(text));
		Initialize(reader, fileName, ignoreErrors);
	}

	internal XmlLoggingConfiguration(XmlElement element, string fileName)
	{
		using StringReader input = new StringReader(element.OuterXml);
		XmlReader reader = XmlReader.Create(input);
		Initialize(reader, fileName, ignoreErrors: false);
	}

	internal XmlLoggingConfiguration(XmlElement element, string fileName, bool ignoreErrors)
	{
		using StringReader input = new StringReader(element.OuterXml);
		XmlReader reader = XmlReader.Create(input);
		Initialize(reader, fileName, ignoreErrors);
	}

	public override LoggingConfiguration Reload()
	{
		string configText = Singleton<NLogConfigFile>.Instance.GetConfigText();
		if (!string.IsNullOrEmpty(configText))
		{
			return new XmlLoggingConfiguration(configText, Singleton<NLogConfigFile>.Instance.FileInfo.FullName);
		}
		Logger.Info("Using default configuration because unable to locate {0}.", Singleton<NLogConfigFile>.Instance.RelativeName);
		return null;
	}

	private static bool IsTargetElement(string name)
	{
		if (!name.Equals("target", StringComparison.OrdinalIgnoreCase) && !name.Equals("wrapper", StringComparison.OrdinalIgnoreCase) && !name.Equals("wrapper-target", StringComparison.OrdinalIgnoreCase))
		{
			return name.Equals("compound-target", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsTargetRefElement(string name)
	{
		if (!name.Equals("target-ref", StringComparison.OrdinalIgnoreCase) && !name.Equals("wrapper-target-ref", StringComparison.OrdinalIgnoreCase))
		{
			return name.Equals("compound-target-ref", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string CleanWhitespace(string s)
	{
		s = s.Replace(" ", string.Empty);
		return s;
	}

	private static string StripOptionalNamespacePrefix(string attributeValue)
	{
		if (attributeValue == null)
		{
			return null;
		}
		int num = attributeValue.IndexOf(':');
		if (num < 0)
		{
			return attributeValue;
		}
		return attributeValue.Substring(num + 1);
	}

	private static Target WrapWithAsyncTargetWrapper(Target target)
	{
		AsyncTargetWrapper asyncTargetWrapper = new AsyncTargetWrapper();
		asyncTargetWrapper.WrappedTarget = target;
		asyncTargetWrapper.Name = target.Name;
		target.Name += "_wrapped";
		Logger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}", asyncTargetWrapper.Name, target.Name);
		target = asyncTargetWrapper;
		return target;
	}

	private void Initialize(XmlReader reader, string fileName, bool ignoreErrors)
	{
		try
		{
			reader.MoveToContent();
			NLogXmlElement content = null;
			try
			{
				content = new NLogXmlElement(reader);
			}
			catch (Exception ex)
			{
				Logger.Error("Fail to read configuration file {0} because {1}", fileName, ex.Message);
			}
			if (fileName != null)
			{
				Logger.Info("Initialize NLog based upon {0}", fileName);
				string fullPath = Path.GetFullPath(fileName);
				visitedFile[fullPath] = true;
				ParseTopLevel(content, Path.GetDirectoryName(fileName));
			}
			else
			{
				ParseTopLevel(content, null);
			}
		}
		catch (Exception ex2)
		{
			if (ex2.MustBeRethrown())
			{
				throw;
			}
			NLogConfigurationException ex3 = new NLogConfigurationException("Exception occurred when loading configuration from " + fileName, ex2);
			if (!ignoreErrors)
			{
				if (Singleton<NLogManager>.Instance.ThrowExceptions)
				{
					throw ex3;
				}
				Logger.Error("Error in Parsing Configuration File. Exception : {0}", ex3);
			}
			else
			{
				Logger.Assert(ex3, "Error in Parsing Configuration File. Exception : {0}");
			}
		}
	}

	private void ConfigureFromFile(string fileName)
	{
		string fullPath = Path.GetFullPath(fileName);
		if (!visitedFile.ContainsKey(fullPath))
		{
			visitedFile[fullPath] = true;
			ParseTopLevel(new NLogXmlElement(fileName), Path.GetDirectoryName(fileName));
		}
	}

	private void ParseTopLevel(NLogXmlElement content, string baseDirectory)
	{
		content.AssertName("nlog", "configuration");
		string text = content.LocalName.ToUpper(CultureInfo.InvariantCulture);
		if (!(text == "CONFIGURATION"))
		{
			if (text == "NLOG")
			{
				ParseNLogElement(content, baseDirectory);
			}
		}
		else
		{
			ParseConfigurationElement(content, baseDirectory);
		}
	}

	private void ParseConfigurationElement(NLogXmlElement configurationElement, string baseDirectory)
	{
		Logger.Trace("ParseConfigurationElement");
		configurationElement.AssertName("configuration");
		foreach (NLogXmlElement item in configurationElement.Elements("nlog"))
		{
			ParseNLogElement(item, baseDirectory);
		}
	}

	private void ParseNLogElement(NLogXmlElement nlogElement, string baseDirectory)
	{
		Logger.Trace("ParseNLogElement");
		nlogElement.AssertName("nlog");
		if (nlogElement.GetOptionalBooleanAttribute("useInvariantCulture", defaultValue: false))
		{
			DefaultCultureInfo = CultureInfo.InvariantCulture;
		}
		AutoReload = nlogElement.GetOptionalBooleanAttribute("autoReload", defaultValue: false);
		Singleton<NLogManager>.Instance.ThrowExceptions = nlogElement.GetOptionalBooleanAttribute("throwExceptions", Singleton<NLogManager>.Instance.ThrowExceptions);
		Singleton<NLogManager>.Instance.GlobalLogLevel = LogLevels.Everything;
		Singleton<NLogManager>.Instance.AssertException = bool.Parse(nlogElement.GetOptionalAttribute("assertException", false.ToString()));
		Singleton<NLogManager>.Instance.ShowUnityLog = bool.Parse(nlogElement.GetOptionalAttribute("enableUnityLogListener", false.ToString()));
		foreach (NLogXmlElement child in nlogElement.Children)
		{
			switch (child.LocalName.ToUpper(CultureInfo.InvariantCulture))
			{
			case "EXTENSIONS":
				ParseExtensionsElement(child, baseDirectory);
				break;
			case "INCLUDE":
				ParseIncludeElement(child, baseDirectory);
				break;
			case "APPENDERS":
			case "TARGETS":
				ParseTargetsElement(child);
				break;
			case "VARIABLE":
				ParseVariableElement(child);
				break;
			case "RULES":
				ParseRulesElement(child, LoggingRules);
				break;
			case "TIME":
				ParseTimeElement(child);
				break;
			default:
				Logger.Warn("Skipping unknown node: {0}", child.LocalName);
				break;
			}
		}
	}

	private void ParseRulesElement(NLogXmlElement rulesElement, IList<LoggingRule> rulesCollection)
	{
		Logger.Trace("ParseRulesElement");
		rulesElement.AssertName("rules");
		foreach (NLogXmlElement item in rulesElement.Elements("logger"))
		{
			ParseLoggerElement(item, rulesCollection);
		}
	}

	private void ParseLoggerElement(NLogXmlElement loggerElement, IList<LoggingRule> rulesCollection)
	{
		loggerElement.AssertName("logger");
		if (!loggerElement.GetOptionalBooleanAttribute("enabled", defaultValue: true))
		{
			Logger.Debug("The logger named '{0}' are disabled");
			return;
		}
		LoggingRule loggingRule = new LoggingRule();
		string optionalAttribute = loggerElement.GetOptionalAttribute("appendTo", null);
		if (optionalAttribute == null)
		{
			optionalAttribute = loggerElement.GetOptionalAttribute("target", null);
		}
		loggingRule.namePatternMatch.LoggerNamePattern = loggerElement.GetOptionalAttribute("name", "*");
		loggingRule.namespacePatternMatch.LoggerNamePattern = loggerElement.GetOptionalAttribute("namespace", "*");
		loggingRule.TargetPlatforms = loggerElement.GetOptionalAttribute("platforms", "Everything");
		if (optionalAttribute != null)
		{
			string[] array = optionalAttribute.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				Target target = FindTargetByName(text);
				if (target != null)
				{
					loggingRule.Targets.Add(target);
					continue;
				}
				throw new NLogConfigurationException($"The rule having pattern \"{loggingRule.namePatternMatch.LoggerNamePattern}\" has an unknown target \"{text}\".");
			}
		}
		loggingRule.Final = loggerElement.GetOptionalBooleanAttribute("final", defaultValue: false);
		if (loggerElement.AttributeValues.TryGetValue("level", out var value))
		{
			LogLevels level = value.ToEnum((LogLevels)0);
			loggingRule.EnableLoggingForLevel(level);
		}
		else if (loggerElement.AttributeValues.TryGetValue("levels", out value))
		{
			value = CleanWhitespace(value);
			string[] array = value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!string.IsNullOrEmpty(array[i]))
				{
					LogLevels level2 = value.ToEnum((LogLevels)0);
					loggingRule.EnableLoggingForLevel(level2);
				}
			}
		}
		else
		{
			LogLevels logLevels = LogLevels.Assert;
			LogLevels logLevels2 = LogLevels.Trace;
			if (loggerElement.AttributeValues.TryGetValue("minLevel", out var value2))
			{
				logLevels = value2.ToEnum((LogLevels)0);
			}
			if (loggerElement.AttributeValues.TryGetValue("maxLevel", out var value3))
			{
				logLevels2 = value3.ToEnum((LogLevels)0);
			}
			for (int num = (int)logLevels; num <= (int)logLevels2; num <<= 1)
			{
				loggingRule.EnableLoggingForLevel((LogLevels)num);
			}
		}
		foreach (NLogXmlElement child in loggerElement.Children)
		{
			string text2 = child.LocalName.ToUpper(CultureInfo.InvariantCulture);
			if (!(text2 == "FILTERS"))
			{
				if (text2 == "LOGGER")
				{
					ParseLoggerElement(child, loggingRule.ChildRules);
				}
			}
			else
			{
				ParseFilters(loggingRule, child);
			}
		}
		rulesCollection.Add(loggingRule);
	}

	private void ParseFilters(LoggingRule rule, NLogXmlElement filtersElement)
	{
		filtersElement.AssertName("filters");
		foreach (NLogXmlElement child in filtersElement.Children)
		{
			string localName = child.LocalName;
			Filter filter = configurationItemFactory.Filters.CreateInstance(localName);
			ConfigureObjectFromAttributes(filter, child, ignoreType: false);
			rule.Filters.Add(filter);
		}
	}

	private void ParseVariableElement(NLogXmlElement variableElement)
	{
		variableElement.AssertName("variable");
		string requiredAttribute = variableElement.GetRequiredAttribute("name");
		string value = ExpandVariables(variableElement.GetRequiredAttribute("value"));
		variables[requiredAttribute] = value;
	}

	private void ParseTargetsElement(NLogXmlElement targetsElement)
	{
		targetsElement.AssertName("targets", "appenders");
		bool optionalBooleanAttribute = targetsElement.GetOptionalBooleanAttribute("async", defaultValue: false);
		NLogXmlElement nLogXmlElement = null;
		Dictionary<string, NLogXmlElement> dictionary = new Dictionary<string, NLogXmlElement>();
		foreach (NLogXmlElement child in targetsElement.Children)
		{
			string localName = child.LocalName;
			string text = StripOptionalNamespacePrefix(child.GetOptionalAttribute("type", null));
			switch (localName.ToUpper(CultureInfo.InvariantCulture))
			{
			case "DEFAULT-WRAPPER":
				nLogXmlElement = child;
				break;
			case "DEFAULT-TARGET-PARAMETERS":
				if (text == null)
				{
					throw new NLogConfigurationException("Missing 'type' attribute on <" + localName + "/>.");
				}
				dictionary[text] = child;
				break;
			case "TARGET":
			case "APPENDER":
			case "WRAPPER":
			case "WRAPPER-TARGET":
			case "COMPOUND-TARGET":
			{
				if (text == null)
				{
					throw new NLogConfigurationException("Missing 'type' attribute on <" + localName + "/>.");
				}
				Target target = configurationItemFactory.Targets.CreateInstance(text);
				if (dictionary.TryGetValue(text, out var value))
				{
					ParseTargetElement(target, value);
				}
				ParseTargetElement(target, child);
				if (optionalBooleanAttribute)
				{
					target = WrapWithAsyncTargetWrapper(target);
				}
				if (nLogXmlElement != null)
				{
					target = WrapWithDefaultWrapper(target, nLogXmlElement);
				}
				Logger.Debug("Adding target {0}", target);
				AddTarget(target.Name, target);
				break;
			}
			}
		}
	}

	private void ParseTargetElement(Target target, NLogXmlElement targetElement)
	{
		CompoundTargetBase compoundTargetBase = target as CompoundTargetBase;
		WrapperTargetBase wrapperTargetBase = target as WrapperTargetBase;
		ConfigureObjectFromAttributes(target, targetElement, ignoreType: true);
		foreach (NLogXmlElement child in targetElement.Children)
		{
			string localName = child.LocalName;
			if (compoundTargetBase != null)
			{
				if (IsTargetRefElement(localName))
				{
					string requiredAttribute = child.GetRequiredAttribute("name");
					Target target2 = FindTargetByName(requiredAttribute);
					if (target2 == null)
					{
						throw new NLogConfigurationException("Referenced target '" + requiredAttribute + "' not found.");
					}
					compoundTargetBase.Targets.Add(target2);
					continue;
				}
				if (IsTargetElement(localName))
				{
					string itemName = StripOptionalNamespacePrefix(child.GetRequiredAttribute("type"));
					Target target3 = configurationItemFactory.Targets.CreateInstance(itemName);
					if (target3 != null)
					{
						ParseTargetElement(target3, child);
						if (target3.Name != null)
						{
							AddTarget(target3.Name, target3);
						}
						compoundTargetBase.Targets.Add(target3);
					}
					continue;
				}
			}
			if (wrapperTargetBase != null)
			{
				if (IsTargetRefElement(localName))
				{
					string requiredAttribute2 = child.GetRequiredAttribute("name");
					Target target4 = FindTargetByName(requiredAttribute2);
					if (target4 == null)
					{
						throw new NLogConfigurationException("Referenced target '" + requiredAttribute2 + "' not found.");
					}
					wrapperTargetBase.WrappedTarget = target4;
					continue;
				}
				if (IsTargetElement(localName))
				{
					string itemName2 = StripOptionalNamespacePrefix(child.GetRequiredAttribute("type"));
					Target target5 = configurationItemFactory.Targets.CreateInstance(itemName2);
					if (target5 != null)
					{
						ParseTargetElement(target5, child);
						if (target5.Name != null)
						{
							AddTarget(target5.Name, target5);
						}
						if (wrapperTargetBase.WrappedTarget != null)
						{
							throw new NLogConfigurationException("Wrapper target can only have one child.");
						}
						wrapperTargetBase.WrappedTarget = target5;
					}
					continue;
				}
			}
			SetPropertyFromElement(target, child);
		}
	}

	private void ParseExtensionsElement(NLogXmlElement extensionsElement, string baseDirectory)
	{
		extensionsElement.AssertName("extensions");
		foreach (NLogXmlElement item in extensionsElement.Elements("add"))
		{
			string text = item.GetOptionalAttribute("prefix", null);
			if (text != null)
			{
				text += ".";
			}
			string text2 = StripOptionalNamespacePrefix(item.GetOptionalAttribute("type", null));
			if (text2 != null)
			{
				configurationItemFactory.RegisterType(Type.GetType(text2, throwOnError: true), text);
			}
			string optionalAttribute = item.GetOptionalAttribute("assemblyFile", null);
			if (optionalAttribute != null)
			{
				try
				{
					string text3 = Path.Combine(baseDirectory, optionalAttribute);
					Logger.Info("Loading assembly file: {0}", text3);
					Assembly assembly = Assembly.LoadFrom(text3);
					configurationItemFactory.RegisterItemsFromAssembly(assembly, text);
				}
				catch (Exception ex)
				{
					if (ex.MustBeRethrown())
					{
						throw;
					}
					Logger.Error("Error loading extensions: {0}", ex);
					if (Singleton<NLogManager>.Instance.ThrowExceptions)
					{
						throw new NLogConfigurationException("Error loading extensions: " + optionalAttribute, ex);
					}
				}
				continue;
			}
			string optionalAttribute2 = item.GetOptionalAttribute("assembly", null);
			if (optionalAttribute2 == null)
			{
				continue;
			}
			try
			{
				Logger.Info("Loading assembly name: {0}", optionalAttribute2);
				Assembly assembly2 = Assembly.Load(optionalAttribute2);
				configurationItemFactory.RegisterItemsFromAssembly(assembly2, text);
			}
			catch (Exception ex2)
			{
				if (ex2.MustBeRethrown())
				{
					throw;
				}
				Logger.Error("Error loading extensions: {0}", ex2);
				if (Singleton<NLogManager>.Instance.ThrowExceptions)
				{
					throw new NLogConfigurationException("Error loading extensions: " + optionalAttribute2, ex2);
				}
			}
		}
	}

	private void ParseIncludeElement(NLogXmlElement includeElement, string baseDirectory)
	{
		includeElement.AssertName("include");
		string text = includeElement.GetRequiredAttribute("file");
		try
		{
			text = ExpandVariables(text);
			text = SimpleLayout.Evaluate(text);
			if (baseDirectory != null)
			{
				text = Path.Combine(baseDirectory, text);
			}
			if (File.Exists(text))
			{
				Logger.Debug("Including file '{0}'", text);
				ConfigureFromFile(text);
				return;
			}
			throw new FileNotFoundException("Included file not found: " + text);
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Error("Error when including '{0}' {1}", text, ex);
			if (includeElement.GetOptionalBooleanAttribute("ignoreErrors", defaultValue: false))
			{
				return;
			}
			throw new NLogConfigurationException("Error when including: " + text, ex);
		}
	}

	private void ParseTimeElement(NLogXmlElement timeElement)
	{
		timeElement.AssertName("time");
		string requiredAttribute = timeElement.GetRequiredAttribute("type");
		TimeSource timeSource = configurationItemFactory.TimeSources.CreateInstance(requiredAttribute);
		ConfigureObjectFromAttributes(timeSource, timeElement, ignoreType: true);
		Logger.Debug("Selecting time source {0}", timeSource);
		TimeSource.Current = timeSource;
	}

	private void SetPropertyFromElement(object o, NLogXmlElement element)
	{
		if (!AddArrayItemFromElement(o, element) && !SetLayoutFromElement(o, element))
		{
			PropertyHelper.SetPropertyFromString(o, element.LocalName, ExpandVariables(element.Value), configurationItemFactory);
		}
	}

	private bool AddArrayItemFromElement(object o, NLogXmlElement element)
	{
		string localName = element.LocalName;
		if (!PropertyHelper.TryGetPropertyInfo(o, localName, out var result))
		{
			return false;
		}
		Type arrayItemType = PropertyHelper.GetArrayItemType(result);
		if (arrayItemType != null)
		{
			IList list = (IList)result.GetValue(o, null);
			object obj = FactoryHelper.CreateInstance(arrayItemType);
			ConfigureObjectFromAttributes(obj, element, ignoreType: true);
			ConfigureObjectFromElement(obj, element);
			list.Add(obj);
			return true;
		}
		return false;
	}

	private void ConfigureObjectFromAttributes(object targetObject, NLogXmlElement element, bool ignoreType)
	{
		foreach (KeyValuePair<string, string> attributeValue in element.AttributeValues)
		{
			string key = attributeValue.Key;
			string value = attributeValue.Value;
			if (!ignoreType || !key.Equals("type", StringComparison.OrdinalIgnoreCase))
			{
				PropertyHelper.SetPropertyFromString(targetObject, key, ExpandVariables(value), configurationItemFactory);
			}
		}
	}

	private bool SetLayoutFromElement(object o, NLogXmlElement layoutElement)
	{
		string localName = layoutElement.LocalName;
		if (PropertyHelper.TryGetPropertyInfo(o, localName, out var result) && typeof(Layout).IsAssignableFrom(result.PropertyType))
		{
			string text = StripOptionalNamespacePrefix(layoutElement.GetOptionalAttribute("type", null));
			if (text != null)
			{
				Layout layout = configurationItemFactory.Layouts.CreateInstance(ExpandVariables(text));
				ConfigureObjectFromAttributes(layout, layoutElement, ignoreType: true);
				ConfigureObjectFromElement(layout, layoutElement);
				result.SetValue(o, layout, null);
				return true;
			}
		}
		return false;
	}

	private void ConfigureObjectFromElement(object targetObject, NLogXmlElement element)
	{
		foreach (NLogXmlElement child in element.Children)
		{
			SetPropertyFromElement(targetObject, child);
		}
	}

	private Target WrapWithDefaultWrapper(Target t, NLogXmlElement defaultParameters)
	{
		string itemName = StripOptionalNamespacePrefix(defaultParameters.GetRequiredAttribute("type"));
		Target target = configurationItemFactory.Targets.CreateInstance(itemName);
		WrapperTargetBase wrapperTargetBase = target as WrapperTargetBase;
		if (wrapperTargetBase == null)
		{
			throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
		}
		ParseTargetElement(target, defaultParameters);
		while (wrapperTargetBase.WrappedTarget != null)
		{
			wrapperTargetBase = wrapperTargetBase.WrappedTarget as WrapperTargetBase;
			if (wrapperTargetBase == null)
			{
				throw new NLogConfigurationException("Child target type specified on <default-wrapper /> is not a wrapper.");
			}
		}
		wrapperTargetBase.WrappedTarget = t;
		target.Name = t.Name;
		t.Name += "_wrapped";
		Logger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}", target.Name, target.GetType().Name, t.Name);
		return target;
	}

	private string ExpandVariables(string input)
	{
		string text = input;
		foreach (KeyValuePair<string, string> variable in variables)
		{
			text = text.Replace("${" + variable.Key + "}", variable.Value);
		}
		return text;
	}
}
