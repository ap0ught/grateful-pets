using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class HulkHoney_Tonic_Applicator : IPart
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
		if (E.Damage == null)
		{
			if (E.Actor.HasEffect<Terrified>())
			{
				E.ApplyScore(100);
			}
			else
			{
				double num = E.Actor.Health();
				if (num < 0.2)
				{
					E.ApplyScore((int)(1.0 / num));
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
			HulkHoney_Tonic e = new HulkHoney_Tonic(GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "HulkHoney", Stat.Random(intParameter, 10 * intParameter) + 40 * intParameter, intParameter));
			if (!gameObjectParameter2.ApplyEffect(e))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
