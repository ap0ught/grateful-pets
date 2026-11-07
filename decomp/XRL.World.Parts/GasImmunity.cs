using System;

namespace XRL.World.Parts;

[Serializable]
public class GasImmunity : IPart
{
	public string GasType;

	public override bool SameAs(IPart p)
	{
		if (p is GasImmunity gasImmunity)
		{
			return gasImmunity.GasType == GasType;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<CheckGasCanAffectEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckGasCanAffectEvent E)
	{
		if (E.Gas.GasType == GasType)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
