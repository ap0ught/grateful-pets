using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Anatomy;

[HasModSensitiveStaticCache]
public static class Anatomies
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, BodyPartType> _BodyPartTypeTable;

	[ModSensitiveStaticCache(false)]
	private static List<BodyPartType> _BodyPartTypeList;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, Anatomy> _AnatomyTable;

	[ModSensitiveStaticCache(false)]
	private static List<Anatomy> _AnatomyList;

	public static Dictionary<string, BodyPartType> BodyPartTypeTable
	{
		get
		{
			CheckInit();
			return _BodyPartTypeTable;
		}
	}

	public static List<BodyPartType> BodyPartTypeList
	{
		get
		{
			CheckInit();
			return _BodyPartTypeList;
		}
	}

	public static Dictionary<string, Anatomy> AnatomyTable
	{
		get
		{
			CheckInit();
			return _AnatomyTable;
		}
	}

	public static List<Anatomy> AnatomyList
	{
		get
		{
			CheckInit();
			return _AnatomyList;
		}
	}

	public static void CheckInit()
	{
		if (_BodyPartTypeTable == null)
		{
			Loading.LoadTask("Loading Bodies.xml", Init);
		}
	}

	private static void Init()
	{
		_BodyPartTypeTable = new Dictionary<string, BodyPartType>(32);
		_BodyPartTypeList = new List<BodyPartType>(32);
		_AnatomyTable = new Dictionary<string, Anatomy>(64);
		_AnatomyList = new List<Anatomy>(64);
		foreach (DataFile item in DataManager.GetXMLFilesWithRoot("Bodies"))
		{
			try
			{
				ProcessBodiesXmlFile(item, item.IsMod);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.Mod, message);
			}
		}
	}

	public static Anatomy GetRandomAnatomy()
	{
		return AnatomyTable.Values.GetRandomElement();
	}

	public static BodyPartType GetBodyPartType(string name)
	{
		if (BodyPartTypeTable.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public static BodyPartType GetBodyPartTypeOrFail(string name)
	{
		if (BodyPartTypeTable.TryGetValue(name, out var value))
		{
			return value;
		}
		throw new Exception("invalid body part type " + name);
	}

	public static Anatomy GetAnatomy(string name)
	{
		if (AnatomyTable.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public static Anatomy GetAnatomyOrFail(string name)
	{
		if (AnatomyTable.TryGetValue(name, out var value))
		{
			return value;
		}
		throw new Exception("invalid anatomy " + name);
	}

	private static void ProcessBodiesXmlFile(string file, bool mod)
	{
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(file);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType == XmlNodeType.Element)
			{
				if (!(xmlTextReader.Name == "bodies"))
				{
					throw new XmlUnsupportedElementException(xmlTextReader);
				}
				LoadBodiesNode(xmlTextReader, mod);
			}
		}
		xmlTextReader.Close();
	}

	public static void LoadBodiesNode(XmlTextReader Reader, bool mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "bodyparttypes")
				{
					LoadBodyPartTypeTableNode(Reader, mod);
					continue;
				}
				if (Reader.Name == "bodyparttypevariants")
				{
					LoadBodyPartTypeVariantsNode(Reader, mod);
					continue;
				}
				if (!(Reader.Name == "anatomies"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadAnatomiesNode(Reader, mod);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadBodyPartTypeTableNode(XmlTextReader Reader, bool mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "bodyparttype")
				{
					LoadBodyPartTypeNode(Reader, mod);
					continue;
				}
				if (!(Reader.Name == "removebodyparttype"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				string attribute = Reader.GetAttribute("Name");
				if (string.IsNullOrEmpty(attribute))
				{
					throw new XmlException("removebodyparttype tag had no Name attribute", Reader);
				}
				_BodyPartTypeTable.Remove(attribute);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	private static void LoadBodyPartTypeNode(XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("Type");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new XmlException(Reader.Name + " tag had no Type attribute", Reader);
		}
		BodyPartType bodyPartType;
		if (_BodyPartTypeTable.ContainsKey(attribute))
		{
			if (!mod)
			{
				Debug.LogError("duplicate bodyparttype " + attribute);
			}
			bodyPartType = _BodyPartTypeTable[attribute];
		}
		else
		{
			bodyPartType = new BodyPartType(attribute);
			_BodyPartTypeTable.Add(bodyPartType.Type, bodyPartType);
			_BodyPartTypeList.Add(bodyPartType);
		}
		LoadBodyPartInfo(bodyPartType, Reader, mod);
	}

	public static void LoadBodyPartTypeVariantsNode(XmlTextReader Reader, bool mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "bodyparttypevariant")
				{
					LoadBodyPartTypeVariantNode(Reader, mod);
					continue;
				}
				if (!(Reader.Name == "removebodyparttypevariant"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				string attribute = Reader.GetAttribute("Name");
				if (string.IsNullOrEmpty(attribute))
				{
					throw new XmlException("removebodyparttypevariant tag had no Name attribute", Reader);
				}
				if (_BodyPartTypeTable.ContainsKey(attribute))
				{
					if (_BodyPartTypeTable[attribute].FinalType == attribute)
					{
						throw new XmlException("cannot remove type " + attribute + " as variant", Reader);
					}
					_BodyPartTypeTable.Remove(attribute);
				}
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	private static void LoadBodyPartTypeVariantNode(XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("VariantOf");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new XmlException(Reader.Name + " tag had no VariantOf attribute", Reader);
		}
		string attribute2 = Reader.GetAttribute("Type");
		if (string.IsNullOrEmpty(attribute2))
		{
			throw new XmlException(Reader.Name + " tag had no Type attribute", Reader);
		}
		BodyPartType bodyPartType;
		if (_BodyPartTypeTable.ContainsKey(attribute2))
		{
			if (!mod)
			{
				Debug.LogError("duplicate bodyparttypevariant " + attribute2);
			}
			bodyPartType = _BodyPartTypeTable[attribute2];
			if (bodyPartType.Type == bodyPartType.FinalType)
			{
				throw new XmlException("cannot modify type " + attribute2 + " as variant", Reader);
			}
		}
		else
		{
			bodyPartType = new BodyPartType(GetBodyPartTypeOrFail(attribute), attribute2);
			bodyPartType.Description = bodyPartType.Type;
			bodyPartType.Name = bodyPartType.Type.ToLower();
			_BodyPartTypeTable.Add(bodyPartType.Type, bodyPartType);
			_BodyPartTypeList.Add(bodyPartType);
		}
		LoadBodyPartInfo(bodyPartType, Reader, mod);
	}

	private static void LoadBodyPartInfo(BodyPartType Entry, XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute != null)
		{
			Entry.Name = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("Description");
		if (attribute != null)
		{
			Entry.Description = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("DescriptionPrefix");
		if (attribute != null)
		{
			Entry.DescriptionPrefix = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("DefaultBehavior");
		if (attribute != null)
		{
			Entry.DefaultBehavior = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("UsuallyOn");
		if (attribute != null)
		{
			Entry.UsuallyOn = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("UsuallyOnVariant");
		if (attribute != null)
		{
			Entry.UsuallyOnVariant = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("ImpliedBy");
		if (attribute != null)
		{
			Entry.ImpliedBy = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("RequiresType");
		if (attribute != null)
		{
			Entry.RequiresType = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("LimbBlueprintProperty");
		if (attribute != null)
		{
			Entry.LimbBlueprintProperty = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("LimbBlueprintDefault");
		if (attribute != null)
		{
			Entry.LimbBlueprintDefault = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("EquipSound");
		if (attribute != null)
		{
			Entry.EquipSound = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("UnequipSound");
		if (attribute != null)
		{
			Entry.UnequipSound = ((attribute == "") ? null : attribute);
		}
		attribute = Reader.GetAttribute("Category");
		if (attribute != null)
		{
			Entry.Category = ((attribute == "") ? ((int?)null) : new int?(BodyPartCategory.GetCode(attribute)));
		}
		attribute = Reader.GetAttribute("Laterality");
		if (attribute != null)
		{
			Entry.Laterality = ((attribute == "") ? ((int?)null) : new int?(Laterality.GetCode(attribute)));
		}
		attribute = Reader.GetAttribute("ImpliedPer");
		if (attribute != null)
		{
			Entry.ImpliedPer = ((attribute == "") ? ((int?)null) : new int?(TryInt(attribute, "ImpliedPer attribute")));
		}
		attribute = Reader.GetAttribute("RequiresLaterality");
		if (attribute != null)
		{
			Entry.RequiresLaterality = ((attribute == "") ? ((int?)null) : new int?(Laterality.GetCode(attribute)));
		}
		attribute = Reader.GetAttribute("Mobility");
		if (attribute != null)
		{
			Entry.Mobility = ((attribute == "") ? ((int?)null) : new int?(TryInt(attribute, "Mobility attribute")));
		}
		attribute = Reader.GetAttribute("ChimeraWeight");
		if (attribute != null)
		{
			Entry.ChimeraWeight = ((attribute == "") ? ((int?)null) : new int?(TryInt(attribute, "ChimeraWeight attribute")));
		}
		attribute = Reader.GetAttribute("Appendage");
		if (attribute != null)
		{
			Entry.Appendage = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Integral");
		if (attribute != null)
		{
			Entry.Integral = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Abstract");
		if (attribute != null)
		{
			Entry.Abstract = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Mortal");
		if (attribute != null)
		{
			Entry.Mortal = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Extrinsic");
		if (attribute != null)
		{
			Entry.Extrinsic = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Plural");
		if (attribute != null)
		{
			Entry.Plural = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Mass");
		if (attribute != null)
		{
			Entry.Mass = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Contact");
		if (attribute != null)
		{
			Entry.Contact = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("IgnorePosition");
		if (attribute != null)
		{
			Entry.IgnorePosition = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("NoArmorAveraging");
		if (attribute != null)
		{
			Entry.NoArmorAveraging = ((attribute == "") ? ((bool?)null) : new bool?(Convert.ToBoolean(attribute)));
		}
		attribute = Reader.GetAttribute("Branching");
		if (string.IsNullOrEmpty(attribute))
		{
			return;
		}
		string[] array = attribute.Split(',');
		int[] array2 = new int[array.Length];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			try
			{
				array2[i] = Laterality.GetAxisCode(array[i]);
			}
			catch (Exception ex)
			{
				throw new XmlException(ex.Message, Reader);
			}
		}
		Entry.Branching = array2;
	}

	public static void LoadAnatomiesNode(XmlTextReader Reader, bool mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "anatomy"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadAnatomyNode(Reader, mod);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadAnatomyNode(XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new XmlException(Reader.Name + " tag had no Name attribute", Reader);
		}
		Anatomy anatomy;
		if (_AnatomyTable.ContainsKey(attribute))
		{
			if (!mod)
			{
				Debug.LogError("duplicate anatomy " + attribute);
			}
			anatomy = _AnatomyTable[attribute];
		}
		else
		{
			anatomy = new Anatomy(attribute);
			_AnatomyTable.Add(attribute, anatomy);
			_AnatomyList.Add(anatomy);
		}
		string attribute2 = Reader.GetAttribute("Category");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomy.Category = BodyPartCategory.GetCode(attribute2);
		}
		attribute2 = Reader.GetAttribute("BodyType");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomy.BodyType = attribute2;
		}
		attribute2 = Reader.GetAttribute("BodyCategory");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomy.BodyCategory = BodyPartCategory.GetCode(attribute2);
		}
		attribute2 = Reader.GetAttribute("BodyMobility");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomy.BodyMobility = TryInt(attribute2, "BodyMobility attribute");
		}
		attribute2 = Reader.GetAttribute("ThrownWeapon");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomy.ThrownWeapon = attribute2;
		}
		attribute2 = Reader.GetAttribute("FloatingNearby");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomy.FloatingNearby = attribute2;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "part"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadAnatomyPartNode(anatomy, null, Reader, mod);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	public static void LoadAnatomyPartNode(Anatomy anatomy, AnatomyPart parentPart, XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("Type");
		if (string.IsNullOrEmpty(attribute))
		{
			throw new XmlException(Reader.Name + " tag had no Type attribute", Reader);
		}
		AnatomyPart anatomyPart = new AnatomyPart(GetBodyPartType(attribute) ?? throw new XmlException("unknown bodyparttype " + attribute, Reader));
		string attribute2 = Reader.GetAttribute("SupportsDependent");
		if (attribute2 != null)
		{
			anatomyPart.SupportsDependent = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("DependsOn");
		if (attribute2 != null)
		{
			anatomyPart.DependsOn = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("RequiresType");
		if (attribute2 != null)
		{
			anatomyPart.DependsOn = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("DefaultBehavior");
		if (attribute2 != null)
		{
			anatomyPart.DefaultBehavior = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("Category");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Category = BodyPartCategory.GetCode(attribute2);
		}
		attribute2 = Reader.GetAttribute("Laterality");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Laterality = Laterality.GetCode(attribute2);
		}
		attribute2 = Reader.GetAttribute("RequiresLaterality");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.RequiresLaterality = Laterality.GetCode(attribute2);
		}
		attribute2 = Reader.GetAttribute("Mobility");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Mobility = TryInt(attribute2, "Mobility attribute");
		}
		attribute2 = Reader.GetAttribute("Integral");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Integral = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Mortal");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Mortal = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Abstract");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Abstract = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Extrinsic");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Extrinsic = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Plural");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Plural = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Mass");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Mass = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Contact");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.Contact = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("IgnorePosition");
		if (!string.IsNullOrEmpty(attribute2))
		{
			anatomyPart.IgnorePosition = Convert.ToBoolean(attribute2);
		}
		if (parentPart != null)
		{
			parentPart.Subparts.Add(anatomyPart);
		}
		else
		{
			anatomy.Parts.Add(anatomyPart);
		}
		if (Reader.IsEmptyElement)
		{
			return;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (!(Reader.Name == "part"))
				{
					throw new XmlUnsupportedElementException(Reader);
				}
				LoadAnatomyPartNode(anatomy, anatomyPart, Reader, mod);
			}
			else if (Reader.NodeType == XmlNodeType.EndElement)
			{
				break;
			}
		}
	}

	private static int TryInt(string Spec, string What)
	{
		try
		{
			return Convert.ToInt32(Spec);
		}
		catch
		{
			Debug.LogError("Error in " + What + ": " + Spec);
		}
		return 0;
	}

	public static Dictionary<BodyPartType, int> GetBodyPartTypeSelector(bool IncludeVariants = true, bool? RequireAppendage = true, bool? RequireAbstract = false, bool RequireLiveCategory = true, int[] IncludeCategories = null, int[] ExcludeCategories = null, bool UseChimeraWeight = false)
	{
		Dictionary<BodyPartType, int> dictionary = new Dictionary<BodyPartType, int>();
		int i = 0;
		for (int count = BodyPartTypeList.Count; i < count; i++)
		{
			BodyPartType bodyPartType = BodyPartTypeList[i];
			if ((IncludeVariants || !(bodyPartType.FinalType != bodyPartType.Type)) && (!RequireAppendage.HasValue || bodyPartType.Appendage == true == RequireAppendage) && (!RequireAbstract.HasValue || bodyPartType.Abstract == true == RequireAbstract) && (!RequireLiveCategory || BodyPartCategory.IsLiveCategory(bodyPartType.Category ?? 1)) && (IncludeCategories == null || Array.IndexOf(IncludeCategories, bodyPartType.Category ?? 1) != -1) && (ExcludeCategories == null || Array.IndexOf(ExcludeCategories, bodyPartType.Category ?? 1) != -1))
			{
				int num = ((!UseChimeraWeight || !bodyPartType.ChimeraWeight.HasValue) ? 1 : (bodyPartType.ChimeraWeight ?? 1));
				if (num > 0)
				{
					dictionary.Add(bodyPartType, num);
				}
			}
		}
		if (IncludeVariants)
		{
			Dictionary<BodyPartType, int> dictionary2 = new Dictionary<BodyPartType, int>(dictionary);
			foreach (BodyPartType key in dictionary2.Keys)
			{
				if (!(key.FinalType == key.Type))
				{
					continue;
				}
				foreach (KeyValuePair<BodyPartType, int> item in dictionary2)
				{
					if (item.Key != key && item.Key.FinalType == key.Type)
					{
						dictionary[key] += item.Value;
					}
				}
			}
		}
		return dictionary;
	}

	public static BodyPartType GetRandomBodyPartType(bool IncludeVariants = true, bool? RequireAppendage = true, bool? RequireAbstract = false, bool RequireLiveCategory = true, int[] IncludeCategories = null, int[] ExcludeCategories = null, bool UseChimeraWeight = false)
	{
		return GetBodyPartTypeSelector(IncludeVariants, RequireAppendage, RequireAbstract, RequireLiveCategory, IncludeCategories, ExcludeCategories, UseChimeraWeight).GetRandomElement();
	}

	public static List<BodyPartType> FindUsualChildBodyPartTypes(BodyPartType ParentType)
	{
		List<BodyPartType> list = null;
		int i = 0;
		for (int count = BodyPartTypeList.Count; i < count; i++)
		{
			BodyPartType bodyPartType = BodyPartTypeList[i];
			bool flag = false;
			if (!string.IsNullOrEmpty(bodyPartType.UsuallyOnVariant))
			{
				if (bodyPartType.UsuallyOnVariant == ParentType.Type && bodyPartType.Category == ParentType.Category)
				{
					flag = true;
				}
			}
			else if (!string.IsNullOrEmpty(bodyPartType.UsuallyOn) && bodyPartType.UsuallyOn == ParentType.Type && bodyPartType.Category == ParentType.Category)
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			if (list == null)
			{
				list = new List<BodyPartType> { bodyPartType };
				continue;
			}
			bool flag2 = false;
			foreach (BodyPartType item in list)
			{
				if (item.FinalType == bodyPartType.FinalType)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				list.Add(bodyPartType);
			}
		}
		return list;
	}
}
