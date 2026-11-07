using System;

namespace XRL.World.Parts;

[Serializable]
public class NoXPGain : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AwardingXPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardingXPEvent E)
	{
		return false;
	}
}
