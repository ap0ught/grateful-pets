using System;

namespace XRL.World.Parts;

[Serializable]
public class DisabledNaturalHealing : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Regenerating");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating")
		{
			E.SetParameter("Amount", 0);
		}
		return base.FireEvent(E);
	}
}
