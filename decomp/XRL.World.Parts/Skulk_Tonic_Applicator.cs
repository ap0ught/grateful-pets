using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Skulk_Tonic_Applicator : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
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
			int duration = GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Skulk", 1000 * intParameter + Stat.Random(intParameter, 200 * intParameter), intParameter);
			gameObjectParameter2.ApplyEffect(new Skulk_Tonic(duration));
		}
		return base.FireEvent(E);
	}
}
