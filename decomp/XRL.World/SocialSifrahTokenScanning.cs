using System;
using XRL.World.Capabilities;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenScanning : SifrahToken
{
	public SocialSifrahTokenScanning()
	{
		Description = "scanning";
		Tile = "Items/sw_aircurrent.bmp";
		RenderString = "å";
		ColorString = "&C";
		DetailColor = 'W';
	}

	public SocialSifrahTokenScanning(Scanning.Scan scan)
		: this()
	{
		Description = "interpret " + Scanning.GetScanSubjectName(scan);
	}
}
