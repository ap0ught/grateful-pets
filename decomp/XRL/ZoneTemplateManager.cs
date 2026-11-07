using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using XRL.UI;

namespace XRL;

[HasModSensitiveStaticCache]
public static class ZoneTemplateManager
{
	[ModSensitiveStaticCache(false)]
	public static Dictionary<string, ZoneTemplate> _Templates;

	public static Dictionary<string, ZoneTemplate> Templates
	{
		get
		{
			CheckInit();
			return _Templates;
		}
	}

	public static bool HasTemplates(string TemplatesName)
	{
		return Templates.ContainsKey(TemplatesName);
	}

	[PreGameCacheInit]
	public static void CheckInit()
	{
		if (_Templates == null)
		{
			Loading.LoadTask("Loading ZoneTemplates.xml", LoadTemplates);
		}
	}

	private static void LoadTemplates()
	{
		_Templates = new Dictionary<string, ZoneTemplate>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("ZoneTemplates"))
		{
			try
			{
				while (item.Read())
				{
					if (item.NodeType == XmlNodeType.Element && item.Name == "zonetemplate")
					{
						ZoneTemplate zoneTemplate = LoadTemplate(item);
						if (_Templates.ContainsKey(zoneTemplate.Name))
						{
							_Templates[zoneTemplate.Name] = zoneTemplate;
						}
						else
						{
							_Templates.Add(zoneTemplate.Name, zoneTemplate);
						}
					}
				}
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.modInfo, message);
			}
		}
	}

	private static ZoneTemplate LoadTemplate(XmlReader Reader)
	{
		ZoneTemplate zoneTemplate = new ZoneTemplate();
		zoneTemplate.Name = Reader.GetAttribute("Name");
		zoneTemplate.RegionSize = ReadIntAttribte(Reader, "RegionSize", 100);
		zoneTemplate.Root = new ZTGroupNode();
		zoneTemplate.GlobalRoot = new ZTGroupNode();
		zoneTemplate.SingleRoot = new ZTGroupNode();
		ZoneTemplateNode zoneTemplateNode = null;
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneTemplate;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "eachregion")
			{
				zoneTemplateNode = zoneTemplate.Root;
				zoneTemplate.Root.Style = ((Reader.GetAttribute("Style") == null) ? "pickeach" : Reader.GetAttribute("Style"));
			}
			else if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "global")
			{
				zoneTemplateNode = zoneTemplate.GlobalRoot;
				zoneTemplate.GlobalRoot.Style = ((Reader.GetAttribute("Style") == null) ? "pickeach" : Reader.GetAttribute("Style"));
			}
			else if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "single")
			{
				zoneTemplateNode = zoneTemplate.SingleRoot;
				zoneTemplate.SingleRoot.Style = ((Reader.GetAttribute("Style") == null) ? "pickeach" : Reader.GetAttribute("Style"));
			}
			else if (Reader.NodeType == XmlNodeType.Element)
			{
				ZoneTemplateNode item = LoadNode(Reader);
				if (zoneTemplateNode == null)
				{
					Debug.LogError("zone template error, no context specified. Things should be inside global or eachregion tags.");
				}
				else
				{
					zoneTemplateNode.Children.Add(item);
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "zonetemplate")
			{
				return zoneTemplate;
			}
		}
		return zoneTemplate;
	}

	private static int ReadIntAttribte(XmlReader Reader, string Attribute, int Default)
	{
		string attribute = Reader.GetAttribute(Attribute);
		if (string.IsNullOrEmpty(attribute))
		{
			return Default;
		}
		return Convert.ToInt32(attribute);
	}

	private static ZoneTemplateNode LoadNode(XmlReader Reader)
	{
		ZoneTemplateNode zoneTemplateNode = null;
		if (Reader.Name == "exit")
		{
			zoneTemplateNode = new ZTExitNode();
		}
		if (Reader.Name == "group")
		{
			zoneTemplateNode = new ZTGroupNode();
		}
		if (Reader.Name == "builder")
		{
			zoneTemplateNode = new ZTBuilderNode();
		}
		if (Reader.Name == "population")
		{
			zoneTemplateNode = new ZTPopulatonNode();
		}
		if (Reader.Name == "graph")
		{
			zoneTemplateNode = new ZTGraphNode();
		}
		if (Reader.Name == "cellfilterout")
		{
			zoneTemplateNode = new ZTCellFilterOutNode();
		}
		if (Reader.Name == "branch")
		{
			zoneTemplateNode = new ZTBranchNode();
		}
		if (Reader.Name.Length == 1)
		{
			zoneTemplateNode = new ZTGraphElementNode();
		}
		if (zoneTemplateNode == null)
		{
			Debug.LogError("Unexpected node type: " + Reader.Name);
		}
		zoneTemplateNode.Name = Reader.Name;
		zoneTemplateNode.Criteria = Reader.GetAttribute("Criteria");
		zoneTemplateNode.Filter = Reader.GetAttribute("Filter");
		zoneTemplateNode.Hint = Reader.GetAttribute("Hint");
		zoneTemplateNode.Weight = ((Reader.GetAttribute("Weight") == null) ? 1 : Convert.ToInt32(Reader.GetAttribute("Weight")));
		zoneTemplateNode.Style = ((Reader.GetAttribute("Style") == null) ? "pickeach" : Reader.GetAttribute("Style"));
		zoneTemplateNode.Chance = Reader.GetAttribute("Chance");
		zoneTemplateNode.MaxApplications = ReadIntAttribte(Reader, "MaxApplications", 0);
		zoneTemplateNode.Load(Reader);
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneTemplateNode;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				ZoneTemplateNode item = LoadNode(Reader);
				zoneTemplateNode.Children.Add(item);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				return zoneTemplateNode;
			}
		}
		return null;
	}
}
