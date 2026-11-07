using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsReactiveCranialPlating : IPart
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
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "CanApplyDazed");
		E.Implantee.RegisterPartEvent(this, "ApplyDazed");
		E.Implantee.RegisterPartEvent(this, "CanApplyStun");
		E.Implantee.RegisterPartEvent(this, "ApplyStun");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "CanApplyDazed");
		E.Implantee.UnregisterPartEvent(this, "ApplyDazed");
		E.Implantee.UnregisterPartEvent(this, "CanApplyStun");
		E.Implantee.UnregisterPartEvent(this, "ApplyStun");
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyDazed" || E.ID == "ApplyDazed" || E.ID == "CanApplyStun" || E.ID == "ApplyStun")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
