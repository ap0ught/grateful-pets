using System;

namespace XRL.World.Parts;

[Serializable]
public class TemperatureOnEat : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			E.GetGameObjectParameter("Eater").Physics.Temperature += 100;
		}
		return base.FireEvent(E);
	}
}
