using System;
using System.Collections.Generic;

namespace XRL;

[Serializable]
public class CryptOfWarriorsAnchorSystem : ITombAnchorSystem
{
	public static List<string> warriorsAllowedStairsZones = new List<string> { "JoppaWorld.53.3.0.0.8", "JoppaWorld.53.3.0.1.8", "JoppaWorld.53.3.0.2.8", "JoppaWorld.53.3.1.0.8", "JoppaWorld.53.3.1.2.8", "JoppaWorld.53.3.2.0.8", "JoppaWorld.53.3.2.1.8", "JoppaWorld.53.3.2.2.8" };

	public override int Depth => 8;
}
