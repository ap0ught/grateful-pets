using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenOfferMaintenanceServices : SifrahToken
{
	public SocialSifrahTokenOfferMaintenanceServices()
	{
		Description = "offer maintenance services";
		Tile = "Items/sw_toolbox.bmp";
		RenderString = "÷";
		ColorString = "&c";
		DetailColor = 'C';
	}
}
