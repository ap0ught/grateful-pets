using System.Collections.Generic;
using System.Linq;
using XRL.Collections;
using XRL.Messages;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

[HasWishCommand]
public static class AutoAct
{
	public const string OngoingActionSetting = "o";

	public static string ResumeSetting = "";

	public static int Digging = 0;

	public static int Count;

	public static OngoingAction _Action = null;

	public static OngoingAction _ResumeAction = null;

	public static bool Attacking;

	private static FindPath TargetPath = new FindPath();

	private static FindPath TryPath = new FindPath();

	private static int EdgeLastAct;

	private static Rack<GameObject> AutoexploreObjects = new Rack<GameObject>();

	private static Rack<Cell> AutoexploreCells = new Rack<Cell>();

	private static Zone AutoexploreZone;

	private static Cell AutoexploreLastTarget;

	private static FindPath AutoexplorePath = new FindPath();

	private static int AutoexploreLastAct;

	private static int AutoexploreLastCourse;

	private static List<string> PropsToRemove = new List<string>();

	public static string Setting
	{
		get
		{
			return The.Core.PlayerWalking;
		}
		set
		{
			if (value != "" && The.Core.PlayerWalking == "")
			{
				Count++;
			}
			The.Core.PlayerWalking = value;
			Digging = 0;
		}
	}

	public static OngoingAction Action
	{
		get
		{
			return _Action;
		}
		set
		{
			if (value != null && IsActive())
			{
				ResumeAction = _Action;
				ResumeSetting = Setting;
			}
			_Action = value;
			if (_Action != null)
			{
				Setting = "o";
			}
		}
	}

	public static OngoingAction ResumeAction
	{
		get
		{
			return _ResumeAction;
		}
		set
		{
			_ResumeAction = value;
			if (_ResumeAction != null)
			{
				ResumeSetting = "o";
			}
		}
	}

	public static bool IsActive(string What)
	{
		if (What != null && What != "")
		{
			return What != "ReopenMissileUI";
		}
		return false;
	}

	public static bool IsActive(bool IgnoreAutoget = false)
	{
		if (IgnoreAutoget && Setting == "g")
		{
			return false;
		}
		return IsActive(Setting);
	}

	public static bool IsInterruptable(string What)
	{
		return IsActive(What);
	}

	public static bool IsInterruptable()
	{
		return IsInterruptable(Setting);
	}

	public static bool ShouldHostilesInterrupt(string What, OngoingAction Action = null, bool logSpot = false, bool popSpot = false, bool CheckingPrior = true)
	{
		if (!IsActive(What))
		{
			return false;
		}
		if (What == "g")
		{
			if (Options.AutogetIfHostiles)
			{
				return false;
			}
		}
		else if (What == "o" && Action != null && !Action.ShouldHostilesInterrupt())
		{
			return false;
		}
		return The.Player.ArePerceptibleHostilesNearby(logSpot, popSpot, null, Action, What, CheckingPrior: CheckingPrior, IgnoreEasierThan: IsResting(What) ? int.MinValue : Options.AutoexploreIgnoreEasyEnemies, IgnoreFartherThan: Options.AutoexploreIgnoreDistantEnemies, IgnorePlayerTarget: IsCombat(What));
	}

	public static bool ShouldHostilesInterrupt(bool logSpot = false, bool popSpot = false, bool CheckingPrior = true)
	{
		return ShouldHostilesInterrupt(Setting, Action, logSpot, popSpot, CheckingPrior);
	}

	public static bool CheckHostileInterrupt(bool logSpot)
	{
		if (ShouldHostilesInterrupt(logSpot, popSpot: false, CheckingPrior: false))
		{
			Interrupt();
			return true;
		}
		return false;
	}

	public static bool CheckHostileInterrupt()
	{
		return CheckHostileInterrupt(!IsOnlyGathering() || Sidebar.AnyAutogotItems());
	}

	public static bool ShouldHostilesPreventAutoget()
	{
		return ShouldHostilesInterrupt("g");
	}

	public static bool IsMovement(string What, OngoingAction Action = null)
	{
		if (!IsActive(What))
		{
			return false;
		}
		switch (What[0])
		{
		case '.':
			return false;
		case 'g':
			return false;
		case 'o':
			return Action?.IsMovement() ?? false;
		case 'r':
		case 'z':
			return false;
		default:
			return true;
		}
	}

	public static bool IsMovement()
	{
		return IsMovement(Setting, Action);
	}

	public static bool IsAnyMovement()
	{
		if (!IsMovement(Setting, Action))
		{
			return IsMovement(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsCombat(string What, OngoingAction Action = null)
	{
		return What switch
		{
			"ReopenMissileUI" => true, 
			"a" => true, 
			"o" => Action?.IsCombat() ?? false, 
			_ => false, 
		};
	}

	public static bool IsCombat()
	{
		return IsCombat(Setting, Action);
	}

	public static bool IsAnyCombat()
	{
		if (!IsCombat(Setting, Action))
		{
			return IsCombat(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsExploration(string What, OngoingAction Action = null)
	{
		if (What == "?")
		{
			return true;
		}
		if (What == "o")
		{
			return Action?.IsExploration() ?? false;
		}
		return false;
	}

	public static bool IsExploration()
	{
		return IsExploration(Setting, Action);
	}

	public static bool IsAnyExploration()
	{
		if (!IsExploration(Setting, Action))
		{
			return IsExploration(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsGathering(string What, OngoingAction Action = null)
	{
		if (What == "g")
		{
			return true;
		}
		if (What == "o")
		{
			return Action?.IsGathering() ?? false;
		}
		return false;
	}

	public static bool IsGathering()
	{
		return IsGathering(Setting, Action);
	}

	public static bool IsAnyGathering()
	{
		if (!IsGathering(Setting, Action))
		{
			return IsGathering(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsOnlyGathering()
	{
		if (IsGathering(Setting, Action))
		{
			if (!ResumeSetting.IsNullOrEmpty())
			{
				return IsGathering(ResumeSetting, ResumeAction);
			}
			return true;
		}
		return false;
	}

	public static bool IsResting(string What, OngoingAction Action = null)
	{
		if (!IsActive(What))
		{
			return false;
		}
		switch (What[0])
		{
		case '.':
		case 'r':
		case 'z':
			return true;
		case 'o':
			return Action?.IsResting() ?? false;
		default:
			return false;
		}
	}

	public static bool IsResting()
	{
		return IsResting(Setting, Action);
	}

	public static bool IsAnyResting()
	{
		if (!IsResting(Setting, Action))
		{
			return IsResting(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static bool IsRateLimited(string What, OngoingAction Action = null)
	{
		if (What == "o")
		{
			return Action?.IsRateLimited() ?? false;
		}
		if (IsGathering(What, Action))
		{
			return true;
		}
		if (IsCombat(What, Action))
		{
			return true;
		}
		if (IsMovement(What, Action) && Digging <= 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsRateLimited()
	{
		return IsRateLimited(Setting, Action);
	}

	public static bool IsAnyRateLimited()
	{
		if (!IsRateLimited(Setting, Action))
		{
			return IsRateLimited(ResumeSetting, ResumeAction);
		}
		return true;
	}

	public static string GetDescription(string What, OngoingAction action)
	{
		if (!IsActive(What))
		{
			return "acting";
		}
		switch (What[0])
		{
		case '?':
			return "exploring";
		case '.':
			return "waiting";
		case 'd':
			if (Digging > 0)
			{
				return "digging";
			}
			break;
		case 'g':
			return "gathering";
		case 'o':
			return action?.GetDescription() ?? "acting";
		case 'r':
		case 'z':
			return "resting";
		case 'a':
			return "attacking";
		}
		return "moving";
	}

	public static string GetDescription()
	{
		if (!ResumeSetting.IsNullOrEmpty())
		{
			return GetDescription(ResumeSetting, ResumeAction);
		}
		return GetDescription(Setting, Action);
	}

	public static void ClearAutoMoveStop()
	{
		GameObject.AutomoveInterruptTurn = -2147483648L;
	}

	public static void Interrupt(string Because = null, Cell IndicateCell = null, GameObject IndicateObject = null, bool IsThreat = false)
	{
		GameObject.AutomoveInterruptBecause = Because;
		GameObject.AutomoveInterruptTurn = The.Game.Turns;
		if (IsActive())
		{
			if (Because == null && Action != null)
			{
				Because = Action.GetInterruptBecause();
			}
			if (!Because.IsNullOrEmpty() && (!IsOnlyGathering() || Sidebar.AnyAutogotItems()))
			{
				MessageQueue.AddPlayerMessage(Event.NewStringBuilder().Append("{{").Append(IsThreat ? 'r' : 'y')
					.Append("|You stop ")
					.Append(GetDescription())
					.Append(" because ")
					.Append(Because)
					.Append(".}}")
					.ToString());
			}
			if (IndicateObject != null && (!IsOnlyGathering() || Sidebar.AnyAutogotItems()))
			{
				IndicateObject.Indicate(IsThreat);
			}
			if (IndicateCell != null && (!IsOnlyGathering() || Sidebar.AnyAutogotItems()))
			{
				IndicateCell.Indicate(IsThreat);
			}
		}
		ResumeAction?.Interrupt();
		ResumeAction?.End();
		Action?.Interrupt();
		Action?.End();
		Setting = "";
		Action = null;
		ResumeSetting = "";
		ResumeAction = null;
		Attacking = false;
		The.Core.PlayerAvoid.Clear();
	}

	public static void Interrupt(GameObject BecauseOf, bool ShowIndicator = true, bool IsThreat = false)
	{
		if (BecauseOf != null && IsActive())
		{
			if (!IsOnlyGathering() || Sidebar.AnyAutogotItems())
			{
				MessageQueue.AddPlayerMessage(The.Player.GenerateSpotMessage(BecauseOf), IsThreat ? 'r' : 'y');
			}
			if (ShowIndicator)
			{
				BecauseOf.Indicate(IsThreat);
			}
		}
		ResumeAction?.Interrupt();
		ResumeAction?.End();
		Action?.Interrupt();
		Action?.End();
		Setting = "";
		Action = null;
		ResumeSetting = "";
		ResumeAction = null;
		Attacking = false;
		The.Core.PlayerAvoid.Clear();
	}

	public static void Resume()
	{
		Setting = ResumeSetting;
		Action = ResumeAction;
		ResumeSetting = "";
		ResumeAction = null;
	}

	public static bool TryFindEdgeStep(char Direction, out string Step)
	{
		Step = ".";
		Cell playerCell = The.PlayerCell;
		FindPath findPath = TargetPath;
		int nav = Zone.CalculateNav(The.Player);
		if (findPath.Found && findPath.Steps[0].ParentZone == playerCell.ParentZone && EdgeLastAct == Count)
		{
			int count = findPath.Steps.Count;
			int num = findPath.Steps.IndexOf(playerCell);
			if (num == count - 1)
			{
				return true;
			}
			if (num != -1)
			{
				Cell cell = findPath.Steps[num + 1];
				int num2 = findPath.Weights[num + 1];
				if (cell.NavigationWeight(The.Player, nav) == num2)
				{
					Step = findPath.Directions[num];
					return true;
				}
			}
		}
		EdgeLastAct = Count;
		int num3 = playerCell.X;
		int num4 = playerCell.Y;
		int num5 = 0;
		int num6 = 0;
		FindPath findPath2 = TryPath;
		switch (Direction)
		{
		case 'N':
			num4 = 0;
			num6 = 1;
			break;
		case 'S':
			num4 = 24;
			num6 = -1;
			break;
		case 'E':
			num3 = 79;
			num5 = -1;
			break;
		case 'W':
			num3 = 0;
			num5 = 1;
			break;
		default:
			Interrupt();
			MetricsManager.LogError("invalid screen edge " + Direction);
			return false;
		}
		Zone parentZone = playerCell.ParentZone;
		int num7 = int.MaxValue;
		bool flag = num6 != 0;
		Cell cell2 = null;
		while (cell2 == null)
		{
			int i = 0;
			for (int num8 = parentZone.Width * 2; i < num8; i++)
			{
				int num9 = i / 2;
				if (flag)
				{
					if (num3 + num9 >= parentZone.Width && num3 - num9 < 0)
					{
						break;
					}
				}
				else if (num4 + num9 >= parentZone.Height && num4 - num9 < 0)
				{
					break;
				}
				if (i % 2 == 1)
				{
					num9 = -num9;
				}
				int num10 = (flag ? (num3 + num9) : num3);
				int num11 = (flag ? num4 : (num4 + num9));
				Cell cell3 = parentZone.GetCell(num10, num11);
				if (cell3 == null)
				{
					continue;
				}
				if (cell3 == playerCell)
				{
					return true;
				}
				if (cell3.NavigationWeight(The.Player, nav) <= 95)
				{
					findPath2.PerformPathfind(playerCell.ParentZone, playerCell.X, playerCell.Y, num10, num11, 0, The.Player, CardinalDirectionsOnly: false, 95);
					if (findPath2.Usable && findPath2.Steps.Count < num7)
					{
						cell2 = cell3;
						num7 = findPath2.Steps.Count;
						FindPath findPath3 = findPath;
						findPath = findPath2;
						findPath2 = findPath3;
					}
				}
			}
			if (cell2 != null)
			{
				continue;
			}
			if (num3 != -1)
			{
				num3 += num5;
				if ((num5 < 0 && num3 <= playerCell.X) || (num5 > 0 && num3 >= playerCell.X))
				{
					break;
				}
			}
			if (num4 != -1)
			{
				num4 += num6;
				if ((num6 < 0 && num4 <= playerCell.Y) || (num6 > 0 && num4 >= playerCell.Y))
				{
					break;
				}
			}
		}
		if (cell2 != null)
		{
			TargetPath = findPath;
			TryPath = findPath2;
			Step = findPath.Directions[0];
			return true;
		}
		return false;
	}

	public static bool TryFindPathStep(Cell Target, out string Step)
	{
		Step = ".";
		Cell playerCell = The.PlayerCell;
		FindPath targetPath = TargetPath;
		if (targetPath.Found && targetPath.Steps[0].ParentZone == playerCell.ParentZone)
		{
			int num = targetPath.Steps.IndexOf(playerCell);
			if (num != -1 && num < targetPath.Steps.LastIndexOf(Target))
			{
				Cell cell = targetPath.Steps[num + 1];
				int num2 = targetPath.Weights[num + 1];
				if (cell.NavigationWeight(Nav: Zone.CalculateNav(The.Player), Looker: The.Player) == num2)
				{
					Step = targetPath.Directions[num];
					return true;
				}
			}
		}
		targetPath.PerformPathfind(playerCell.ParentZone, playerCell.X, playerCell.Y, Target.X, Target.Y, 0, The.Player, CardinalDirectionsOnly: false, 95);
		if (targetPath.Found)
		{
			Step = targetPath.Directions[0];
			return true;
		}
		return false;
	}

	public static void FindAutoexploreStep(out string Step, out bool Blackout)
	{
		FindAutoexploreStep(Force: false, out Step, out Blackout);
	}

	public static void FindAutoexploreStep(bool Force, out string Step, out bool Blackout)
	{
		Blackout = false;
		Step = ".";
		Zone zone = The.PlayerCell?.ParentZone;
		if (zone == null)
		{
			return;
		}
		if (Force || AutoexploreZone != zone)
		{
			AutoexploreZone = zone;
			AutoexploreLastAct = Count;
			AutoexploreObjects.Clear();
			AutoexploreCells.Clear();
			AutoexplorePath.Clear();
			zone.FloodAutoexplore(AutoexploreCells, AutoexploreObjects, AutoexploreLastCourse);
			AutoexploreCells.Reverse();
			AutoexploreObjects.Reverse();
		}
		Cell autoexploreLastTarget = AutoexploreLastTarget;
		if (autoexploreLastTarget != null && autoexploreLastTarget.ParentZone == zone && !autoexploreLastTarget.IsBlackedOut() && TryGetAutoexploreStepToLast(autoexploreLastTarget, out Step))
		{
			return;
		}
		AutoexploreLastTarget = null;
		if (AutoexploreLastAct != Count)
		{
			FindAutoexploreStep(Force: true, out Step, out Blackout);
			return;
		}
		if (!AutoexploreObjects.IsNullOrEmpty())
		{
			GameObject[] array = AutoexploreObjects.GetArray();
			for (int num = AutoexploreObjects.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = array[num];
				if (!IsAutoexploreObject(gameObject))
				{
					AutoexploreObjects.RemoveAt(num);
				}
				else
				{
					Cell currentCell = gameObject.CurrentCell;
					if (currentCell.IsBlackedOut())
					{
						Blackout = true;
					}
					else if (currentCell.IsExplored() && TryGetAutoexploreStepToCell(currentCell, AllowDigging: true, out Step))
					{
						return;
					}
				}
			}
		}
		if (!AutoexploreCells.IsNullOrEmpty())
		{
			Cell[] array2 = AutoexploreCells.GetArray();
			for (int num2 = AutoexploreCells.Count - 1; num2 >= 0; num2--)
			{
				Cell cell = array2[num2];
				if (!IsAutoexploreCell(cell))
				{
					AutoexploreCells.RemoveAt(num2);
				}
				else if (cell.IsBlackedOut())
				{
					Blackout = true;
				}
				else if (TryGetAutoexploreStepToCell(cell, AllowDigging: false, out Step))
				{
					return;
				}
			}
		}
		if (!Force)
		{
			FindAutoexploreStep(Force: true, out Step, out Blackout);
		}
	}

	private static bool TryGetAutoexploreStepToLast(Cell Cell, out string Step)
	{
		if (IsAutoexploreCell(Cell))
		{
			return TryGetAutoexploreStepToCell(Cell, AllowDigging: false, out Step);
		}
		if (Cell.HasObject(IsAutoexploreObject))
		{
			return TryGetAutoexploreStepToCell(Cell, AllowDigging: true, out Step);
		}
		Step = ".";
		return false;
	}

	private static bool TryGetAutoexploreStepToCell(Cell Cell, bool AllowDigging, out string Step)
	{
		Step = ".";
		AutoexploreLastTarget = Cell;
		Cell playerCell = The.PlayerCell;
		if (Cell == playerCell)
		{
			return false;
		}
		FindPath autoexplorePath = AutoexplorePath;
		if (autoexplorePath.Found)
		{
			int num = autoexplorePath.Steps.IndexOf(playerCell);
			if (num != -1 && num < autoexplorePath.Steps.LastIndexOf(Cell))
			{
				Cell cell = autoexplorePath.Steps[num + 1];
				int num2 = autoexplorePath.Weights[num + 1];
				if (cell.NavigationWeight(Nav: Zone.CalculateNav(The.Player, Autoexploring: true, Juggernaut: false, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, AllowDigging), Looker: The.Player) == num2)
				{
					Step = autoexplorePath.Directions[num];
					return true;
				}
			}
		}
		autoexplorePath.PerformPathfind(playerCell.ParentZone, playerCell.X, playerCell.Y, Cell.ParentZone, Cell.X, Cell.Y, PathGlobal: false, The.Player, Unlimited: true, AddNoise: false, CardinalDirectionsOnly: false, null, 95, ExploredOnly: false, Autoexploring: true, Juggernaut: false, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, AllowDigging);
		if (autoexplorePath.Found)
		{
			Step = autoexplorePath.Directions[0];
			AutoexploreLastCourse = playerCell.GetGeneralCourseFrom(Cell);
			return true;
		}
		return false;
	}

	public static bool IsAutoexploreObject(GameObject Object)
	{
		if (!Object.IsValid() || Object.CurrentZone != AutoexploreZone)
		{
			return false;
		}
		if (Object.CurrentCell.IsSolidFor(The.Player) && !Object.CanInteractInCellWithSolid(The.Player))
		{
			return false;
		}
		if (Options.AutoexploreChests && Object.ShouldAutoexploreAsChest())
		{
			return true;
		}
		return AutoexploreObjectEvent.Check(The.Player, Object, Setting, Action);
	}

	public static bool IsAutoexploreCell(Cell Cell)
	{
		if (Cell.ParentZone != AutoexploreZone)
		{
			return false;
		}
		return !Cell.IsExplored();
	}

	public static bool TryToMove(GameObject Actor, Cell FromCell, ref GameObject LastDoor, Cell ToCell = null, string Direction = null, bool AllowDigging = true, bool OpenDoors = true, bool Peaceful = true, bool PostMoveHostileCheck = true, bool PostMoveSidebarCheck = true)
	{
		GameObject gameObject = LastDoor;
		LastDoor = null;
		if (FromCell == null)
		{
			Interrupt("your spacetime context is incoherent");
			return false;
		}
		if (ToCell == null && !Direction.IsNullOrEmpty())
		{
			ToCell = FromCell.GetCellFromDirection(Direction);
		}
		if (ToCell == null)
		{
			Interrupt("you can go no further");
			return false;
		}
		if (!ToCell.IsAdjacentTo(FromCell))
		{
			Interrupt("something is wrong with the spacetime connectivity of your environment");
			return false;
		}
		if (Direction.IsNullOrEmpty())
		{
			Direction = FromCell.GetDirectionFromCell(ToCell);
		}
		if (Direction.IsNullOrEmpty() || Direction == "." || Direction == "?")
		{
			Interrupt("the direction you seem to be trying to go makes no sense");
			return false;
		}
		if (OpenDoors)
		{
			int i = 0;
			for (int count = ToCell.Objects.Count; i < count; i++)
			{
				GameObject gameObject2 = ToCell.Objects[i];
				if (!gameObject2.TryGetPart<Door>(out var Part) || Part.Open || !gameObject2.PhaseMatches(Actor))
				{
					continue;
				}
				if (gameObject2 == gameObject)
				{
					Interrupt("you cannot keep " + gameObject2.t() + " open", null, gameObject2);
					return false;
				}
				LastDoor = gameObject2;
				Event obj = Event.New("Open");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Opener", Actor);
				obj.SetFlag("UsePopupsForFailures", Actor.IsPlayer());
				obj.SetFlag("FromMove", State: true);
				if (!Part.FireEvent(obj))
				{
					if (!gameObject2.IsCreature)
					{
						Interrupt("you cannot open " + gameObject2.t(), null, gameObject2);
						return false;
					}
					continue;
				}
				goto IL_018a;
			}
		}
		if (AllowDigging && ToCell.IsSolidFor(Actor) && Actor.PathAsBurrower)
		{
			if (Digging > 1000)
			{
				Interrupt("you cannot seem to make any progress", ToCell);
				return false;
			}
			int num = 0;
			int j = 0;
			for (int count2 = ToCell.Objects.Count; j < count2; j++)
			{
				num += ToCell.Objects[j].hitpoints;
			}
			if (!Actor.AttackDirection(Direction))
			{
				Interrupt(null, ToCell, null, IsThreat: true);
				return false;
			}
			int num2 = 0;
			int k = 0;
			for (int count3 = ToCell.Objects.Count; k < count3; k++)
			{
				num2 += ToCell.Objects[k].hitpoints;
			}
			if (num2 < num)
			{
				Digging = 1;
			}
			else
			{
				Digging++;
			}
		}
		else
		{
			string direction = Direction;
			bool peaceful = Peaceful;
			if (!Actor.Move(direction, out var Blocking, Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, null, null, null, peaceful))
			{
				if (Blocking != null)
				{
					Interrupt(Blocking.does("are") + " in the way", null, Blocking);
				}
				else
				{
					Interrupt(null, ToCell);
				}
				return false;
			}
			if (Actor.CurrentCell == FromCell)
			{
				Interrupt("you tried to move but stayed in the same place", FromCell);
				return false;
			}
			Digging = 0;
		}
		goto IL_0368;
		IL_018a:
		Digging = 0;
		goto IL_0368;
		IL_0368:
		if (PostMoveHostileCheck)
		{
			CheckHostileInterrupt();
		}
		if (PostMoveSidebarCheck && Actor.IsPlayer())
		{
			Cell currentCell = Actor.CurrentCell;
			if (currentCell != null && currentCell != FromCell)
			{
				if (currentCell.X > 42 && Sidebar.State == "right")
				{
					Sidebar.SetSidebarState("left");
				}
				else if (currentCell.X < 38 && Sidebar.State == "left")
				{
					Sidebar.SetSidebarState("right");
				}
			}
		}
		return true;
	}

	public static bool TryToMove(GameObject Actor, Cell FromCell, Cell ToCell = null, string Direction = null, bool AllowDigging = true, bool OpenDoors = true, bool Peaceful = true, bool PostMoveHostileCheck = true, bool PostMoveSidebarCheck = true)
	{
		GameObject LastDoor = null;
		return TryToMove(Actor, FromCell, ref LastDoor, ToCell, Direction, AllowDigging, OpenDoors, Peaceful, PostMoveHostileCheck, PostMoveSidebarCheck);
	}

	public static string AutoexploreActionProperty(string Command)
	{
		return "AutoexploreAction" + (Command.IsNullOrEmpty() ? "_" : Command);
	}

	public static int GetAutoexploreActionProperty(GameObject Object, string Command)
	{
		return Object.GetIntProperty(AutoexploreActionProperty(Command));
	}

	public static void SetAutoexploreActionProperty(GameObject Object, string Command, int Value)
	{
		Object.SetIntProperty(AutoexploreActionProperty(Command), Value);
	}

	public static string AutoexploreSuppressionProperty()
	{
		return "AutoexploreSuppression";
	}

	public static bool AutoexploreSuppressed(GameObject Object)
	{
		return Object.GetIntProperty(AutoexploreSuppressionProperty()) > 0;
	}

	public static void SetAutoexploreSuppression(GameObject Object, bool Flag)
	{
		Object.SetIntProperty(AutoexploreSuppressionProperty(), Flag ? 1 : 0, RemoveIfZero: true);
	}

	public static void ResetZoneAutoexploreAction(string Command)
	{
		Zone parentZone = The.PlayerCell.ParentZone;
		string prop = AutoexploreActionProperty(Command);
		parentZone.ForeachObjectWithTagOrProperty(prop, (GameObject obj) =>
		{
			SetAutoexploreActionProperty(obj, prop, 0);
		});
		ResetZoneAutoexploreSupression();
	}

	public static void ResetZoneAutoexploreSupression()
	{
		Zone parentZone = The.PlayerCell.ParentZone;
		string prop = AutoexploreSuppressionProperty();
		parentZone.ForeachObjectWithTagOrProperty(prop, (GameObject obj) =>
		{
			SetAutoexploreActionProperty(obj, prop, 0);
		});
	}

	[WishCommand("resetauto", null)]
	public static bool ResetAutoexploreProperties()
	{
		The.PlayerCell.ParentZone.ForeachObject((GameObject obj) =>
		{
			PropsToRemove.Clear();
			foreach (string key in obj.IntProperty.Keys)
			{
				if (key.StartsWith("Autoexplore"))
				{
					PropsToRemove.Add(key);
				}
			}
			if (PropsToRemove.Count > 0)
			{
				string text = PropsToRemove.Aggregate(null, (string a, string b) => (a != null) ? (a + ", " + b) : b);
				MessageQueue.AddPlayerMessage("Resetting " + text + " on " + obj.DebugName);
				obj.IntProperty.RemoveAll(PropsToRemove);
				PropsToRemove.Clear();
			}
		});
		return true;
	}
}
