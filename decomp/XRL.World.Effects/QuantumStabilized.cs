using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class QuantumStabilized : Effect
{
	public QuantumStabilized()
	{
		Duration = 1;
		DisplayName = "{{m|quantum-locked}}";
	}

	public QuantumStabilized(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 83886336;
	}

	public override string GetDetails()
	{
		return "Stable from implosion.\nNothing can emerge.";
	}

	public override bool Apply(GameObject Object)
	{
		SpaceTimeVortex partDescendedFrom = Object.GetPartDescendedFrom<SpaceTimeVortex>();
		if (partDescendedFrom == null)
		{
			return false;
		}
		if (partDescendedFrom.SpaceTimeAnomalyEmergencePermillageBaseChance() <= 0)
		{
			return false;
		}
		if (partDescendedFrom.SpaceTimeAnomalyEmergenceExplodePercentageBaseChance() <= 0)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent>.ID)
		{
			return ID == PooledEvent<GetSpaceTimeAnomalyEmergencePermillageChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent E)
	{
		E.Chance = 0;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSpaceTimeAnomalyEmergencePermillageChanceEvent E)
	{
		E.Chance = 0;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			E.AddAdjective("stable");
		}
		return base.HandleEvent(E);
	}
}
