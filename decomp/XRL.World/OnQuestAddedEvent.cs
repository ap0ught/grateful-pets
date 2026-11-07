using System;

namespace XRL.World;

[Obsolete("Use QuestStartedEvent.")]
public abstract class OnQuestAddedEvent : IQuestEvent
{
	public new static readonly int ID = QuestStartedEvent.ID;

	public GameObject Subject => The.Player;

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}
}
