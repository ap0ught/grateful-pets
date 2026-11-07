using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Teleportation : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandTeleport";

	public Teleportation()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetRetreatAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetMovementCapabilitiesEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Teleport", COMMAND_NAME, 10000, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (GameObject.Validate(E.Actor) && E.TargetCell != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && CheckMyRealityDistortionAdvisability())
		{
			int num = GetRadius(Level) + 1;
			if (E.StandoffDistance > 0)
			{
				Cell cell = null;
				int num2 = 1000;
				int num3 = 1000;
				foreach (Cell item in E.TargetCell.GetLocalAdjacentCellsAtRadius(E.StandoffDistance))
				{
					if (!item.IsEmptyOfSolidFor(E.Actor) || !IComponent<GameObject>.CheckRealityDistortionAdvisability(null, item, E.Actor, null, this))
					{
						continue;
					}
					int navigationWeightFor = item.GetNavigationWeightFor(E.Actor);
					if (navigationWeightFor < 10)
					{
						int num4 = E.Actor.DistanceTo(item);
						if (num4 + navigationWeightFor < num2 + num3)
						{
							cell = item;
							num2 = num4;
							num3 = navigationWeightFor;
						}
					}
				}
				if (cell != null && Stat.Random(1, 40) <= Math.Max(num2 - num3, 1))
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell);
				}
			}
			else if (!E.TargetCell.IsEmptyOfSolidFor(E.Actor) || E.TargetCell.GetNavigationWeightFor(E.Actor) >= 10)
			{
				Cell randomLocalAdjacentCell = E.TargetCell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, null, this) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCell != null && E.Actor.DistanceTo(randomLocalAdjacentCell) > num)
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCell);
				}
			}
			else if (E.Distance > num && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, E.TargetCell, E.Actor, null, this))
			{
				E.Add(COMMAND_NAME);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		if (GameObject.Validate(E.Actor) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && CheckMyRealityDistortionAdvisability())
		{
			int errorRadius = GetRadius(Level) + 1;
			if (E.TargetCell == null)
			{
				if (E.AvoidCell != null || E.Target != null)
				{
					Cell avoidCell = E.AvoidCell ?? E.Target?.CurrentCell;
					if (avoidCell != null)
					{
						Cell cell = null;
						int cellDistance = 0;
						int cellWeight = 0;
						E.Actor.CurrentZone?.ForeachCell((Cell c) =>
						{
							try
							{
								if (c != null && c.IsEmptyOfSolidFor(E.Actor) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, null, this))
								{
									int navigationWeightFor = c.GetNavigationWeightFor(E.Actor);
									if (navigationWeightFor < 10)
									{
										int num = avoidCell.DistanceTo(c);
										if (num - navigationWeightFor > cellDistance - cellWeight && E.Actor.DistanceTo(c) > errorRadius)
										{
											cell = c;
											cellDistance = num;
											cellWeight = navigationWeightFor;
										}
									}
								}
							}
							catch (Exception x)
							{
								MetricsManager.LogException("Teleportation::AIGetRetreatAbilityListEvent", x);
							}
						});
						if (cell != null)
						{
							E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell);
						}
					}
				}
				else
				{
					Zone currentZone = E.Actor.CurrentZone;
					Cell cell2 = ((currentZone != null) ? currentZone.GetEmptyReachableCells().GetRandomElement() : null);
					if (cell2 != null && E.Actor.DistanceTo(cell2) > errorRadius && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, cell2, E.Actor, null, this) && cell2.GetNavigationWeightFor(E.Actor) < 10)
					{
						E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell2);
					}
				}
			}
			else if (E.StandoffDistance > 0)
			{
				Cell randomLocalAdjacentCellAtRadius = E.TargetCell.GetRandomLocalAdjacentCellAtRadius(E.StandoffDistance, (Cell c) => c.IsEmptyOfSolidFor(E.Actor) && E.Actor.DistanceTo(c) > errorRadius && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, null, this) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCellAtRadius != null)
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCellAtRadius);
				}
			}
			else if (!E.TargetCell.IsEmptyOfSolidFor(E.Actor) || E.TargetCell.GetNavigationWeightFor(E.Actor) >= 10)
			{
				Cell randomLocalAdjacentCell = E.TargetCell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(E.Actor) && E.Actor.DistanceTo(c) > errorRadius && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, c, E.Actor, null, this) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCell != null)
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCell);
				}
			}
			else if (E.Distance > errorRadius && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, E.TargetCell, E.Actor, null, this))
			{
				E.Add(COMMAND_NAME);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (GameObject.Validate(E.Target) && E.Distance > 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, E.Target.CurrentCell, E.Actor, null, this))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !Cast(this, "5-6", E, E.TargetCell, null, Automatic: false, E.StandoffDistance))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You teleport to a nearby location.";
	}

	public static int GetCooldown(int Level)
	{
		return Math.Max(103 - 3 * Level, 5);
	}

	public int GetRadius(int Level)
	{
		return Math.Max(13 - Level, 2);
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("Teleport to a random location within a designated area.\n" + "Uncertainty radius: {{rules|" + GetRadius(Level) + "}}\n", "Cooldown: {{rules|", GetCooldown(Level).ToString(), "}} rounds");
	}

	public static bool Cast(Teleportation Mutation = null, string Level = "5-6", IEvent FromEvent = null, Cell Destination = null, GameObject Subject = null, bool Automatic = false, int WantDistance = 0)
	{
		if (Mutation == null)
		{
			Mutation = new Teleportation();
			Mutation.ParentObject = Subject ?? The.Player;
			Mutation.Level = Level.RollCached();
		}
		Cell cell = null;
		GameObject parentObject = Mutation.ParentObject;
		if (!parentObject.IsRealityDistortionUsable())
		{
			RealityStabilized.ShowGenericInterdictMessage(parentObject);
			return false;
		}
		int num = 0;
		int num2 = Math.Max(Mutation.Level, 1);
		List<Cell> list = null;
		while (cell == null && num2-- > 0)
		{
			if (Destination != null)
			{
				cell = Destination;
				if (WantDistance > 0)
				{
					cell = cell.GetRandomLocalAdjacentCellAtRadius(WantDistance) ?? cell;
				}
				if (num > 0)
				{
					cell = cell.GetLocalAdjacentCells(num).GetRandomElement() ?? cell;
				}
			}
			else
			{
				if (list == null)
				{
					if (parentObject.IsPlayer())
					{
						if (parentObject.OnWorldMap())
						{
							if (!Automatic)
							{
								parentObject.Fail("You may not teleport on the world map.");
							}
							return false;
						}
						list = Mutation.PickCircle(Mutation.GetRadius(Mutation.Level), 9999, Locked: false, AllowVis.OnlyExplored, "Teleport");
					}
					else
					{
						GameObject gameObject = parentObject.Target ?? parentObject.PartyLeader;
						if (gameObject == null)
						{
							return false;
						}
						list = gameObject.CurrentCell.GetLocalAdjacentCells(num);
					}
					if (list == null)
					{
						return false;
					}
				}
				cell = list.GetRandomElement();
			}
			if (cell == null)
			{
				break;
			}
			if (!cell.IsEmptyFor(parentObject))
			{
				cell = cell.GetConnectedSpawnLocation();
				if (cell != null && parentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You are shunted to another location!");
				}
			}
		}
		if (cell == null)
		{
			return parentObject.Fail("The teleportation fails!");
		}
		if (parentObject.InActiveZone() && !Options.UseParticleVFX)
		{
			parentObject.ParticleBlip("&C\u000f", 10, 0L);
		}
		Event e = Event.New("InitiateRealityDistortionTransit", "Object", parentObject, "Mutation", Mutation, "Cell", cell);
		if (!parentObject.FireEvent(e, FromEvent) || !cell.FireEvent(e, FromEvent))
		{
			return false;
		}
		if (parentObject.InActiveZone())
		{
			parentObject.GetCurrentCell();
			parentObject.TeleportSwirl(null, "&C", Voluntary: true, null, 'ù', IsOut: true);
			parentObject.TeleportTo(cell, 0);
			parentObject.TeleportSwirl(null, "&C", Voluntary: true);
			if (!Options.UseParticleVFX)
			{
				parentObject.ParticleBlip("&C\u000f", 10, 0L);
			}
			if (parentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You teleport!");
			}
		}
		Mutation.UseEnergy(1000, "Mental Mutation Teleportation");
		int cooldown = GetCooldown(Mutation.Level);
		Mutation.CooldownMyActivatedAbility(Mutation.ActivatedAbilityID, cooldown);
		return true;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("UncertaintyRadius", GetRadius(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Teleport", COMMAND_NAME, "Mental Mutations", null, "\u001d", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
