using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SociallyRepugnant : BaseMutation
{
	[NonSerialized]
	public static string[] waterRitualOptions = new string[8] { "Swap spittle?", "Let's get to it.", "{{emote|licks lips}}", "My thirst is mine, your water is yours.", "Thirst. Water. Drink.", "Slurp time.", "????", "My water is thirst, your water is yours" };

	public int appliedBonus;

	public SociallyRepugnant()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Others find it difficult to tolerate you in social settings.\n\n-50 reputation with every faction";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		GO.ModIntProperty("AllVisibleRepModifier", -50);
		appliedBonus = -50;
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.ModIntProperty("AllVisibleRepModifier", -appliedBonus);
		appliedBonus = 0;
		return base.Unmutate(GO);
	}
}
