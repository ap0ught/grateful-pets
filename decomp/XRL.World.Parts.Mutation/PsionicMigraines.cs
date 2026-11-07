using System;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class PsionicMigraines : BaseMutation
{
	public PsionicMigraines()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginEquip");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You suffer from powerful psionic migraines that render your head extremely sensitive.\n\nYou can't wear hats or helmets.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginEquip" && (E.GetParameter("BodyPart") as BodyPart).Type == "Head")
		{
			if (ParentObject.IsPlayer() && !E.HasParameter("AutoEquipTry") && !E.IsSilent())
			{
				Popup.Show("Your head is too sensitive to wear that.");
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Body body = GO.Body;
		if (body != null)
		{
			foreach (BodyPart item in body.GetPart("Head"))
			{
				item.UnequipPartAndChildren();
			}
		}
		return base.Mutate(GO, Level);
	}
}
