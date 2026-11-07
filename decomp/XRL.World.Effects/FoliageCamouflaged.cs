using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.Effects;

/// This class is not used in the base game.
[Serializable]
public class FoliageCamouflaged : ICamouflageEffect
{
	public FoliageCamouflaged()
	{
		DisplayName = "{{camouflage|camouflaged}}";
	}

	public override bool Render(RenderEvent E)
	{
		int currentFrameLong = XRLCore.CurrentFrameLong10;
		if (currentFrameLong >= 1000 && currentFrameLong < 3000)
		{
			E.ColorString = "&g";
			E.DetailColor = "K";
		}
		else if (currentFrameLong >= 7000 && currentFrameLong < 9000)
		{
			E.ColorString = "&K";
			E.DetailColor = "g";
		}
		return true;
	}

	public override bool EnablesCamouflage(GameObject GO)
	{
		return GO.HasPart<PlantProperties>();
	}
}
