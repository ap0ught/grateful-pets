using System;

namespace XRL.World.Parts;

[Serializable]
public class RequireSlots : IPart
{
	public int Base;

	public int Increases;

	public int Decreases;

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == PooledEvent<GetSlotsRequiredEvent>.ID;
	}

	public override bool HandleEvent(GetSlotsRequiredEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.Base += Base;
			E.Increases += Increases;
			E.Decreases += Decreases;
		}
		return base.HandleEvent(E);
	}
}
