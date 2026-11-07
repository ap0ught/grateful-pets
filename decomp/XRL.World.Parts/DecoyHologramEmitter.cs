using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, save difficulty to resist being distracted
/// by the hologram is increased by the standard power load bonus, i.e.
/// 2 for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class DecoyHologramEmitter : IPoweredPart
{
	public int FLAG_STATIONARY = 1;

	public string Blueprint;

	public int Range = 10;

	public int MaxDecoys = 1;

	public int Difficulty = 15;

	public int Flags;

	public bool HologramActive;

	[NonSerialized]
	public List<GameObject> Holograms = new List<GameObject>();

	public Guid ActivatedAbilityID;

	public bool Stationary
	{
		get
		{
			return Flags.HasBit(FLAG_STATIONARY);
		}
		set
		{
			Flags.SetBit(FLAG_STATIONARY, value);
		}
	}

	public DecoyHologramEmitter()
	{
		ChargeUse = 2;
		WorksOnHolder = true;
		WorksOnWearer = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.WriteGameObjectList(Holograms);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		Reader.ReadGameObjectList(Holograms);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		if (MaxDecoys == 1)
		{
			stats.Set("DecoyNumber", "a");
			stats.Set("Decoys", "decoy");
		}
		else
		{
			stats.Set("DecoyNumber", MaxDecoys);
			stats.Set("Decoys", "decoys");
		}
		if (ChargeUse * MaxDecoys >= 100)
		{
			stats.Set("ChargeDraw", "Medium");
		}
		else
		{
			stats.Set("ChargeDraw", "Low");
		}
		stats.Set("Range", Range);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != EquippedEvent.ID && ID != PooledEvent<ExamineSuccessEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != OnDestroyObjectEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != UnequippedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "ToggleHologramEmitter" && GameObject.Validate(E.Actor))
		{
			if (HologramActive)
			{
				PlayWorldSound("Sounds/Interact/sfx_interact_hologramBracelet_deactivate");
				DestroyHolograms();
				E.Actor.UseEnergy(1000, "Item Deactivation");
				E.RequestInterfaceExit();
			}
			else
			{
				ActivateHologramBracelet(E.Actor, E);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Object == ParentObject && E.Complete)
		{
			SetUpActivatedAbility(ParentObject.Equipped);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (!HologramActive && !IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (E.Distance > 1 || 20.in100()))
		{
			E.Add("ActivateHologramBracelet", 2, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (WorksOnSelf && ParentObject.IsCombatObject())
		{
			SetUpActivatedAbility(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (HologramActive && Holograms.Contains(E.Object) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		DestroyHolograms(null, E.Actor, FromDialog: true);
		E.Actor.RegisterPartEvent(this, "AfterMoved");
		E.Actor.RegisterPartEvent(this, "BeginTakeAction");
		if (ParentObject.Understood())
		{
			SetUpActivatedAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AfterMoved");
		E.Actor.UnregisterPartEvent(this, "BeginTakeAction");
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		if (HologramActive)
		{
			DestroyHolograms(null, E.Actor, FromDialog: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		DestroyHolograms();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (HologramActive)
		{
			if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.NeedsSubject)
			{
				E.AddAction("Deactivate", "deactivate", "DeactivateHologramBracelet", null, 'a', FireOnActor: false, 10);
			}
		}
		else if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.NeedsSubject)
		{
			E.AddAction("Activate", "activate", "ActivateHologramBracelet", null, 'a', FireOnActor: false, 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateHologramBracelet")
		{
			ActivateHologramBracelet(E.Actor, E);
		}
		else if (E.Command == "DeactivateHologramBracelet" && HologramActive)
		{
			PlayWorldSound("Sounds/Interact/sfx_interact_hologramBracelet_deactivate");
			DestroyHolograms(null, E.Actor, FromDialog: true);
			E.Actor.UseEnergy(1000, "Item Deactivation");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("BootSequenceInitialized");
		Registrar.Register("EffectApplied");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (HologramActive)
			{
				int value = (int)Math.Ceiling((double)ChargeUse * (1.0 * (double)Holograms.Count / (double)MaxDecoys));
				if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, value, UseChargeIfUnpowered: false, 0L))
				{
					DestroyHolograms();
				}
			}
		}
		else if (E.ID == "AfterMoved")
		{
			if (HologramActive && !Stationary)
			{
				for (int i = 0; i < Holograms.Count; i++)
				{
					GameObject gameObject = Holograms[i];
					if (gameObject.IsInvalid())
					{
						DestroyHolograms(gameObject);
					}
					else
					{
						gameObject.Move(Directions.GetOppositeDirection(E.GetStringParameter("Direction")), Forced: true);
					}
				}
			}
		}
		else if (E.ID == "BootSequenceInitialized")
		{
			DestroyHolograms();
		}
		else if (E.ID == "EffectApplied" && HologramActive && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			DestroyHolograms();
		}
		return base.FireEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (ParentObject.OnWorldMap())
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "InvalidContext";
	}

	public static GameObject CreateHologramOf(GameObject Object)
	{
		GameObject gameObject = GameObject.Create("Hologram Distraction");
		gameObject.Render.Tile = Object.Render.Tile;
		gameObject.Render.RenderString = Object.Render.RenderString;
		gameObject.Render.DisplayName = Object.Render.DisplayName;
		gameObject.GetPart<Description>().Short = "Light stammers in parallax to form the image of an object. " + Object.GetPart<Description>().Short;
		gameObject.SetStringProperty("HologramOf", Object.an());
		if (Object.HasProperName)
		{
			gameObject.SetIntProperty("ProperNoun", 1);
		}
		return gameObject;
	}

	public void PlaceHologram(Cell C, GameObject Who, int Load, int ID)
	{
		GameObject gameObject = ((!Blueprint.IsNullOrEmpty()) ? GameObject.Create(Blueprint) : CreateHologramOf(Who));
		gameObject.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
		if (gameObject.TryGetPart<Distraction>(out var Part))
		{
			Part.ID = ID;
			Part.Original = Who;
			Part.Source = ParentObject;
			Part.SaveTarget = Difficulty + IComponent<GameObject>.PowerLoadBonus(Load);
		}
		C.AddObject(gameObject);
		Holograms.Add(gameObject);
		PlayWorldSound("Sounds/Interact/sfx_interact_hologramBracelet_activate");
		IComponent<GameObject>.EmitMessage(gameObject, "An image of " + gameObject.GetTagOrStringProperty("HologramOf") + " appears.");
	}

	public ActivePartStatus CreateHolograms(GameObject Who = null)
	{
		if (Holograms.Count > 0)
		{
			DestroyHolograms(null, Who);
		}
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			return activePartStatus;
		}
		if (Who == null)
		{
			Who = ParentObject.Equipped ?? ParentObject.GetCurrentCell()?.GetCombatObject();
			if (Who == null)
			{
				return ActivePartStatus.LocallyDefinedFailure;
			}
		}
		if (Who.IsPlayer())
		{
			Cell cell = The.PlayerCell;
			int num2 = 0;
			while (num2 < MaxDecoys)
			{
				cell = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, Range, cell.X, cell.Y, Locked: false, AllowVis.OnlyVisible, null, null, null, null, "Summon Decoy");
				if (cell == null)
				{
					break;
				}
				if (Who.DistanceTo(cell) > Range)
				{
					Popup.Show(string.Format("That is out of range ({0} {1})", Range, Range.Things("square", "squares")));
					continue;
				}
				PlaceHologram(cell, Who, num, num2);
				num2++;
			}
		}
		else
		{
			List<Cell> list = Who.CurrentCell.GetAdjacentCells(2).ShuffleInPlace();
			for (int i = 0; i < MaxDecoys && i < list.Count; i++)
			{
				if (list[i].IsEmpty())
				{
					PlaceHologram(list[i], Who, num, i);
				}
			}
		}
		HologramActive = Holograms.Count > 0;
		return activePartStatus;
	}

	private bool ActivateHologramBracelet(GameObject Who, IEvent E = null)
	{
		if (Who.OnWorldMap())
		{
			if (Who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			return false;
		}
		if (!HologramActive)
		{
			ActivePartStatus activePartStatus = CreateHolograms(Who);
			if (HologramActive)
			{
				Who.UseEnergy(1000, "Item Activation");
				E?.RequestInterfaceExit();
				SyncActivatedAbilityName(Who);
				return true;
			}
			if (Who.IsPlayer())
			{
				if (activePartStatus == ActivePartStatus.Booting && ParentObject.GetPart<BootSequence>().IsObvious())
				{
					Popup.Show(ParentObject.T() + ParentObject.Is + " still starting up.");
				}
				else
				{
					switch (activePartStatus)
					{
					case ActivePartStatus.Unpowered:
						Popup.Show(ParentObject.T() + ParentObject.GetVerb("do") + " not have enough charge to sustain the hologram.");
						break;
					default:
						Popup.Show(ParentObject.T() + ParentObject.Is + " unresponsive.");
						break;
					case ActivePartStatus.Operational:
						break;
					}
				}
			}
		}
		return false;
	}

	public void DestroyHolograms(GameObject Hologram = null, GameObject Who = null, bool FromDialog = false)
	{
		if (Hologram != null)
		{
			if (!Hologram.IsInvalid())
			{
				IComponent<GameObject>.EmitMessage(Hologram, "An image of " + Hologram.GetTagOrStringProperty("HologramOf") + " disappears.", ' ', FromDialog);
				Hologram.Destroy();
			}
			Holograms.Remove(Hologram);
		}
		else
		{
			for (int num = Holograms.Count - 1; num >= 0; num--)
			{
				Hologram = Holograms[num];
				if (!Hologram.IsInvalid())
				{
					IComponent<GameObject>.EmitMessage(Hologram, "An image of " + Hologram.GetTagOrStringProperty("HologramOf") + " disappears.", ' ', FromDialog);
					Hologram.Destroy();
				}
				Holograms.RemoveAt(num);
			}
		}
		HologramActive = Holograms.Count > 0;
		SyncActivatedAbilityName(Who);
	}

	public void SetUpActivatedAbility(GameObject Who)
	{
		if (Who != null)
		{
			ActivatedAbilityID = Who.AddActivatedAbility(GetActivatedAbilityName(Who), "ToggleHologramEmitter", (Who == ParentObject) ? "Maneuvers" : "Items", null, "\u0001");
		}
	}

	public string GetActivatedAbilityName(GameObject Who = null)
	{
		if (Who == null)
		{
			Who = ParentObject.Equipped ?? ParentObject;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(HologramActive ? "Deactivate" : "Activate").Append(' ').Append((Who == null || Who == ParentObject) ? "Holographic Decoy" : Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped));
		return stringBuilder.ToString();
	}

	public void SyncActivatedAbilityName(GameObject Who = null)
	{
		if (!(ActivatedAbilityID == Guid.Empty))
		{
			if (Who == null)
			{
				Who = ParentObject.Equipped ?? ParentObject;
			}
			Who.SetActivatedAbilityDisplayName(ActivatedAbilityID, GetActivatedAbilityName(Who));
		}
	}
}
