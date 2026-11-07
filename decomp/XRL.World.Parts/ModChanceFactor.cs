using System;

namespace XRL.World.Parts;

[Serializable]
public class ModChanceFactor : IPart
{
	public string Mod;

	public float Factor;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetModRarityWeightEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetModRarityWeightEvent E)
	{
		if ((string.IsNullOrEmpty(Mod) || E.Mod.Part == Mod) && E.Object == ParentObject)
		{
			E.FactorAdjustment += Factor;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
