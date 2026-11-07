using System;

namespace XRL.World.Parts;

[Serializable]
public class ImmuneToConfusionGas : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanApplyConfusionGas");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyConfusionGas")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
