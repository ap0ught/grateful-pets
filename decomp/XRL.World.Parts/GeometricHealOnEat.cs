using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GeometricHealOnEat : IPart
{
	public string Amount;

	public string Ratio;

	public string Duration;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetDefensiveItemListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (!ParentObject.IsImportant() && E.Actor.HasPart<Stomach>() && !E.Actor.HasEffect<GeometricHeal>())
		{
			int hitpoints = E.Actor.hitpoints;
			int baseHitpoints = E.Actor.baseHitpoints;
			if (hitpoints < baseHitpoints)
			{
				int num = 0;
				int num2 = baseHitpoints - hitpoints;
				if (num2 >= baseHitpoints * 7 / 10)
				{
					num++;
				}
				if (num2 >= baseHitpoints * 8 / 10)
				{
					num++;
				}
				if (num2 >= baseHitpoints * 9 / 10)
				{
					num++;
				}
				if (num > 0)
				{
					E.Add("Eat", num, ParentObject, Inv: true, Self: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter != null && CanApplyEffectEvent.Check<GeometricHeal>(gameObjectParameter))
			{
				gameObjectParameter.ApplyEffect(new GeometricHeal(Stat.Roll(Amount), Stat.Roll(Ratio), Stat.Roll(Duration)));
			}
		}
		return base.FireEvent(E);
	}
}
