using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Hoarshroom_Tonic_Applicator : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage != null)
		{
			if (E.Damage.HasAttribute("Cold"))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					E.ApplyScore(3 * E.Damage.Amount / E.Actor.baseHitpoints - E.Actor.Physics.Temperature / 50);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					E.ApplyScore(3 * E.Damage.Amount / E.Actor.hitpoints - E.Actor.Physics.Temperature / 50);
				}
			}
		}
		else
		{
			double num = E.Actor.Health();
			if (E.ForPermission)
			{
				if (num < 1.0)
				{
					E.ApplyScore(1);
				}
			}
			else if (num < 0.2)
			{
				E.ApplyScore((int)(1.0 / num));
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyTonic");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			int intParameter = E.GetIntParameter("Dosage");
			if (intParameter <= 0)
			{
				return false;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Subject");
			Hoarshroom_Tonic e = new Hoarshroom_Tonic(GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Hoarshroom", 180 * intParameter + Stat.Random(0, 40 * intParameter), intParameter, Healing: true));
			if (!gameObjectParameter2.ApplyEffect(e))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
