using System;
using System.Collections.Generic;
using System.Xml;
using XRL.Rules;
using XRL.UI;
using XRL.World;

namespace XRL.Names;

[HasModSensitiveStaticCache]
public static class NameStyles
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, NameStyle> _NameStyleTable;

	[ModSensitiveStaticCache(false)]
	private static List<NameStyle> _NameStyleList;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, List<NameValue>> _DefaultTemplateVars;

	public static int NameGenerationFailures;

	public static Dictionary<string, NameStyle> NameStyleTable
	{
		get
		{
			CheckInit();
			return _NameStyleTable;
		}
	}

	public static List<NameStyle> NameStyleList
	{
		get
		{
			CheckInit();
			return _NameStyleList;
		}
	}

	public static Dictionary<string, List<NameValue>> DefaultTemplateVars
	{
		get
		{
			CheckInit();
			return _DefaultTemplateVars;
		}
	}

	public static void CheckInit()
	{
		if (_NameStyleTable == null)
		{
			Loading.LoadTask("Loading Naming.xml", Init);
		}
	}

	private static void Init()
	{
		_NameStyleTable = new Dictionary<string, NameStyle>(16);
		_NameStyleList = new List<NameStyle>(16);
		_DefaultTemplateVars = new Dictionary<string, List<NameValue>>();
		foreach (DataFile item in DataManager.GetXMLFilesWithRoot("Naming"))
		{
			try
			{
				ProcessNamingXmlFile(item, item.IsMod);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.Mod, message);
			}
		}
	}

	private static void ProcessNamingXmlFile(string file, bool Mod)
	{
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(file);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType == XmlNodeType.Element)
			{
				if (!(xmlTextReader.Name == "naming"))
				{
					throw new XmlUnsupportedElementException(xmlTextReader);
				}
				LoadNamingNode(xmlTextReader, Mod);
			}
		}
		xmlTextReader.Close();
	}

	public static void LoadNamingNode(XmlTextReader Reader, bool Mod = false)
	{
		string loadMode = (Mod ? Reader.GetAttribute("Load") : null);
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "namestyles")
				{
					LoadNameStylesNode(Reader, Mod, loadMode);
					continue;
				}
				if (!(Reader.Name == "defaulttemplatevars"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadDefaultTemplateVarsNode(Reader, Mod, loadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylesNode(XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "namestyle"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleNode(Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleNode(XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
		}
		if (_NameStyleTable.TryGetValue(attribute, out var value))
		{
			if (!Mod)
			{
				MetricsManager.LogError("duplicate name style " + attribute);
				return;
			}
			if (LoadMode != "Merge")
			{
				_NameStyleList.Remove(value);
				value = new NameStyle();
				value.Name = attribute;
				_NameStyleTable[attribute] = value;
			}
		}
		else
		{
			value = new NameStyle();
			value.Name = attribute;
			_NameStyleTable.Add(attribute, value);
			_NameStyleList.Add(value);
		}
		string attribute2 = Reader.GetAttribute("HyphenationChance");
		if (!attribute2.IsNullOrEmpty())
		{
			if (!int.TryParse(attribute2, out value.HyphenationChance))
			{
				throw new XmlException("invalid HyphenationChance: " + attribute2, Reader);
			}
			value.HyphenationChanceSet = true;
		}
		attribute2 = Reader.GetAttribute("TwoNameChance");
		if (!attribute2.IsNullOrEmpty())
		{
			if (!int.TryParse(attribute2, out value.TwoNameChance))
			{
				throw new XmlException("invalid TwoNameChance: " + attribute2, Reader);
			}
			value.TwoNameChanceSet = true;
		}
		attribute2 = Reader.GetAttribute("Base");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Base = attribute2;
		}
		attribute2 = Reader.GetAttribute("Format");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Format = attribute2;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "prefixes")
				{
					LoadNameStylePrefixesNode(value, Reader, Mod, LoadMode);
					continue;
				}
				if (Reader.Name == "infixes")
				{
					LoadNameStyleInfixesNode(value, Reader, Mod, LoadMode);
					continue;
				}
				if (Reader.Name == "postfixes")
				{
					LoadNameStylePostfixesNode(value, Reader, Mod, LoadMode);
					continue;
				}
				if (Reader.Name == "templates" || Reader.Name == "titletemplates")
				{
					LoadNameStyleTemplatesNode(value, Reader, Mod, LoadMode);
					continue;
				}
				if (Reader.Name == "templatevars")
				{
					LoadNameStyleTemplateVarsNode(value, Reader, Mod, LoadMode);
					continue;
				}
				if (!(Reader.Name == "scopes"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleScopesNode(value, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylePrefixesNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Prefixes.Clear();
			}
		}
		string attribute = Reader.GetAttribute("Amount");
		if (!attribute.IsNullOrEmpty())
		{
			style.PrefixAmount = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "prefix"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStylePrefixNode(style, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylePrefixNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NamePrefix namePrefix = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			namePrefix = style.Prefixes.Find(attribute);
			if (namePrefix != null)
			{
				if (LoadMode != "Merge")
				{
					style.Prefixes.Remove(namePrefix);
					namePrefix = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Prefixes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (namePrefix == null)
		{
			namePrefix = new NamePrefix();
			namePrefix.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out namePrefix.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Prefixes.Add(namePrefix);
		}
	}

	public static void LoadNameStyleInfixesNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Infixes.Clear();
			}
		}
		string attribute = Reader.GetAttribute("Amount");
		if (!attribute.IsNullOrEmpty())
		{
			style.InfixAmount = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "infix"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleInfixNode(style, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleInfixNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameInfix nameInfix = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameInfix = style.Infixes.Find(attribute);
			if (nameInfix != null)
			{
				if (LoadMode != "Merge")
				{
					style.Infixes.Remove(nameInfix);
					nameInfix = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Infixes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameInfix == null)
		{
			nameInfix = new NameInfix();
			nameInfix.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out nameInfix.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Infixes.Add(nameInfix);
		}
	}

	public static void LoadNameStylePostfixesNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Postfixes.Clear();
			}
		}
		string attribute = Reader.GetAttribute("Amount");
		if (!attribute.IsNullOrEmpty())
		{
			style.PostfixAmount = attribute;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "postfix"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStylePostfixNode(style, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStylePostfixNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NamePostfix namePostfix = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			namePostfix = style.Postfixes.Find(attribute);
			if (namePostfix != null)
			{
				if (LoadMode != "Merge")
				{
					style.Postfixes.Remove(namePostfix);
					namePostfix = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Postfixes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (namePostfix == null)
		{
			namePostfix = new NamePostfix();
			namePostfix.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out namePostfix.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Postfixes.Add(namePostfix);
		}
	}

	public static void LoadNameStyleTemplatesNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.Templates.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "template") && !(Reader.Name == "titletemplate"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleTemplateNode(style, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleTemplateNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameTemplate nameTemplate = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameTemplate = style.Templates.Find(attribute);
			if (nameTemplate != null)
			{
				if (LoadMode != "Merge")
				{
					style.Templates.Remove(nameTemplate);
					nameTemplate = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (style.Templates.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameTemplate == null)
		{
			nameTemplate = new NameTemplate();
			nameTemplate.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out nameTemplate.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		if (!flag)
		{
			style.Templates.Add(nameTemplate);
		}
	}

	public static void LoadNameStyleTemplateVarsNode(NameStyle style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				style.TemplateVars.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "templatevar"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleTemplateVarNode(style, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleTemplateVarNode(NameStyle Style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		List<NameValue> value = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (Style.TemplateVars != null && Style.TemplateVars.TryGetValue(attribute, out value))
			{
				if (LoadMode != "Merge")
				{
					value.Clear();
				}
				flag = true;
			}
		}
		else if (Style.TemplateVars.ContainsKey(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (value == null)
		{
			value = new List<NameValue>();
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "value"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleTemplateValueNode(Style, value, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
		if (!flag)
		{
			if (Style.TemplateVars == null)
			{
				Style.TemplateVars = new Dictionary<string, List<NameValue>>();
			}
			Style.TemplateVars[attribute] = value;
		}
	}

	public static void LoadNameStyleTemplateValueNode(NameStyle Style, List<NameValue> List, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameValue nameValue = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameValue = List.Find(attribute);
			if (nameValue != null)
			{
				if (LoadMode != "Merge")
				{
					Style.TemplateVars.Remove(attribute);
					nameValue = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (List.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameValue == null)
		{
			nameValue = new NameValue();
			nameValue.Name = attribute;
		}
		if (!flag)
		{
			List.Add(nameValue);
		}
	}

	public static void LoadNameStyleScopesNode(NameStyle Style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				Style.Scopes.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "scope"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadNameStyleScopeNode(Style, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadNameStyleScopeNode(NameStyle Style, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameScope nameScope = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameScope = Style.Scopes.Find(attribute);
			if (nameScope != null)
			{
				if (LoadMode != "Merge")
				{
					Style.Scopes.Remove(nameScope);
					nameScope = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (Style.Scopes.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameScope == null)
		{
			nameScope = new NameScope();
			nameScope.Name = attribute;
		}
		string attribute2 = Reader.GetAttribute("Weight");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out nameScope.Weight))
		{
			throw new XmlException("invalid Weight: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Genotype");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Genotype = attribute2;
		}
		attribute2 = Reader.GetAttribute("Subtype");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Subtype = attribute2;
		}
		attribute2 = Reader.GetAttribute("Species");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Species = attribute2;
		}
		attribute2 = Reader.GetAttribute("Culture");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Culture = attribute2;
		}
		attribute2 = Reader.GetAttribute("Faction");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Faction = attribute2;
		}
		attribute2 = Reader.GetAttribute("Region");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Region = attribute2;
		}
		attribute2 = Reader.GetAttribute("Gender");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Gender = attribute2;
		}
		attribute2 = Reader.GetAttribute("Mutation");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Mutation = attribute2;
		}
		attribute2 = Reader.GetAttribute("Tag");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Tag = attribute2;
		}
		attribute2 = Reader.GetAttribute("Special");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Special = attribute2;
		}
		attribute2 = Reader.GetAttribute("Type");
		if (!attribute2.IsNullOrEmpty())
		{
			nameScope.Type = attribute2;
		}
		attribute2 = Reader.GetAttribute("Priority");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out nameScope.Priority))
		{
			throw new XmlException("invalid Priority: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Chance");
		if (!attribute2.IsNullOrEmpty() && !int.TryParse(attribute2, out nameScope.Chance))
		{
			throw new XmlException("invalid Chance: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("Combine");
		if (!attribute2.IsNullOrEmpty() && !bool.TryParse(attribute2, out nameScope.Combine))
		{
			throw new XmlException("invalid Combine: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("SkipIfHasHonorific");
		if (!attribute2.IsNullOrEmpty() && !bool.TryParse(attribute2, out nameScope.SkipIfHasHonorific))
		{
			throw new XmlException("invalid SkipIfHasHonorific: " + attribute2, Reader);
		}
		attribute2 = Reader.GetAttribute("SkipIfHasEpithet");
		if (!attribute2.IsNullOrEmpty() && !bool.TryParse(attribute2, out nameScope.SkipIfHasEpithet))
		{
			throw new XmlException("invalid SkipIfHasEpithet: " + attribute2, Reader);
		}
		if (!flag)
		{
			Style.Scopes.Add(nameScope);
		}
	}

	public static void LoadDefaultTemplateVarsNode(XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (LoadMode != "Merge")
			{
				_DefaultTemplateVars.Clear();
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "templatevar"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadDefaultTemplateVarNode(Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadDefaultTemplateVarNode(XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		List<NameValue> value = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			if (_DefaultTemplateVars != null && _DefaultTemplateVars.TryGetValue(attribute, out value))
			{
				if (LoadMode != "Merge")
				{
					value.Clear();
				}
				flag = true;
			}
		}
		else if (_DefaultTemplateVars.ContainsKey(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (value == null)
		{
			value = new List<NameValue>();
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "value"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadDefaultTemplateValueNode(value, Reader, Mod, LoadMode);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
		if (!flag)
		{
			if (_DefaultTemplateVars == null)
			{
				_DefaultTemplateVars = new Dictionary<string, List<NameValue>>();
			}
			_DefaultTemplateVars[attribute] = value;
		}
	}

	public static void LoadDefaultTemplateValueNode(List<NameValue> List, XmlTextReader Reader, bool Mod = false, string LoadMode = null)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute == null)
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		NameValue nameValue = null;
		bool flag = false;
		if (Mod)
		{
			LoadMode = Reader.GetAttribute("Load") ?? LoadMode;
			nameValue = List.Find(attribute);
			if (nameValue != null)
			{
				if (LoadMode != "Merge")
				{
					_DefaultTemplateVars.Remove(attribute);
					nameValue = null;
				}
				else
				{
					flag = true;
				}
			}
		}
		else if (List.Has(attribute))
		{
			throw new XmlException("duplicate element", Reader);
		}
		if (nameValue == null)
		{
			nameValue = new NameValue();
			nameValue.Name = attribute;
		}
		if (!flag)
		{
			List.Add(nameValue);
		}
	}

	public static string Generate(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, Dictionary<string, string> NamingContext = null, bool FailureOkay = false, bool SpecialFaildown = false, NameStyle Skip = null, List<NameStyle> SkipList = null, int? HyphenationChance = null, int? TwoNameChance = null, bool? HasHonorific = null, bool? HasEpithet = null, bool ForProcessed = false)
	{
		if (!ForProcessed && GameObject.Validate(ref For))
		{
			Genotype = For.GetGenotype();
			Subtype = For.GetSubtype();
			Species = For.GetSpecies();
			Culture = For.GetCulture();
			Faction = For.GetPrimaryFaction();
			Region = For.GetNativeRegion();
			Gender = For.GetGender().Name;
			Mutations = For.GetMutationNames();
			Tag = For.GetPropertyOrTag("NamingTag");
			Region = For.GetPropertyOrTag("NamingRegion");
			bool valueOrDefault = HasHonorific == true;
			if (!HasHonorific.HasValue)
			{
				valueOrDefault = For.HasHonorific;
				HasHonorific = valueOrDefault;
			}
			valueOrDefault = HasEpithet == true;
			if (!HasEpithet.HasValue)
			{
				valueOrDefault = For.HasEpithet;
				HasEpithet = valueOrDefault;
			}
		}
		while (true)
		{
			List<(NameStyle, NameScope)> list = new List<(NameStyle, NameScope)>();
			bool flag = true;
			foreach (NameStyle nameStyle in NameStyleList)
			{
				if (nameStyle == Skip || (SkipList != null && SkipList.Contains(nameStyle)))
				{
					continue;
				}
				NameScope nameScope = nameStyle.CheckApply(Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, HasHonorific == true, HasEpithet == true);
				if (nameScope == null)
				{
					continue;
				}
				if (nameScope.Combine & flag)
				{
					list.Add((nameStyle, nameScope));
					continue;
				}
				bool flag2 = true;
				foreach (var item in list)
				{
					if ((!nameScope.Combine || !item.Item2.Combine) && item.Item2.Priority > nameScope.Priority)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					list.Clear();
					list.Add((nameStyle, nameScope));
					flag = nameScope.Combine;
				}
			}
			switch (list.Count)
			{
			case 1:
			{
				string text3 = list[0].Item1.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, NamingContext, FailureOkay, SpecialFaildown, Skip, SkipList, HyphenationChance, TwoNameChance, HasHonorific, HasEpithet);
				if (!text3.IsNullOrEmpty())
				{
					return text3;
				}
				break;
			}
			default:
			{
				int num = 0;
				foreach (var item2 in list)
				{
					if (item2.Item2.Priority > 0)
					{
						num += item2.Item2.Priority;
					}
				}
				if (num <= 0)
				{
					break;
				}
				int num2 = Stat.Random(0, num);
				int num3 = 0;
				foreach (var item3 in list)
				{
					if (item3.Item2.Priority <= 0)
					{
						continue;
					}
					num3 += item3.Item2.Priority;
					if (num2 < num3)
					{
						string text = item3.Item1.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, NamingContext, FailureOkay, SpecialFaildown, Skip, SkipList, HyphenationChance, TwoNameChance, HasHonorific, HasEpithet);
						if (!text.IsNullOrEmpty())
						{
							return text;
						}
					}
				}
				foreach (var item4 in list)
				{
					if (item4.Item2.Priority > 0)
					{
						string text2 = item4.Item1.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, NamingContext, FailureOkay, SpecialFaildown, Skip, SkipList, HyphenationChance, TwoNameChance, HasHonorific, HasEpithet);
						if (!text2.IsNullOrEmpty())
						{
							return text2;
						}
					}
				}
				break;
			}
			case 0:
				break;
			}
			if (!SpecialFaildown || Special.IsNullOrEmpty())
			{
				break;
			}
			Special = null;
		}
		if (FailureOkay)
		{
			return null;
		}
		return "NameGenFail" + ++NameGenerationFailures;
	}
}
