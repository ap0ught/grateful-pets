using System;

namespace XRL.World.Parts;

[Serializable]
public class FinishQuestStepWhenSlain : IPart
{
	public string Quest;

	public string Step;

	public string GameState;

	public bool RequireQuest;

	public virtual bool Clean => true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ReplicaCreatedEvent.ID && ID != AfterDieEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == PooledEvent<ReplaceInContextEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		if (E.Dying == ParentObject)
		{
			Trigger();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (Clean && The.Game.HasFinishedQuestStep(Quest, Step))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		ParentObject.RemovePart(this);
		E.Replacement.AddPart(this);
		return base.HandleEvent(E);
	}

	public virtual void Trigger()
	{
		if (GameState != null)
		{
			The.Game.SetIntGameState(GameState, 1);
		}
		if (!The.Game.TryGetQuest(this.Quest, out var Quest))
		{
			if (!RequireQuest)
			{
				return;
			}
			Quest = The.Game.StartQuest(this.Quest);
		}
		The.Game.FinishQuestStep(Quest, Step);
		if (Clean)
		{
			ParentObject.RemovePart(this);
		}
	}
}
