using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class VehicleRecall : IPoweredPart
{
	public string AdjacentBlueprint;

	public string RequiredBlueprint;

	public string RequiredType;

	public string Sound = "Sounds/Interact/sfx_interact_vehicle_recall";

	[NonSerialized]
	private string _VehicleTerm;

	public string VehicleTerm
	{
		get
		{
			if (_VehicleTerm == null)
			{
				if (ParentObject.Property.TryGetValue("VehicleTerm", out _VehicleTerm))
				{
					return _VehicleTerm;
				}
				if (!RequiredBlueprint.IsNullOrEmpty() && GameObjectFactory.Factory.Blueprints.TryGetValue(RequiredBlueprint, out var value))
				{
					_VehicleTerm = value.CachedDisplayNameStripped;
				}
				else
				{
					_VehicleTerm = "vehicle";
				}
			}
			return _VehicleTerm;
		}
	}

	public VehicleRecall()
	{
		WorksOnAdjacentCellContents = true;
	}

	public override bool WorksForEveryone()
	{
		return false;
	}

	public override bool WorksFor(GameObject Object)
	{
		return Object.Blueprint == AdjacentBlueprint;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != CanSmartUseEvent.ID)
		{
			return ID == CommandSmartUseEarlyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("RecallVehicle", "recall " + VehicleTerm, "RecallVehicle", null, 'c');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RecallVehicle")
		{
			switch (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
			case ActivePartStatus.Unpowered:
				return E.Actor.ShowFailure(ParentObject.Does("do") + " not have enough charge to operate.");
			default:
				return E.Actor.ShowFailure(ParentObject.T() + " merely" + ParentObject.GetVerb("click") + ".");
			case ActivePartStatus.Operational:
				break;
			}
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			Cell cell = activePartFirstSubject?.CurrentCell;
			if (cell == null)
			{
				return E.Actor.ShowFailure("There is nowhere for the " + VehicleTerm + " to be recalled to.");
			}
			if (!cell.IsPassable())
			{
				return E.Actor.ShowFailure(activePartFirstSubject.T() + " is obstructed.");
			}
			List<GameObject> list = VehicleRecord.ResolveRecordsFor(E.Actor, RequiredBlueprint, RequiredType);
			if (list.IsNullOrEmpty())
			{
				return E.Actor.ShowFailure("You have no " + VehicleTerm + " to recall.");
			}
			GameObject gameObject = list.First();
			if (list.Count > 1 || gameObject.CurrentCell == null)
			{
				string[] array = new string[list.Count];
				IRenderable[] array2 = new IRenderable[list.Count];
				char[] array3 = new char[list.Count];
				char c = 'a';
				for (int i = 0; i < list.Count; i++)
				{
					gameObject = list[i];
					array[i] = gameObject.ShortDisplayName;
					array2[i] = gameObject.RenderForUI();
					array3[i] = ((c <= 'z') ? c++ : ' ');
					if (gameObject.CurrentCell == null)
					{
						array[i] = "{{R|destroyed}} " + array[i] + "\nÿÿÿÿ{{C|1 dram of " + LiquidVolume.GetLiquid("sunslag").Name + " to reshape}}";
					}
				}
				int num = Popup.PickOption("Choose a " + VehicleTerm + " to recall", null, "", "Sounds/UI/ui_notification", array, array3, array2, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
				if (num < 0 || num >= list.Count)
				{
					return false;
				}
				gameObject = list[num];
			}
			bool flag = gameObject.CurrentCell == null;
			if (flag && !E.Actor.UseDrams(1, "sunslag"))
			{
				return E.Actor.ShowFailure("You do not have 1 dram of " + LiquidVolume.GetLiquid("sunslag").GetName() + ".");
			}
			if (ConsumeCharge() && gameObject.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: false, UsePopups: false, "disappear", null))
			{
				The.ZoneManager.CachedObjects.Remove(gameObject.ID);
				gameObject.Physics.DidXToY("are", flag ? "reshaped at" : "transported to", activePartFirstSubject, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, E.Actor.IsPlayer());
				gameObject.TeleportSwirl(null, "&C", Voluntary: true, Sound);
				Vehicle part = gameObject.GetPart<Vehicle>();
				if (!gameObject.IsPlayer() && part != null && part.Follower)
				{
					gameObject.PartyLeader = E.Actor;
					gameObject.Brain.Stay(cell);
				}
				if (flag)
				{
					gameObject.RestorePristineHealth(UseHeal: true);
					gameObject.FireEvent(Event.New("Regenera", "Level", 10, "Source", ParentObject));
					gameObject.DustPuff();
					RepairedEvent.Send(E.Actor, gameObject, ParentObject);
				}
				gameObject.GetPart<TetheredOnboardRecoilerTeleporter>()?.SetDestination(cell);
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}
}
