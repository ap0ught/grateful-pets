using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Ubernostrum_Tonic_Applicator : IPart
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
		if (E.Damage == null)
		{
			Body body = E.Actor.Body;
			if (body != null && body.FindRegenerablePart() != null)
			{
				E.ApplyScore(80);
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
				else if (num < 0.1)
				{
					E.ApplyScore((int)(0.6 / num));
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
			int duration = GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Ubernostrum", 10 * intParameter, intParameter, Healing: true);
			gameObjectParameter2.FireEvent("ApplyingUbernostrum");
			Ubernostrum_Tonic e = new Ubernostrum_Tonic(duration);
			if (!gameObjectParameter2.ApplyEffect(e))
			{
				return false;
			}
			gameObjectParameter2.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality_big");
		}
		return base.FireEvent(E);
	}
}
