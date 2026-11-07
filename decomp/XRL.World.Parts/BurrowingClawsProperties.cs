using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

public class BurrowingClawsProperties : IPart
{
	public int WallBonusPenetration;

	public double WallBonusPercentage;

	[NonSerialized]
	private BurrowingClaws Mutation;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetWeaponHitDiceEvent.ID)
		{
			return ID == PooledEvent<PreferDefaultBehaviorEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetWeaponHitDiceEvent E)
	{
		if (E.Defender.IsDiggable())
		{
			E.PenetrationBonus += WallBonusPenetration;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PreferDefaultBehaviorEvent E)
	{
		if (E.Part?.DefaultBehavior == ParentObject)
		{
			GameObject target = E.Target;
			if (target != null && target.IsDiggable() && (Mutation != null || E.Actor.TryGetPart<BurrowingClaws>(out Mutation)) && Mutation.IsMyActivatedAbilityToggledOn(Mutation.EnableActivatedAbilityID))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && gameObjectParameter.IsDiggable())
			{
				Damage parameter = E.GetParameter<Damage>("Damage");
				int num = (int)Math.Floor((double)gameObjectParameter.baseHitpoints * WallBonusPercentage / 100.0);
				if (num >= parameter.Amount)
				{
					parameter.Amount = num;
					CombatJuice.playPrefabAnimation(gameObjectParameter, "Impacts/ImpactVFXDig");
					gameObjectParameter.PlayWorldSound("sfx_ability_mutation_burrowingClaws_burrow");
				}
			}
		}
		return base.FireEvent(E);
	}
}
