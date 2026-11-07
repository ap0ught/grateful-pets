using System;
using System.Collections.Generic;

namespace XRL;

[Serializable]
public class CatacombsAnchorSystem : ITombAnchorSystem
{
	public static List<string> catacombsAllowedStairsZones = new List<string> { "JoppaWorld.53.3.0.0.11", "JoppaWorld.53.3.0.1.11", "JoppaWorld.53.3.1.0.11", "JoppaWorld.53.3.1.2.11", "JoppaWorld.53.3.2.0.11", "JoppaWorld.53.3.2.1.11", "JoppaWorld.53.3.2.2.11" };

	public override int Depth => 11;
}
