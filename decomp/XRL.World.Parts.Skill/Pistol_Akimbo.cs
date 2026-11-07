using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Pistol_Akimbo : BaseSkill
{
	public Guid ActivatedAbilityID;

	public override void Attach()
	{
		if (ActivatedAbilityID == Guid.Empty)
		{
			AddAbility();
		}
		base.Attach();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanFireAllMissileWeaponsEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanFireAllMissileWeaponsEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleAkimbo")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject Object)
	{
		AddAbility();
		return base.AddSkill(Object);
	}

	public override bool RemoveSkill(GameObject Object)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(Object);
	}

	public void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Akimbo", "CommandToggleAkimbo", "Skills", null, "\u001d", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
	}
}
