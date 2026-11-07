using System;
using XRL.World.Parts;

namespace XRL.World.Quests;

[Serializable]
public class WeirdwireConduitSystem : IQuestSystem
{
	public int TotalLength;

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(TookEvent.ID);
	}

	public override bool HandleEvent(TookEvent E)
	{
		int totalLength = TotalLength;
		TotalLength = The.Player.Inventory.GetObjectStackCount((GameObject x) => x.HasPart(typeof(Wire)));
		if (TotalLength >= 200)
		{
			The.Game.FinishQuestStep("Weirdwire Conduit... Eureka!", "Find 200 Feet of Wire");
		}
		else if (TotalLength != totalLength)
		{
			AddPlayerMessage("You now have " + TotalLength + " feet of copper wire.", 'c');
		}
		return base.HandleEvent(E);
	}

	public override void Start()
	{
		int num = 0;
		foreach (GameObject @object in The.Player.Inventory.Objects)
		{
			if (@object.HasPart(typeof(Wire)))
			{
				num += @object.Count;
			}
		}
		if (num >= 200)
		{
			The.Game.FinishQuestStep("Weirdwire Conduit... Eureka!", "Find 200 Feet of Wire");
		}
	}

	public override void Finish()
	{
		GameObject influencer = GetInfluencer();
		if (influencer != null)
		{
			AIWiring aIWiring = influencer.RequirePart<AIWiring>();
			influencer.Brain.Goals.Clear();
			aIWiring.QueueAction();
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Argyve");
	}
}
