using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using XRL.Core;
using XRL.UI;

namespace XRL.World;

public class WorldFactory
{
	public const string ANY_PLANE = "*";

	public static WorldFactory _Factory;

	private List<WorldBlueprint> WorldsScratch = new List<WorldBlueprint>();

	private Dictionary<string, WorldBlueprint> Worlds = new Dictionary<string, WorldBlueprint>();

	private Dictionary<string, CellBlueprint> Cells = new Dictionary<string, CellBlueprint>();

	public Dictionary<string, string> ZoneIDToDisplay = new Dictionary<string, string>();

	public Dictionary<string, string> ZoneDisplayToID = new Dictionary<string, string>();

	public Dictionary<string, string> ZoneIDToDisplayWithIndefiniteArticle = new Dictionary<string, string>();

	public Dictionary<string, string> ZoneDisplayToIDWithIndefiniteArticle = new Dictionary<string, string>();

	public List<IWorldBuilderExtension> WorldBuilderExtensions;

	public WorldBlueprint NewWorld;

	public static WorldFactory Factory => _Factory ?? (_Factory = new WorldFactory());

	public int countWorlds()
	{
		int num = Worlds.Keys.Count;
		if (The.Game != null && The.Game.HasObjectGameState("AdditionalWorld") && The.Game.GetObjectGameState("AdditionalWorld") is Dictionary<string, WorldBlueprint> dictionary)
		{
			num += dictionary.Keys.Count;
		}
		return num;
	}

	public bool hasWorld(string ID)
	{
		if (Worlds.ContainsKey(ID))
		{
			return true;
		}
		if (The.Game != null && The.Game.HasObjectGameState("AdditionalWorld") && The.Game.GetObjectGameState("AdditionalWorld") is Dictionary<string, WorldBlueprint> dictionary && dictionary.ContainsKey(ID))
		{
			return true;
		}
		return false;
	}

	public void addAdditionalWorld(string ID, WorldBlueprint World)
	{
		if (The.Game != null)
		{
			if (!The.Game.HasObjectGameState("AdditionalWorld"))
			{
				The.Game.ObjectGameState.Add("AdditionalWorlds", new Dictionary<string, WorldBlueprint>());
			}
			(The.Game.GetObjectGameState("AdditionalWorld") as Dictionary<string, WorldBlueprint>)[ID] = World;
		}
	}

	public List<WorldBlueprint> getWorlds()
	{
		WorldsScratch.Clear();
		if (The.Game != null && The.Game.HasObjectGameState("AdditionalWorld") && The.Game.GetObjectGameState("AdditionalWorld") is Dictionary<string, WorldBlueprint> dictionary)
		{
			WorldsScratch.AddRange(dictionary.Values);
		}
		WorldsScratch.AddRange(Worlds.Values);
		return WorldsScratch;
	}

	public WorldBlueprint getWorld(string ID)
	{
		if (Worlds.TryGetValue(ID, out var value))
		{
			return value;
		}
		if (The.Game != null && The.Game.HasObjectGameState("AdditionalWorld") && The.Game.GetObjectGameState("AdditionalWorld") is Dictionary<string, WorldBlueprint> dictionary && dictionary.TryGetValue(ID, out value))
		{
			return value;
		}
		return null;
	}

	public void BuildWorlds()
	{
		foreach (string key in Worlds.Keys)
		{
			BuildWorld(key);
		}
	}

	public void BuildWorld(string World)
	{
		if (WorldBuilderExtensions == null)
		{
			WorldBuilderExtensions = ModManager.GetInstancesWithAttribute<IWorldBuilderExtension>(typeof(WorldBuilderExtension));
		}
		foreach (ZoneBuilderBlueprint builder in Worlds[World].Builders)
		{
			string text = "XRL.World.WorldBuilders." + builder.Class;
			Type type = ModManager.ResolveType(text);
			if (type == null)
			{
				XRLCore.LogError("Unknown world builder " + text + "!");
				break;
			}
			object newBuilder = Activator.CreateInstance(type);
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				ZoneBuilderBlueprint zoneBuilderBlueprint = builder;
				if (zoneBuilderBlueprint.Parameters == null)
				{
					zoneBuilderBlueprint.Parameters = new Dictionary<string, object>();
				}
				if (builder.Parameters.ContainsKey(fieldInfo.Name))
				{
					if (fieldInfo.FieldType == typeof(bool))
					{
						fieldInfo.SetValue(newBuilder, Convert.ToBoolean(builder.Parameters[fieldInfo.Name]));
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						fieldInfo.SetValue(newBuilder, Convert.ToInt32(builder.Parameters[fieldInfo.Name]));
					}
					else if (fieldInfo.FieldType == typeof(short))
					{
						fieldInfo.SetValue(newBuilder, Convert.ToInt16(builder.Parameters[fieldInfo.Name]));
					}
					else
					{
						fieldInfo.SetValue(newBuilder, builder.Parameters[fieldInfo.Name]);
					}
				}
			}
			WorldBuilderExtensions.ForEach((IWorldBuilderExtension e) =>
			{
				e.OnBeforeBuild(World, newBuilder);
			});
			The.ZoneManager.AdjustZoneGenerationTierTo(World);
			MethodInfo method = type.GetMethod("BuildWorld");
			if (method != null && !(bool)method.Invoke(newBuilder, new object[1] { World }))
			{
				break;
			}
			WorldBuilderExtensions.ForEach((IWorldBuilderExtension e) =>
			{
				e.OnAfterBuild(World, newBuilder);
			});
			newBuilder = null;
			MemoryHelper.GCCollect();
		}
	}

	public string ZoneDisplayName(string ID)
	{
		if (ZoneIDToDisplay.TryGetValue(ID, out var value))
		{
			return value;
		}
		value = The.ZoneManager.GetZoneDisplayName(ID);
		ZoneIDToDisplay.Add(ID, value);
		return value;
	}

	public string ZoneDisplayNameWithIndefiniteArticle(string ID)
	{
		if (ZoneIDToDisplayWithIndefiniteArticle.TryGetValue(ID, out var value))
		{
			return value;
		}
		value = The.ZoneManager.GetZoneDisplayName(ID, WithIndefiniteArticle: true);
		ZoneIDToDisplayWithIndefiniteArticle.Add(ID, value);
		return value;
	}

	public void UpdateZoneDisplayName(string ID, string Name)
	{
		if (ZoneIDToDisplay.TryGetValue(ID, out var value))
		{
			ZoneIDToDisplay[ID] = Name;
			string value2 = null;
			if (ZoneDisplayToID.TryGetValue(value, out value2) && value2 == ID)
			{
				ZoneDisplayToID.Remove(value);
				ZoneDisplayToID.Set(Name, ID);
			}
		}
		if (ZoneIDToDisplayWithIndefiniteArticle.TryGetValue(ID, out value))
		{
			ZoneIDToDisplayWithIndefiniteArticle[ID] = Name;
			string value3 = null;
			if (ZoneDisplayToIDWithIndefiniteArticle.TryGetValue(value, out value3) && value3 == ID)
			{
				ZoneDisplayToIDWithIndefiniteArticle.Remove(value);
				ZoneDisplayToIDWithIndefiniteArticle.Set(Name, ID);
			}
		}
	}

	public void BuildZoneNameMap()
	{
		ZoneIDToDisplay = new Dictionary<string, string>();
		ZoneDisplayToID = new Dictionary<string, string>();
		ZoneIDToDisplayWithIndefiniteArticle = new Dictionary<string, string>();
		ZoneDisplayToIDWithIndefiniteArticle = new Dictionary<string, string>();
		if (Options.EnableWishRegionNames)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(512);
		StringBuilder stringBuilder2 = new StringBuilder(512);
		StringBuilder stringBuilder3 = new StringBuilder(512);
		StringBuilder stringBuilder4 = new StringBuilder(512);
		StringBuilder stringBuilder5 = new StringBuilder(512);
		int num = 0;
		foreach (string key in Factory.Worlds.Keys)
		{
			num++;
			for (int i = 0; i < 80; i++)
			{
				if (i % 2 == 0)
				{
					if (num == 1)
					{
						WorldCreationProgress.StepProgress("Generating rivers...");
					}
					else
					{
						WorldCreationProgress.StepProgress("Generating canyons...");
					}
				}
				stringBuilder.Length = 0;
				stringBuilder.Append(key);
				stringBuilder.Append(".");
				stringBuilder.Append(i);
				stringBuilder.Append(".");
				for (int j = 0; j < 25; j++)
				{
					stringBuilder2.Length = 0;
					stringBuilder2.Append(stringBuilder);
					stringBuilder2.Append(j);
					stringBuilder2.Append(".");
					for (int k = 0; k < 3; k++)
					{
						stringBuilder3.Length = 0;
						stringBuilder3.Append(stringBuilder2);
						stringBuilder3.Append(k);
						stringBuilder3.Append(".");
						for (int l = 0; l < 3; l++)
						{
							stringBuilder4.Length = 0;
							stringBuilder4.Append(stringBuilder3);
							stringBuilder4.Append(l);
							stringBuilder4.Append(".");
							for (int m = 10; m <= 10; m++)
							{
								stringBuilder5.Length = 0;
								stringBuilder5.Append(stringBuilder4);
								stringBuilder5.Append(m);
								string text = stringBuilder5.ToString();
								string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(text, key, i, j, k, l, m);
								if (!ZoneDisplayToID.ContainsKey(zoneDisplayName))
								{
									ZoneDisplayToID.Add(zoneDisplayName, text);
								}
							}
						}
					}
				}
			}
		}
	}

	public void Init()
	{
		Worlds = new Dictionary<string, WorldBlueprint>();
		Cells = new Dictionary<string, CellBlueprint>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Worlds"))
		{
			try
			{
				LoadWorldsNode(item);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.modInfo, message);
			}
		}
	}

	public void LoadWorldsNode(XmlTextReader Reader)
	{
		while (Reader.Read())
		{
			if (!(Reader.Name == "world"))
			{
				continue;
			}
			WorldBlueprint worldBlueprint = LoadWorldNode(Reader);
			if (Worlds.TryGetValue(worldBlueprint.Name, out var value))
			{
				foreach (ZoneBuilderBlueprint Builder in worldBlueprint.Builders)
				{
					if (Builder.Class.StartsWith("-"))
					{
						string builderToRemove = Builder.Class.Substring(1);
						value.Builders.RemoveAll((ZoneBuilderBlueprint b) => b.Class == builderToRemove);
					}
					else
					{
						value.Builders.RemoveAll((ZoneBuilderBlueprint b) => b.Class == Builder.Class);
						value.Builders.Add(Builder);
					}
				}
				foreach (KeyValuePair<string, CellBlueprint> item in worldBlueprint.CellBlueprintsByName)
				{
					if (value.CellBlueprintsByName.TryGetValue(item.Key, out var value2) && !value2.ApplyTo.IsNullOrEmpty())
					{
						value.CellBlueprintsByApplication.Remove(value2.ApplyTo);
					}
					value.CellBlueprintsByName[item.Key] = item.Value;
					if (!item.Value.ApplyTo.IsNullOrEmpty())
					{
						value.CellBlueprintsByApplication[item.Value.ApplyTo] = item.Value;
					}
				}
			}
			else
			{
				Worlds.Add(worldBlueprint.Name, worldBlueprint);
			}
		}
	}

	public WorldBlueprint LoadWorldNode(XmlTextReader Reader)
	{
		NewWorld = new WorldBlueprint();
		NewWorld.DisplayName = Reader.GetAttribute("DisplayName");
		NewWorld.Name = Reader.GetAttribute("Name");
		NewWorld.Map = Reader.GetAttribute("Map");
		NewWorld.AmbientBed = Reader.GetAttribute("AmbientBed");
		NewWorld.ZoneFactory = Reader.GetAttribute("ZoneFactory");
		NewWorld.ZoneFactoryRegex = Reader.GetAttribute("ZoneFactoryRegex");
		string attribute = Reader.GetAttribute("Plane");
		if (!attribute.IsNullOrEmpty())
		{
			NewWorld.Plane = attribute;
		}
		string attribute2 = Reader.GetAttribute("Protocol");
		if (!attribute2.IsNullOrEmpty())
		{
			NewWorld.Protocol = attribute2;
		}
		string attribute3 = Reader.GetAttribute("CustomClock");
		if (!attribute3.IsNullOrEmpty())
		{
			NewWorld.CustomClock = attribute3;
		}
		string attribute4 = Reader.GetAttribute("PsychicHunterChance");
		if (!attribute4.IsNullOrEmpty() && int.TryParse(attribute4, out var result))
		{
			NewWorld.PsychicHunterChance = result;
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return NewWorld;
		}
		while (Reader.Read())
		{
			string name = Reader.Name;
			if (name == "builder")
			{
				NewWorld.Builders.Add(LoadBuilderNode(Reader, "builder").Blueprint);
			}
			else if (name == "cell")
			{
				CellBlueprint cellBlueprint = LoadCellNode(Reader, NewWorld);
				if (!cellBlueprint.ApplyTo.IsNullOrEmpty())
				{
					NewWorld.CellBlueprintsByApplication[cellBlueprint.ApplyTo] = cellBlueprint;
				}
				NewWorld.CellBlueprintsByName[cellBlueprint.Name] = cellBlueprint;
				Cells[NewWorld.Name + cellBlueprint.Name] = cellBlueprint;
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "world"))
			{
				return NewWorld;
			}
		}
		return NewWorld;
	}

	private void ParseRangeSpec(string Spec, out int Start, out int End)
	{
		if (Spec.IndexOf('-') != -1)
		{
			string[] array = Spec.Split('-');
			Start = Convert.ToInt32(array[0]);
			End = Convert.ToInt32(array[1]);
		}
		else
		{
			Start = Convert.ToInt32(Spec);
			End = Convert.ToInt32(Spec);
		}
	}

	public CellBlueprint LoadCellNode(XmlTextReader Reader, WorldBlueprint World)
	{
		CellBlueprint cellBlueprint = new CellBlueprint();
		cellBlueprint.LandingZone = "1,1";
		cellBlueprint.Name = Reader.GetAttribute("Name");
		cellBlueprint.Inherits = Reader.GetAttribute("Inherits");
		cellBlueprint.ApplyTo = Reader.GetAttribute("ApplyTo");
		string attribute = Reader.GetAttribute("Mutable");
		if (attribute != null && !attribute.EqualsNoCase("true"))
		{
			cellBlueprint.Mutable = false;
		}
		string attribute2 = Reader.GetAttribute("LandingZone");
		if (attribute2 != null)
		{
			cellBlueprint.LandingZone = attribute2;
		}
		bool flag = !Reader.GetAttribute("HasBiomes").EqualsNoCase("false");
		string attribute3 = Reader.GetAttribute("Tier");
		if (attribute3.IsNullOrEmpty() || !int.TryParse(attribute3, out var result))
		{
			result = 0;
		}
		if (cellBlueprint.Inherits != null && Cells.TryGetValue(World.Name + cellBlueprint.Inherits, out var value) && value != null)
		{
			cellBlueprint.CopyFrom(value);
		}
		string attribute4 = Reader.GetAttribute("AmbientBed");
		if (attribute4 != null)
		{
			cellBlueprint.AmbientBed = attribute4;
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return cellBlueprint;
		}
		while (Reader.Read())
		{
			string name = Reader.Name;
			switch (name)
			{
			case "zone":
			{
				ZoneBlueprint zoneBlueprint = LoadZoneNode(Reader, cellBlueprint);
				if (result > 0 && zoneBlueprint.Tier <= 0)
				{
					zoneBlueprint.Tier = result;
				}
				if (zoneBlueprint.HasBiomes && !flag)
				{
					zoneBlueprint.HasBiomes = false;
				}
				int Start = 0;
				int End = 0;
				int Start2 = 0;
				int End2 = 0;
				int Start3 = 0;
				int End3 = 0;
				ParseRangeSpec(zoneBlueprint.Level, out Start, out End);
				ParseRangeSpec(zoneBlueprint.x, out Start2, out End2);
				ParseRangeSpec(zoneBlueprint.y, out Start3, out End3);
				for (int i = Start3; i <= End3; i++)
				{
					for (int j = Start2; j <= End2; j++)
					{
						for (int k = Start; k <= End; k++)
						{
							cellBlueprint.LevelBlueprint[j, i, k] = zoneBlueprint;
						}
					}
				}
				break;
			}
			case "property":
				LoadPropertyNode<string>(Reader, cellBlueprint.SetProperty);
				break;
			case "intproperty":
				LoadPropertyNode<int>(Reader, cellBlueprint.SetProperty);
				break;
			case "boolproperty":
				LoadPropertyNode<bool>(Reader, cellBlueprint.SetProperty);
				break;
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (name == "" || name == "cell"))
			{
				return cellBlueprint;
			}
		}
		return cellBlueprint;
	}

	public ZoneBlueprint LoadZoneNode(XmlTextReader Reader, CellBlueprint Parent)
	{
		ZoneBlueprint zoneBlueprint = new ZoneBlueprint(null);
		zoneBlueprint.Cell = Parent;
		zoneBlueprint.Level = Reader.GetAttribute("Level");
		zoneBlueprint.x = Reader.GetAttribute("x");
		zoneBlueprint.y = Reader.GetAttribute("y");
		zoneBlueprint.disableForcedConnections = Reader.GetAttribute("DisableForcedConnections").EqualsNoCase("Yes");
		zoneBlueprint.ProperName = Reader.GetAttribute("ProperName").EqualsNoCase("true");
		zoneBlueprint.NameContext = Reader.GetAttribute("NameContext");
		zoneBlueprint.IndefiniteArticle = Reader.GetAttribute("IndefiniteArticle");
		zoneBlueprint.DefiniteArticle = Reader.GetAttribute("DefiniteArticle");
		zoneBlueprint.AmbientBed = Reader.GetAttribute("AmbientBed") ?? Parent.AmbientBed;
		zoneBlueprint.AmbientSounds = Reader.GetAttribute("AmbientSounds");
		zoneBlueprint.AmbientVolume = (int.TryParse(Reader.GetAttribute("AmbientVolume"), out var result) ? result : (-1));
		zoneBlueprint.IncludeContextInZoneDisplay = (Reader.GetAttribute("IncludeContextInZoneDisplay") ?? "true").EqualsNoCase("true");
		zoneBlueprint.IncludeStratumInZoneDisplay = (Reader.GetAttribute("IncludeStratumInZoneDisplay") ?? "true").EqualsNoCase("true");
		zoneBlueprint.HasWeather = Reader.GetAttribute("HasWeather").EqualsNoCase("true");
		zoneBlueprint.HasBiomes = !Reader.GetAttribute("HasBiomes").EqualsNoCase("false");
		zoneBlueprint.WindSpeed = Reader.GetAttribute("WindSpeed");
		zoneBlueprint.WindDirections = Reader.GetAttribute("WindDirections");
		zoneBlueprint.WindDuration = Reader.GetAttribute("WindDuration");
		string attribute = Reader.GetAttribute("GroundLiquid");
		if (attribute != null)
		{
			zoneBlueprint.GroundLiquid = attribute;
		}
		string attribute2 = Reader.GetAttribute("Name");
		if (attribute2 != null)
		{
			zoneBlueprint.Name = attribute2;
		}
		if (Reader.GetAttribute("DisableForcedConnections").EqualsNoCase("Yes"))
		{
			zoneBlueprint.SetProperty("DisableForcedConnections", "Yes");
		}
		string attribute3 = Reader.GetAttribute("Tier");
		if (!attribute3.IsNullOrEmpty() && int.TryParse(attribute3, out var result2))
		{
			zoneBlueprint.Tier = result2;
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneBlueprint;
		}
		while (Reader.Read())
		{
			string name = Reader.Name;
			switch (name)
			{
			case "part":
			{
				ZoneBlueprint zoneBlueprint2 = zoneBlueprint;
				if (zoneBlueprint2.Parts == null)
				{
					zoneBlueprint2.Parts = new ZonePartCollection();
				}
				zoneBlueprint.Parts.Add(LoadPartNode(Reader, "part"));
				break;
			}
			case "population":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "population", "Population"));
				break;
			case "prebuilder":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "prebuilder", null, 3999));
				break;
			case "builder":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "builder"));
				break;
			case "postbuilder":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "postbuilder", null, 5000));
				break;
			case "map":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "map", "MapBuilder"));
				break;
			case "music":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "music", "Music"));
				break;
			case "widget":
				zoneBlueprint.Builders.Add(LoadBuilderNode(Reader, "widget", "AddWidgetBuilder"));
				break;
			case "property":
				LoadPropertyNode<string>(Reader, zoneBlueprint.SetProperty);
				break;
			case "intproperty":
				LoadPropertyNode<int>(Reader, zoneBlueprint.SetProperty);
				break;
			case "boolproperty":
				LoadPropertyNode<bool>(Reader, zoneBlueprint.SetProperty);
				break;
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (name == "" || name == "zone"))
			{
				return zoneBlueprint;
			}
		}
		return zoneBlueprint;
	}

	public OrderedBuilderBlueprint LoadBuilderNode(XmlTextReader Reader, string Tag, string DefaultClass = null, int DefaultPriority = 4000)
	{
		string text = Reader.GetAttribute("Class");
		if (text.IsNullOrEmpty())
		{
			text = DefaultClass;
		}
		if (!int.TryParse(Reader.GetAttribute("Priority"), out var result))
		{
			result = DefaultPriority;
		}
		Dictionary<string, object> parameterBuffer = ZoneBuilderBlueprint.GetParameterBuffer();
		while (Reader.MoveToNextAttribute())
		{
			string name = Reader.Name;
			if (name != "Class" && name != "Priority")
			{
				parameterBuffer[name] = Reader.Value;
			}
		}
		Reader.MoveToElement();
		ZoneBuilderBlueprint blueprint = ZoneBuilderBlueprint.Get(text, parameterBuffer);
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return new OrderedBuilderBlueprint(blueprint, result);
		}
		while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == Tag))))
		{
		}
		return new OrderedBuilderBlueprint(blueprint, result);
	}

	public ZonePartBlueprint LoadPartNode(XmlTextReader Reader, string Tag)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			return null;
		}
		Dictionary<string, object> parameterBuffer = ZonePartBlueprint.GetParameterBuffer();
		while (Reader.MoveToNextAttribute())
		{
			string name = Reader.Name;
			if (name != "Name")
			{
				parameterBuffer[name] = Reader.Value;
			}
		}
		Reader.MoveToElement();
		ZonePartBlueprint result = ZonePartBlueprint.Get(attribute, parameterBuffer);
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == Tag))))
			{
			}
		}
		return result;
	}

	public void LoadPropertyNode<T>(XmlTextReader Reader, Action<string, object> Setter)
	{
		try
		{
			string attribute = Reader.GetAttribute("Name");
			object arg = Convert.ChangeType(Reader.GetAttribute("Value"), typeof(T));
			Setter(attribute, arg);
		}
		catch (Exception x)
		{
			MetricsManager.LogException($"Error reading {Reader.Name} at {Reader.LineNumber},{Reader.LinePosition}", x);
		}
		Reader.SkipToEnd();
	}
}
