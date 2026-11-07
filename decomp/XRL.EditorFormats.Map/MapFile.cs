using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRL.Collections;

namespace XRL.EditorFormats.Map;

[HasModSensitiveStaticCache]
public class MapFile
{
	private static StringMap<Rack<DataFile>> Repository = new StringMap<Rack<DataFile>>();

	private static Rack<char> KeyBuffer = new Rack<char>(64);

	private static bool Initialized;

	public string ID;

	public MapFileRegion Cells;

	public int width
	{
		get
		{
			return Cells.width;
		}
		set
		{
			MapFileRegion cells = new MapFileRegion(value, height);
			cells.SetRegion(Cells);
			Cells = cells;
		}
	}

	public int height
	{
		get
		{
			return Cells.height;
		}
		set
		{
			MapFileRegion cells = new MapFileRegion(width, value);
			cells.SetRegion(Cells);
			Cells = cells;
		}
	}

	[ModSensitiveCacheInit]
	public static void Reset()
	{
		if (Initialized)
		{
			Repository.Clear();
		}
		string key;
		Rack<DataFile> value;
		if (Repository.IsNullOrEmpty())
		{
			string text = DataManager.FilePath("");
			FileInfo[] files = new DirectoryInfo(text).GetFiles("*.rpm", SearchOption.AllDirectories);
			foreach (FileInfo fileInfo in files)
			{
				CacheFile(text, fileInfo.FullName);
			}
			foreach (KeyValuePair<string, Rack<DataFile>> item in Repository)
			{
				item.Deconstruct(out key, out value);
				value.Sort();
			}
		}
		if (!ModManager.Initialized)
		{
			return;
		}
		foreach (ModInfo activeMod in ModManager.ActiveMods)
		{
			foreach (ModFile file in activeMod.Files)
			{
				if (file.Type == ModFileType.Map)
				{
					CacheFile(activeMod.Directory.FullName, file.OriginalName, activeMod);
				}
			}
		}
		foreach (KeyValuePair<string, Rack<DataFile>> item2 in Repository)
		{
			item2.Deconstruct(out key, out value);
			value.Sort();
		}
		Initialized = true;
	}

	private static void CacheFile(string Root, string Path, ModInfo Mod = null)
	{
		try
		{
			using XmlReader xmlReader = XmlReader.Create(Path);
			while (xmlReader.Read())
			{
				if (!xmlReader.Name.IsNullOrEmpty() && !(xmlReader.Name == "xml"))
				{
					string attribute = xmlReader.GetAttribute("ID");
					ReadOnlySpan<char> key = GetKey(attribute.IsNullOrEmpty() ? System.IO.Path.GetRelativePath(Root, Path) : attribute);
					if (!Repository.TryGetValue(key, out var Value))
					{
						Value = (Repository[key] = new Rack<DataFile>());
					}
					int priority = (int.TryParse(xmlReader.GetAttribute("LoadPriority"), out var result) ? result : 0);
					Value.Add(new DataFile
					{
						Path = Path,
						Mod = Mod,
						Priority = priority
					});
					break;
				}
			}
		}
		catch (Exception ex)
		{
			MetricsManager.LogPotentialModError(Mod, Path + ": " + ex);
		}
	}

	private static ReadOnlySpan<char> GetKey(ReadOnlySpan<char> Base)
	{
		int num = Base.Length;
		char[] array = KeyBuffer.GetArray(num);
		for (int i = 0; i < num; i++)
		{
			char c = Base[i];
			switch (c)
			{
			case '.':
				break;
			case ' ':
			case '-':
			case '/':
			case '\\':
				array[i] = '_';
				continue;
			default:
				array[i] = char.ToLowerInvariant(c);
				continue;
			}
			num = i;
			break;
		}
		return array.AsSpan(0, num);
	}

	public static bool TryGetFiles(string Key, out Rack<DataFile> Files)
	{
		if (Repository.TryGetValue(GetKey(Key), out Files))
		{
			return true;
		}
		return false;
	}

	public int RightmostObject()
	{
		return Cells.MaxX(requireObjects: true);
	}

	public int BottommostObject()
	{
		return Cells.MaxY(requireObjects: true);
	}

	public static bool TryResolve(string ID, out MapFile Map)
	{
		if (TryGetFiles(ID, out var Files))
		{
			Map = new MapFile(0, 0);
			foreach (DataFile item in Files)
			{
				Map.LoadFile(item.Path);
			}
			Map.Cells.FillEmptyCells();
			return true;
		}
		Map = null;
		return false;
	}

	public static MapFile Resolve(string ID, bool Required = true)
	{
		if (TryResolve(ID, out var Map))
		{
			return Map;
		}
		if (Required)
		{
			throw new KeyNotFoundException("A map by ID '" + ID + "' could not be found.");
		}
		return null;
	}

	[Obsolete("Use MapFile.Resolve")]
	public static MapFile LoadWithMods(string filename)
	{
		return Resolve(filename);
	}

	public MapFile(int width = 80, int height = 25)
	{
		Cells = new MapFileRegion(width, height);
	}

	public HashSet<string> UsedBlueprints(HashSet<string> result = null)
	{
		if (result == null)
		{
			result = new HashSet<string>();
		}
		foreach (MapFileObjectReference item in Cells.AllObjects())
		{
			result.Add(item.blueprint.Name);
		}
		return result;
	}

	public void FlipHorizontal()
	{
		Cells = Cells.FlippedHorizontal();
	}

	public void FlipVertical()
	{
		Cells = Cells.FlippedVertical();
	}

	public void NewMap(int width = 80, int height = 25)
	{
		Cells = new MapFileRegion(width, height);
	}

	public MapFile Load(XmlTextReader Reader)
	{
		Reader.WhitespaceHandling = WhitespaceHandling.None;
		while (Reader.Read())
		{
			if (Reader.Name == "Map")
			{
				LoadMapNode(Reader);
			}
		}
		Reader.Close();
		return this;
	}

	public void LoadFile(string FileName)
	{
		Load(new XmlTextReader(FileName));
	}

	private void LoadMapNode(XmlTextReader Reader, bool Mod = false)
	{
		if (ID == null)
		{
			ID = Reader.GetAttribute("ID");
		}
		bool merge = false;
		string attribute = Reader.GetAttribute("Load");
		if (attribute != null && attribute.EqualsNoCase("merge"))
		{
			merge = true;
		}
		attribute = Reader.GetAttribute("width");
		if (attribute != null)
		{
			width = Convert.ToInt32(attribute);
		}
		attribute = Reader.GetAttribute("height");
		if (attribute != null)
		{
			height = Convert.ToInt32(attribute);
		}
		while (Reader.Read())
		{
			if (Reader.Name == "cell")
			{
				LoadCellNode(Reader, merge);
			}
		}
	}

	private void LoadCellNode(XmlTextReader Reader, bool Merge = false)
	{
		int num = Convert.ToInt32(Reader.GetAttribute("X"));
		int num2 = Convert.ToInt32(Reader.GetAttribute("Y"));
		if (num + 1 > width)
		{
			width = num + 1;
		}
		if (num2 + 1 > height)
		{
			height = num2 + 1;
		}
		MapFileCell orCreateCellAt = Cells.GetOrCreateCellAt(num, num2);
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return;
		}
		if (!Merge)
		{
			orCreateCellAt.Objects.Clear();
		}
		orCreateCellAt.Clear = Reader.GetAttribute("Clear").EqualsNoCase("true");
		while (Reader.Read())
		{
			if (Reader.Name == "object")
			{
				orCreateCellAt.Objects.Add(LoadObjectNode(Reader));
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "cell"))
			{
				break;
			}
		}
	}

	private MapFileObjectBlueprint LoadObjectNode(XmlTextReader Reader)
	{
		MapFileObjectBlueprint mapFileObjectBlueprint = new MapFileObjectBlueprint("");
		mapFileObjectBlueprint.Name = Reader.GetAttribute("Name");
		mapFileObjectBlueprint.Owner = Reader.GetAttribute("Owner");
		mapFileObjectBlueprint.Part = Reader.GetAttribute("Part");
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return mapFileObjectBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "property")
			{
				MapFileObjectBlueprint mapFileObjectBlueprint2 = mapFileObjectBlueprint;
				if (mapFileObjectBlueprint2.Properties == null)
				{
					mapFileObjectBlueprint2.Properties = new Dictionary<string, string>();
				}
				mapFileObjectBlueprint.Properties[Reader.GetAttribute("Name")] = Reader.GetAttribute("Value");
				Reader.SkipToEnd();
			}
			else if (Reader.Name == "intproperty")
			{
				MapFileObjectBlueprint mapFileObjectBlueprint2 = mapFileObjectBlueprint;
				if (mapFileObjectBlueprint2.IntProperties == null)
				{
					mapFileObjectBlueprint2.IntProperties = new Dictionary<string, int>();
				}
				mapFileObjectBlueprint.IntProperties[Reader.GetAttribute("Name")] = int.Parse(Reader.GetAttribute("Value"));
				Reader.SkipToEnd();
			}
			else if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "object" || Reader.Name == "cell"))
			{
				return mapFileObjectBlueprint;
			}
		}
		return null;
	}

	public void Save(string FileName)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		XmlWriter xmlWriter = XmlWriter.Create(FileName, xmlWriterSettings);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("Map");
		if (!ID.IsNullOrEmpty())
		{
			xmlWriter.WriteAttributeString("ID", ID);
		}
		xmlWriter.WriteAttributeString("Width", width.ToString());
		xmlWriter.WriteAttributeString("Height", height.ToString());
		foreach (MapFileCellReference item in Cells.AllCells())
		{
			xmlWriter.WriteStartElement("cell");
			int x = item.x;
			xmlWriter.WriteAttributeString("X", x.ToString());
			x = item.y;
			xmlWriter.WriteAttributeString("Y", x.ToString());
			if (item.cell.Clear)
			{
				xmlWriter.WriteAttributeString("Clear", "true");
			}
			foreach (MapFileObjectBlueprint @object in item.cell.Objects)
			{
				xmlWriter.WriteStartElement("object");
				xmlWriter.WriteAttributeString("Name", @object.Name);
				if (!@object.Owner.IsNullOrEmpty())
				{
					xmlWriter.WriteAttributeString("Owner", @object.Owner);
				}
				if (!@object.Part.IsNullOrEmpty())
				{
					xmlWriter.WriteAttributeString("Part", @object.Part);
				}
				if (!@object.Properties.IsNullOrEmpty())
				{
					foreach (KeyValuePair<string, string> property in @object.Properties)
					{
						xmlWriter.WriteStartElement("property");
						xmlWriter.WriteAttributeString("Name", property.Key);
						xmlWriter.WriteAttributeString("Value", property.Value);
						xmlWriter.WriteEndElement();
					}
				}
				if (!@object.IntProperties.IsNullOrEmpty())
				{
					foreach (KeyValuePair<string, int> intProperty in @object.IntProperties)
					{
						xmlWriter.WriteStartElement("intproperty");
						xmlWriter.WriteAttributeString("Name", intProperty.Key);
						xmlWriter.WriteAttributeString("Value", intProperty.Value.ToString());
						xmlWriter.WriteEndElement();
					}
				}
				xmlWriter.WriteFullEndElement();
			}
			xmlWriter.WriteFullEndElement();
		}
		xmlWriter.WriteFullEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
		xmlWriter.Close();
	}
}
