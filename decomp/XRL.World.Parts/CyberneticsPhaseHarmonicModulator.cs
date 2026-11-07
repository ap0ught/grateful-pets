using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPhaseHarmonicModulator : IPart
{
	public static readonly string COMMAND_NAME = "CommandTogglePhaseHarmonicModulator";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Phase Harmonic Modulator", COMMAND_NAME, "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		E.Implantee.ForceApplyEffect(new Omniphase(Name));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveEffect(typeof(Omniphase), OurEffect);
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && E.Actor == ParentObject.Implantee)
		{
			E.Actor.ToggleActivatedAbility(ActivatedAbilityID);
			if (E.Actor.IsActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				if (!E.Actor.HasEffect(typeof(Omniphase), OurEffect))
				{
					E.Actor.ForceApplyEffect(new Omniphase(Name));
				}
			}
			else
			{
				E.Actor.RemoveEffect(typeof(Omniphase), OurEffect);
			}
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Omniphase omniphase)
		{
			return omniphase.SourceKey == Name;
		}
		return false;
	}
}
