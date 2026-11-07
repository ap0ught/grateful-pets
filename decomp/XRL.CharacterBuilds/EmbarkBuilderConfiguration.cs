using System;
using System.Collections.Generic;

namespace XRL.CharacterBuilds;

[HasModSensitiveStaticCache]
public static class EmbarkBuilderConfiguration
{
	public static List<AbstractEmbarkBuilderModule> activeModules;

	public static Dictionary<string, AbstractEmbarkBuilderModule> modules;

	public static void Init()
	{
		activeModules = new List<AbstractEmbarkBuilderModule>();
		modules = new Dictionary<string, AbstractEmbarkBuilderModule>();
		Dictionary<string, Action<XmlDataHelper>> handlers = null;
		handlers = new Dictionary<string, Action<XmlDataHelper>>
		{
			{
				"embarkmodules",
				(XmlDataHelper xml) =>
				{
					xml.HandleNodes(handlers);
				}
			},
			{ "module", HandleModule }
		};
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("EmbarkModules"))
		{
			item.HandleNodes(handlers);
		}
		static void HandleModule(XmlDataHelper xml)
		{
			string attribute = xml.GetAttribute("Class");
			ModManager.ResolveType(attribute);
			AbstractEmbarkBuilderModule value = null;
			if (!modules.TryGetValue(attribute, out value))
			{
				value = Activator.CreateInstance(ModManager.ResolveType(attribute)) as AbstractEmbarkBuilderModule;
				modules.Add(attribute, value);
				activeModules.Add(value);
			}
			if (value == null)
			{
				MetricsManager.LogError("Unknown embark builder type: " + attribute);
				xml.DoneWithElement();
			}
			else
			{
				value.HandleNodes(xml);
			}
		}
	}
}
