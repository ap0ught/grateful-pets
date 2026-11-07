using System;
using XRL.Liquids;

namespace XRL.World.Parts;

[Serializable]
public class BoomOnHit : IPart
{
	public int Chance;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter2, ParentObject, "Part BoomOnHit Activation", Chance).in100())
			{
				LiquidNeutronFlux.Explode(gameObjectParameter, gameObjectParameter2);
			}
		}
		return base.FireEvent(E);
	}
}
