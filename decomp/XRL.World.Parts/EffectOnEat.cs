using System;

namespace XRL.World.Parts;

[Serializable]
public class EffectOnEat : IPart
{
	public string DisplayName;

	public string Effect = "CookingDomainReflect_Reflect100_ProceduralCookingTriggeredAction_Effect";

	public string Duration;

	public override bool SameAs(IPart p)
	{
		EffectOnEat effectOnEat = p as EffectOnEat;
		if (effectOnEat.Effect == Effect)
		{
			return effectOnEat.Duration == Duration;
		}
		return false;
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
			Effect effect = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + Effect)) as Effect;
			if (!string.IsNullOrEmpty(Duration))
			{
				effect.Duration = Duration.RollCached();
			}
			if (!string.IsNullOrEmpty(DisplayName))
			{
				effect.DisplayName = DisplayName;
			}
			E.GetGameObjectParameter("Eater").ApplyEffect(effect);
		}
		return base.FireEvent(E);
	}
}
