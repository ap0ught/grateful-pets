using System;

namespace XRL.World.Parts;

[Serializable]
public class SpecialItem : IPart
{
	public bool RemovePlayerTake = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (RemovePlayerTake && (ID == TakenEvent.ID || ID == EquippedEvent.ID))
		{
			return true;
		}
		return base.WantEvent(ID, cascade);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		if (RemovePlayerTake && E.Actor != null && E.Actor.IsPlayerControlled())
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (RemovePlayerTake && E.Actor != null && E.Actor.IsPlayerControlled())
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}
}
