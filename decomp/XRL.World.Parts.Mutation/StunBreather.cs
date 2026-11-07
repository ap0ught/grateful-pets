using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class StunBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Stun Gas";
	}

	public override string GetDescription()
	{
		return "You breathe stun gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes stun gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "StunGas80";
	}

	public override string GetBreathName()
	{
		return "stun gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "C", "c", "y");
	}
}
