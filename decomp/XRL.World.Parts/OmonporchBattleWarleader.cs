using System;
using XRL.World.Quests;

namespace XRL.World.Parts;

[Serializable]
public class OmonporchBattleWarleader : FinishQuestStepWhenSlain
{
	[NonSerialized]
	private ReclamationSystem _System;

	public ReclamationSystem System => _System ?? (_System = The.Game.GetSystem<ReclamationSystem>());

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == SuspendingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		ReclamationSystem system = System;
		if (system == null || !The.Game.Quests.ContainsKey(system.QuestID))
		{
			ParentObject.RemovePart(this);
		}
		else
		{
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone != null && !system.Perimeter.Contains(currentZone.ZoneID))
			{
				Trigger();
			}
		}
		return base.HandleEvent(E);
	}
}
