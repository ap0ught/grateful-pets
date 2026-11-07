using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IPlayerPart : IPart
{
	public override IPart DeepCopy(GameObject Parent)
	{
		return null;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == PooledEvent<AfterPlayerBodyChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (E.OldBody == ParentObject)
		{
			E.OldBody.RemovePart(this);
		}
		if (E.NewBody != null && !E.NewBody.PartsList.Contains(this))
		{
			E.NewBody.AddPart(this);
		}
		return base.HandleEvent(E);
	}
}
