using System;
using XRL.UI;

namespace XRL.World.Quests;

[Serializable]
public class PaxKlanqIPresumeSystem : IQuestSystem
{
	public const string PaxQuestID = "Pax Klanq, I Presume?";

	public PaxKlanqIPresumeSystem()
	{
		QuestID = "Pax Klanq, I Presume?";
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
	}

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(AfterConsumeEvent.ID);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (E.Zone.X == 1 && E.Zone.Y == 1 && E.Zone.Z == 10 && E.Zone.GetTerrainObject()?.Blueprint == "TerrainFungalCenter")
		{
			The.Game.FinishQuestStep("Pax Klanq, I Presume?", "Seek the Heart of the Rainbow");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterConsumeEvent E)
	{
		if (E.Ingest && E.Object.Blueprint == "Godshroom Cap")
		{
			The.Game.FinishQuestStep("Pax Klanq, I Presume?", "Eat the God's Flesh");
		}
		return base.HandleEvent(E);
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Pax Klanq");
	}

	public static bool UnderConstructionMessage()
	{
		Popup.ShowSpace("You've reached the temporary end of the main questline.\n\nYou may continue to explore the world, and stay tuned for updates as we prepare to leave Early Access.");
		return true;
	}
}
