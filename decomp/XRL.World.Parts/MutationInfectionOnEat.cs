using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class MutationInfectionOnEat : IPart
{
	public string Duration = "2400-3600";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			E.GetGameObjectParameter("Eater")?.ApplyEffect(new MutationInfection(Stat.Roll(Duration)));
			return true;
		}
		return base.FireEvent(E);
	}
}
