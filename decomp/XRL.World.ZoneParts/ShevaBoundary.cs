using System;

namespace XRL.World.ZoneParts;

[Serializable]
public class ShevaBoundary : IZonePart
{
	public override bool WantEvent(int ID, int cascade)
	{
		return ID == EnteringZoneEvent.ID;
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		return false;
	}
}
