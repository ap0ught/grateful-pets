using System.Collections.Generic;
using Newtonsoft.Json;
using XRL.Serialization;

namespace XRL;

public class ModManifest
{
	public string ID;

	public int? LoadOrder;

	public string Title;

	public string Description;

	[JsonConverter(typeof(CommaDelimitedArrayConverter))]
	public string[] Tags;

	public Version Version;

	public string Author;

	public string PreviewImage;

	public ModDirectory[] Directories;

	public Dictionary<string, string> Dependencies;

	[JsonConverter(typeof(StringArrayConverter))]
	public string[] LoadBefore;

	[JsonConverter(typeof(StringArrayConverter))]
	public string[] LoadAfter;

	[JsonProperty]
	private string Dependency
	{
		init
		{
			Dependencies = new Dictionary<string, string> { { value, "*" } };
		}
	}
}
