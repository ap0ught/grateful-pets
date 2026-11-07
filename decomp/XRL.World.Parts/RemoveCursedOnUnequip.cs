using System;

namespace XRL.World.Parts;

[Serializable]
public class RemoveCursedOnUnequip : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		ParentObject.RemovePart<Cursed>();
		return base.HandleEvent(E);
	}
}
