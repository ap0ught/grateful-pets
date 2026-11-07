using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Conditions;
using AiUnity.NLog.Core.Filters;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.LayoutRenderers;
using AiUnity.NLog.Core.Layouts;
using AiUnity.NLog.Core.Targets;
using AiUnity.NLog.Core.Time;

namespace AiUnity.NLog.Core.Config;

public class ConfigurationItemFactory
{
	private readonly IList<object> allFactories;

	private readonly Factory<Target, TargetAttribute> targets;

	private readonly Factory<Filter, FilterAttribute> filters;

	private readonly Factory<LayoutRenderer, LayoutRendererAttribute> layoutRenderers;

	private readonly Factory<Layout, LayoutAttribute> layouts;

	private readonly MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute> conditionMethods;

	private readonly Factory<LayoutRenderer, AmbientPropertyAttribute> ambientProperties;

	private readonly Factory<TimeSource, TimeSourceAttribute> timeSources;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public static ConfigurationItemFactory Default { get; set; }

	public ConfigurationItemCreator CreateInstance { get; set; }

	public INamedItemFactory<Target, Type> Targets => targets;

	public INamedItemFactory<Filter, Type> Filters => filters;

	public INamedItemFactory<LayoutRenderer, Type> LayoutRenderers => layoutRenderers;

	public INamedItemFactory<Layout, Type> Layouts => layouts;

	public INamedItemFactory<LayoutRenderer, Type> AmbientProperties => ambientProperties;

	public INamedItemFactory<TimeSource, Type> TimeSources => timeSources;

	public INamedItemFactory<MethodInfo, MethodInfo> ConditionMethods => conditionMethods;

	static ConfigurationItemFactory()
	{
		Default = BuildDefaultFactory();
	}

	public ConfigurationItemFactory(params Assembly[] assemblies)
	{
		CreateInstance = FactoryHelper.CreateInstance;
		targets = new Factory<Target, TargetAttribute>(this);
		filters = new Factory<Filter, FilterAttribute>(this);
		layoutRenderers = new Factory<LayoutRenderer, LayoutRendererAttribute>(this);
		layouts = new Factory<Layout, LayoutAttribute>(this);
		conditionMethods = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>();
		ambientProperties = new Factory<LayoutRenderer, AmbientPropertyAttribute>(this);
		timeSources = new Factory<TimeSource, TimeSourceAttribute>(this);
		allFactories = new List<object> { targets, filters, layoutRenderers, layouts, conditionMethods, ambientProperties, timeSources };
		foreach (Assembly assembly in assemblies)
		{
			RegisterItemsFromAssembly(assembly);
		}
	}

	public void RegisterItemsFromAssembly(Assembly assembly)
	{
		RegisterItemsFromAssembly(assembly, string.Empty);
	}

	public void RegisterItemsFromAssembly(Assembly assembly, string itemNamePrefix)
	{
		Logger.Debug("ScanAssembly('{0}')", assembly.FullName);
		Type[] type = assembly.SafeGetTypes();
		foreach (IFactory allFactory in allFactories)
		{
			allFactory.ScanTypes(type, itemNamePrefix);
		}
	}

	public void Clear()
	{
		foreach (IFactory allFactory in allFactories)
		{
			allFactory.Clear();
		}
	}

	public void RegisterType(Type type, string itemNamePrefix)
	{
		foreach (IFactory allFactory in allFactories)
		{
			allFactory.RegisterType(type, itemNamePrefix);
		}
	}

	private static ConfigurationItemFactory BuildDefaultFactory()
	{
		List<string> searchAssemblyNames = new List<string>
		{
			"Assembly-CSharp",
			Assembly.GetExecutingAssembly().GetName().Name
		};
		ConfigurationItemFactory configurationItemFactory = new ConfigurationItemFactory(Enumerable.ToArray((from a in AppDomain.CurrentDomain.GetAssemblies()
			where searchAssemblyNames.Any((string t) => a.FullName.StartsWith(t))
			select a).ToList()));
		configurationItemFactory.RegisterExtendedItems();
		return configurationItemFactory;
	}

	private void RegisterExtendedItems()
	{
		string assemblyQualifiedName = typeof(NLogger).AssemblyQualifiedName;
		string text = "NLog,";
		string text2 = "NLog.Extended,";
		int num = assemblyQualifiedName.IndexOf(text, StringComparison.OrdinalIgnoreCase);
		if (num >= 0)
		{
			assemblyQualifiedName = ", " + text2 + assemblyQualifiedName.Substring(num + text.Length);
			string text3 = typeof(FileTarget).Namespace;
			targets.RegisterNamedType("AspNetTrace", text3 + ".AspNetTraceTarget" + assemblyQualifiedName);
			targets.RegisterNamedType("MSMQ", text3 + ".MessageQueueTarget" + assemblyQualifiedName);
			targets.RegisterNamedType("AspNetBufferingWrapper", text3 + ".Wrappers.AspNetBufferingTargetWrapper" + assemblyQualifiedName);
			string text4 = typeof(MessageLayoutRenderer).Namespace;
			layoutRenderers.RegisterNamedType("appsetting", text4 + ".AppSettingLayoutRenderer" + assemblyQualifiedName);
			layoutRenderers.RegisterNamedType("aspnet-application", text4 + ".AspNetApplicationValueLayoutRenderer" + assemblyQualifiedName);
			layoutRenderers.RegisterNamedType("aspnet-request", text4 + ".AspNetRequestValueLayoutRenderer" + assemblyQualifiedName);
			layoutRenderers.RegisterNamedType("aspnet-sessionid", text4 + ".AspNetSessionIDLayoutRenderer" + assemblyQualifiedName);
			layoutRenderers.RegisterNamedType("aspnet-session", text4 + ".AspNetSessionValueLayoutRenderer" + assemblyQualifiedName);
			layoutRenderers.RegisterNamedType("aspnet-user-authtype", text4 + ".AspNetUserAuthTypeLayoutRenderer" + assemblyQualifiedName);
			layoutRenderers.RegisterNamedType("aspnet-user-identity", text4 + ".AspNetUserIdentityLayoutRenderer" + assemblyQualifiedName);
		}
	}
}
