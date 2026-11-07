using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenPhysicalManipulation : SifrahToken
{
	public TinkeringSifrahTokenPhysicalManipulation()
	{
		Description = "physical manipulation";
		Tile = "Items/sw_flexors.bmp";
		RenderString = "\u001d";
		ColorString = "&y";
		DetailColor = 'K';
	}
}
