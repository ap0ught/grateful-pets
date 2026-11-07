using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenListenSympathetically : SifrahToken
{
	public SocialSifrahTokenListenSympathetically()
	{
		Description = "listen sympathetically";
		Tile = "Items/sw_heightenedhearing.bmp";
		RenderString = "@";
		ColorString = "&y";
		DetailColor = 'w';
	}
}
