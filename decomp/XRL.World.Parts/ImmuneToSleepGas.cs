using System;

namespace XRL.World.Parts;

[Serializable]
public class ImmuneToSleepGas : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanApplySleepGas");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplySleepGas")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
