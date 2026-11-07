using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Smokecaster : IPart
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
		if (Stat.Random(1, 60) <= 1 && ParentObject.CurrentCell != null)
		{
			ParentObject.Smoke(RangeLow, RangeHigh);
		}
		return base.HandleEvent(E);
	}
}
