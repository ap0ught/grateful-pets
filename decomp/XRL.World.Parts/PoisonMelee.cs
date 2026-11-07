using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PoisonMelee : IPart
{
	public int Chance = 25;

	public int Strength = 10;

	public string DamageIncrement = "3d3";

	public string Duration = "3d3";

	public override bool SameAs(IPart p)
	{
		PoisonMelee poisonMelee = p as PoisonMelee;
		if (poisonMelee.Chance != Chance)
		{
			return false;
		}
		if (poisonMelee.Strength != Strength)
		{
			return false;
		}
		if (poisonMelee.DamageIncrement != DamageIncrement)
		{
			return false;
		}
		if (poisonMelee.Duration != Duration)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage" && Chance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && !gameObjectParameter.MakeSave("Toughness", 10 + Strength, null, null, "Melee Injected Damaging Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				gameObjectParameter.ApplyEffect(new Poisoned(Duration.RollCached(), DamageIncrement, Strength, ParentObject.Physics.Equipped));
			}
		}
		return base.FireEvent(E);
	}
}
