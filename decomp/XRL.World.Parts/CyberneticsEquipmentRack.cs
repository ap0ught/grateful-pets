using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsEquipmentRack : IPart
{
	public string ManagerID => ParentObject.ID + "::CyberneticsEquipmentRack";

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
		BodyPart body = E.Part.ParentBody.GetBody();
		string managerID = ManagerID;
		string[] orInsertBefore = new string[5] { "Missile Weapon", "Hands", "Feet", "Roots", "Thrown Weapon" };
		body.AddPartAt("Equipment Rack", 0, null, null, null, null, managerID, null, null, null, null, null, null, null, null, null, null, null, null, null, "Back", orInsertBefore);
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
