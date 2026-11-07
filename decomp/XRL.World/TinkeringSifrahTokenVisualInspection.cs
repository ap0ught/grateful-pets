using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenVisualInspection : SifrahToken
{
	public TinkeringSifrahTokenVisualInspection()
	{
		Description = "visual inspection";
		Tile = "Mutations/night_vision_mutation.bmp";
		RenderString = "\u001d";
		ColorString = "&y";
		DetailColor = 'B';
	}
}
