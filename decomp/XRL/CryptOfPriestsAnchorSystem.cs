using System;
using System.Collections.Generic;

namespace XRL;

[Serializable]
public class CryptOfPriestsAnchorSystem : ITombAnchorSystem
{
	public static List<string> priestsAllowedStairsZones = new List<string> { "JoppaWorld.53.3.0.0.7", "JoppaWorld.53.3.0.1.7", "JoppaWorld.53.3.1.0.7", "JoppaWorld.53.3.1.2.7", "JoppaWorld.53.3.2.0.7", "JoppaWorld.53.3.2.1.7", "JoppaWorld.53.3.2.2.7" };

	public override int Depth => 7;
}
