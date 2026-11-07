using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Ret : BaseInitiatorySkill
{
	public const int SHAKE_OFF = 25;

	public const int MENTAL_ACTION_REDUCTION = 50;

	private static List<Effect> targetEffects = new List<Effect>();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != SingletonEvent<GetEnergyCostEvent>.ID)
		{
			return ID == PooledEvent<GetPsionicSifrahSetupEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		E.Rating++;
		E.Turns++;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type != null && E.Type.Contains("Mental"))
		{
			E.PercentageReduction += 50;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (AffectEffect(E.Effect) && 25.in100() && ParentObject.IsPlayer())
		{
			string stateDescription = E.Effect.GetStateDescription();
			if (stateDescription.IsNullOrEmpty())
			{
				IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off the effect!", 'g');
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off being " + stateDescription + "!", 'g');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ParentObject.Effects != null && 25.in100())
		{
			targetEffects.Clear();
			int i = 0;
			for (int count = ParentObject.Effects.Count; i < count; i++)
			{
				if (AffectEffect(ParentObject.Effects[i]))
				{
					targetEffects.Add(ParentObject.Effects[i]);
				}
			}
			if (targetEffects.Count > 0)
			{
				Effect randomElement = targetEffects.GetRandomElement();
				if (randomElement != null)
				{
					if (ParentObject.IsPlayer())
					{
						string stateDescription = randomElement.GetStateDescription();
						if (stateDescription.IsNullOrEmpty())
						{
							IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off a mental state!", 'g');
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off being " + stateDescription + "!", 'g');
						}
					}
					ParentObject.RemoveEffect(randomElement);
				}
			}
			targetEffects.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool AffectEffect(Effect FX)
	{
		if (FX.IsOfTypes(100663298))
		{
			return !FX.IsOfType(134217728);
		}
		return false;
	}
}
