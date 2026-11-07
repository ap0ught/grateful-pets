using System;
using XRL.World.Capabilities;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenScanning : SifrahToken
{
	public TinkeringSifrahTokenScanning()
	{
		Description = "scanning";
		Tile = "Items/sw_aircurrent.bmp";
		RenderString = "å";
		ColorString = "&C";
		DetailColor = 'W';
	}

	public TinkeringSifrahTokenScanning(Scanning.Scan scan)
		: this()
	{
		Description = "read " + Scanning.GetScanSubjectName(scan);
	}
}
