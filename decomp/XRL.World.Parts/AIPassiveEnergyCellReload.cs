using System;

namespace XRL.World.Parts;

[Serializable]
public class AIPassiveEnergyCellReload : IAIEnergyCellReload
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetPassiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetPassiveItemListEvent E)
	{
		CheckEnergyCellReload(E, 10);
		return base.HandleEvent(E);
	}
}
