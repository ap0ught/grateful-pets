using System;

namespace XRL.World.Parts;

[Serializable]
public class DosageChange : IPart
{
	public int Amount = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as DosageChange).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetTonicDosageEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTonicDosageEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.Dosage += Amount;
		}
		return base.HandleEvent(E);
	}
}
