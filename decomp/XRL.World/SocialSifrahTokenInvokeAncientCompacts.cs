using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenInvokeAncientCompacts : SifrahToken
{
	public SocialSifrahTokenInvokeAncientCompacts()
	{
		Description = "invoke ancient compacts";
		Tile = "Items/sw_scroll2.bmp";
		RenderString = "\u0015";
		ColorString = "&y";
		DetailColor = 'r';
	}
}
