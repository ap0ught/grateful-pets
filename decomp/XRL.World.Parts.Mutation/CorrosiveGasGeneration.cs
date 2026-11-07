using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class CorrosiveGasGeneration : GasGeneration
{
	public CorrosiveGasGeneration()
		: base("AcidGas")
	{
	}

	public override int GetReleaseDuration(int Level)
	{
		return Level + 2;
	}

	public override int GetReleaseCooldown(int Level)
	{
		return 40;
	}

	public override string GetReleaseAbilityName()
	{
		return "Release Corrosive Gas";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("salt", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}
}
