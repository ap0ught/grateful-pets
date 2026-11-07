using System;

namespace XRL.World.Parts;

[Serializable]
public class NotNameable : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == CanBeNamedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeNamedEvent E)
	{
		if (E.Item == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
