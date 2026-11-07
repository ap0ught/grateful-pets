using System;
using XRL.Liquids;

namespace XRL.World.Parts;

[Serializable]
public class RandomStatusOnHit : IPart
{
	public int Chance = 100;

	public RandomStatusOnHit()
	{
	}

	public RandomStatusOnHit(int Chance)
		: this()
	{
		this.Chance = Chance;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID.EndsWith("Hit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part RandomStatusOnHit Activation", Chance, subject).in100())
			{
				LiquidWarmStatic.ApplyRandomEffectTo(gameObjectParameter2, gameObjectParameter.GetTier(), EmitMessage: false);
			}
		}
		return base.FireEvent(E);
	}
}
