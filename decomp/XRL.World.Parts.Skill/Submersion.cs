using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Submersion : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (!ParentObject.HasEffect<Submerged>() && Submerged.CanSubmerge(ParentObject) && ParentObject.CanChangeBodyPosition("Submerge") && IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			ParentObject.ApplyEffect(new Submerged());
			ParentObject.UseEnergy(1000, "Skill Submersion Submerge");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandSubmerge")
		{
			if (ParentObject.HasEffect<Submerged>())
			{
				if (!ParentObject.CanChangeBodyPosition("Surface", ShowMessage: true))
				{
					return false;
				}
				ParentObject.RemoveEffect<Submerged>();
				ParentObject.UseEnergy(1000, "Skill Submersion Submerge");
			}
			else
			{
				if (!ParentObject.CanChangeBodyPosition("Submerge", ShowMessage: true))
				{
					return false;
				}
				if (!Submerged.CanSubmerge(ParentObject))
				{
					if (ParentObject.IsPlayer())
					{
						GameObject gameObject = ParentObject.CurrentCell?.GetOpenLiquidVolume();
						if (gameObject == null)
						{
							Popup.ShowFail("There is nothing for you to submerge in here.");
						}
						else
						{
							Popup.ShowFail(gameObject.The + gameObject.ShortDisplayName + gameObject.Is + " too shallow for you to submerge in.");
						}
					}
					return false;
				}
				if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot do that right now.");
					}
					return false;
				}
				ParentObject.ApplyEffect(new Submerged());
				ParentObject.UseEnergy(1000, "Skill Submersion Submerge");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Submerge", "CommandSubmerge", "Maneuvers", null, "÷");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
