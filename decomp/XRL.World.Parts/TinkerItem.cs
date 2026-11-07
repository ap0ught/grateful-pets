using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.Language;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
[HasGameBasedStaticCache]
public class TinkerItem : IPart
{
	public bool CanDisassemble = true;

	public bool CanBuild;

	public int BuildTier = 1;

	public int NumberMade = 1;

	public string Ingredient = "";

	public string SubstituteBlueprint;

	public string RepairCost;

	public string RustedRepairCost;

	[NonSerialized]
	[GameBasedStaticCache(true, false)]
	public static Dictionary<string, string> BitCostMap = new Dictionary<string, string>();

	[NonSerialized]
	[GameBasedStaticCache(true, false)]
	public static Dictionary<string, string> BitSpecMap = new Dictionary<string, string>();

	[NonSerialized]
	private static BitCost ModBitCost = new BitCost();

	[NonSerialized]
	private static Dictionary<string, TinkerData> ModRecipes = new Dictionary<string, TinkerData>();

	[NonSerialized]
	private static BitCost Cost = new BitCost();

	public UnityEngine.GameObject[] objectsToDisable;

	public UnityEngine.GameObject[] objectsToEnable;

	public string Bits
	{
		get
		{
			if (BitCostMap.TryGetValue(ActiveBlueprint, out var value))
			{
				Cost.Clear();
				Cost.Import(value);
				if (GlobalConfig.GetBoolSetting("IncludeModBitsInItemBits"))
				{
					ModifyBitCostEvent.Process(The.Player, Cost, "Disassemble");
					int modificationSlotsUsed = ParentObject.GetModificationSlotsUsed();
					if (modificationSlotsUsed > 0)
					{
						List<IModification> partsDescendedFrom = ParentObject.GetPartsDescendedFrom<IModification>();
						if (!partsDescendedFrom.IsNullOrEmpty())
						{
							int num = modificationSlotsUsed - ParentObject.GetIntProperty("NoCostMods") + ParentObject.GetTechTier();
							int i = 0;
							for (int count = partsDescendedFrom.Count; i < count; i++)
							{
								IModification modification = partsDescendedFrom[i];
								if (modification.GetModificationSlotUsage() > 0 && ModificationFactory.ModsByPart.TryGetValue(modification.Name, out var value2))
								{
									TinkerData modRecipe = GetModRecipe(value2);
									if (modRecipe != null)
									{
										ModBitCost.Clear();
										ModBitCost.Increment(BitType.TierBits[Tier.Constrain(modRecipe.Tier)]);
										int key = Tier.Constrain(num);
										num--;
										ModBitCost.Increment(BitType.TierBits[key]);
										ModifyBitCostEvent.Process(The.Player, ModBitCost, "DisassembleMod");
										ModBitCost.AddTo(Cost);
									}
								}
							}
						}
					}
				}
				else
				{
					ModifyBitCostEvent.Process(The.Player, Cost, "Disassemble");
				}
				return Cost.ToBits();
			}
			return "0";
		}
		set
		{
			BitSpecMap.TryAdd(ParentObject.Blueprint, value);
		}
	}

	public string ActiveBlueprint
	{
		get
		{
			if (!SubstituteBlueprint.IsNullOrEmpty())
			{
				return SubstituteBlueprint;
			}
			return ParentObject.Blueprint;
		}
		set
		{
			if (value == ParentObject.Blueprint)
			{
				SubstituteBlueprint = null;
			}
			else
			{
				SubstituteBlueprint = value;
			}
		}
	}

	public static string GetBitCostFor(string Blueprint)
	{
		if (!BitCostMap.TryGetValue(Blueprint, out var value))
		{
			value = GameObjectFactory.Factory.GetBlueprint(Blueprint)?.GetPartParameter<string>("TinkerItem", "Bits");
			if (value.IsNullOrEmpty())
			{
				MetricsManager.LogError("Obtaining bit cost for invalid blueprint:" + Blueprint);
				return "1";
			}
			value = BitType.ToRealBits(value, Blueprint);
			BitCostMap.Add(Blueprint, value);
		}
		if (value == null || value.Contains("0"))
		{
			value = GameObjectFactory.Factory.GetBlueprint(Blueprint)?.GetPartParameter<string>("TinkerItem", "Bits");
			if (value.IsNullOrEmpty())
			{
				MetricsManager.LogError("Obtaining bit cost for invalid blueprint:" + Blueprint);
				return "1";
			}
			value = BitType.ToRealBits(value, Blueprint);
			BitCostMap[Blueprint] = value;
		}
		return value;
	}

	private static TinkerData GetModRecipe(ModEntry Mod)
	{
		if (!ModRecipes.TryGetValue(Mod.Part, out var value))
		{
			int i = 0;
			for (int count = TinkerData.TinkerRecipes.Count; i < count; i++)
			{
				TinkerData tinkerData = TinkerData.TinkerRecipes[i];
				if (tinkerData.DisplayName == Mod.TinkerDisplayName && tinkerData.Type == "Mod")
				{
					value = tinkerData;
					ModRecipes[Mod.Part] = value;
					break;
				}
			}
		}
		return value;
	}

	public override void Initialize()
	{
		if (!BitCostMap.ContainsKey(ActiveBlueprint) && BitSpecMap.TryGetValue(ParentObject.Blueprint, out var value))
		{
			BitCostMap.Add(ActiveBlueprint, BitType.ToRealBits(value, ActiveBlueprint));
		}
		for (int i = 0; i < Bits.Length; i++)
		{
			if (BitType.BitMap.TryGetValue(Bits[i], out var value2))
			{
				if (value2.Level > BuildTier)
				{
					BuildTier = value2.Level;
				}
			}
			else
			{
				MetricsManager.LogWarning($"Bit '{Bits[i]}' unrecognized from {ParentObject.DebugName} on Bits: \"{Bits}\" char {i} ");
			}
		}
	}

	public static TinkerData LoadBlueprint(GameObjectBlueprint Blueprint)
	{
		if (Blueprint.HasTag("BaseObject") || Blueprint.HasTag("NoDataDisk"))
		{
			return null;
		}
		GamePartBlueprint part = Blueprint.GetPart("TinkerItem");
		if (part == null)
		{
			return null;
		}
		if (!part.GetParameter("CanBuild", Default: false))
		{
			return null;
		}
		if (!part.GetParameter("SubstituteBlueprint", "").IsNullOrEmpty())
		{
			return null;
		}
		TinkerData tinkerData = new TinkerData();
		tinkerData.Blueprint = Blueprint.Name;
		tinkerData.Cost = GetBitCostFor(Blueprint.Name);
		int num = 0;
		for (int i = 0; i < tinkerData.Cost.Length; i++)
		{
			if (BitType.BitMap[tinkerData.Cost[i]].Level > num)
			{
				num = BitType.BitMap[tinkerData.Cost[i]].Level;
			}
		}
		tinkerData.Tier = part.GetParameter("BuildTier", num);
		tinkerData.Type = "Build";
		tinkerData.Category = Blueprint.GetTag("TinkerCategory", "utility");
		tinkerData.Ingredient = part.GetParameter<string>("Ingredient");
		tinkerData.DisplayName = TinkeringHelpers.TinkeredItemDisplayName(Blueprint.Name);
		return tinkerData;
	}

	public static void SaveGlobals(SerializationWriter Writer)
	{
		Writer.Write(BitCostMap);
		Writer.WriteOptimized(TinkerData.KnownRecipes.Count);
		foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
		{
			Writer.WriteComposite(knownRecipe);
		}
	}

	public static void LoadGlobals(SerializationReader Reader)
	{
		BitCostMap = Reader.ReadDictionary<string, string>();
		int num = Reader.ReadOptimizedInt32();
		TinkerData.KnownRecipes = new List<TinkerData>(num);
		for (int i = 0; i < num; i++)
		{
			TinkerData.KnownRecipes.Add(Reader.ReadComposite<TinkerData>());
		}
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void Awake()
	{
		objectsToEnable.FirstOrDefault((UnityEngine.GameObject o) =>
		{
			o.SetActive(value: true);
			return false;
		});
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetInventoryActionsAlwaysEvent.ID && ID != PooledEvent<GetScanTypeEvent>.ID && ID != PooledEvent<IdleQueryEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject && (CanBuild || CanDisassemble))
		{
			E.ScanType = Scanning.Scan.Tech;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if ((E.AsIfKnown || (The.Player != null && The.Player.HasSkill("Tinkering_Disassemble"))) && CanDisassemble && 1160 < E.Cutoff && ParentObject != null && !ParentObject.IsCombat && E.Context != "Tinkering" && E.Understood() && E.Object != null && !E.Object.HasProperName)
		{
			E.AddTag("{{y|<{{|" + BitType.GetString(Bits) + "}}>}}", 60);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		if (E.Actor.IsPlayer() && CanBeDisassembled(E.Actor))
		{
			int num = -1;
			if ((HasTag("DefaultDisassemble") || TinkeringHelpers.ConsiderScrap(ParentObject, E.Actor)) && !E.Object.IsImportant())
			{
				num = 200;
			}
			E.AddAction("Disassemble", "disassemble", "Disassemble", null, 'm', FireOnActor: false, num);
			E.AddAction("Disassemble All", "disassemble all", "DisassembleAll", null, 'm', FireOnActor: false, -1, -1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if ((E.Command == "Disassemble" || E.Command == "DisassembleAll") && E.Actor.IsPlayer() && CanBeDisassembled(E.Actor))
		{
			int count = ParentObject.Count;
			bool flag = E.Command == "DisassembleAll";
			bool flag2 = flag && count > 1;
			List<Action<GameObject>> list = null;
			if (!E.Actor.InSameOrAdjacentCellTo(ParentObject))
			{
				E.Actor.ShowFailure("You need be near " + ParentObject.t() + " in order to disassemble " + ParentObject.them + ".");
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				if ((flag & flag2) && AutoAct.ShouldHostilesInterrupt("o"))
				{
					Popup.ShowFail("You cannot use disassemble all with hostiles nearby.");
					return false;
				}
				if (ParentObject.IsInStasis())
				{
					Popup.ShowFail("You cannot seem to affect " + ParentObject.t() + " in any way.");
					return false;
				}
				if (!ParentObject.Owner.IsNullOrEmpty() && !ParentObject.HasPropertyOrTag("DontWarnOnDisassemble"))
				{
					if (Popup.ShowYesNoCancel(ParentObject.The + ParentObject.DisplayNameOnly + (flag2 ? "are" : ParentObject.Is) + " not owned by you. Are you sure you want to disassemble " + (flag2 ? "them" : ParentObject.them) + "?") != DialogResult.Yes)
					{
						return false;
					}
					if (list == null)
					{
						list = new List<Action<GameObject>>();
					}
					list.Add(ParentObject.Physics.BroadcastForHelp);
				}
				if (E.Item.IsImportant())
				{
					if (!E.Item.ConfirmUseImportant(null, "disassemble", null, (!flag2) ? 1 : count))
					{
						return false;
					}
				}
				else if (ConfirmBeforeDisassembling(ParentObject) && Popup.ShowYesNoCancel("Are you sure you want to disassemble " + (flag2 ? ("all the " + (ParentObject.GetGender().Plural ? ParentObject.ShortDisplayName : Grammar.Pluralize(ParentObject.ShortDisplayName))) : ParentObject.t()) + "?") != DialogResult.Yes)
				{
					return false;
				}
				GameObject inInventory = ParentObject.InInventory;
				if (inInventory != null && inInventory != E.Actor && !inInventory.Owner.IsNullOrEmpty() && inInventory.Owner != ParentObject.Owner && !inInventory.HasPropertyOrTag("DontWarnOnDisassemble"))
				{
					if (Popup.ShowYesNoCancel(inInventory.Does("are") + " not owned by you. Are you sure you want to disassemble " + (flag2 ? "items" : ParentObject.an()) + " inside " + inInventory.them + "?") != DialogResult.Yes)
					{
						return false;
					}
					if (list == null)
					{
						list = new List<Action<GameObject>>();
					}
					list.Add(inInventory.Physics.BroadcastForHelp);
				}
			}
			if (AutoAct.Action is Disassembly disassembly)
			{
				disassembly.Enqueue(ParentObject, (!flag2) ? 1 : count, E.Auto, list);
			}
			else
			{
				Disassembly disassembly2 = new Disassembly(ParentObject, (!flag2) ? 1 : count, E.Auto, list);
				if (flag2 || E.Auto)
				{
					AutoAct.Action = disassembly2;
					E.RequestInterfaceExit();
					E.Actor.ForfeitTurn(EnergyNeutral: true);
				}
				else
				{
					disassembly2.EnergyCostPer = 0;
					disassembly2.Continue();
					if (disassembly2.CanComplete())
					{
						disassembly2.Complete();
					}
					else
					{
						disassembly2.Interrupt();
					}
					disassembly2.End();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (ParentObject.CurrentCell != null && E.Actor.Owns(ParentObject) && Tinkering_Repair.IsRepairableBy(ParentObject, E.Actor))
		{
			GameObject who = E.Actor;
			who.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
			{
				if (who.DistanceTo(ParentObject) <= 1)
				{
					InventoryActionEvent.Check(who, who, ParentObject, "Repair");
				}
				h.FailToParent();
			}));
			who.Brain.PushGoal(new MoveTo(ParentObject, careful: false, overridesCombat: false, 1));
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool CanBeDisassembled(GameObject who = null)
	{
		bool flag = false;
		if (who != null && who.HasSkill("Tinkering_ReverseEngineer"))
		{
			foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
			{
				if (tinkerRecipe.Type == "Mod" && ParentObject.HasPart(tinkerRecipe.PartName))
				{
					flag = true;
					break;
				}
			}
		}
		if (!CanDisassemble && !flag)
		{
			return false;
		}
		if (ParentObject.IsCombat)
		{
			return false;
		}
		if (who != null && !who.HasSkill("Tinkering_Disassemble"))
		{
			return false;
		}
		if (ParentObject.HasRegisteredEvent("CanBeDisassembled") && !ParentObject.FireEvent("CanBeDisassembled"))
		{
			return false;
		}
		return true;
	}

	public static bool ConfirmBeforeDisassembling(GameObject obj)
	{
		if (obj.Equipped != null)
		{
			return true;
		}
		if (obj.Implantee != null)
		{
			return true;
		}
		if (obj.GetIntProperty("Renamed") == 1)
		{
			return true;
		}
		if (!obj.FireEvent("ConfirmBeforeDisassembling"))
		{
			return true;
		}
		return false;
	}
}
