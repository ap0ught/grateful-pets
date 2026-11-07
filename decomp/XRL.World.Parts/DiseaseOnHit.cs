using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class DiseaseOnHit : IPart
{
	public int Chance;

	public int Strength = 25;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (Object.IsCreature)
		{
			Registrar.Register("AttackerHit");
			return;
		}
		if (Object.IsProjectile)
		{
			Registrar.Register("ProjectileHit");
			return;
		}
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID.EndsWith("Hit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part DiseaseOnHit Activation", Chance, subject).in100() && gameObjectParameter2 != null && !gameObjectParameter2.MakeSave("Toughness", Strength, null, null, "Disease", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, gameObjectParameter2))
			{
				if ((ParentObject.GetCurrentCell()?.ParentZone.Z ?? 0) % 2 == 0)
				{
					gameObjectParameter2.ApplyEffect(new IronshankOnset());
				}
				else
				{
					gameObjectParameter2.ApplyEffect(new GlotrotOnset());
				}
			}
		}
		return base.FireEvent(E);
	}
}
