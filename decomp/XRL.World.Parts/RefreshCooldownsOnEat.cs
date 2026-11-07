using System;

namespace XRL.World.Parts;

[Serializable]
public class RefreshCooldownsOnEat : IPart
{
	public bool Controlled;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		Registrar.Register("AfterEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			ActivatedAbilities activatedAbilities = E.GetGameObjectParameter("Eater")?.ActivatedAbilities;
			if (activatedAbilities != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value.Class.Contains("Mental") && value.Cooldown > 0)
					{
						value.Refresh(this);
					}
				}
			}
		}
		if (E.ID == "AfterEat" && E.GetGameObjectParameter("Eater").IsPlayer())
		{
			IComponent<GameObject>.EmitMessage(IComponent<GameObject>.ThePlayer, "{{rules|All your mental cooldowns are refreshed.}}", ' ', FromDialog: false, UsePopup: true);
		}
		return base.FireEvent(E);
	}
}
