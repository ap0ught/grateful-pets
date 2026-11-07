using System;
using XRL.World.Conversations.Parts;

namespace XRL.World.Parts;

[Serializable]
public class ArkOpenedSystem : IGameSystem
{
	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(SingletonEvent<EndTurnEvent>.ID);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!EndGame.IsAnyEnding)
		{
			EndGame.CheckMarooned();
		}
		return base.HandleEvent(E);
	}
}
