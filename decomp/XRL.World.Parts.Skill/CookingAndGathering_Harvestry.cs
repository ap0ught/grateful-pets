using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_Harvestry : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("salt", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command == null && IsMyActivatedAbilityToggledOn(ActivatedAbilityID) && E.Item.TryGetPart<Harvestable>(out var Part) && Part.IsHarvestable() && !E.Item.IsImportant())
		{
			E.Command = "Harvest";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandHarvestToggle");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandHarvestToggle")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Harvest Plants", "CommandHarvestToggle", "Skills", "Toggle to enable or disable the harvesting of plants", "h", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
