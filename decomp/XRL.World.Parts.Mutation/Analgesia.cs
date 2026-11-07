using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Analgesia : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "You lack a developed sense of pain.\n\nYou only know your general state of health and not your precise number of hit points.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		GO.ModIntProperty("Analgesia", 1);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.ModIntProperty("Analgesia", -1, RemoveIfZero: true);
		return base.Unmutate(GO);
	}
}
