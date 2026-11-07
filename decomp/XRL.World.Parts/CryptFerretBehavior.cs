using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class CryptFerretBehavior : IPart
{
	public bool Fleeing;

	public string behaviorState = "hunting";

	public Location2D startingLocation;

	private List<Cell> hiddenCells = new List<Cell>();

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("AICantAttackRange");
		base.Register(Object, Registrar);
	}

	public void scurry()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || !cell.IsVisible())
		{
			return;
		}
		if (hiddenCells.Count == 0)
		{
			hiddenCells.Clear();
			hiddenCells.AddRange(from c in cell.ParentZone.GetCells()
				where !c.IsVisible() && c.IsPassable()
				select c);
			hiddenCells.Sort((Cell a, Cell b) => a.DistanceTo(ParentObject).CompareTo(b.DistanceTo(ParentObject)));
		}
		List<Cell> list = null;
		int num = 4;
		foreach (Cell hiddenCell in hiddenCells)
		{
			if (!hiddenCell.IsVisible())
			{
				if (ParentObject.canPathTo(hiddenCell))
				{
					ParentObject.Brain.Goals.Clear();
					ParentObject.Brain.FleeTo(hiddenCell, 1);
					ParentObject.Brain.MoveTo(hiddenCell, ClearFirst: false);
					if (list == null)
					{
						return;
					}
					{
						foreach (Cell item in list)
						{
							hiddenCells.Remove(item);
						}
						return;
					}
				}
				if (list == null)
				{
					list = new List<Cell>();
				}
				list.Add(hiddenCell);
			}
			num--;
			if (num < 0)
			{
				break;
			}
		}
		hiddenCells.Clear();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICantAttackRange")
		{
			if (Fleeing)
			{
				GameObject target = ParentObject.Target;
				if (target != null && (target.IsPlayer() || target.IsPlayerLed()))
				{
					scurry();
					return false;
				}
				if (ParentObject.CurrentCell.FastFloodVisibilityFirstBlueprint("Reliquary", ParentObject) != null)
				{
					behaviorState = "looting";
				}
			}
			return true;
		}
		if (E.ID == "BeginTakeAction")
		{
			if (behaviorState == "fleeing")
			{
				behaviorState = "hunting";
				ParentObject.UseEnergy(1000);
				ParentObject.TeleportSwirl(null, "&C", Voluntary: false, null, 'ù', IsOut: true);
				ParentObject.TeleportTo(ParentObject.CurrentZone.GetEmptyCells().GetRandomElement(), 0);
				ParentObject.TeleportSwirl();
			}
			else if (behaviorState == "looting")
			{
				GameObject gameObject = ParentObject.CurrentCell.FastFloodVisibilityFirstBlueprint("Reliquary", ParentObject);
				if (gameObject != null)
				{
					if (ParentObject.DistanceTo(gameObject) <= 1)
					{
						GameObject gameObject2 = gameObject.Inventory.Objects.RemoveRandomElement();
						if (gameObject2 != null)
						{
							gameObject.GetPart<AICryptHelpBroadcaster>().BroadcastHelp();
							DidXToY("filch", gameObject2, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
							ParentObject.TakeObject(gameObject2, NoStack: false, Silent: false, 0);
						}
						behaviorState = "fleeing";
					}
					else
					{
						if (ParentObject.Brain.Goals.Items.Any((GoalHandler g) => g.GetType() == typeof(Step) || g.GetType() == typeof(MoveTo)))
						{
							return true;
						}
						ParentObject.Brain.PushGoal(new NoFightGoal());
						ParentObject.Brain.MoveTo(gameObject, ClearFirst: false);
					}
				}
			}
			else if (behaviorState == "hunting")
			{
				int num = 200;
				if (ParentObject.CurrentZone != null)
				{
					if (ParentObject.CurrentZone.Z == 14)
					{
						num = The.Game.RequireSystem<CatacombsAnchorSystem>().Timer;
					}
					if (ParentObject.CurrentZone.Z == 13)
					{
						num = The.Game.RequireSystem<CryptOfLandlordsAnchorSystem>().Timer;
					}
					if (ParentObject.CurrentZone.Z == 12)
					{
						num = The.Game.RequireSystem<CryptOfWarriorsAnchorSystem>().Timer;
					}
					if (ParentObject.CurrentZone.Z == 11)
					{
						num = The.Game.RequireSystem<CryptOfPriestsAnchorSystem>().Timer;
					}
				}
				if (num <= 20)
				{
					if (Fleeing)
					{
						ParentObject.Brain.Goals.Clear();
						Fleeing = false;
					}
				}
				else if (!Fleeing)
				{
					ParentObject.Brain.Goals.Clear();
					Fleeing = true;
				}
			}
		}
		return base.FireEvent(E);
	}
}
