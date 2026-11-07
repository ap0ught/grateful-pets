using System;

namespace XRL.World.Parts;

[Serializable]
public class MemorialPlacement : IPart
{
	public int FillOrder;

	public override bool SameAs(IPart Part)
	{
		if ((Part as MemorialPlacement).FillOrder != FillOrder)
		{
			return false;
		}
		return base.SameAs(Part);
	}
}
