using System;

namespace XRL.World.Parts;

[Serializable]
public class AdjustLiquidWeightWhileWorn : IPart
{
	public float Factor;

	public override bool SameAs(IPart p)
	{
		if ((p as AdjustLiquidWeightWhileWorn).Factor != Factor)
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
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null)
			{
				double liquidWeight = liquidVolume.GetLiquidWeight();
				if (liquidWeight != 0.0)
				{
					E.Weight -= liquidWeight;
					if (Factor != 0f)
					{
						E.Weight += liquidWeight * (double)Factor;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
