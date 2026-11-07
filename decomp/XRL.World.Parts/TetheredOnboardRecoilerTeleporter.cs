using System;

namespace XRL.World.Parts;

[Serializable]
public class TetheredOnboardRecoilerTeleporter : ITeleporter
{
	public const string ABL_CMD = "CommandActivateTetheredRecoilerTeleporter";

	public string TetheredBlueprint;

	public int Cooldown = 50;

	public Guid AbilityID;

	public TetheredOnboardRecoilerTeleporter()
	{
		ChargeUse = 0;
		WorksOnCarrier = false;
		WorksOnHolder = false;
		WorksOnSelf = true;
		NameForStatus = "TeleportationSystem";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Initialize()
	{
		CheckAbility();
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref AbilityID);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(AbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(AbilityID, ParentObject?.Implantee);
		int num = stats.CollectComputePowerAdjustDown(ability, "Cooldown", Cooldown);
		stats.CollectCooldownTurns(ability, num, num - Cooldown);
		if (!DestinationZone.IsNullOrEmpty())
		{
			stats.Set("Destination", The.ZoneManager.GetZoneBaseDisplayName(DestinationZone, Mutate: false));
			stats.Set("TetherStatus", ValidateTether() ? "Tethered" : "Broken");
		}
		else
		{
			stats.Set("Destination", "{{R|Uninitialized}}");
		}
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandActivateTetheredRecoilerTeleporter")
		{
			ActuateTeleport(GetActivePartFirstSubject(), E);
		}
		return base.HandleEvent(E);
	}

	private void ActuateTeleport(GameObject Subject, IEvent E)
	{
		if (!Subject.IsActivatedAbilityUsable(AbilityID))
		{
			Subject.ShowFailure("You can't recoil yet.");
		}
		else if (!ValidateTether())
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateSampleObject(TetheredBlueprint);
			Subject.ShowFailure("There is no " + gameObject.ShortDisplayNameSingle + " at the destination to receive you.");
		}
		else if (AttemptTeleport(Subject, E))
		{
			Subject.UseEnergy(1000, "Recoiler");
			Subject.CooldownActivatedAbility(AbilityID, GetAvailableComputePowerEvent.AdjustDown(Subject, Cooldown));
			E.RequestInterfaceExit();
			GameObject partyLeader = Subject.PartyLeader;
			if (partyLeader != null && partyLeader.CurrentZone != Subject.CurrentZone)
			{
				Subject.Brain.Staying = true;
			}
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
	}

	private void CheckAbility()
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return;
		}
		if (DestinationZone.IsNullOrEmpty())
		{
			activePartFirstSubject.RemoveActivatedAbility(ref AbilityID);
			return;
		}
		string text = ((The.ZoneManager == null) ? "Recoil" : ("Recoil to " + The.ZoneManager.GetZoneBaseDisplayName(DestinationZone, Mutate: false)));
		if (AbilityID == Guid.Empty)
		{
			AbilityID = activePartFirstSubject.AddActivatedAbility(text, "CommandActivateTetheredRecoilerTeleporter", "Maneuvers");
		}
		else
		{
			activePartFirstSubject.GetActivatedAbility(AbilityID).DisplayName = text;
		}
	}

	private bool ValidateTether()
	{
		if (TetheredBlueprint.IsNullOrEmpty())
		{
			return true;
		}
		if (DestinationZone.IsNullOrEmpty())
		{
			return false;
		}
		Zone zone = The.ZoneManager.GetZone(DestinationZone);
		Cell cell = zone?.GetCell(DestinationX, DestinationY);
		if (cell == null)
		{
			return false;
		}
		GameObject gameObject = cell.FindObject(TetheredBlueprint);
		if (gameObject != null)
		{
			return true;
		}
		gameObject = zone.FindObject(TetheredBlueprint);
		if (gameObject != null)
		{
			cell = gameObject.CurrentCell;
			DestinationX = cell.X;
			DestinationY = cell.Y;
			return true;
		}
		return false;
	}

	public void SetDestination(Cell C)
	{
		SetDestination(C.ParentZone.ZoneID, C.X, C.Y);
	}

	public void SetDestination(string ZoneID, int X, int Y)
	{
		DestinationZone = ZoneID;
		DestinationX = X;
		DestinationY = Y;
		CheckAbility();
	}
}
