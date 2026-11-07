using System;

namespace XRL.World.Parts;

[Serializable]
public class StressSmokecaster : IPart
{
	public int RangeLow = 170;

	public int RangeHigh = 350;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		for (int i = 0; i < 200; i++)
		{
			ParentObject.Smoke(RangeLow, RangeHigh);
		}
		return base.HandleEvent(E);
	}
}
