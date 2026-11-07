using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete("Replaced by GameUnique")]
public class Eskhind : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		ParentObject.RemovePart(this);
		The.Game.RemoveStringGameState("EskhindMoved");
		return base.HandleEvent(E);
	}
}
