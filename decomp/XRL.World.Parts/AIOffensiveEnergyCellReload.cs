using System;

namespace XRL.World.Parts;

[Serializable]
public class AIOffensiveEnergyCellReload : IAIEnergyCellReload
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		CheckEnergyCellReload(E);
		return base.HandleEvent(E);
	}
}
