using System;

namespace XRL.World.Parts;

[Serializable]
public class SetBooleanStateOnRead : IPart
{
	public string State;

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == PooledEvent<AfterReadBookEvent>.ID;
	}

	public override bool HandleEvent(AfterReadBookEvent E)
	{
		if (!State.IsNullOrEmpty())
		{
			The.Game.SetBooleanGameState(State, Value: true);
		}
		return base.HandleEvent(E);
	}
}
