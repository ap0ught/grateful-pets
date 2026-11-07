using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class PoisonBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Poison Gas";
	}

	public override string GetDescription()
	{
		return "You breathe poison gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes poison gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "PoisonGas80";
	}

	public override string GetBreathName()
	{
		return "poison gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "G", "g", "w");
	}
}
