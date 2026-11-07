using System;

namespace XRL.World.ZoneParts;

[Serializable]
public class NoPolypPlucking : IZonePart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AllowPolypPluckingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AllowPolypPluckingEvent E)
	{
		return false;
	}
}
