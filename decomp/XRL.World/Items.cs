using System.Collections.Generic;

namespace XRL.World;

public static class Items
{
	public static readonly Dictionary<string, string> ItemTableNames = new Dictionary<string, string>
	{
		{ "swords", "Long Blades" },
		{ "daggers", "Short Blades" },
		{ "axes", "Axes" },
		{ "cudgels", "Cudgels" },
		{ "pistols", "Pistols" },
		{ "rifles", "Rifles" },
		{ "shields", "Shields" },
		{ "helms", "Helms" },
		{ "gloves", "Gloves" },
		{ "boots", "Shoes" },
		{ "body", "Chest Pieces" },
		{ "bracelets", "Bracelets" },
		{ "cloaks", "Cloaks" }
	};

	public static readonly Dictionary<string, string> ItemCategoryDisplayNames = new Dictionary<string, string>
	{
		{ "swords", "long blades" },
		{ "daggers", "short blades" },
		{ "axes", "axes" },
		{ "cudgels", "cudgels" },
		{ "pistols", "pistols" },
		{ "rifles", "rifles" },
		{ "shields", "shields" },
		{ "helms", "helms" },
		{ "gloves", "gloves" },
		{ "boots", "shoes" },
		{ "body", "chest pieces" },
		{ "bracelets", "bracelets" },
		{ "cloaks", "cloaks" }
	};
}
