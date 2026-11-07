using System;
using XRL.World.Parts;

namespace XRL.World.Quests;

[Serializable]
public class ArgyveKnicknackSystem : IQuestSystem
{
	public virtual string StepID => "Find a Knickknack";

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(TookEvent.ID);
	}

	public override bool HandleEvent(TookEvent E)
	{
		if (IsValidItem(E.Item))
		{
			Game.FinishQuestStep(QuestID, StepID);
		}
		return base.HandleEvent(E);
	}

	public bool IsValidItem(GameObject Object)
	{
		if (Object.HasPart(typeof(TinkerItem)) && Object.TryGetPart<Examiner>(out var Part))
		{
			return Part.Complexity > 0;
		}
		return false;
	}

	public override void Start()
	{
		foreach (GameObject @object in The.Player.Inventory.Objects)
		{
			if (IsValidItem(@object))
			{
				Game.FinishQuestStep(QuestID, StepID);
				break;
			}
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Argyve");
	}
}
