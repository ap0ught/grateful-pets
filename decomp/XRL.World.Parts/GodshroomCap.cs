using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GodshroomCap : IPart
{
	public bool Controlled;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Eaten");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eaten")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter.IsPlayer())
			{
				gameObjectParameter?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_godshroom");
				if (gameObjectParameter.TryGetEffect<FungalVisionary>(out var Effect))
				{
					Effect.Duration += 1000;
				}
				else
				{
					gameObjectParameter.ApplyEffect(new FungalVisionary(1000));
				}
			}
		}
		return base.FireEvent(E);
	}
}
