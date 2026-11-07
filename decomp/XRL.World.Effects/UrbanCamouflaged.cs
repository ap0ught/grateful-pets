using System;
using XRL.Core;

namespace XRL.World.Effects;

/// This class is not used in the base game.
[Serializable]
public class UrbanCamouflaged : ICamouflageEffect
{
	public UrbanCamouflaged()
	{
		DisplayName = "{{urban camouflage|urban camouflage}}";
	}

	public override string GetStateDescription()
	{
		return "{{urban camouflage|camouflage for an urban environment}}";
	}

	public override bool Render(RenderEvent E)
	{
		int currentFrameLong = XRLCore.CurrentFrameLong10;
		if ((currentFrameLong >= 9000 && currentFrameLong < 10000) || (currentFrameLong >= 0 && currentFrameLong < 1000))
		{
			E.ColorString = "&y";
			E.DetailColor = "K";
		}
		else if (currentFrameLong >= 5000 && currentFrameLong < 7000)
		{
			E.ColorString = "&K";
			E.DetailColor = "y";
		}
		return true;
	}

	public override bool EnablesCamouflage(GameObject GO)
	{
		return GO.HasTagOrProperty("EnableUrbanCamouflage");
	}
}
