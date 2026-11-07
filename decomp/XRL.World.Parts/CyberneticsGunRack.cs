using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsGunRack : IPart
{
	public string ManagerID => ParentObject.ID + "::CyberneticsGunRack";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == PooledEvent<BeforeDismemberEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		BodyPart body = E.Part.ParentBody.GetBody();
		int? num = 6;
		string managerID = ManagerID;
		int? category = num;
		string[] orInsertBefore = new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" };
		bool? integral = true;
		BodyPart insertAfter = body.AddPartAt("Hardpoint", 2, null, null, null, null, managerID, category, null, null, null, integral, null, null, null, null, null, null, null, null, "Missile Weapon", orInsertBefore);
		num = 6;
		string managerID2 = ManagerID;
		int? category2 = num;
		integral = true;
		body.AddPartAt(insertAfter, "Hardpoint", 1, null, null, null, null, managerID2, category2, null, null, null, integral);
		E.Implantee.WantToReequip();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Part.ParentBody.RemovePartsByManager(ManagerID, EvenIfDismembered: true);
		E.Implantee.WantToReequip();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDismemberEvent E)
	{
		if (E.IsObject && E.Part?.Cybernetics == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
