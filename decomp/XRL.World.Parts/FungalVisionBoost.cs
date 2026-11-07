using System;

namespace XRL.World.Parts;

[Serializable]
public class FungalVisionBoost : IPart
{
	public bool bBoosted;

	public override bool SameAs(IPart p)
	{
		if ((p as FungalVisionBoost).bBoosted != bBoosted)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
