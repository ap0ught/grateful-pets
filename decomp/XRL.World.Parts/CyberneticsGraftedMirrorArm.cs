using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsGraftedMirrorArm : IPart
{
	public string ManagerID => ParentObject.ID + "::CyberneticsGraftedMirrorArm";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
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
		E.Part.ParentBody.GetBody().AddPartAt("Thrown Weapon", 0, (string)null, (string)null, (string)null, (string)null, ManagerID, (int?)null, (int?)null, (int?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, (bool?)null, "Thrown Weapon", (string)null, true);
		E.Implantee.WantToReequip();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		E.Implantee.WantToReequip();
		return base.HandleEvent(E);
	}
}
