using System;

namespace XRL.World.Parts;

[Serializable]
public class RemoveInterestingOnTake : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AddedToInventoryEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		ParentObject.RemovePart<Interesting>();
		return base.HandleEvent(E);
	}
}
