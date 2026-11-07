using System;

namespace XRL.World.Parts;

[Serializable]
public class RipePlant : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		The.Game.IncrementIntGameState("ripe plants killed", 1);
		return base.HandleEvent(E);
	}
}
