using System;
using HistoryKit;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Fetches : IPart
{
	public int SniffChance = 7;

	public int SniffMessageChance = 33;

	public bool FetchLiquids;

	public string SniffMessage = "=subject.The==subject.name= &y=verb:sniff= the air.";

	public string FetchMessage = "=subject.The==subject.name= &y=verb:run= off to fetch ";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<AIBoredEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (ParentObject.CurrentCell != null && 7.in100())
		{
			if (If.Chance(SniffMessageChance))
			{
				IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(SniffMessage, ParentObject));
			}
			GameObject randomElement = ParentObject.CurrentZone.GetObjectsReadonly(ShouldFetch).GetRandomElement();
			if (randomElement != null)
			{
				IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(FetchMessage, ParentObject) + randomElement.an() + "!");
				ParentObject.Brain.PushGoal(new GoFetch(randomElement.GetCurrentCell()));
			}
			ParentObject.UseEnergy(1000);
		}
		return base.HandleEvent(E);
	}

	public bool ShouldFetch(GameObject o)
	{
		if (o.ShouldAutoget() && !o.IsOpenLiquidVolume() && o.GetCurrentCell() != null)
		{
			return !o.GetCurrentCell().IsSolid();
		}
		return false;
	}
}
