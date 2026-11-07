using System;

namespace XRL.World.Parts;

[Serializable]
public class NoRust : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyRusted");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyRusted")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
