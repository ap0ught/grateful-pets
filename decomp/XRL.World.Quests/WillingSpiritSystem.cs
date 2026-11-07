using System;

namespace XRL.World.Quests;

[Serializable]
public class WillingSpiritSystem : IQuestSystem
{
	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(TookEvent.ID);
	}

	public override bool HandleEvent(TookEvent E)
	{
		CheckObject(E.Item);
		return base.HandleEvent(E);
	}

	public override void Start()
	{
		foreach (GameObject @object in The.Player.Inventory.GetObjects())
		{
			CheckObject(@object);
		}
	}

	public void CheckObject(GameObject Object)
	{
		if (Object.Blueprint == "Scrapped Waydroid")
		{
			Game.FinishQuestStep("More Than a Willing Spirit", "Travel to Golgotha");
			Game.FinishQuestStep("More Than a Willing Spirit", "Find a Dysfunctional Waydroid");
		}
		else if (Object.Blueprint == "Dormant Waydroid")
		{
			Game.FinishQuestStep("More Than a Willing Spirit", "Travel to Golgotha");
			Game.FinishQuestStep("More Than a Willing Spirit", "Find a Dysfunctional Waydroid");
			Game.FinishQuestStep("More Than a Willing Spirit", "Repair the Waydroid");
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Mafeo");
	}
}
