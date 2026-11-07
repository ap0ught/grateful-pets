using System;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class InteractQuestTarget : IPart
{
	public string EventID;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(EventID);
	}

	public override bool FireEvent(Event E)
	{
		string eventID = EventID;
		if (E.ID == eventID)
		{
			The.Game.GetSystem<InteractWithAnObjectDynamicQuestManager.System>()?.QuestableInteract(ParentObject);
		}
		return base.FireEvent(E);
	}
}
