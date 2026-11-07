using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsAnchorSpikes : IPart
{
	public static readonly string COMMAND_NAME = "CommandToggleAnchorSpikes";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != ImplantedEvent.ID && ID != PooledEvent<IsRootedInPlaceEvent>.ID && ID != ModifyDefendingSaveEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Anchor Spikes", COMMAND_NAME, "Cybernetics", null, "\u00b8", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject.Implantee && ParentObject.Implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Move", E) && ParentObject.Implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			E.Roll += E.Difficulty;
			E.IgnoreNatural1 = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRootedInPlaceEvent E)
	{
		if (E.Object == ParentObject.Implantee && ParentObject.Implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
