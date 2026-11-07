using System;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Ket : BaseInitiatorySkill
{
	public const int COOLDOWN = 100;

	public int Cooldown;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDieEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID)
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

	public override bool HandleEvent(BeforeDieEvent E)
	{
		if (Cooldown <= 0)
		{
			Cooldown = 100;
			if (Visible())
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You feel a sense of infinite grace flow through your being as you are brought from the brink of death to miraculous health.", "white");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + "{{white|" + ParentObject.GetVerb("shine") + " with a supernal light}} as " + ParentObject.its + " injuries disappear.");
				}
			}
			ParentObject.RestorePristineHealth(UseHeal: true);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Cooldown > 0)
		{
			Cooldown--;
		}
		return base.HandleEvent(E);
	}
}
