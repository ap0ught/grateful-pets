using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPenetratingRadar : IPoweredPart
{
	public static readonly string COMMAND_NAME = "CommandTogglePenetratingRadar";

	public int Radius = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsPenetratingRadar()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "PhasedRadarArray";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee);
		int num = stats.CollectComputePowerAdjustUp(ability, "Radius", Radius);
		stats.Set("Radius", num, num != Radius, num - Radius);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && implantee.IsPlayer() && WasReady() && implantee.IsActivatedAbilityUsable(ActivatedAbilityID) && implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			Cell cell = implantee.CurrentCell;
			if (cell != null && !cell.OnWorldMap())
			{
				int r = GetAvailableComputePowerEvent.AdjustUp(implantee, Radius);
				cell.ParentZone?.AddLight(cell.X, cell.Y, r, LightLevel.Radar);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's range.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Penetrating Radar", COMMAND_NAME, "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, 1, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: true);
	}
}
