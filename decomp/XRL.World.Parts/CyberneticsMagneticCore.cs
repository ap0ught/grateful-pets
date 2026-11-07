using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMagneticCore : IPart
{
	public string ManagerID => ParentObject.ID + "::CyberneticsMagneticCore";

	public override bool SameAs(IPart p)
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
		E.Part.ParentBody.GetBody().AddPart("Floating Nearby", 0, null, null, null, null, ManagerID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
