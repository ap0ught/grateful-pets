using System;

namespace XRL.World.Parts;

[Serializable]
public class Backpack : IPart
{
	public int WeightWhenWorn = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as Backpack).WeightWhenWorn != WeightWhenWorn)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AdjustTotalWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AdjustTotalWeightEvent E)
	{
		if (ParentObject.Equipped != null)
		{
			E.Weight = E.Weight - E.BaseWeight + (double)WeightWhenWorn;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
