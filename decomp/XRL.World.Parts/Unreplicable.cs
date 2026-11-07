using System;

namespace XRL.World.Parts;

[Serializable]
public class Unreplicable : IPart
{
	public bool AllowTemporary;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == CanBeReplicatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeReplicatedEvent E)
	{
		if (!AllowTemporary || !E.Temporary)
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
