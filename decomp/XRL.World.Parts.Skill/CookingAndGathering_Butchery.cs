using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_Butchery : BaseSkill
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
		if (!E.AutogetOnlyMode && E.Command == null && IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			Butcherable part = E.Item.GetPart<Butcherable>();
			CyberneticsButcherableCybernetic part2 = E.Item.GetPart<CyberneticsButcherableCybernetic>();
			if (((part != null && part.IsButcherable()) || (part2 != null && part2.IsButcherable())) && !E.Item.IsImportant())
			{
				E.Command = "Butcher";
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandButcherToggle");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandButcherToggle")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Butcher Corpses", "CommandButcherToggle", "Skills", null, "b", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
