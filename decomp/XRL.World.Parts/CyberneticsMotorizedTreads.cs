using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMotorizedTreads : IPart
{
	public string PartName;

	public string PartDescription;

	public string PartDependsOn;

	public int PartLaterality;

	public int PartMobility;

	public bool PartIntegral;

	public bool PartPlural;

	public bool PartMass;

	public int PartCategory;

	public string AdditionsManagerID => ParentObject.ID + "::MotorizedTreads::Add";

	public string ChangesManagerID => ParentObject.ID + "::MotorizedTreads::Change";

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
		BodyPart part = E.Part;
		int? num = 6;
		string additionsManagerID = AdditionsManagerID;
		int? category = num;
		bool? integral = true;
		BodyPart insertAfter = part.AddPartAt("Tread", 2, null, null, null, null, additionsManagerID, category, null, null, null, integral, null, null, null, null, null, null, null, null, "Tread", "Fungal Outcrop");
		BodyPart part2 = E.Part;
		num = 6;
		string additionsManagerID2 = AdditionsManagerID;
		int? category2 = num;
		integral = true;
		part2.AddPartAt(insertAfter, "Tread", 1, null, null, null, null, additionsManagerID2, category2, null, null, null, integral);
		E.Part.Manager = ChangesManagerID;
		if (!E.ForDeepCopy)
		{
			PartName = E.Part.Name;
			PartDescription = E.Part.Description;
			PartDependsOn = E.Part.DependsOn;
			PartLaterality = E.Part.Laterality;
			PartMobility = E.Part.Mobility;
			PartIntegral = E.Part.Integral;
			PartPlural = E.Part.Plural;
			PartMass = E.Part.Mass;
			PartCategory = E.Part.Category;
			E.Part.Name = "lower body";
			E.Part.Description = "Lower Body";
			E.Part.DependsOn = null;
			E.Part.Laterality = 0;
			E.Part.Mobility = 0;
			E.Part.Integral = true;
			E.Part.Plural = false;
			E.Part.Mass = false;
			E.Part.Category = 6;
		}
		E.Implantee.WantToReequip();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		BodyPart bodyPartByManager = E.Implantee.GetBodyPartByManager(ChangesManagerID, EvenIfDismembered: true);
		if (bodyPartByManager != null)
		{
			bodyPartByManager.Name = PartName;
			bodyPartByManager.Description = PartDescription;
			bodyPartByManager.DependsOn = PartDependsOn;
			bodyPartByManager.Laterality = PartLaterality;
			bodyPartByManager.Mobility = PartMobility;
			bodyPartByManager.Integral = PartIntegral;
			bodyPartByManager.Plural = PartPlural;
			bodyPartByManager.Mass = PartMass;
			bodyPartByManager.Category = PartCategory;
		}
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
