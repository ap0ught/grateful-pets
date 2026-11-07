using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PoisonOnEat : IPart
{
	public int Duration = 40;

	public string Damage = "2d6";

	public int Strength = 30;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			E.GetGameObjectParameter("Eater").ApplyEffect(new Poisoned(Duration, Damage, Strength));
		}
		return base.FireEvent(E);
	}
}
