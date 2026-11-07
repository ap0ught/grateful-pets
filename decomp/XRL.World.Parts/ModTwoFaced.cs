using System;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModTwoFaced : IModification
{
	public int ExtraFaceID;

	public ModTwoFaced()
	{
	}

	public ModTwoFaced(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Head");
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyIfComplex(1);
	}

	public override bool SameAs(IPart Part)
	{
		if ((Part as ModTwoFaced).ExtraFaceID != ExtraFaceID)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDismemberEvent>.ID && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDismemberEvent E)
	{
		if (E.IsObject && E.Part != null && E.Part.IDMatch(ExtraFaceID))
		{
			ParentObject.ApplyEffect(new Broken());
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		AddFace(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		RemoveFace(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("two-faced");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static string GetDescription(int Tier)
	{
		return "Two-faced: This item grants an additional face slot.";
	}

	public void AddFace(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		if (Subject != null)
		{
			BodyPart bodyPart = ParentObject.EquippedOn();
			if (bodyPart != null)
			{
				int? category = BodyPartCategory.BestGuessForCategoryDerivedFromGameObject(ParentObject);
				bool? extrinsic = true;
				string[] orInsertBefore = new string[2] { "Fungal Outcrop", "Icy Outcrop" };
				BodyPart bodyPart2 = bodyPart.AddPartAt("Extra Face", 0, null, null, null, null, null, category, null, null, null, null, null, null, extrinsic, null, null, null, null, null, "Face", orInsertBefore);
				ExtraFaceID = bodyPart2.ID;
				Subject.WantToReequip();
			}
		}
	}

	public void RemoveFace(GameObject Subject = null)
	{
		if (ExtraFaceID != 0)
		{
			if (Subject == null)
			{
				Subject = ParentObject.Equipped;
			}
			Subject?.Body?.RemovePartByID(ExtraFaceID);
			ExtraFaceID = 0;
			Subject?.WantToReequip();
		}
	}
}
