using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ThickFur : BaseMutation
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
		return string.Concat("" + "You are covered in a thick coat of fur, which protects you from the elements.\n\n", "+5 Heat Resistance\n+5 Cold Resistance\n+100 reputation with {{w|apes}}, {{w|baboons}}, {{w|bears}}, and {{w|grazing hedonists}}");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetExtraPhysicalFeaturesEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetExtraPhysicalFeaturesEvent E)
	{
		E.Features.Add("thick fur");
		return base.HandleEvent(E);
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
		StatShifter.SetStatShift(GO, "HeatResistance", 5, baseValue: true);
		StatShifter.SetStatShift(GO, "ColdResistance", 5, baseValue: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts(GO);
		return base.Unmutate(GO);
	}
}
