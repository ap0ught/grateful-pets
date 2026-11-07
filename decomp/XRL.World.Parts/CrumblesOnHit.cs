using System;

namespace XRL.World.Parts;

[Serializable]
public class CrumblesOnHit : IPart
{
	public int Chance = 100;

	[NonSerialized]
	private bool Crumbling;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
		Registrar.Register("WeaponAfterAttack");
		Registrar.Register("ThrownProjectileHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponThrowHit")
		{
			if (Chance.in100())
			{
				Crumbling = true;
			}
		}
		else if (Crumbling && (E.ID == "WeaponAfterAttack" || E.ID == "ThrownProjectileHit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				DidX("crumble", "to dust", "!", null, null, null, gameObjectParameter);
				SoundManager.PlaySound("Sounds/Damage/sfx_destroy_stone");
			}
			ParentObject.Destroy(null, Silent: true);
		}
		return base.FireEvent(E);
	}
}
