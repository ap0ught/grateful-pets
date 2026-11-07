using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsOnboardRecoilerImprinting : IProgrammableRecoiler
{
	public const int COOLDOWN = 100;

	[NonSerialized]
	private int NameTick;

	public Guid ActivatedAbilityID = Guid.Empty;

	public string CommandId;

	public CyberneticsOnboardRecoilerImprinting()
	{
		ChargeUse = 0;
		WorksOn(AdjacentCellContents: false, Carrier: false, CellContents: false, Enclosed: false, Equipper: false, Holder: false, Implantee: true);
		NameForStatus = "GeospatialCore";
		Reprogrammable = true;
	}

	public override void ProgrammedForLocation(Zone Z, Cell C)
	{
		UpdateName();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee);
		string name = GetName();
		if (name.IsNullOrEmpty())
		{
			stats.Set("Location", "none");
		}
		else
		{
			stats.Set("Location", name);
		}
		stats.CollectCooldownTurns(ability, 100);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != ImplantedEvent.ID && ID != InventoryActionEvent.ID && ID != UnimplantedEvent.ID)
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

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out CommandId, "CommandActivateOnboardRecoilerImprinting", "Imprint with current location", "Cybernetics");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command != null && E.Command == CommandId && E.Actor == ParentObject.Implantee && ProgramRecoiler(E.Actor, E))
		{
			E.Actor.UseEnergy(1000, "Cybernetics Recoiler Imprint");
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, 100);
			E.RequestInterfaceExit();
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	public void UpdateName()
	{
		if (ParentObject.TryGetPart<CyberneticsOnboardRecoilerTeleporter>(out var Part))
		{
			ActivatedAbilityEntry activatedAbilityEntry = ParentObject.Implantee?.GetActivatedAbility(Part.ActivatedAbilityID);
			if (activatedAbilityEntry != null)
			{
				string name = GetName();
				activatedAbilityEntry.DisplayName = (name.IsNullOrEmpty() ? "Recoil" : ("Recoil to " + name));
			}
		}
		NameTick = The.ZoneManager.NameUpdateTick;
	}

	public string GetName()
	{
		CyberneticsOnboardRecoilerTeleporter part = ParentObject.GetPart<CyberneticsOnboardRecoilerTeleporter>();
		if (part != null)
		{
			string destinationZone = part.DestinationZone;
			if (!string.IsNullOrEmpty(destinationZone))
			{
				string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(destinationZone, WithIndefiniteArticle: true);
				if (string.IsNullOrEmpty(zoneDisplayName))
				{
					return null;
				}
				return zoneDisplayName;
			}
		}
		return null;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (The.Game.ZoneManager != null && The.ZoneManager.NameUpdateTick > NameTick)
		{
			UpdateName();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(IComponent<GameObject>.ThePlayer))
		{
			E.AddAction("Imprint", "imprint", "ImprintRecoiler", null, 'i', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ImprintRecoiler")
		{
			if (!E.Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("You can't imprint yet.");
				}
			}
			else if (ProgramRecoiler(E.Actor, E))
			{
				E.Actor.UseEnergy(1000, "Cybernetics Recoiler Imprint");
				E.Actor.CooldownActivatedAbility(ActivatedAbilityID, 100);
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
