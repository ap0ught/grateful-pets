using System;

namespace XRL.World.Parts;

[Serializable]
public class AISuppressIndependentBehavior : AIBehaviorPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanAIDoIndependentBehavior");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanAIDoIndependentBehavior")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
