using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ForceWall : BaseMutation
{
	public ForceWall()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetRetreatAbilityListEvent.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && CheckMyRealityDistortionAdvisability())
		{
			Cell cell = E.Actor?.CurrentCell;
			if (cell != null && cell.ParentZone != null)
			{
				GameObject gameObject = cell.ParentZone.FastSquareVisibilityFirst(cell.X, cell.Y, 25, "ForceWallTarget", E.Actor);
				if (gameObject != null && E.Distance < E.Actor.DistanceTo(gameObject))
				{
					E.Add("CommandForceWall");
				}
				else if (GameObject.Validate(E.Target))
				{
					Cell cell2 = E.Target.CurrentCell;
					if (cell2 != null && cell2.ParentZone != null && !cell2.ParentZone.FastSquareVisibilityAny(cell2.X, cell2.Y, 5, "Forcefield", E.Actor))
					{
						E.Add("CommandForceWall");
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && CheckMyRealityDistortionAdvisability())
		{
			GameObject target = null;
			int targetDistance = int.MaxValue;
			E.Actor.CurrentZone.ForeachObject((GameObject obj) =>
			{
				if (obj.IsCreature && obj != E.Actor && E.Actor.IsHostileTowards(obj) && E.Actor.HasLOSTo(obj, IncludeSolid: true, BlackoutStops: false, UseTargetability: true) && IComponent<GameObject>.CheckRealityDistortionAdvisability(null, obj.CurrentCell, E.Actor, null, this))
				{
					int num = E.Actor.DistanceTo(obj);
					if (num < targetDistance)
					{
						target = obj;
						targetDistance = num;
					}
				}
			});
			if (target != null)
			{
				E.Add("CommandForceWall", 1, null, Inv: false, Self: false, target);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandForceWall")
		{
			if (ParentObject.OnWorldMap())
			{
				ParentObject.Fail("You cannot do that on the world map.");
				return false;
			}
			if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			List<Cell> list = null;
			if (ParentObject.IsPlayer())
			{
				list = PickField(9);
			}
			else
			{
				Cell cell = ParentObject.CurrentCell;
				List<GameObject> list2 = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 25, "ForceWallTarget", ParentObject);
				if (list2.Count > 0)
				{
					list = new List<Cell>(list2.Count);
					foreach (GameObject item in list2)
					{
						list.Add(item.CurrentCell);
					}
				}
				else
				{
					GameObject gameObject = E.Target ?? ParentObject.Target;
					if (gameObject == null)
					{
						return false;
					}
					if (DoIHaveAMissileWeapon() || ParentObject.HasGoal("Retreat") || ParentObject.HasGoal("Flee") || ParentObject.HasGoal("FleeLocation") || ParentObject.HasPart<Cryokinesis>() || ParentObject.HasPart<Pyrokinesis>() || ParentObject.HasPart(typeof(FlamingRay)) || ParentObject.HasPart(typeof(FreezingRay)))
					{
						list = gameObject.CurrentCell.GetLocalAdjacentCells();
					}
					else
					{
						string directionFromCell = cell.GetDirectionFromCell(gameObject.CurrentCell);
						Cell cellFromDirection = gameObject.CurrentCell.GetCellFromDirection(directionFromCell);
						if (cellFromDirection != null)
						{
							list = new List<Cell>(9) { cellFromDirection };
							string[] orthogonalDirections = Directions.GetOrthogonalDirections(directionFromCell);
							Cell cell2 = cellFromDirection;
							Cell cell3 = cellFromDirection;
							for (int i = 0; i < 4; i++)
							{
								cell2 = cell2?.GetCellFromDirection(orthogonalDirections[0]);
								cell3 = cell3?.GetCellFromDirection(orthogonalDirections[1]);
								if (cell2 != null)
								{
									list.Add(cell2);
								}
								if (cell3 != null)
								{
									list.Add(cell3);
								}
							}
						}
					}
				}
			}
			if (list == null || list.Count == 0)
			{
				return false;
			}
			UseEnergy(1000, "Mental Mutation ForceWall");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			int duration = GetDuration(Level) + 1;
			foreach (Cell item2 in list)
			{
				if (IComponent<GameObject>.CheckRealityDistortionAccessibility(null, item2, ParentObject, null, this))
				{
					GameObject gameObject2 = GameObject.Create("Forcefield");
					Forcefield part = gameObject2.GetPart<Forcefield>();
					part.Creator = ParentObject;
					part.RejectOwner = false;
					gameObject2.AddPart(new Temporary(duration));
					Phase.carryOver(ParentObject, gameObject2);
					item2.AddObject(gameObject2);
				}
			}
			DidX("conjure", "a {{B|wall of force}}", "!", null, null, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You generate a wall of force that protects you from your enemies.";
	}

	public int GetCooldown(int Level)
	{
		return 100;
	}

	public int GetDuration(int Level)
	{
		return 14 + Level * 2;
	}

	public override string GetLevelText(int Level)
	{
		int cooldown = GetCooldown(Level);
		return string.Concat(string.Concat("Creates 9 contiguous squares of immobile forcefield.\n" + "Duration: {{rules|" + GetDuration(Level) + "}} rounds\n", "Cooldown: ", cooldown.ToString(), " rounds\n"), "You may fire missile weapons through the forcefield.");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Duration", GetDuration(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Force Wall", "CommandForceWall", "Mental Mutations", null, "°", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
