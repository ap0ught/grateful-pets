using System;
using System.Text;
using XRL.Liquids;

namespace XRL.World.Parts;

[Serializable]
public class VomitOnHit : IPart
{
	public int Hurls = 1;

	public int Chance = 100;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("DealingMissileDamage");
		Registrar.Register("WeaponMissileWeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "AttackerAfterDamage" || E.ID == "DealingMissileDamage" || E.ID == "WeaponMissileWeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter3;
				int chance = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part VomitOnHit Activation", Chance, subject, projectile);
				if (chance.in100())
				{
					StringBuilder stringBuilder = Event.NewStringBuilder();
					BaseLiquid liquid = LiquidVolume.GetLiquid("putrid");
					for (int num = Hurls; num > 0; num--)
					{
						liquid.Drank(null, 0, Object, stringBuilder);
						if (!chance.in100())
						{
							break;
						}
					}
					if (stringBuilder.Length > 0)
					{
						IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString());
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
