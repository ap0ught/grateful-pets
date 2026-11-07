using System;

namespace XRL.World.Parts;

[Serializable]
public class CreateObjectOnHit : IPart
{
	public string Blueprint = "";

	public int Chance = 100;

	public int Charges = 1;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "AttackerHit" || E.ID == "ProjectileHit" || E.ID == "WeaponHit" || E.ID == "WeaponThrowHit") && !Blueprint.IsNullOrEmpty() && Charges > 0)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter2 == null || gameObjectParameter2.CurrentCell == null)
			{
				return true;
			}
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part CreateObjectOnHit Activation", Chance, subject).in100())
			{
				gameObjectParameter2?.CurrentCell?.AddObject(Blueprint);
				Charges--;
			}
		}
		return base.FireEvent(E);
	}
}
