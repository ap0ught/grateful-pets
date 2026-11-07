using System;

namespace XRL.World.Parts;

[Serializable]
public class PlayerOthoWander1Safeguard : IPart
{
	public long startTurn;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && The.Game.Turns - startTurn > 150)
		{
			The.Game.SetIntGameState("OmonporchReady", 1);
			ParentObject.RemovePart(this);
		}
		return true;
	}
}
