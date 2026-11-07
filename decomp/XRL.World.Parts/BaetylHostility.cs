using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class BaetylHostility : IPart
{
	public double AngerThreshold = 0.2;

	public int ExplodeForce = 25000;

	public string ExplodeDamage = "10d10+75";

	public bool ExplodeNeutron = true;

	public string LeaveBehind = "Space-Time Vortex";

	public string Message = "";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckBaetylHostility();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanBeAngeredByBeingAttacked");
		Registrar.Register("CanBeAngeredByDamage");
		Registrar.Register("CanBeAngeredByFriendlyFire");
		Registrar.Register("CanBeAngeredByPropertyCrime");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "CanBeAngeredByBeingAttacked" || E.ID == "CanBeAngeredByDamage" || E.ID == "CanBeAngeredByFriendlyFire" || E.ID == "CanBeAngeredByPropertyCrime") && ParentObject.Health() > AngerThreshold)
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public void CheckBaetylHostility()
	{
		bool flag = IComponent<GameObject>.ThePlayer != null && ParentObject.IsHostileTowards(IComponent<GameObject>.ThePlayer);
		if ((flag || ParentObject.Target != null) && ParentObject.Health() <= AngerThreshold)
		{
			if (flag && !string.IsNullOrEmpty(Message) && Visible())
			{
				Popup.Show(Message);
			}
			Cell cell = ParentObject.CurrentCell;
			ParentObject.FireEvent("BaetylLeaving");
			if (ExplodeForce > 0)
			{
				DidX("explode", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
				ParentObject.Explode(ExplodeForce, null, ExplodeDamage, 1f, ExplodeNeutron);
			}
			else
			{
				DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
				ParentObject.Obliterate();
			}
			if (!string.IsNullOrEmpty(LeaveBehind))
			{
				cell.AddObject(LeaveBehind);
			}
		}
	}
}
