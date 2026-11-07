using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace Kobold;

public class Configuration
{
	public string BasePath;

	public List<ImportDefinition> Imports = new List<ImportDefinition>();

	public static bool WildcardCompare(string pattern, string text, bool caseSensitive = false)
	{
		pattern = pattern.Replace(".", "\\.");
		pattern = pattern.Replace("?", ".");
		pattern = pattern.Replace("*", ".*?");
		pattern = pattern.Replace("\\", "\\\\");
		pattern = pattern.Replace("\\", "\\/");
		pattern = pattern.Replace(" ", "\\s");
		return new Regex(pattern, (!caseSensitive) ? RegexOptions.IgnoreCase : RegexOptions.None).IsMatch(text);
	}

	public ImportDefinition DetermineImport(string FileName)
	{
		ImportDefinition result = null;
		foreach (ImportDefinition import in Imports)
		{
			if (WildcardCompare(import.Spec, FileName.Replace('\\', '/')))
			{
				result = import;
			}
		}
		return result;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("BasePath: " + BasePath);
		foreach (ImportDefinition import in Imports)
		{
			stringBuilder.Append("\nImport Definition: " + import.ToString());
		}
		return stringBuilder.ToString();
	}

	public static Configuration LoadXML(string BasePath, TextAsset Asset)
	{
		Configuration configuration = new Configuration();
		configuration.BasePath = BasePath;
		XmlReader xmlReader = XmlReader.Create(new StringReader(Asset.text));
		while (xmlReader.Read())
		{
			if (xmlReader.Name == "import")
			{
				configuration.Imports.Add(LoadImport(xmlReader));
			}
		}
		return configuration;
	}

	public static ImportDefinition LoadImport(XmlReader Reader)
	{
		ImportDefinition importDefinition = new ImportDefinition();
		importDefinition.Spec = Reader.GetAttribute("spec");
		while (Reader.Read())
		{
			if (Reader.Name == "atlas" && Reader.NodeType != XmlNodeType.EndElement)
			{
				importDefinition.DiffuseAtlas = new AtlasDefinition();
				importDefinition.DiffuseAtlas.Compression = Reader.GetAttribute("Compression");
				importDefinition.DiffuseAtlas.Prefix = Reader.GetAttribute("Name");
				importDefinition.DiffuseAtlas.XSize = Convert.ToInt16(Reader.GetAttribute("SizeX"));
				importDefinition.DiffuseAtlas.YSize = Convert.ToInt16(Reader.GetAttribute("SizeY"));
				if (string.IsNullOrEmpty(Reader.GetAttribute("Trim")))
				{
					importDefinition.DiffuseAtlas.Trim = false;
				}
				else
				{
					importDefinition.DiffuseAtlas.Trim = Convert.ToBoolean(Reader.GetAttribute("Trim"));
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "import")
			{
				return importDefinition;
			}
		}
		return importDefinition;
	}
}
