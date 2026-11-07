using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SphynxSalt_Tonic_Applicator : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyTonic");
		Registrar.Register("GameRestored");
		base.Register(Object, Registrar);
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
		if (E.ForPermission)
		{
			E.ApplyScore(1);
		}
		else if (E.Damage != null)
		{
			if (E.Damage.Amount >= E.Actor.baseHitpoints * 2 / 3)
			{
				E.ApplyScore(8 * E.Damage.Amount / E.Actor.baseHitpoints);
			}
			else if (E.Damage.Amount >= E.Actor.hitpoints * 3 / 4)
			{
				E.ApplyScore(8 * E.Damage.Amount / E.Actor.hitpoints);
			}
		}
		else if (E.Actor.HasEffect<Confused>())
		{
			E.ApplyScore(100);
		}
		else
		{
			double num = E.Actor.Health();
			if (num < 0.1)
			{
				E.ApplyScore((int)(0.7 / num));
			}
		}
		return base.HandleEvent(E);
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
			int duration = GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "SphynxSalt", Stat.Random(18 * intParameter, 22 * intParameter), intParameter);
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Owner");
			if (gameObjectParameter3 != null && !gameObjectParameter3.IsPlayer() && IComponent<GameObject>.Visible(gameObjectParameter3) && !E.HasFlag("External") && !E.HasFlag("Involuntary"))
			{
				IComponent<GameObject>.AddPlayerMessage(gameObjectParameter3.Does("apply") + " " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
			}
			ParentObject.Destroy();
			gameObjectParameter2.ApplyEffect(new SphynxSalt_Tonic(duration));
			return false;
		}
		return base.FireEvent(E);
	}
}
