using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsBionicLiver : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject.Implantee && E.Damage.HasAttribute("Poison"))
		{
			NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
			E.Damage.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "BeforeApplyDamage");
		E.Implantee.RegisterPartEvent(this, "CanApplyPoison");
		E.Implantee.RegisterPartEvent(this, "ApplyPoison");
		E.Implantee.RegisterPartEvent(this, "CanApplyPoisonGasPoison");
		E.Implantee.RegisterPartEvent(this, "ApplyPoisonGasPoison");
		E.Implantee.RegisterPartEvent(this, "CanApplyDisease");
		E.Implantee.RegisterPartEvent(this, "ApplyDisease");
		E.Implantee.RegisterPartEvent(this, "CanApplyDiseaseOnset");
		E.Implantee.RegisterPartEvent(this, "ApplyDiseaseOnset");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "BeforeApplyDamage");
		E.Implantee.UnregisterPartEvent(this, "CanApplyPoison");
		E.Implantee.UnregisterPartEvent(this, "ApplyPoison");
		E.Implantee.UnregisterPartEvent(this, "CanApplyPoisonGasPoison");
		E.Implantee.UnregisterPartEvent(this, "ApplyPoisonGasPoison");
		E.Implantee.UnregisterPartEvent(this, "CanApplyDisease");
		E.Implantee.UnregisterPartEvent(this, "ApplyDisease");
		E.Implantee.UnregisterPartEvent(this, "CanApplyDiseaseOnset");
		E.Implantee.UnregisterPartEvent(this, "ApplyDiseaseOnset");
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyPoison" || E.ID == "ApplyPoison" || E.ID == "CanApplyPoisonGasPoison" || E.ID == "ApplyPoisonGasPoison" || E.ID == "CanApplyDisease" || E.ID == "ApplyDisease" || E.ID == "CanApplyDiseaseOnset" || E.ID == "ApplyDiseaseOnset")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
