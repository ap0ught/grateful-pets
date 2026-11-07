using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsStasisProjector : IPoweredPart
{
	public int BaseCoverage = 6;

	public int BaseCooldown = 100;

	public string BaseDuration = "6-8";

	public string CommandID;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsStasisProjector()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "StasisProjector";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetRetreatAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee);
		int num = stats.CollectComputePowerAdjustUp(ability, "Squares of stasis field", BaseCoverage);
		stats.Set("Coverage", num, num != BaseCoverage, num - BaseCoverage);
		var (dieRoll, num2) = stats.CollectComputePowerAdjustRangeUp(ability, "Duration", BaseDuration.GetCachedDieRoll());
		stats.Set("Duration", dieRoll.ToString(), num2 != 0, num2);
		int num3 = stats.CollectComputePowerAdjustDown(ability, "Cooldown", BaseCooldown);
		stats.CollectCooldownTurns(ability, num3, num3 - BaseCooldown);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!CommandID.IsNullOrEmpty() && E.Distance == 1 && GameObject.Validate(E.Actor) && GameObject.Validate(E.Target) && IsObjectActivePartSubject(E.Actor) && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, E.Target.CurrentCell, E.Actor, ParentObject))
		{
			E.Add(CommandID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		if (!CommandID.IsNullOrEmpty() && GameObject.Validate(E.Actor) && IsObjectActivePartSubject(E.Actor) && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			int coverage = GetCoverage(E.Actor);
			GameObject target = null;
			int targetDistance = int.MaxValue;
			E.Actor.CurrentZone.ForeachObject((GameObject obj) =>
			{
				if (obj.IsCreature && obj != E.Actor && E.Actor.IsHostileTowards(obj) && E.Actor.HasLOSTo(obj, IncludeSolid: true, BlackoutStops: false, UseTargetability: true) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, obj.CurrentCell, E.Actor, ParentObject))
				{
					int num = E.Actor.DistanceTo(obj);
					if (num < targetDistance && num < coverage)
					{
						target = obj;
						targetDistance = num;
					}
				}
			});
			if (target != null)
			{
				E.Add(CommandID, 1, null, Inv: false, Self: false, target);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out CommandID, "ProjectStasisField", "Project Stasis Field", "Cybernetics", null, "é");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID)
		{
			if (!IsObjectActivePartSubject(E.Actor))
			{
				return false;
			}
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
			if (!E.Actor.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", E.Actor, "Device", this), E))
			{
				return false;
			}
			int coverage = GetCoverage(E.Actor);
			List<Cell> list = null;
			if (E.Actor.IsPlayer())
			{
				list = PickFieldAdjacent(coverage, E.Actor, "Field");
			}
			else if (E.Target != null)
			{
				list = new List<Cell>();
				Cell cell = E.Target.CurrentCell;
				Cell cell2 = E.Actor.CurrentCell;
				int num = 0;
				while (list.Count < coverage && cell2 != null && cell2 != cell && ++num < 100)
				{
					string generalDirectionFromCell = cell2.GetGeneralDirectionFromCell(cell);
					if (generalDirectionFromCell.IsNullOrEmpty())
					{
						continue;
					}
					Cell cell3 = cell2.GetCellFromDirection(generalDirectionFromCell);
					if (cell3 != null)
					{
						if (list.Contains(cell3))
						{
							cell3 = null;
						}
						else
						{
							list.Add(cell3);
						}
					}
					cell2 = cell3;
				}
			}
			else
			{
				Cell cell4 = E.Actor.CurrentCell;
				List<GameObject> list2 = cell4.ParentZone.FastCombatSquareVisibility(cell4.X, cell4.Y, 25, E.Actor, E.Actor.IsHostileTowards);
				if (list2.Count > 0)
				{
					int num2 = 0;
					List<GameObject> list3 = Event.NewGameObjectList();
					foreach (GameObject item in list2)
					{
						if (item.InAdjacentCellTo(E.Actor))
						{
							list3.Add(item);
						}
					}
					if (list3.Count > 0)
					{
						list = new List<Cell>(list2.Count);
						GameObject randomElement = list3.GetRandomElement();
						list2.Remove(randomElement);
						list.Add(randomElement.CurrentCell);
						List<Cell> list4 = new List<Cell>();
						while (list2.Count > 1 && list.Count < coverage && ++num2 < 100)
						{
							Cell cell5 = list[list.Count - 1];
							Cell cell6 = null;
							list4.Clear();
							foreach (Cell localCardinalAdjacentCell in cell5.GetLocalCardinalAdjacentCells())
							{
								if (!list.Contains(localCardinalAdjacentCell) && !localCardinalAdjacentCell.Objects.Contains(E.Actor))
								{
									list4.Add(localCardinalAdjacentCell);
								}
							}
							list4.ShuffleInPlace();
							foreach (Cell item2 in list4)
							{
								foreach (GameObject item3 in list2)
								{
									if (item2.Objects.Contains(item3))
									{
										cell6 = item2;
										break;
									}
								}
								if (cell6 != null)
								{
									break;
								}
							}
							if (cell6 == null)
							{
								int num3 = 0;
								foreach (Cell item4 in list4)
								{
									int num4 = 0;
									foreach (GameObject item5 in list2)
									{
										num4 += 100 - item5.DistanceTo(item4);
									}
									if (num4 > num3)
									{
										cell6 = item4;
										num3 = num4;
									}
								}
							}
							if (cell6 == null)
							{
								break;
							}
							list.Add(cell6);
							foreach (GameObject @object in cell6.Objects)
							{
								list2.Remove(@object);
							}
						}
					}
				}
			}
			if (list == null || list.Count == 0)
			{
				return false;
			}
			E.Actor.UseEnergy(1000, "Cybernetics Stasis Projector");
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown(E.Actor));
			DeployToCells(list, E.Actor);
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			if (E.Actor.IsPlayer())
			{
				IComponent<GameObject>.XDidY(E.Actor, "project", "a stasis field", null, null, null, E.Actor);
			}
			else
			{
				IComponent<GameObject>.XDidY(E.Actor, "project", "a stasis field", null, null, null, E.Actor, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
			}
		}
		return base.HandleEvent(E);
	}

	private GameObject DeployToCells(List<Cell> Cells, GameObject Actor)
	{
		GameObject result = null;
		TextConsole textConsole = Look._TextConsole;
		The.Core.RenderBase();
		textConsole.WaitFrame();
		foreach (Cell Cell in Cells)
		{
			if (IComponent<GameObject>.CheckRealityDistortionAccessibility(null, Cell, Actor, ParentObject))
			{
				GameObject gameObject = GameObject.Create("Stasisfield");
				gameObject.GetPart<Forcefield>().Creator = Actor;
				gameObject.AddPart(new Temporary(GetDuration() + 1));
				Phase.carryOver(Actor, gameObject);
				Cell.AddObject(gameObject);
				result = gameObject;
				The.Core.RenderBase();
			}
			textConsole.WaitFrame();
		}
		return result;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int GetCoverage(GameObject who = null)
	{
		return GetAvailableComputePowerEvent.AdjustUp(who ?? ParentObject.Implantee, BaseCoverage);
	}

	public int GetDuration(GameObject who = null)
	{
		return GetAvailableComputePowerEvent.AdjustUp(who ?? ParentObject.Implantee, BaseDuration.RollCached());
	}

	public int GetCooldown(GameObject who = null)
	{
		return GetAvailableComputePowerEvent.AdjustDown(who ?? ParentObject.Implantee, BaseCooldown);
	}
}
