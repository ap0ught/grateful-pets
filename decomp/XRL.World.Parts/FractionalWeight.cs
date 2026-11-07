using System;

namespace XRL.World.Parts;

[Serializable]
public class FractionalWeight : IPart
{
	public double Weight;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetIntrinsicWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		E.Weight += Weight;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
