using System;

namespace XRL.World.Parts;

[Serializable]
public class SlogTrophy : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterEvent(this, BeforeApplyDamageEvent.ID, 0, Serialize: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterEvent(this, BeforeApplyDamageEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Damage.HasAttribute("Poison"))
		{
			E.Damage.Amount /= 2;
			if (E.Damage.Amount <= 0)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}
}
