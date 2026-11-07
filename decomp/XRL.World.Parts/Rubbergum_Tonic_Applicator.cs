using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Rubbergum_Tonic_Applicator : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	private void ApplyModifiedScore(GetUtilityScoreEvent E, int Score)
	{
		Bleeding effect = E.Actor.GetEffect<Bleeding>();
		if (effect != null)
		{
			Score -= (effect.Damage.RollMinCached() + effect.Damage.RollMaxCached()) / 2;
		}
		if (Score > 0)
		{
			E.ApplyScore(Score);
		}
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage != null)
		{
			if (E.Damage.IsElectricDamage())
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					ApplyModifiedScore(E, 30 * E.Damage.Amount / E.Actor.baseHitpoints);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					ApplyModifiedScore(E, 30 * E.Damage.Amount / E.Actor.hitpoints);
				}
			}
			else if (Rubbergum_Tonic.AffectsDamage(E.Damage))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					ApplyModifiedScore(E, 20 * E.Damage.Amount / E.Actor.baseHitpoints);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					ApplyModifiedScore(E, 20 * E.Damage.Amount / E.Actor.hitpoints);
				}
			}
			else if (E.Damage.HasAttribute("Cold"))
			{
				if (E.Damage.Amount >= E.Actor.baseHitpoints / 2)
				{
					ApplyModifiedScore(E, 10 * E.Damage.Amount / E.Actor.baseHitpoints - E.Actor.Physics.Temperature / 50);
				}
				else if (E.Damage.Amount >= E.Actor.hitpoints * 2 / 3)
				{
					ApplyModifiedScore(E, 10 * E.Damage.Amount / E.Actor.hitpoints - E.Actor.Physics.Temperature / 50);
				}
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
			Rubbergum_Tonic e = new Rubbergum_Tonic(GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Rubbergum", Stat.Random(intParameter, 10 * intParameter) + 40 * intParameter, intParameter));
			return gameObjectParameter2.ApplyEffect(e);
		}
		return base.FireEvent(E);
	}
}
