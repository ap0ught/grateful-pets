using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Albino : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Regenerating2");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "Your skin, hair, and eyes are absent of pigment.\n\nYou regenerate hit points at one-fifth the usual rate in the daylight.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating2")
		{
			if (!IsDay())
			{
				return true;
			}
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone == null || !currentZone.IsOutside())
			{
				return true;
			}
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 5);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
