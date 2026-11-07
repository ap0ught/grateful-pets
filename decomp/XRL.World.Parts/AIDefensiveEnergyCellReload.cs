using System;

namespace XRL.World.Parts;

[Serializable]
public class AIDefensiveEnergyCellReload : IAIEnergyCellReload
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetDefensiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		CheckEnergyCellReload(E);
		return base.HandleEvent(E);
	}
}
