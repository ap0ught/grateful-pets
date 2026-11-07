using System;

namespace XRL.World.Parts;

[Serializable]
public class PermuteMutationBuysOnEat : IPart
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
			E.GetGameObjectParameter("Eater")?.PermuteRandomMutationBuys();
		}
		return base.FireEvent(E);
	}
}
