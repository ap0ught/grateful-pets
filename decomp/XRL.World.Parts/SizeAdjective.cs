using System;

namespace XRL.World.Parts;

[Serializable]
public class SizeAdjective : IPart
{
	public string Adjective;

	public int DescriptionPriority = 20;

	public int OrderAdjust;

	public override bool SameAs(IPart p)
	{
		SizeAdjective sizeAdjective = p as SizeAdjective;
		if (sizeAdjective.Adjective != Adjective)
		{
			return false;
		}
		if (sizeAdjective.DescriptionPriority != DescriptionPriority)
		{
			return false;
		}
		if (sizeAdjective.OrderAdjust != OrderAdjust)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.ApplySizeAdjective(Adjective, DescriptionPriority, OrderAdjust);
		return base.HandleEvent(E);
	}
}
