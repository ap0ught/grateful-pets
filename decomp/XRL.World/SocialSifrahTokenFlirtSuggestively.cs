using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenFlirtSuggestively : SifrahToken
{
	public SocialSifrahTokenFlirtSuggestively()
	{
		Description = "flirt suggestively";
		Tile = "Items/sw_banana.bmp";
		RenderString = "õ";
		ColorString = "&W";
		DetailColor = 'Y';
	}
}
