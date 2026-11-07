using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Yuckwheat : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			gameObjectParameter.RemoveEffect<Confused>();
			gameObjectParameter.RemoveEffect<Poisoned>();
		}
		return base.FireEvent(E);
	}
}
