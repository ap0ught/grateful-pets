using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Carnivorous : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You eat meat exclusively.\n\nYou get no satiation from foods that aren't meat.\nIf you eat raw food that isn't meat, there's a 50% chance you become ill for 2 hours.\nYou can't cook with plant or fungus ingredients.\nYou don't get ill when you eat raw meat.\nYou can eat raw meat without being famished.\n";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		return true;
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
