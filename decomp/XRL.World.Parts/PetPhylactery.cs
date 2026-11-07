using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class PetPhylactery : IPoweredPart
{
	public string wraithBlueprint = "PetWraith";

	public string wraithID;

	public GameObject wraith;

	public string spawnerID;

	public PetPhylactery()
	{
		ChargeUse = 1;
		WorksOnHolder = true;
		WorksOnCarrier = true;
		WorksOnEquipper = true;
		MustBeUnderstood = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveItemListEvent.ID && ID != AfterObjectCreatedEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		if (wraith == null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Add("ActivateTemplarPhylactery", 100, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (E.Object == wraith && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Despawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		Despawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (GameObject.Validate(ref wraith) && IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		GameObject objectContext = ParentObject.GetObjectContext();
		if (objectContext == null)
		{
			Despawn();
		}
		else if (!objectContext.IDMatch(spawnerID))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.GetObjectContext() != null)
		{
			if (GameObject.Validate(ref wraith))
			{
				E.AddAction("Deactivate", "deactivate", "DeactivateTemplarPhylactery", null, 'a');
			}
			else
			{
				E.AddAction("Activate", "activate", "ActivateTemplarPhylactery", null, 'a');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTemplarPhylactery")
		{
			Spawn();
		}
		else if (E.Command == "DeactivateTemplarPhylactery")
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (E.Context != "Sample" && E.ReplacementObject == null && wraithID == null)
		{
			GameObject gameObject = GameObject.Create(wraithBlueprint);
			gameObject.RequirePart<HologramMaterial>();
			gameObject.RequirePart<HologramInvulnerability>();
			gameObject.RequirePart<Unreplicable>();
			gameObject.ModIntProperty("IgnoresWalls", 1);
			foreach (GameObject item in gameObject.GetInventoryAndEquipment())
			{
				if (item.HasPropertyOrTag("MeleeWeapon"))
				{
					item.AddPart(new ModPsionic());
				}
				else
				{
					item.Obliterate();
				}
			}
			ParentObject.Render.DisplayName = "phylactery of " + gameObject.ShortDisplayNameWithoutTitles;
			if (The.ZoneManager != null)
			{
				wraithID = The.ZoneManager.CacheObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public void Despawn()
	{
		if (GameObject.Validate(ref wraith))
		{
			wraith.Splatter("&M-");
			wraith.Splatter("&M.");
			wraith.Splatter("&M/");
			IComponent<GameObject>.XDidY(wraith, "disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
			wraith.Obliterate();
			wraith = null;
		}
	}

	public void Spawn()
	{
		Despawn();
		GameObject objectContext = ParentObject.GetObjectContext();
		if (objectContext == null)
		{
			return;
		}
		Cell cell = objectContext.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		cell = cell.GetEmptyAdjacentCells(1, 5).GetRandomElement();
		if (cell != null)
		{
			bool flag = false;
			if (!objectContext.IsPlayerControlled() && objectContext.HasProperty("PsychicHunter") && objectContext.HasPart<Extradimensional>())
			{
				flag = true;
			}
			spawnerID = objectContext.ID;
			wraith = The.ZoneManager.peekCachedObject(wraithID).DeepCopy(CopyEffects: false, CopyID: true);
			Temporary.AddHierarchically(wraith, 0, null, ParentObject);
			wraith.MakeActive();
			objectContext.PlayWorldOrUISound("Sounds/Interact/sfx_interact_phylactery_on");
			cell.AddObject(wraith);
			wraith.TeleportSwirl(null, "&B", Voluntary: true);
			IComponent<GameObject>.XDidYToZ(objectContext, "activate", wraith);
			IComponent<GameObject>.XDidY(wraith, "appear");
			wraith.PartyLeader = objectContext;
			if (flag)
			{
				wraith.TakeAllegiance<AllySummon>(objectContext);
			}
		}
	}
}
