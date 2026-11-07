using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class SlogGladsItem : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Unequipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Unequipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("UnequippingObject");
			gameObjectParameter?.GetPart<SlogGlands>()?.Unmutate(gameObjectParameter);
		}
		return base.FireEvent(E);
	}
}
