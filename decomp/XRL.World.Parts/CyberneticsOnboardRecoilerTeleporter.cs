using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsOnboardRecoilerTeleporter : ITeleporter
{
	public const int COOLDOWN = 50;

	public string CommandId;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsOnboardRecoilerTeleporter()
	{
		ChargeUse = 0;
		WorksOnCarrier = false;
		WorksOnHolder = false;
		WorksOnImplantee = true;
		NameForStatus = "TeleportationSystem";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public string GetName()
	{
		string destinationZone = DestinationZone;
		if (!string.IsNullOrEmpty(destinationZone))
		{
			string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(destinationZone, WithIndefiniteArticle: true);
			if (string.IsNullOrEmpty(zoneDisplayName))
			{
				return null;
			}
			return zoneDisplayName;
		}
		return null;
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
		int num = stats.CollectComputePowerAdjustDown(ability, "Cooldown", GetBaseCooldown());
		stats.CollectCooldownTurns(ability, num, num - GetBaseCooldown());
	}

	public int GetBaseCooldown()
	{
		return 50;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != InventoryActionEvent.ID && ID != UnimplantedEvent.ID)
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
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out CommandId, "CommandActivateOnboardRecoilerTeleporter", "Recoil", "Cybernetics");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command != null && E.Command == CommandId && E.Actor == ParentObject.Implantee)
		{
			ActuateTeleport(IComponent<GameObject>.ThePlayer, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice reduces this item's cooldown.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(IComponent<GameObject>.ThePlayer))
		{
			E.AddAction("Activate", "activate", "ActivateRecoilerTeleporter", null, 'a', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateRecoilerTeleporter")
		{
			ActuateTeleport(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool VisibleInRecoilerList()
	{
		if (base.VisibleInRecoilerList())
		{
			return ParentObject.Implantee != null;
		}
		return false;
	}

	private void ActuateTeleport(GameObject who, IEvent E)
	{
		if (who.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			if (AttemptTeleport(who, E))
			{
				who.UseEnergy(1000, "Cybernetics Recoiler");
				int turns = GetAvailableComputePowerEvent.AdjustDown(who, 50);
				who.CooldownActivatedAbility(ActivatedAbilityID, turns);
				E.RequestInterfaceExit();
				ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			}
		}
		else if (who.IsPlayer())
		{
			Popup.ShowFail("You can't recoil yet.");
		}
	}
}
