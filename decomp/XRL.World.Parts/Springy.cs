using System;

namespace XRL.World.Parts;

[Serializable]
public class Springy : IPart
{
	public float Factor = 0.5f;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetSpringinessEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSpringinessEvent E)
	{
		if (Factor > 0f)
		{
			E.LinearIncrease += ParentObject.GetWeightTimes(Factor);
		}
		else
		{
			E.LinearReduction += -ParentObject.GetWeightTimes(Factor);
		}
		return base.HandleEvent(E);
	}
}
