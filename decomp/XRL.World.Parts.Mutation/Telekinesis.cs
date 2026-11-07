using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

/// This part is not used in the base game.
[Serializable]
public class Telekinesis : BaseMutation
{
	public Guid TelekinesisActivatedAbilityID = Guid.Empty;

	public Guid TelekineticThrowingActivatedAbilityID = Guid.Empty;

	public Telekinesis()
	{
		Type = "Mental";
	}

	public static int GetTelekineticRange(int Level)
	{
		return Math.Max((Level + 1) * 5, 1);
	}

	public int GetTelekineticRange()
	{
		return GetTelekineticRange(Level);
	}

	public static int GetTelekineticStrength(int Level)
	{
		return Level * 4;
	}

	public int GetTelekineticStrength()
	{
		return GetTelekineticStrength(Level);
	}

	public int GetTelekineticWeightCapacity()
	{
		return GetTelekineticStrength() * RuleSettings.MAXIMUM_CARRIED_WEIGHT_PER_STRENGTH;
	}

	public override string GetDescription()
	{
		string text = "You can move things with your mind.";
		if (Options.AnySifrah)
		{
			text += "\nUseful in many tinkering Sifrah games.";
		}
		return text;
	}

	public override string GetLevelText(int Level)
	{
		string text = "You can manipulate objects at a distance and perform some physical tasks with your mind while immobilized.\n";
		int telekineticRange = GetTelekineticRange(Level);
		text = text + "Range: " + telekineticRange + " " + ((telekineticRange == 1) ? "square" : "squares") + "\n";
		return text + "Telekinetic Strength: " + GetTelekineticStrength(Level) + "\n";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<GetThrowProfileEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == OwnerGetInventoryActionsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (GameObject.Validate(E.Target) && IsMyActivatedAbilityVoluntarilyUsable(TelekinesisActivatedAbilityID) && E.Actor.HasLOSTo(E.Target, IncludeSolid: false))
		{
			if (FindExplosiveToDetonateNear(E.Target) != null)
			{
				E.Add("CommandAIDetonateExplosive", 2);
			}
			if (FindHurlAtObjectNear(E.Target) != null)
			{
				E.Add("CommandAIHurlAt");
			}
			if (FindHurlIntoObjectNear(E.Target) != null)
			{
				E.Add("CommandAIHurlInto");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetThrowProfileEvent E)
	{
		if (IsTelekineticThrowingActive())
		{
			int telekineticRange = GetTelekineticRange();
			int telekineticStrength = GetTelekineticStrength();
			E.Strength = Math.Max(E.Strength, telekineticStrength);
			E.Range += telekineticRange;
			E.Telekinetic = true;
			if (E.Distance <= telekineticRange)
			{
				E.AimVariance = 0;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (E.Object.IsReal)
		{
			if (E.Object.CurrentCell != null)
			{
				if (E.Object.DistanceTo(ParentObject) > 1 && (E.Object.IsTakeable() || E.Object.IsCreature))
				{
					E.AddAction("Telekinetic Pull", "telekinetically pull toward you", "TelekineticPull", null, 'p', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					if (E.Object.Count > 1)
					{
						E.AddAction("Telekinetic Pull One", "telekinetically pull one toward you", "TelekineticPullOne", null, 'P', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					}
				}
				if (E.Object.IsTakeable())
				{
					E.AddAction("Telekinetic Pull And Take", "telekinetically pull towards you and take", "TelekineticPullAndTake", "take", 't', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					if (E.Object.Count > 1)
					{
						E.AddAction("Telekinetic Pull And Take One", "telekinetically pull one towards you and take", "TelekineticPullAndTakeOne", "take", 'T', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					}
				}
			}
			if ((E.Object.CurrentCell != null || E.Object.CanBeUnequipped(null, ParentObject, Forced: false, SemiForced: true)) && (E.Object.IsTakeable() || E.Object.IsCreature))
			{
				E.AddAction("Telekinetic Hurl", "telekinetically hurl", "TelekineticHurl", null, 'h', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				E.AddAction("Telekinetic Move", "telekinetically move", "TelekineticMove", null, 'm', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				if (E.Object.Count > 1)
				{
					E.AddAction("Telekinetic Hurl One", "telekinetically hurl one", "TelekineticHurlOne", null, 'H', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
					E.AddAction("Telekinetic Move One", "telekinetically move one", "TelekineticMove", null, 'M', FireOnActor: true, -1, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "TelekineticPullAndTake" || E.Command == "TelekineticPullAndTakeOne" || E.Command == "TelekineticPull" || E.Command == "TelekineticPullOne")
		{
			if (!Activate())
			{
				return false;
			}
			if (E.Command == "TelekineticPullAndTake")
			{
				_ = 1;
			}
			else
				_ = E.Command == "TelekineticPullAndTakeOne";
			bool num = E.Command == "TelekineticPullAndTakeOne" || E.Command == "TelekineticPullOne";
			GameObject Object = E.Item;
			if (num)
			{
				Object.SplitFromStack();
			}
			Cell cell = Object.CurrentCell;
			double num2 = cell.RealDistanceTo(ParentObject);
			int num3 = 0;
			if (num2 > 1.0)
			{
				num3 = Object.Accelerate(Math.Min(GetTelekineticWeightCapacity(), (int)((num2 + 1.0) * (double)Object.GetKineticResistance())), null, ParentObject.CurrentCell, null, "Telekinetic", ParentObject, Accidental: true, null, null, 1.0, SuspendFalling: true, OneShort: true);
			}
			bool flag = false;
			if (GameObject.Validate(Object))
			{
				if (E.Command == "TelekineticPullAndTake" && Object.CurrentCell.DistanceTo(ParentObject) <= 1 && ParentObject.ReceiveObject(Object))
				{
					flag = true;
				}
				else if (num3 > 0)
				{
					flag = true;
				}
				else if (Object.CurrentCell == cell && E.Actor.IsPlayer())
				{
					if (Object.IsPlayer())
					{
						Popup.ShowFail("You do not budge.");
					}
					else
					{
						Popup.ShowFail(Object.The + Object.ShortDisplayName + Object.GetVerb("do") + " not budge.");
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				ParentObject.UseEnergy(1000, "Mental Mutation Telekinesis Pull");
				E.RequestInterfaceExit();
			}
			if (GameObject.Validate(ref Object) && Object.CurrentCell == cell && num3 == 0)
			{
				Object.CheckStack();
			}
		}
		else if (E.Command == "TelekineticHurl" || E.Command == "TelekineticHurlOne" || E.Command == "TelekineticMove" || E.Command == "TelekineticMoveOne")
		{
			GameObject Object2 = E.Item;
			bool flag2 = E.Command == "TelekineticHurl" || E.Command == "TelekineticHurlOne";
			bool flag3 = E.Command == "TelekineticMove" || E.Command == "TelekineticMoveOne";
			bool flag4 = E.Command == "TelekineticHurlOne" || E.Command == "TelekineticMoveOne";
			Cell cell2 = Object2.GetCurrentCell();
			if (cell2 != null)
			{
				Cell cell3 = PickTarget.ShowPicker(PickTarget.PickStyle.Line, 999, 999, cell2.X, cell2.Y, Locked: false, AllowVis.Any, null, null, null, cell2.Pos2D, "Telekinesis");
				if (cell3 != null)
				{
					if (flag4)
					{
						Object2.SplitFromStack();
					}
					if (Object2.CurrentCell != null || Object2.TryRemoveFromContext())
					{
						if (Object2.CurrentCell == null)
						{
							cell2.AddObject(Object2);
						}
						int num4 = GetTelekineticWeightCapacity();
						if (!flag2)
						{
							num4 = Math.Min(num4, ((int)cell2.RealDistanceTo(cell3) + 1) * Object2.GetKineticResistance());
						}
						int num5 = Object2.Accelerate(num4, null, cell3, null, "Telekinetic", ParentObject, Accidental: false, null, null, 1.0, SuspendFalling: true, OneShort: false, flag2);
						if (!GameObject.Validate(Object2) || num5 > 0)
						{
							if (flag2)
							{
								ParentObject.UseEnergy(1000, "Mental Mutation Telekinesis Hurl");
							}
							else if (flag3)
							{
								ParentObject.UseEnergy(1000, "Mental Mutation Telekinesis Move");
							}
							else
							{
								ParentObject.UseEnergy(1000, "Mental Mutation Telekinesis");
							}
							E.RequestInterfaceExit();
						}
						else if (Object2.CurrentCell == cell2)
						{
							if (E.Actor.IsPlayer())
							{
								if (Object2.IsPlayer())
								{
									Popup.ShowFail("You do not budge.");
								}
								else
								{
									Popup.ShowFail(Object2.The + Object2.ShortDisplayName + Object2.GetVerb("do") + " not budge.");
								}
							}
						}
						else
						{
							E.RequestInterfaceExit();
						}
						if (GameObject.Validate(ref Object2) && Object2.CurrentCell == cell2 && num5 == 0)
						{
							Object2.CheckStack();
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandAIDetonateExplosive");
		Registrar.Register("CommandAIHurlAt");
		Registrar.Register("CommandAIHurlInto");
		Registrar.Register("CommandTelekinesis");
		Registrar.Register("CommandToggleTelekineticThrowing");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandAIHurlAt")
		{
			if (!Activate(AllowMessage: false))
			{
				return false;
			}
			GameObject target = ParentObject.Target;
			GameObject gameObject = FindHurlAtObjectNear(target);
			if (gameObject == null || ParentObject.DistanceTo(gameObject) > GetTelekineticRange())
			{
				return false;
			}
			Cell cell = target.CurrentCell;
			if (cell == null)
			{
				return false;
			}
			ParentObject.UseEnergy(1000, "Mental Mutation Telekinesis Hurl");
			gameObject.Accelerate(GetTelekineticWeightCapacity(), null, cell, null, "Telekinetic", ParentObject, Accidental: true, target, null, 1.0, SuspendFalling: true, OneShort: false, Repeat: true);
		}
		else if (E.ID == "CommandAIHurlInto")
		{
			if (!Activate(AllowMessage: false))
			{
				return false;
			}
			GameObject target2 = ParentObject.Target;
			GameObject gameObject2 = FindHurlIntoObjectNear(target2);
			if (gameObject2 == null || ParentObject.DistanceTo(target2) > GetTelekineticRange())
			{
				return false;
			}
			Cell cell2 = gameObject2.CurrentCell;
			if (cell2 == null)
			{
				return false;
			}
			ParentObject.UseEnergy(1000, "Mental Mutation Telekinesis Hurl");
			target2.Accelerate(GetTelekineticWeightCapacity(), null, cell2, null, "Telekinetic", ParentObject, Accidental: true, target2, null, 1.0, SuspendFalling: true, OneShort: false, Repeat: true);
		}
		else if (E.ID == "CommandAIDetonateExplosive")
		{
			if (!Activate(AllowMessage: false))
			{
				return false;
			}
			GameObject gameObject3 = FindExplosiveToDetonateNear(ParentObject.Target, out var action);
			if (gameObject3 == null || action == null || ParentObject.DistanceTo(gameObject3) > GetTelekineticRange())
			{
				return false;
			}
			gameObject3.TelekinesisBlip();
			action.Process(gameObject3, ParentObject, Telekinetic: true);
		}
		else if (E.ID == "CommandTelekinesis")
		{
			if (!AttemptTelekinesis())
			{
				return false;
			}
		}
		else if (E.ID == "CommandToggleTelekineticThrowing")
		{
			ToggleMyActivatedAbility(TelekineticThrowingActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		TelekinesisActivatedAbilityID = GO.AddActivatedAbility("Telekinesis", "CommandTelekinesis", "Mental Mutations", "You can use this ability to choose an object to interact with telekinetically. You can also use telekinesis directly by interacting with an object via Look or Interact Nearby.\n\nWhen moving things telekinetically, \"move\" or \"pull\" indicates moving something gently, with as little force as needed, minimizing any injuries or damage from accidents. \"Hurl\" indicates moving something with maximum possible force.");
		TelekineticThrowingActivatedAbilityID = GO.AddActivatedAbility("Telekinetic Throwing", "CommandToggleTelekineticThrowing", "Mental Mutations", "Normally, one uses one's telekinesis to augment throwing. Toggle this ability to control this behavior.", "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref TelekinesisActivatedAbilityID);
		RemoveMyActivatedAbility(ref TelekineticThrowingActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public bool IsTelekineticThrowingActive()
	{
		return IsMyActivatedAbilityVoluntarilyUsable(TelekineticThrowingActivatedAbilityID);
	}

	public bool Activate(bool AllowMessage = true)
	{
		if (!IsMyActivatedAbilityUsable(TelekinesisActivatedAbilityID))
		{
			if (AllowMessage && ParentObject.IsPlayer())
			{
				Popup.ShowFail("Your psyche is too exhausted.");
			}
			return false;
		}
		return true;
	}

	public bool AttemptTelekinesis()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (!Activate())
		{
			return false;
		}
		int telekineticRange = GetTelekineticRange();
		int x = cell.X;
		int y = cell.Y;
		List<GameObject> list = Event.NewGameObjectList();
		while (true)
		{
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, telekineticRange, telekineticRange, x, y, Locked: false, AllowVis.OnlyVisible, null, null, null, null, "Telekinesis");
			if (cell2 == null)
			{
				return false;
			}
			x = cell2.X;
			y = cell2.Y;
			foreach (GameObject @object in cell2.Objects)
			{
				if (EquipmentAPI.CanBeTwiddled(@object, ParentObject, TelekineticOnly: true))
				{
					list.Add(@object);
				}
			}
			if (list.Count > 0)
			{
				break;
			}
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("There is nothing you can telekinetically manipulate there.");
			}
		}
		GameObject gameObject = null;
		if (list.Count == 1)
		{
			gameObject = list[0];
		}
		else
		{
			gameObject = ((!ParentObject.IsPlayer()) ? list.GetRandomElement() : PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, ParentObject));
			if (gameObject == null)
			{
				return false;
			}
		}
		return gameObject.TelekineticTwiddle();
	}

	public GameObject FindExplosiveToDetonateNear(GameObject target, out InventoryAction action)
	{
		action = null;
		if (target == null)
		{
			return null;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.ParentZone == null)
		{
			return null;
		}
		Cell cell2 = target.CurrentCell;
		if (cell2 == null || cell2.ParentZone == null)
		{
			return null;
		}
		List<GameObject> list = cell2.ParentZone.FastFloodVisibility(cell2.X, cell2.Y, 3, "Physics", ParentObject);
		list.Remove(ParentObject);
		if (list.Count <= 0)
		{
			return null;
		}
		int telekineticRange = GetTelekineticRange();
		List<GameObject> list2 = Event.NewGameObjectList();
		List<InventoryAction> list3 = new List<InventoryAction>();
		foreach (GameObject item in list)
		{
			InventoryAction usefulDetonateAction = GetUsefulDetonateAction(item, ParentObject, target, telekineticRange);
			if (usefulDetonateAction != null)
			{
				list2.Add(item);
				list3.Add(usefulDetonateAction);
			}
		}
		if (list2.Count <= 0)
		{
			return null;
		}
		int index = Stat.Random(0, list2.Count - 1);
		action = list3[index];
		return list2[index];
	}

	public GameObject FindExplosiveToDetonateNear(GameObject target)
	{
		InventoryAction action;
		return FindExplosiveToDetonateNear(target, out action);
	}

	private static InventoryAction GetUsefulDetonateAction(GameObject obj, GameObject who, GameObject target, int range)
	{
		if (who == null || target == null)
		{
			return null;
		}
		if (obj.DistanceTo(target) > 3)
		{
			return null;
		}
		if (obj.DistanceTo(who) <= 3)
		{
			return null;
		}
		if (who.DistanceTo(obj) > range)
		{
			return null;
		}
		if (obj.HasPropertyOrTag("NoAIEquip"))
		{
			return null;
		}
		if (!who.HasPart<Clairvoyance>() && !who.HasLOSTo(obj))
		{
			return null;
		}
		List<InventoryAction> inventoryActions = EquipmentAPI.GetInventoryActions(obj, who, TelekineticOnly: true);
		if (inventoryActions == null)
		{
			return null;
		}
		foreach (InventoryAction item in inventoryActions)
		{
			if (item.Name == "Detonate")
			{
				return item;
			}
		}
		return null;
	}

	public GameObject FindHurlIntoObjectNear(GameObject target)
	{
		if (target == null)
		{
			return null;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.ParentZone == null)
		{
			return null;
		}
		Cell cell2 = target.CurrentCell;
		if (cell2 == null || cell2.ParentZone == null)
		{
			return null;
		}
		if (target.HasTag("ExcavatoryTerrainFeature"))
		{
			return null;
		}
		if (!Physics.IsMoveable(target))
		{
			return null;
		}
		if (ParentObject.DistanceTo(target) > GetTelekineticRange())
		{
			return null;
		}
		int num = GetTelekineticWeightCapacity() / Math.Max(target.GetKineticResistance(), 1);
		if (num < 1)
		{
			return null;
		}
		List<GameObject> list = cell2.ParentZone.FastFloodVisibility(cell2.X, cell2.Y, num, "Physics", ParentObject);
		list.Remove(ParentObject);
		if (list.Count <= 0)
		{
			return null;
		}
		List<GameObject> list2 = Event.NewGameObjectList();
		foreach (GameObject item in list)
		{
			if (IsUsefulHurlIntoObject(item, ParentObject, target, num))
			{
				list2.Add(item);
			}
		}
		return list2.GetRandomElement();
	}

	private static bool IsUsefulHurlIntoObject(GameObject obj, GameObject who, GameObject target, int hurlRange)
	{
		if (who == null || target == null)
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		if (target.DistanceTo(obj) > hurlRange)
		{
			return false;
		}
		if (who.DistanceTo(target) <= 1 && obj.DistanceTo(who) <= 1)
		{
			return false;
		}
		if (obj.GetMatterPhase() >= 2)
		{
			return false;
		}
		if (obj.IsCombat)
		{
			if (!who.IsHostileTowards(obj))
			{
				return false;
			}
			int combatDV = Stats.GetCombatDV(obj);
			if (Stat.Random(1, 30) <= combatDV || Stat.Random(1, 30) <= combatDV)
			{
				return false;
			}
		}
		else if (!obj.ConsiderSolid())
		{
			return false;
		}
		if (!obj.PhaseMatches(target) && Stat.Random(1, 30) <= who.Stat("Intelligence"))
		{
			return false;
		}
		if (!who.HasPart<Clairvoyance>() && !who.HasLOSTo(obj))
		{
			return false;
		}
		return true;
	}

	public GameObject FindHurlAtObjectNear(GameObject target)
	{
		if (target == null)
		{
			return null;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.ParentZone == null)
		{
			return null;
		}
		Cell cell2 = target.CurrentCell;
		if (cell2 == null || cell2.ParentZone == null)
		{
			return null;
		}
		int telekineticRange = GetTelekineticRange();
		List<GameObject> list = cell2.ParentZone.FastFloodVisibility(cell.X, cell.Y, telekineticRange, "Physics", ParentObject);
		list.Remove(ParentObject);
		if (list.Count <= 0)
		{
			return null;
		}
		int telekineticWeightCapacity = GetTelekineticWeightCapacity();
		List<GameObject> list2 = Event.NewGameObjectList();
		foreach (GameObject item in list)
		{
			if (IsUsefulHurlAtObject(item, ParentObject, target, telekineticRange, telekineticWeightCapacity))
			{
				list2.Add(item);
			}
		}
		return list2.GetRandomElement();
	}

	private static bool IsUsefulHurlAtObject(GameObject obj, GameObject who, GameObject target, int range, int weightCapacity)
	{
		if (who == null || target == null)
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		int num = obj.DistanceTo(target);
		if (num < 1)
		{
			return false;
		}
		int num2 = who.DistanceTo(target);
		if (num2 > range)
		{
			return false;
		}
		if (num2 <= 1 && obj.DistanceTo(who) <= 1)
		{
			return false;
		}
		if (obj.GetMatterPhase() >= 2)
		{
			return false;
		}
		int num3 = Math.Max(obj.GetKineticResistance(), 1);
		int num4 = weightCapacity / num3;
		if (num > num4)
		{
			return false;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		if (!Physics.IsMoveable(obj))
		{
			return false;
		}
		int combatDV = Stats.GetCombatDV(target);
		if (Stat.Random(1, 30) <= combatDV || Stat.Random(1, 30) <= combatDV)
		{
			return false;
		}
		if (num > 1 && !obj.HasLOSTo(target))
		{
			return false;
		}
		if (obj.IsCombat)
		{
			if (!who.IsHostileTowards(obj))
			{
				return false;
			}
		}
		else if (!obj.ConsiderSolid() && (!obj.IsTakeable() || !50.in100()))
		{
			return false;
		}
		if (!obj.PhaseMatches(target) && Stat.Random(1, 30) <= who.Stat("Intelligence"))
		{
			return false;
		}
		if (!who.HasPart<Clairvoyance>() && !who.HasLOSTo(obj))
		{
			return false;
		}
		return true;
	}
}
