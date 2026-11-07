using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsHighFidelityMatterRecompositer : IPart
{
	public string commandId = "";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee), GetAvailableComputePowerEvent.AdjustDown(ParentObject, 50));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetRetreatAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("High-Fidelity Matter Recompositer", commandId, 11000, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject.Implantee))
		{
			E.Add("travel", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (!commandId.IsNullOrEmpty() && GameObject.Validate(E.Actor) && E.TargetCell != null && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			if (E.StandoffDistance > 0)
			{
				Cell cell = null;
				int num = 1000;
				int num2 = 1000;
				foreach (Cell item in E.TargetCell.GetLocalAdjacentCellsAtRadius(E.StandoffDistance))
				{
					if (!item.IsEmptyOfSolidFor(E.Actor) || !IComponent<GameObject>.CheckRealityDistortionAdvisability(null, item, E.Actor, ParentObject))
					{
						continue;
					}
					int navigationWeightFor = item.GetNavigationWeightFor(E.Actor);
					if (navigationWeightFor < 10)
					{
						int num3 = E.Actor.DistanceTo(item);
						if (num3 + navigationWeightFor < num + num2)
						{
							cell = item;
							num = num3;
							num2 = navigationWeightFor;
						}
					}
				}
				if (cell != null && Stat.Random(1, 40) <= Math.Max(num - num2, 1))
				{
					E.Add(commandId, 1, null, Inv: false, Self: false, null, cell);
				}
			}
			else if (!E.TargetCell.IsEmptyOfSolidFor(E.Actor) || E.TargetCell.GetNavigationWeightFor(E.Actor) >= 10)
			{
				Cell randomLocalAdjacentCell = E.TargetCell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, ParentObject) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCell != null && Stat.Random(1, 40) < E.Actor.DistanceTo(randomLocalAdjacentCell))
				{
					E.Add(commandId, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCell);
				}
			}
			else if (Stat.Random(1, 40) < E.Distance && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, E.TargetCell, E.Actor, ParentObject))
			{
				E.Add(commandId);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		if (!commandId.IsNullOrEmpty() && GameObject.Validate(E.Actor) && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			if (E.TargetCell == null)
			{
				Cell avoidCell = E.AvoidCell ?? E.Target?.CurrentCell;
				if (avoidCell != null)
				{
					Cell cell = null;
					int cellDistance = 0;
					int cellWeight = 0;
					E.Actor.CurrentZone?.ForeachCell((Cell c) =>
					{
						if (c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, ParentObject))
						{
							int navigationWeightFor = c.GetNavigationWeightFor(E.Actor);
							if (navigationWeightFor < 10)
							{
								int num = avoidCell.DistanceTo(c);
								if (num - navigationWeightFor > cellDistance - cellWeight)
								{
									cell = c;
									cellDistance = num;
									cellWeight = navigationWeightFor;
								}
							}
						}
					});
					if (cell != null)
					{
						E.Add(commandId, 1, null, Inv: false, Self: false, null, cell);
					}
				}
				else
				{
					Zone currentZone = E.Actor.CurrentZone;
					Cell cell2 = ((currentZone != null) ? currentZone.GetEmptyReachableCells().GetRandomElement() : null);
					if (cell2 != null && Stat.Random(1, 20) < E.Actor.DistanceTo(cell2) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, cell2, E.Actor, ParentObject) && cell2.GetNavigationWeightFor(E.Actor) < 10)
					{
						E.Add(commandId, 1, null, Inv: false, Self: false, null, cell2);
					}
				}
			}
			else if (E.StandoffDistance > 0)
			{
				Cell randomLocalAdjacentCellAtRadius = E.TargetCell.GetRandomLocalAdjacentCellAtRadius(E.StandoffDistance, (Cell c) => c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, ParentObject) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCellAtRadius != null && Stat.Random(1, 20) < E.Actor.DistanceTo(randomLocalAdjacentCellAtRadius))
				{
					E.Add(commandId, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCellAtRadius);
				}
			}
			else if (!E.TargetCell.IsEmptyOfSolidFor(E.Actor) || E.TargetCell.GetNavigationWeightFor(E.Actor) >= 10)
			{
				Cell randomLocalAdjacentCell = E.TargetCell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, ParentObject) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCell != null && Stat.Random(1, 20) < E.Actor.DistanceTo(randomLocalAdjacentCell))
				{
					E.Add(commandId, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCell);
				}
			}
			else if (Stat.Random(1, 20) < E.Distance && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, E.TargetCell, E.Actor, ParentObject))
			{
				E.Add(commandId);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!commandId.IsNullOrEmpty() && GameObject.Validate(E.Target) && GameObject.Validate(E.Actor) && E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			Cell cell = E.Target.CurrentCell?.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, ParentObject) && c.GetNavigationWeightFor(E.Actor) < 20);
			if (cell != null && Stat.Random(1, 20) < E.Actor.DistanceTo(cell))
			{
				E.Add(commandId, 1, null, Inv: false, Self: false, null, cell);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out commandId, "CommandRecomposite", "Recomposite", "Cybernetics", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor != null && E.Actor == ParentObject.Implantee && E.Actor.CurrentCell != null)
		{
			if (E.Actor.OnWorldMap())
			{
				return E.Actor.Fail("You cannot do that on the world map.");
			}
			Cell cell = E.TargetCell;
			if (E.Actor.IsPlayer())
			{
				cell = new Teleportation
				{
					ParentObject = E.Actor,
					Level = 10
				}.PickDestinationCell(9999, AllowVis.OnlyExplored, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Transit");
			}
			else if (cell == null)
			{
				cell = (E.Target ?? E.Actor.Target)?.CurrentCell?.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(E.Actor));
			}
			if (cell == null)
			{
				return false;
			}
			if (E.Actor.IsPlayer() && !cell.IsExplored())
			{
				return E.Actor.Fail("You can only teleport to a place you have seen before!");
			}
			if (!cell.IsEmptyOfSolidFor(E.Actor))
			{
				return E.Actor.Fail("You may only teleport into an empty square!");
			}
			Event e = Event.New("InitiateRealityDistortionTransit", "Object", E.Actor, "Device", ParentObject, "Cell", cell);
			if (!E.Actor.FireEvent(e, E) || !cell.FireEvent(e, E))
			{
				return false;
			}
			E.Actor.TechTeleportSwirlOut();
			if (E.Actor.TeleportTo(cell, 0))
			{
				E.Actor.TechTeleportSwirlIn();
			}
			IComponent<GameObject>.XDidY(E.Actor, "teleport", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, GetAvailableComputePowerEvent.AdjustDown(E.Actor, 50));
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice reduces this item's cooldown.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
