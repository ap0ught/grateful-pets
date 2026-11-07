using System;
using XRL.World.Quests;

namespace XRL.World.Parts;

[Serializable]
public class OmonporchBattleNephilim : IPart
{
	[NonSerialized]
	private ReclamationSystem _System;

	public ReclamationSystem System => _System ?? (_System = The.Game.GetSystem<ReclamationSystem>());

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == SuspendingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ReclamationSystem system = System;
		if (system == null || !The.Game.Quests.ContainsKey(system.QuestID))
		{
			ParentObject.RemovePart(this);
		}
		else
		{
			system.NephalLocation.SetCell(E.Cell);
		}
		return base.HandleEvent(E);
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
			Zone zone = The.ZoneManager.GetZone(system.GetProperty("NephalRetryZone"));
			if (zone != null)
			{
				Cell randomElement = zone.GetEmptyCells().GetRandomElement();
				ParentObject.TeleportTo(randomElement, 0);
				if (system.Active)
				{
					ParentObject.MakeActive();
				}
			}
		}
		return base.HandleEvent(E);
	}
}
