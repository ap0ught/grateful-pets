using System;

namespace XRL.World.Quests;

[Serializable]
public class PetalsOnTheWindSystem : IQuestSystem
{
	public override void Start()
	{
		ZoneManager.instance.GetZone("JoppaWorld").BroadcastEvent("BeyLahReveal");
		if (The.Game.HasQuest("Find Eskhind"))
		{
			The.Game.CompleteQuest("Petals on the Wind");
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Lulihart");
	}
}
