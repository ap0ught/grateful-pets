using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Infiltrate : BaseMutation
{
	public int Infiltrating;

	[NonSerialized]
	private Cell teleportDestination;

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveAbilityListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (Infiltrating <= 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !E.Actor.OnWorldMap() && E.Distance <= GetTeleportDistance(Level))
		{
			E.Add("CommandInfiltrate");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandInfiltrate");
		base.Register(Object, Registrar);
	}

	private bool ValidGazeTarget(GameObject obj)
	{
		return obj?.IsCombat ?? false;
	}

	public void PickInfiltrateDestination()
	{
		if (Infiltrating <= 0)
		{
			if (!IsPlayer() && ParentObject.CurrentCell != null)
			{
				int num = int.MinValue;
				GameObject gameObject = null;
				foreach (GameObject item in ParentObject.CurrentCell.ParentZone.LoopObjectsWithPart("Combat"))
				{
					if (ParentObject.Brain.GetFeeling(item) <= 0 && ParentObject.DistanceTo(item) > num)
					{
						num = ParentObject.DistanceTo(item);
						gameObject = item;
					}
				}
				if (gameObject != null)
				{
					teleportDestination = gameObject.Physics.CurrentCell.getClosestPassableCell();
				}
			}
			else
			{
				teleportDestination = PickDestinationCell(GetTeleportDistance(Level), AllowVis.Any, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Infiltrate");
			}
			if (teleportDestination == null)
			{
				return;
			}
			if (!teleportDestination.IsPassable())
			{
				teleportDestination = teleportDestination.GetFirstEmptyAdjacentCell(1, 80);
			}
		}
		if (teleportDestination == null)
		{
			return;
		}
		Event.NewGameObjectList().Add(ParentObject);
		foreach (Cell localAdjacentCell in ParentObject.Physics.CurrentCell.GetLocalAdjacentCells(GetTeleportRadius(Level) + GetTurnsToCharge()))
		{
			foreach (GameObject item2 in localAdjacentCell.LoopObjectsWithPart("Combat"))
			{
				if (item2 != null && item2.IsMemberOfFaction("Templar"))
				{
					item2.Brain.PushGoal(new MoveTo(ParentObject));
					break;
				}
			}
		}
	}

	public void performInfiltrate(Cell teleportDestination, bool bDoEffect = true)
	{
		if (ParentObject.Physics.CurrentCell != null)
		{
			try
			{
				ParentObject.DilationSplat();
				Point2D point2D = teleportDestination.Pos2D - ParentObject.Physics.CurrentCell.Pos2D;
				List<GameObject> list = Event.NewGameObjectList();
				list.Add(ParentObject);
				foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(GetTeleportRadius(Level)))
				{
					foreach (GameObject item in localAdjacentCell.LoopObjectsWithPart("Combat"))
					{
						if (!list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
				foreach (GameObject item2 in list)
				{
					Cell cell = item2.Physics.CurrentCell;
					if (cell != null && cell.Location != null)
					{
						Point2D p = cell.Pos2D + point2D;
						if (p.x < 0)
						{
							p.x = 0;
						}
						if (p.y > 79)
						{
							p.y = 79;
						}
						if (p.x < 0)
						{
							p.x = 0;
						}
						if (p.y > 24)
						{
							p.y = 24;
						}
						Cell cell2 = cell.ParentZone.GetCell(p);
						if (cell2 != null)
						{
							if (!cell2.IsPassable())
							{
								cell2 = cell2.GetFirstEmptyAdjacentCell(1, 80);
							}
							if (cell2 != null)
							{
								item2.TeleportSwirl(null, "&C", Voluntary: false, null, 'ù', IsOut: true);
								item2.TeleportTo(cell2, 0);
								item2.TeleportSwirl();
								if (item2.IsPlayer() && !ParentObject.IsPlayer())
								{
									IComponent<GameObject>.AddPlayerMessage("You are teleported by " + ParentObject.the + ParentObject.ShortDisplayName + ".");
								}
							}
						}
					}
					ParentObject.DilationSplat();
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Infiltrator teleport", x);
			}
		}
		Infiltrating = 0;
		teleportDestination = null;
		ParentObject.UseEnergy(1000, "Physical Mutation");
		CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
	}

	public int GetTurnsToCharge()
	{
		return 3;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (teleportDestination != null && ParentObject.CurrentCell != null)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (teleportDestination != null && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = "&r^R";
			}
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (teleportDestination != null && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				buffer.Goto(teleportDestination.X, teleportDestination.Y);
				buffer.Write("&RX");
				buffer.Buffer[teleportDestination.X, teleportDestination.Y].TileForeground = The.Color.DarkRed;
				buffer.Buffer[teleportDestination.X, teleportDestination.Y].Detail = The.Color.DarkRed;
			}
		}
		base.OnPaint(buffer);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Infiltrating > 0)
			{
				if (teleportDestination == null)
				{
					PickInfiltrateDestination();
				}
				if (teleportDestination == null)
				{
					Infiltrating = 0;
				}
				else
				{
					Infiltrating--;
					ParentObject.UseEnergy(1000, "Physical Mutation");
					if (Infiltrating > 0)
					{
						return false;
					}
					performInfiltrate(teleportDestination);
					teleportDestination = null;
				}
			}
		}
		else if (E.ID == "CommandInfiltrate")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't infiltrate on the world map.");
				}
				return false;
			}
			PickInfiltrateDestination();
			if (teleportDestination == null)
			{
				return false;
			}
			Infiltrating = GetTurnsToCharge();
			ParentObject.UseEnergy(1000, "Physical Mutation Infiltrate");
			DidX("focus", ParentObject.its + " sensor somewhere else", null, null, null, ParentObject);
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "You teleport and bring creatures along with you.";
	}

	public int GetTeleportDistance(int Level)
	{
		return 80;
	}

	public int GetTeleportRadius(int Level)
	{
		return 3 + Level;
	}

	public int GetCooldown(int Level)
	{
		return 100;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("You teleport to a nearby location and bring everyone within radius " + GetTeleportRadius(Level) + " along with you.\n", "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("TeleportRadius", GetTeleportRadius(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Infiltrate", "CommandInfiltrate", "Physical Mutations", null, "\u001d");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
