using System;

namespace XRL.World.Parts;

[Serializable]
public class HealOnEat : IPart
{
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
		if (!ParentObject.IsImportant() && E.Actor.HasPart<Stomach>())
		{
			int hitpoints = E.Actor.hitpoints;
			int baseHitpoints = E.Actor.baseHitpoints;
			if (hitpoints < baseHitpoints)
			{
				int num = 0;
				int num2 = baseHitpoints - hitpoints;
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
			if (gameObjectParameter != null && gameObjectParameter.HasStat("Hitpoints"))
			{
				if (gameObjectParameter.Statistics["Hitpoints"].Penalty > 0)
				{
					gameObjectParameter.Heal(gameObjectParameter.Statistics["Hitpoints"].Penalty, Message: true, FloatText: true);
				}
				gameObjectParameter.FireEvent("Recuperating");
			}
		}
		return base.FireEvent(E);
	}
}
