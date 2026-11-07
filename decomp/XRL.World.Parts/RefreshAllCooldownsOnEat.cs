using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class RefreshAllCooldownsOnEat : IPart
{
	public int ChancePerAbility = 25;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ChancePerAbility >= 100)
		{
			E.Postfix.AppendRules("When eaten, each activated ability's cooldown is refreshed.");
		}
		else
		{
			E.Postfix.AppendRules("When eaten, there's " + Grammar.A(ChancePerAbility) + "% chance that each activated ability's cooldown is refreshed.");
		}
		return base.HandleEvent(E);
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
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			ActivatedAbilities activatedAbilities = gameObjectParameter?.ActivatedAbilities;
			if (activatedAbilities != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value.Cooldown > 0 && ChancePerAbility.in100())
					{
						value.Refresh(this);
						if (gameObjectParameter.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You suddenly feel ready to use " + value.DisplayName + " again.");
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
