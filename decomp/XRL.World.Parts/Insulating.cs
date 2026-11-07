using System;

namespace XRL.World.Parts;

[Serializable]
public class Insulating : IPart
{
	public float Amount = 0.9f;

	public override bool SameAs(IPart p)
	{
		if ((p as Insulating).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeTemperatureChangeEvent>.ID && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Amount != 1f)
		{
			string text = (int)((1f - Amount) * 100f) + "%";
			E.Postfix.AppendRules("Severity of cooling effects reduced by " + text + ". Cold damage reduced by " + text + ".");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeTemperatureChangeEvent E)
	{
		if (E.Amount < 0 && (E.Object == ParentObject.Equipped || E.Object == ParentObject))
		{
			E.Amount = (int)((float)E.Amount * Amount);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if ((E.Object == ParentObject.Equipped || E.Object == ParentObject) && E.Damage.IsColdDamage())
		{
			E.Damage.Amount = (int)((float)E.Damage.Amount * Amount);
			if (E.Damage.Amount <= 0)
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
