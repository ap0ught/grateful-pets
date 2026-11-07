using System;

namespace XRL.World.Parts;

[Serializable]
public class NoDamageExcept : IPart
{
	public string Except;

	public override bool SameAs(IPart p)
	{
		if ((p as NoDamageExcept).Except == Except)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<CanBeDismemberedEvent>.ID)
		{
			return ID == PooledEvent<GetElectricalConductivityEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject && !Damage.IsElectricDamage(Except))
		{
			E.Value = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject)
		{
			if (Except.IsNullOrEmpty())
			{
				return false;
			}
			if (!E.Damage.HasAttribute(Except))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeDismemberedEvent E)
	{
		if (E.Object == ParentObject)
		{
			if (Except.IsNullOrEmpty())
			{
				return false;
			}
			if (!E.Attributes.IsNullOrEmpty() && Array.IndexOf(E.Attributes.Split(' '), Except) == -1)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
