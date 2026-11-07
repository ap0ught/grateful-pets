using System;

namespace XRL.World.Parts;

[Serializable]
public class TakenWXU : IPart
{
	public int WXU = 1;

	public bool Triggered;

	public TakenWXU()
	{
	}

	public TakenWXU(int WXU)
	{
		this.WXU = WXU;
	}

	public override bool SameAs(IPart p)
	{
		TakenWXU takenWXU = p as TakenWXU;
		if (takenWXU.WXU != WXU)
		{
			return false;
		}
		if (takenWXU.Triggered != Triggered)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IActOnItemEvent E)
	{
		if (!Triggered && E.Actor != null && E.Actor.IsPlayer())
		{
			Triggered = true;
			WanderSystem.AwardWXU(WXU);
		}
		return true;
	}
}
