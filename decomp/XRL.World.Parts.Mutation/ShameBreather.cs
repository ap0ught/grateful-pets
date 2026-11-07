using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ShameBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Shame Gas";
	}

	public override string GetDescription()
	{
		return "You breathe shame gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes shame gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "ShameGas80";
	}

	public override string GetBreathName()
	{
		return "shame gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "B", "b", "C");
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyShamed");
		Registrar.Register("ApplyShameGas");
		Registrar.Register("CanApplyShamed");
		Registrar.Register("CanApplyShameGas");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyShamed" || E.ID == "CanApplyShameGas" || E.ID == "ApplyShamed" || E.ID == "ApplyShameGas")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
