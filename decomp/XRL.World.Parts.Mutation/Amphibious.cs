using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Amphibious : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Your skin must be kept moist with fresh water.\n\nYou pour water on yourself rather than drinking it to quench your thirst.\nYou require about two-thirds more water than usual.\n+100 reputation with {{w|frogs}}";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
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
