using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XRL.World.Loaders;

public class ObjectBlueprintLoader
{
	private enum NodeType
	{
		Baked,
		Named,
		Unnamed
	}

	private struct KnownNode(string Key, string Attribute, NodeType Type)
	{
		public string Key = Key;

		public string Attribute = Attribute;

		public NodeType Type = Type;
	}

	private struct RemovalNode(KnownNode Node)
	{
		public string Key = "remove" + Node.Key;

		public string Target = Node.Key;

		public string Attribute = Node.Attribute;

		public NodeType Type = Node.Type;
	}

	private struct Mixin : IComparable<Mixin>
	{
		public string Name;

		public string Include;

		public string Exclude;

		public int Priority;

		public bool Fill;

		public int CompareTo(Mixin Other)
		{
			return Priority.CompareTo(Other.Priority);
		}
	}

	public class ObjectBlueprintXMLChildNode
	{
		public string NodeName;

		public Dictionary<string, string> Attributes = new Dictionary<string, string>();

		private static Dictionary<string, string> _tagToNamespace = new Dictionary<string, string>
		{
			{ "part", "XRL.World.Parts" },
			{ "mutation", "XRL.World.Parts.Mutation" },
			{ "skill", "XRL.World.Parts.Skill" },
			{ "builder", "XRL.World.ObjectBuilders" }
		};

		private static Dictionary<string, Func<string, string>> _compatProcessors = new Dictionary<string, Func<string, string>>
		{
			{
				"skill",
				CompatManager.ProcessSkill
			},
			{
				"mutation",
				CompatManager.ProcessMutation
			}
		};

		public string Name
		{
			get
			{
				if (Attributes.ContainsKey("Name"))
				{
					return Attributes["Name"];
				}
				return null;
			}
		}

		public ObjectBlueprintXMLChildNode(string NodeName)
		{
			this.NodeName = NodeName;
		}

		public bool HasAttribute(string name)
		{
			return Attributes.ContainsKey(name);
		}

		public string GetAttribute(string name)
		{
			if (Attributes.TryGetValue(name, out var value))
			{
				return value;
			}
			return null;
		}

		public void Merge(ObjectBlueprintXMLChildNode other)
		{
			NodeName = other.NodeName;
			foreach (KeyValuePair<string, string> attribute in other.Attributes)
			{
				Attributes[attribute.Key] = attribute.Value;
			}
		}

		public ObjectBlueprintXMLChildNode Clone()
		{
			ObjectBlueprintXMLChildNode objectBlueprintXMLChildNode = new ObjectBlueprintXMLChildNode(NodeName);
			foreach (KeyValuePair<string, string> attribute in Attributes)
			{
				objectBlueprintXMLChildNode.Attributes[attribute.Key] = attribute.Value;
			}
			return objectBlueprintXMLChildNode;
		}

		public static ObjectBlueprintXMLChildNode ReadChildNode(XmlTextReader reader)
		{
			ObjectBlueprintXMLChildNode objectBlueprintXMLChildNode = new ObjectBlueprintXMLChildNode(reader.Name);
			if (reader.HasAttributes)
			{
				string text = DataManager.SanitizePathForDisplay(reader.BaseURI);
				string text2 = null;
				string value = null;
				_tagToNamespace.TryGetValue(objectBlueprintXMLChildNode.NodeName, out value);
				GamePartBlueprint.PartReflectionCache partReflectionCache = null;
				if (value != null)
				{
					text2 = reader.GetAttribute("Name");
					if (_compatProcessors.TryGetValue(objectBlueprintXMLChildNode.NodeName, out var value2))
					{
						string text3 = value2(text2);
						if (text3 != text2)
						{
							handleWarning(new Exception($"File: {text}, Line: {reader.LineNumber}:{reader.LinePosition} Compat Warning: {objectBlueprintXMLChildNode.NodeName} Name \"{text2}\" has been renamed \"{text3}\"."));
							text2 = text3;
						}
					}
					partReflectionCache = GamePartBlueprint.PartReflectionCache.Get(ModManager.ResolveType(value, text2));
					if (partReflectionCache == null)
					{
						handleError(new Exception($"File: {text}, Line: {reader.LineNumber}:{reader.LinePosition} Could not find {value}.{text2}, element ignored."));
						return objectBlueprintXMLChildNode;
					}
				}
				reader.MoveToFirstAttribute();
				do
				{
					if (partReflectionCache != null)
					{
						if (typeof(IPart).IsAssignableFrom(partReflectionCache.T) && reader.Name == "Builder")
						{
							if (!typeof(IPartBuilder).IsAssignableFrom(ModManager.ResolveType("XRL.World.PartBuilders", reader.Value)))
							{
								handleError(new Exception($"File: {text}, Line: {reader.LineNumber}:{reader.LinePosition} Could not find IPartBuilder {reader.Value}."));
								continue;
							}
						}
						else if (reader.Name != "Name" && reader.Name != "ChanceIn10000" && reader.Name != "ChanceOneIn")
						{
							var (type, _, obsoleteAttribute) = partReflectionCache.GetPropertyOrField(reader.Name);
							if (obsoleteAttribute != null)
							{
								handleError(new Exception($"File: {text}, Line: {reader.LineNumber}:{reader.LinePosition} {reader.Name} is obsolete: {obsoleteAttribute.Message}"));
							}
							if (type == null)
							{
								handleError(new MissingMemberException($"File: {text}, Line: {reader.LineNumber}:{reader.LinePosition} No {partReflectionCache.T.FullName}.{reader.Name} property exists."));
								continue;
							}
							try
							{
								XmlDataHelper.TryGetAttributeParser(type)?._Parse(reader.Value);
							}
							catch (Exception innerException)
							{
								handleError(new Exception($"File: {text}, Line: {reader.LineNumber}:{reader.LinePosition} Error parsing {reader.Name}=\"{reader.Value}\" as {type}", innerException));
							}
						}
					}
					string str = ((reader.Name == "Name") ? text2 : null) ?? reader.Value;
					objectBlueprintXMLChildNode.Attributes[string.Intern(reader.Name)] = string.Intern(str);
				}
				while (reader.MoveToNextAttribute());
				reader.MoveToElement();
			}
			if (reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement)
			{
				return objectBlueprintXMLChildNode;
			}
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.EndElement)
				{
					if (reader.Name != "" && reader.Name != objectBlueprintXMLChildNode.NodeName)
					{
						throw new Exception("Unexpected end node for " + reader.Name);
					}
					return objectBlueprintXMLChildNode;
				}
			}
			return objectBlueprintXMLChildNode;
		}
	}

	public class ObjectBlueprintXMLChildNodeCollection
	{
		public Dictionary<string, ObjectBlueprintXMLChildNode> Named;

		public List<ObjectBlueprintXMLChildNode> Unnamed;

		public void Add(ObjectBlueprintXMLChildNode node, XmlTextReader reader)
		{
			if (string.IsNullOrEmpty(node.Name))
			{
				if (Unnamed == null)
				{
					Unnamed = new List<ObjectBlueprintXMLChildNode>(1);
				}
				Unnamed.Add(node);
				return;
			}
			if (Named == null)
			{
				Named = new Dictionary<string, ObjectBlueprintXMLChildNode>(1);
			}
			if (Named.ContainsKey(node.Name) && reader != null)
			{
				handleError($"{DataManager.SanitizePathForDisplay(reader.BaseURI)}: Duplicate {node.NodeName} Name='{node.Name}' found at line {reader.LineNumber}");
				Named[node.Name].Merge(node);
			}
			else
			{
				Named[node.Name] = node;
			}
		}

		public ObjectBlueprintXMLChildNodeCollection Clone()
		{
			ObjectBlueprintXMLChildNodeCollection objectBlueprintXMLChildNodeCollection = new ObjectBlueprintXMLChildNodeCollection();
			if (Named != null)
			{
				objectBlueprintXMLChildNodeCollection.Named = new Dictionary<string, ObjectBlueprintXMLChildNode>(Named.Count);
				foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in Named)
				{
					objectBlueprintXMLChildNodeCollection.Named[item.Key] = item.Value.Clone();
				}
			}
			if (Unnamed != null)
			{
				objectBlueprintXMLChildNodeCollection.Unnamed = new List<ObjectBlueprintXMLChildNode>(Unnamed.Count);
				foreach (ObjectBlueprintXMLChildNode item2 in Unnamed)
				{
					objectBlueprintXMLChildNodeCollection.Unnamed.Add(item2.Clone());
				}
			}
			return objectBlueprintXMLChildNodeCollection;
		}

		public override string ToString()
		{
			string text = "";
			if (Named != null)
			{
				text = text + "Named: " + Named.Count + "\n";
				foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in Named)
				{
					text = text + "  [" + item.Key + " ";
					foreach (KeyValuePair<string, string> attribute in item.Value.Attributes)
					{
						text = text + attribute.Key + "=\"" + attribute.Value + "\"";
					}
					text += "]\n";
				}
			}
			if (Unnamed != null)
			{
				text = text + "Unnamed: " + Unnamed.Count + "\n";
				foreach (ObjectBlueprintXMLChildNode item2 in Unnamed)
				{
					text += "  [";
					foreach (KeyValuePair<string, string> attribute2 in item2.Attributes)
					{
						text = text + attribute2.Key + "=\"" + attribute2.Value + "\"";
					}
					text += "]\n";
				}
			}
			return text;
		}

		public void Merge(ObjectBlueprintXMLChildNodeCollection other)
		{
			if (other.Named != null)
			{
				if (Named == null)
				{
					Named = other.Named;
				}
				else
				{
					foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in other.Named)
					{
						if (Named.TryGetValue(item.Key, out var value))
						{
							value.Merge(item.Value);
						}
						else
						{
							Add(item.Value, null);
						}
					}
				}
			}
			if (other.Unnamed != null)
			{
				if (Unnamed == null)
				{
					Unnamed = other.Unnamed;
				}
				else
				{
					Unnamed.AddRange(other.Unnamed);
				}
			}
		}
	}

	public class ObjectBlueprintXMLData
	{
		public string Name;

		public string PreviousName;

		public string Inherits;

		public string Load;

		public ModInfo Mod;

		public Dictionary<string, ObjectBlueprintXMLChildNodeCollection> Children = new Dictionary<string, ObjectBlueprintXMLChildNodeCollection>();

		public IEnumerable<KeyValuePair<string, ObjectBlueprintXMLChildNode>> NamedNodes(string type)
		{
			if (Children.ContainsKey(type))
			{
				if (Children[type].Unnamed != null)
				{
					MetricsManager.LogWarning("Unnamed " + type + " nodes detected in " + Name);
				}
				return Children[type].Named.AsEnumerable();
			}
			return Enumerable.Empty<KeyValuePair<string, ObjectBlueprintXMLChildNode>>();
		}

		public IEnumerable<ObjectBlueprintXMLChildNode> UnnamedNodes(string type)
		{
			if (Children.ContainsKey(type))
			{
				if (Children[type].Named != null)
				{
					MetricsManager.LogWarning("Named " + type + " nodes detected in " + Name);
				}
				return Children[type].Unnamed.AsEnumerable();
			}
			return Enumerable.Empty<ObjectBlueprintXMLChildNode>();
		}

		public void Merge(ObjectBlueprintXMLData other)
		{
			if (!string.IsNullOrEmpty(other.Inherits))
			{
				Inherits = other.Inherits;
			}
			foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in other.Children)
			{
				if (Children.TryGetValue(child.Key, out var value))
				{
					value.Merge(child.Value);
				}
				else
				{
					Children[child.Key] = child.Value;
				}
			}
			if (other.Mod != null)
			{
				Mod = other.Mod;
			}
		}

		public static ObjectBlueprintXMLData ReadObjectNode(XmlTextReader reader)
		{
			ObjectBlueprintXMLData objectBlueprintXMLData = new ObjectBlueprintXMLData();
			int num = 0;
			objectBlueprintXMLData.Name = reader.GetAttribute("Name");
			objectBlueprintXMLData.PreviousName = reader.GetAttribute("PreviousName");
			objectBlueprintXMLData.Inherits = reader.GetAttribute("Inherits");
			objectBlueprintXMLData.Load = reader.GetAttribute("Load");
			if (reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement)
			{
				return objectBlueprintXMLData;
			}
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Comment || reader.NodeType == XmlNodeType.Text)
				{
					continue;
				}
				if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "object")
				{
					return objectBlueprintXMLData;
				}
				if (reader.NodeType == XmlNodeType.Element)
				{
					string name = reader.Name;
					if (!KnownNodes.ContainsKey(name) && !name.StartsWith("xtag"))
					{
						handleError($"{DataManager.SanitizePathForDisplay(reader.BaseURI)}: Unknown object element {reader.Name} at line {reader.LineNumber}");
					}
					ObjectBlueprintXMLChildNode objectBlueprintXMLChildNode = ObjectBlueprintXMLChildNode.ReadChildNode(reader);
					if (!objectBlueprintXMLData.Children.ContainsKey(name))
					{
						objectBlueprintXMLData.Children[name] = new ObjectBlueprintXMLChildNodeCollection();
					}
					if (name.EqualsNoCase("mixin") && objectBlueprintXMLChildNode.Attributes.TryAdd("Priority", num.ToString()))
					{
						num++;
					}
					objectBlueprintXMLData.Children[name].Add(objectBlueprintXMLChildNode, reader);
				}
				else
				{
					handleError($"{DataManager.SanitizePathForDisplay(reader.BaseURI)}: Unknown problem reading object: {reader.NodeType}");
				}
			}
			return objectBlueprintXMLData;
		}

		public override string ToString()
		{
			string text = "[ObjectBlueprintXMLData " + Name + " Inherits=" + Inherits + " Load=" + Load + "]\n";
			foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in Children)
			{
				text = text + "[" + child.Key + "]\n";
				text += child.Value;
			}
			return text;
		}
	}

	private Dictionary<string, ObjectBlueprintXMLData> Objects = new Dictionary<string, ObjectBlueprintXMLData>();

	private Dictionary<string, ObjectBlueprintXMLData> Finalized = new Dictionary<string, ObjectBlueprintXMLData>();

	private List<string> BakeStack = new List<string>();

	private Stack<List<Mixin>> BakeMixins = new Stack<List<Mixin>>();

	private static List<RemovalNode> RemovalNodes;

	private static List<string> ExhaustedRemovals;

	protected static Action<object> handleError;

	protected static Action<object> handleWarning;

	private static Dictionary<string, KnownNode> KnownNodes;

	static ObjectBlueprintLoader()
	{
		ExhaustedRemovals = new List<string>();
		handleError = MetricsManager.LogError;
		handleWarning = MetricsManager.LogWarning;
		KnownNodes = new Dictionary<string, KnownNode>
		{
			{
				"builder",
				new KnownNode("builder", "Name", NodeType.Named)
			},
			{
				"intproperty",
				new KnownNode("intproperty", "Name", NodeType.Named)
			},
			{
				"inventoryobject",
				new KnownNode("inventoryobject", "Blueprint", NodeType.Unnamed)
			},
			{
				"mutation",
				new KnownNode("mutation", "Name", NodeType.Named)
			},
			{
				"part",
				new KnownNode("part", "Name", NodeType.Named)
			},
			{
				"mixin",
				new KnownNode("mixin", "Name", NodeType.Baked)
			},
			{
				"property",
				new KnownNode("property", "Name", NodeType.Named)
			},
			{
				"skill",
				new KnownNode("skill", "Name", NodeType.Named)
			},
			{
				"stag",
				new KnownNode("stag", "Name", NodeType.Named)
			},
			{
				"stat",
				new KnownNode("stat", "Name", NodeType.Named)
			},
			{
				"tag",
				new KnownNode("tag", "Name", NodeType.Named)
			}
		};
		RemovalNodes = new List<RemovalNode>(KnownNodes.Count);
		foreach (var (_, node) in KnownNodes)
		{
			if (node.Type != NodeType.Baked)
			{
				RemovalNodes.Add(new RemovalNode(node));
			}
		}
		foreach (RemovalNode removalNode in RemovalNodes)
		{
			KnownNodes.Add(removalNode.Key, new KnownNode(removalNode.Key, removalNode.Attribute, NodeType.Baked));
		}
	}

	private ObjectBlueprintXMLData Bake(ObjectBlueprintXMLData obj)
	{
		if (Finalized.TryGetValue(obj.Name, out var value))
		{
			return value;
		}
		if (BakeStack.Contains(obj.Name))
		{
			MetricsManager.LogPotentialModError(obj.Mod, "blueprint " + obj.Name + " seems to have an inheritance loop. " + BakeStack.Aggregate("", (string a, string b) => a + " -> " + b) + " -> " + obj.Name);
			return new ObjectBlueprintXMLData();
		}
		BakeStack.Add(obj.Name);
		List<Mixin> list = null;
		ObjectBlueprintXMLData objectBlueprintXMLData = new ObjectBlueprintXMLData();
		objectBlueprintXMLData.Name = obj.Name;
		objectBlueprintXMLData.PreviousName = obj.PreviousName;
		objectBlueprintXMLData.Inherits = obj.Inherits;
		objectBlueprintXMLData.Mod = obj.Mod;
		if (obj.Children.TryGetValue("mixin", out var value2))
		{
			list = ((BakeMixins.Count > 0) ? BakeMixins.Pop() : new List<Mixin>(value2.Named.Count));
			list.Clear();
			foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item2 in value2.Named)
			{
				Mixin mixin = new Mixin
				{
					Name = item2.Key
				};
				Dictionary<string, string> attributes = item2.Value.Attributes;
				mixin.Include = ((attributes != null) ? attributes.GetValue("Include") : null);
				Dictionary<string, string> attributes2 = item2.Value.Attributes;
				mixin.Exclude = ((attributes2 != null) ? attributes2.GetValue("Exclude") : null);
				Mixin item = mixin;
				Dictionary<string, string> attributes3 = item2.Value.Attributes;
				string text = ((attributes3 != null) ? attributes3.GetValue("Priority") : null);
				if (!text.IsNullOrEmpty() && int.TryParse(text, out var result))
				{
					item.Priority = result;
				}
				Dictionary<string, string> attributes4 = item2.Value.Attributes;
				item.Fill = ((attributes4 != null) ? attributes4.GetValue("Load") : null) == "Fill";
				list.Add(item);
			}
			list.Sort();
			foreach (Mixin item3 in list)
			{
				if (item3.Fill)
				{
					Inherit(item3.Name, obj, objectBlueprintXMLData, item3.Include, item3.Exclude);
				}
			}
		}
		if (!string.IsNullOrEmpty(obj.Inherits))
		{
			Inherit(obj.Inherits, obj, objectBlueprintXMLData);
		}
		if (list != null)
		{
			foreach (Mixin item4 in list)
			{
				if (!item4.Fill)
				{
					Inherit(item4.Name, obj, objectBlueprintXMLData, item4.Include, item4.Exclude);
				}
			}
			list.Clear();
			BakeMixins.Push(list);
		}
		foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in obj.Children)
		{
			if (child.Key == "mixin")
			{
				continue;
			}
			if (objectBlueprintXMLData.Children.TryGetValue(child.Key, out var value3))
			{
				if (child.Key.StartsWith("xtag"))
				{
					value3.Unnamed[0].Merge(child.Value.Unnamed[0]);
				}
				else
				{
					value3.Merge(child.Value.Clone());
				}
			}
			else
			{
				objectBlueprintXMLData.Children[child.Key] = child.Value.Clone();
			}
		}
		ProcessTagRemoval(objectBlueprintXMLData);
		Finalized.Add(obj.Name, objectBlueprintXMLData);
		BakeStack.Remove(obj.Name);
		return objectBlueprintXMLData;
	}

	private void ProcessTagRemoval(ObjectBlueprintXMLData Result)
	{
		foreach (RemovalNode removalNode in RemovalNodes)
		{
			if (removalNode.Type == NodeType.Named)
			{
				ProcessNamedRemoval(Result, removalNode.Target, removalNode.Key);
			}
			else
			{
				ProcessUnnamedRemoval(Result, removalNode.Target, removalNode.Key, removalNode.Attribute);
			}
		}
	}

	private void ProcessNamedRemoval(ObjectBlueprintXMLData Result, string CollectionKey, string RemovalKey)
	{
		if (!Result.Children.TryGetValue(RemovalKey, out var value) || value.Named == null || !Result.Children.TryGetValue(CollectionKey, out var value2) || value2.Named == null)
		{
			return;
		}
		bool flag = true;
		ExhaustedRemovals.Clear();
		foreach (var (text2, objectBlueprintXMLChildNode2) in value.Named)
		{
			value2.Named.Remove(text2);
			string attribute = objectBlueprintXMLChildNode2.GetAttribute("Depth");
			if (!attribute.IsNullOrEmpty() && int.TryParse(attribute, out var result) && result >= 1)
			{
				flag = false;
				if (result == 1)
				{
					objectBlueprintXMLChildNode2.Attributes.Remove("Depth");
				}
				else
				{
					objectBlueprintXMLChildNode2.Attributes["Depth"] = (result - 1).ToStringCached();
				}
			}
			else
			{
				ExhaustedRemovals.Add(text2);
			}
		}
		if (flag)
		{
			Result.Children.Remove(RemovalKey);
			return;
		}
		foreach (string exhaustedRemoval in ExhaustedRemovals)
		{
			value.Named.Remove(exhaustedRemoval);
		}
	}

	private void ProcessUnnamedRemoval(ObjectBlueprintXMLData Result, string CollectionKey, string RemovalKey, string AttributeKey)
	{
		if (!Result.Children.TryGetValue(RemovalKey, out var value) || value.Unnamed == null || !Result.Children.TryGetValue(CollectionKey, out var value2) || value2.Unnamed == null)
		{
			return;
		}
		foreach (ObjectBlueprintXMLChildNode item in value.Unnamed)
		{
			string attribute = item.GetAttribute(AttributeKey);
			bool flag = item.GetAttribute("All").EqualsNoCase("true");
			if (attribute.IsNullOrEmpty())
			{
				continue;
			}
			for (int num = value2.Unnamed.Count - 1; num >= 0; num--)
			{
				if (!(attribute != value2.Unnamed[num].GetAttribute(AttributeKey)))
				{
					value2.Unnamed.RemoveAt(num);
					if (!flag)
					{
						break;
					}
				}
			}
		}
		Result.Children.Remove(RemovalKey);
	}

	private void Inherit(string Name, ObjectBlueprintXMLData Object, ObjectBlueprintXMLData Result, string Include = null, string Exclude = null)
	{
		if (CompatManager.TryGetCompatEntry("blueprint", Name, out var NewID))
		{
			MetricsManager.LogPotentialModError(Object.Mod, "blueprint \"Name\" inherited by \"" + Object.Name + "\" was reanamed \"" + NewID + "\"");
			Name = NewID;
		}
		if (!Objects.TryGetValue(Name, out var value))
		{
			MetricsManager.LogPotentialModError(Object.Mod, "blueprint \"" + Name + "\" inherited by " + Object.Name + " not found");
			return;
		}
		foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in Bake(value).Children)
		{
			if ((!Include.IsNullOrEmpty() && !Include.HasDelimitedSubstring(',', child.Key)) || (!Exclude.IsNullOrEmpty() && Exclude.HasDelimitedSubstring(',', child.Key)))
			{
				continue;
			}
			if (Result.Children.TryGetValue(child.Key, out var value2))
			{
				if (child.Key.StartsWith("xtag"))
				{
					value2.Unnamed[0].Merge(child.Value.Unnamed[0]);
				}
				else
				{
					value2.Merge(child.Value.Clone());
				}
			}
			else
			{
				Result.Children[child.Key] = child.Value.Clone();
			}
		}
		if (!Result.Children.TryGetValue("tag", out var value3))
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in value3.Named)
		{
			if (item.Value.Attributes.ContainsValue("*noinherit"))
			{
				list.Add(item.Key);
			}
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			value3.Named.Remove(list[i]);
		}
	}

	public IEnumerable<ObjectBlueprintXMLData> BakedBlueprints()
	{
		foreach (string key in Objects.Keys)
		{
			yield return Bake(Objects[key]);
		}
	}

	public int ReadObjectsNode(XmlTextReader reader, ModInfo modInfo = null)
	{
		int num = 0;
		while (reader.Read())
		{
			if (reader.Name == "object")
			{
				ObjectBlueprintXMLData objectBlueprintXMLData = ObjectBlueprintXMLData.ReadObjectNode(reader);
				ProcessNodeCompat(objectBlueprintXMLData);
				objectBlueprintXMLData.Mod = modInfo;
				num++;
				if (objectBlueprintXMLData.Load == "Merge")
				{
					string text = objectBlueprintXMLData.Name;
					if (CompatManager.TryGetCompatEntry("blueprint", text, out var NewID))
					{
						handleWarning($"File: {DataManager.SanitizePathForDisplay(reader.BaseURI)}, Line: {reader.LineNumber}:{reader.LinePosition} Attempt to merge with {text} which has a new name \"{NewID}\"");
						text = NewID;
					}
					if (!Objects.TryGetValue(text, out var value))
					{
						handleError($"File: {DataManager.SanitizePathForDisplay(reader.BaseURI)}, Line: {reader.LineNumber}:{reader.LinePosition} Attempt to merge with {text} which is an unknown blueprint, node discarded");
					}
					else
					{
						value.Merge(objectBlueprintXMLData);
					}
				}
				else if (objectBlueprintXMLData.Load == "MergeIfExists")
				{
					string text2 = objectBlueprintXMLData.Name;
					if (CompatManager.TryGetCompatEntry("blueprint", text2, out var NewID2))
					{
						handleWarning($"File: {DataManager.SanitizePathForDisplay(reader.BaseURI)}, Line: {reader.LineNumber}:{reader.LinePosition} Attempt to merge with {text2} which has a new name \"{NewID2}\"");
						text2 = NewID2;
					}
					if (Objects.TryGetValue(text2, out var value2))
					{
						value2.Merge(objectBlueprintXMLData);
					}
				}
				else
				{
					Objects[objectBlueprintXMLData.Name] = objectBlueprintXMLData;
				}
			}
			else if (reader.NodeType != XmlNodeType.Comment)
			{
				if (reader.Name == "objects" && reader.NodeType == XmlNodeType.EndElement)
				{
					return num;
				}
				throw new Exception("Unknown node '" + reader.Name + "'");
			}
		}
		return num;
	}

	private void ProcessNodeCompat(ObjectBlueprintXMLData Node)
	{
		if (Node.Children.TryGetValue("tag", out var value) && value.Named != null && value.Named.Remove("NoCombat", out var _))
		{
			if (!Node.Children.TryGetValue("removepart", out var value3))
			{
				value3 = (Node.Children["removepart"] = new ObjectBlueprintXMLChildNodeCollection());
			}
			ObjectBlueprintXMLChildNodeCollection objectBlueprintXMLChildNodeCollection2 = value3;
			if (objectBlueprintXMLChildNodeCollection2.Named == null)
			{
				objectBlueprintXMLChildNodeCollection2.Named = new Dictionary<string, ObjectBlueprintXMLChildNode>();
			}
			if (!value3.Named.ContainsKey("Combat"))
			{
				ObjectBlueprintXMLChildNode objectBlueprintXMLChildNode = new ObjectBlueprintXMLChildNode("removepart");
				objectBlueprintXMLChildNode.Attributes["Name"] = "Combat";
				value3.Named["Combat"] = objectBlueprintXMLChildNode;
			}
			if (Node.Children.TryGetValue("part", out var value4))
			{
				value4.Named?.Remove("Combat");
			}
		}
	}

	public void ReadObjectBlueprintsXML(XmlTextReader reader, ModInfo modInfo = null)
	{
		if (handleError == null)
		{
			handleError = MetricsManager.LogError;
		}
		bool flag = false;
		try
		{
			reader.WhitespaceHandling = WhitespaceHandling.None;
			while (reader.Read())
			{
				if (reader.Name == "objects")
				{
					flag = true;
					if (MarkovCorpusGenerator.Generating && reader.GetAttribute("ExcludeFromCorpusGeneration").EqualsNoCase("true"))
					{
						return;
					}
					ReadObjectsNode(reader, modInfo);
				}
			}
		}
		catch (Exception innerException)
		{
			throw new Exception($"File: {DataManager.SanitizePathForDisplay(reader.BaseURI)}, Line: {reader.LineNumber}:{reader.LinePosition}", innerException);
		}
		finally
		{
			reader.Close();
		}
		if (!flag)
		{
			handleError("No <objects> tag found in ObjectBlueprints.XML");
		}
	}

	public void LoadAllBlueprints()
	{
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Objects"))
		{
			if (item.modInfo != null)
			{
				handleError = item.modInfo.Error;
				handleWarning = item.modInfo.Warn;
			}
			else
			{
				handleError = MetricsManager.LogError;
				handleWarning = MetricsManager.LogWarning;
			}
			try
			{
				ReadObjectBlueprintsXML(item, item.modInfo);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.modInfo, message);
			}
		}
		GamePartBlueprint.PartReflectionCache.ClearOrphaned();
	}
}
