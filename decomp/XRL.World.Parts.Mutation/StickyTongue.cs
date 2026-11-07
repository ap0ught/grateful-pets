using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class StickyTongue : BaseMutation
{
	public int TongueCharging;

	public bool PullSameCreatureType = true;

	public bool PullHostileOnly;

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", GetRange(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (TongueCharging == 0 && E.Distance > 1 && E.Distance <= GetRange() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.HasEffect<Glotrot>())
		{
			E.Add("CommandStickyTongue");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (TongueCharging == 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.HasEffect<Glotrot>())
		{
			CommandEvent.Send(E.Actor, "CommandStickyTongue");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (TongueCharging > 0)
		{
			TongueCharging++;
			ParentObject.UseEnergy(1000, "Physical Mutation");
			if (TongueCharging >= 2)
			{
				if (HarpoonNearest(ParentObject, GetRange(), "&M", 1, PullSameCreatureType, PullHostileOnly) > 0)
				{
					CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
				}
				else
				{
					TakeMyActivatedAbilityOffCooldown(ActivatedAbilityID);
				}
				TongueCharging = 0;
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandStickyTongue")
		{
			if (ParentObject.HasEffect<Glotrot>())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You don't have a tongue!");
				}
				return true;
			}
			TongueCharging = 1;
			ParentObject.UseEnergy(1000, "Physical Mutation Sticky Tongue");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You capture prey with your sticky tongue.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("You pull the nearest creature toward you.\n" + "Range: " + GetRange(Level) + "\n", "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public override bool Render(RenderEvent E)
	{
		if (TongueCharging == 1)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 35 && num < 45)
			{
				E.Tile = null;
				E.RenderString = "*";
				E.ColorString = "&M";
			}
		}
		return base.Render(E);
	}

	public static int GetRange(int Level)
	{
		return 10 + Level * 2;
	}

	public int GetRange()
	{
		return GetRange(Level);
	}

	public static int GetCooldown(int Level)
	{
		return 22 - Level;
	}

	public int GetCooldown()
	{
		return GetCooldown(Level);
	}

	public static int HarpoonNearest(GameObject Actor, int Range, string harpoonColor = "&y", int Count = 1, bool PullSameCreatureType = true, bool PullHostileOnly = false)
	{
		int num = 0;
		Cell cell = Actor.CurrentCell;
		if (cell == null)
		{
			return num;
		}
		List<GameObject> objects = cell.ParentZone.GetObjects((GameObject o) =>
		{
			if (!o.IsCombatObject())
			{
				return false;
			}
			if (o == Actor)
			{
				return false;
			}
			int num6 = Actor.DistanceTo(o);
			if (num6 <= 1 || num6 > Range)
			{
				return false;
			}
			if (!PullSameCreatureType && o.Blueprint == Actor.Blueprint)
			{
				return false;
			}
			if (PullHostileOnly && !Actor.IsHostileTowards(o))
			{
				return false;
			}
			if (!Actor.PhaseMatches(o))
			{
				return false;
			}
			return Actor.HasLOSTo(o) ? true : false;
		});
		if (objects.Count < 1)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("There are no creatures in range.");
			}
			return num;
		}
		Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_tonguePull");
		if (objects.Count > 1)
		{
			objects.Sort((GameObject a, GameObject b) => a.DistanceTo(Actor).CompareTo(b.DistanceTo(Actor)));
		}
		for (int num2 = 0; num2 < objects.Count && num2 < Count; num2++)
		{
			GameObject defender = objects[num2];
			Cell cell2 = defender.CurrentCell;
			List<Cell> localEmptyAdjacentCells = cell.GetLocalEmptyAdjacentCells();
			if (localEmptyAdjacentCells.Count <= 0)
			{
				break;
			}
			localEmptyAdjacentCells.Sort((Cell a, Cell b) => a.DistanceTo(defender).CompareTo(b.DistanceTo(defender)));
			List<Tuple<Cell, char>> lineTo = Actor.GetLineTo(defender);
			if (lineTo[0].Item1 != cell)
			{
				lineTo.Reverse();
			}
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			string text = harpoonColor;
			if (cell.X == cell2.X)
			{
				text += "|";
			}
			else if (cell.Y == cell2.Y)
			{
				text += "-";
			}
			else if (cell.Y < cell2.Y)
			{
				text = ((cell.X <= cell2.X) ? (text + "\\") : (text + "/"));
			}
			else
			{
				text = ((cell.X <= cell2.X) ? (text + "/") : (text + "\\"));
			}
			int num3 = 0;
			bool flag = false;
			Cell cell3 = defender.CurrentCell;
			for (int num4 = lineTo.Count - 2; num4 >= 1; num4--)
			{
				if (!flag)
				{
					if (defender.CurrentCell != cell3)
					{
						break;
					}
					Cell cell4 = cell.ParentZone.GetCell(lineTo[num4].Item1.X, lineTo[num4].Item1.Y);
					if (cell4 == null || defender.CurrentCell != cell3 || !cell4.IsAdjacentTo(cell3) || !defender.Move(cell3.GetDirectionFromCell(cell4), Forced: true, System: false, IgnoreGravity: true, NoStack: false, AllowDashing: true, DoConfirmations: true, Actor))
					{
						flag = true;
					}
					cell3 = cell4;
					num3++;
				}
				bool flag2 = false;
				scrapBuffer.RenderBase();
				for (int num5 = 1; num5 < num4 - 1; num5++)
				{
					if (lineTo[num5].Item1.IsVisible())
					{
						scrapBuffer.Goto(lineTo[num5].Item1.X, lineTo[num5].Item1.Y);
						scrapBuffer.Write(text);
						flag2 = true;
					}
				}
				if (flag2)
				{
					scrapBuffer.Draw();
					Thread.Sleep(50);
				}
			}
			if (Actor != null && defender != null)
			{
				if (num3 == 0)
				{
					IComponent<GameObject>.XDidYToZ(Actor, "try", "to pull", defender, "toward " + Actor.them + ", but cannot", "!", null, null, null, Actor);
				}
				else if (Actor.DistanceTo(defender) <= 1)
				{
					IComponent<GameObject>.XDidYToZ(Actor, "pull", defender, "to " + Actor.them, "!", null, null, null, defender);
				}
				else
				{
					IComponent<GameObject>.XDidYToZ(Actor, "pull", defender, "toward " + Actor.them, "!", null, null, null, defender);
				}
			}
			defender.Gravitate();
			num++;
		}
		return num;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Tongue", "CommandStickyTongue", "Physical Mutations", GetDescription(), "\u00ad");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
