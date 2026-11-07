using System;
using XRL.World;

namespace XRL;

[Serializable]
public class CryptOfLandlordsAnchorSystem : ITombAnchorSystem
{
	public override int Depth => 9;

	public override string GetAnchorZoneFor(Zone Z)
	{
		if (Z.X == 2)
		{
			if (Z.Y == 0)
			{
				return "JoppaWorld.53.3.2.2.9";
			}
			if (Z.Y == 2)
			{
				return "JoppaWorld.53.3.2.0.9";
			}
		}
		if (!50.in100())
		{
			return "JoppaWorld.53.3.2.0.9";
		}
		return "JoppaWorld.53.3.2.2.9";
	}
}
