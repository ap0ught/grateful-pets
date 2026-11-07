using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPalladiumElectrodeposits : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetAvailableComputePowerEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (E.Part?.Type == "Head")
		{
			StatShifter.SetStatShift(E.Implantee, "Intelligence", 2, baseValue: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		StatShifter.RemoveStatShifts(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		if (E.Actor == ParentObject.Implantee)
		{
			E.Amount += 20;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
