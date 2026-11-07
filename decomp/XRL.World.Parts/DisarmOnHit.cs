using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class DisarmOnHit : IPart
{
	public int Chance = 100;

	public DisarmOnHit()
	{
	}

	public DisarmOnHit(int Chance)
		: this()
	{
		this.Chance = Chance;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part DisarmOnHit Activation", Chance, subject).in100())
			{
				Disarming.Disarm(gameObjectParameter2, ParentObject, 40, "Strength", "Agility", null, ParentObject);
			}
		}
		return base.FireEvent(E);
	}
}
