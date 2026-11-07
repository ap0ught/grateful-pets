using System;

namespace XRL.World.Parts;

[Serializable]
public class SmartuseTwiddles : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanSmartUse");
		Registrar.Register("CommandSmartUse");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUse")
		{
			ParentObject.Twiddle();
		}
		return base.FireEvent(E);
	}
}
