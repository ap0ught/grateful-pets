using System;

namespace XRL.World.Quests;

[Serializable]
public class OmonporchEarlSystem : IQuestSystem
{
	public override void Start()
	{
		if (The.Game.GetIntGameState("AsphodelSlain") == 1)
		{
			The.Game.FinishQuestStep("The Earl of Omonporch", "Travel to Omonporch");
			The.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Otho");
	}
}
