using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public class ZoneFeature
{
	public string Name;

	public Dictionary<string, string> Properties = new Dictionary<string, string>();
}
