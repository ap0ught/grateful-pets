using System;

namespace XRL.World.Parts;

[Serializable]
public class QuenchOnHit : IPart
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
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			GameObject projectile = gameObjectParameter3;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part QuenchOnHit Activation", Chance, subject, projectile).in100() && !gameObjectParameter2.FireEvent(new Event("AddWater", "Amount", 60000, "Forced", 1)))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
