using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Blaze_Tonic_Applicator : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

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
			if (E.Damage.HasAttribute("Heat"))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					E.ApplyScore(30 * E.Damage.Amount / E.Actor.baseHitpoints);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					E.ApplyScore(30 * E.Damage.Amount / E.Actor.hitpoints);
				}
			}
		}
		else if (E.Actor.IsFrozen())
		{
			E.ApplyScore(100);
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
			Blaze_Tonic e = new Blaze_Tonic(GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Blaze", Stat.Random(41 * intParameter, 50 * intParameter), intParameter));
			if (!gameObjectParameter2.ApplyEffect(e))
			{
				return false;
			}
			gameObjectParameter2.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_blazeTonic");
		}
		return base.FireEvent(E);
	}
}
