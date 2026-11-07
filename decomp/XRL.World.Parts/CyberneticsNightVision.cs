using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsNightVision : IPart
{
	public static readonly string COMMAND_NAME = "CommandToggleCyberNightVision";

	public int Radius = 40;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && implantee.IsPlayer())
		{
			ActivatedAbilityEntry activatedAbility = implantee.GetActivatedAbility(ActivatedAbilityID);
			if (activatedAbility != null && activatedAbility.IsUsable && activatedAbility.ToggleState && !IsBroken() && !IsRusted() && !IsEMPed())
			{
				Cell cell = implantee.CurrentCell;
				cell?.ParentZone?.AddLight(cell.X, cell.Y, Radius, LightLevel.Darkvision);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Night Vision", COMMAND_NAME, "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
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

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
