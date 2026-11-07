using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AISitting : AIBehaviorPart
{
	public bool bFirst = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && bFirst)
		{
			bFirst = false;
			ParentObject.ApplyEffect(new Sitting());
		}
		return true;
	}
}
