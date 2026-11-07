using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StairsDown : IPart
{
	public bool Connected = true;

	public string ConnectionObject = "StairsUp";

	public bool PullDown;

	public bool GenericFall;

	public bool ConnectLanding = true;

	public string PullMessage = "You fall down a deep shaft!";

	public string JumpPrompt = "It looks like an awfully long fall. Are you sure you want to jump into the shaft?";

	public string Sound = "Sounds/Interact/sfx_interact_stairs_descend";

	public int Levels = 1;

	public override bool SameAs(IPart p)
	{
		StairsDown stairsDown = p as StairsDown;
		if (stairsDown.Connected != Connected)
		{
			return false;
		}
		if (stairsDown.PullDown != PullDown)
		{
			return false;
		}
		if (stairsDown.PullMessage != PullMessage)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != PooledEvent<CheckAttackableEvent>.ID && ID != CommandSmartUseEvent.ID && ID != EnteredCellEvent.ID && (ID != GetInventoryActionsEvent.ID || PullDown) && (ID != GetNavigationWeightEvent.ID || !PullDown) && (ID != GetAdjacentNavigationWeightEvent.ID || !PullDown) && ID != GravitationEvent.ID && ID != PooledEvent<IdleQueryEvent>.ID && (ID != PooledEvent<InterruptAutowalkEvent>.ID || !PullDown) && (ID != InventoryActionEvent.ID || PullDown) && (ID != ObjectEnteredCellEvent.ID || !PullDown) && (ID != ObjectEnteringCellEvent.ID || !PullDown) && ID != PooledEvent<SubjectToGravityEvent>.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(SubjectToGravityEvent E)
	{
		E.SubjectToGravity = false;
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!PullDown && CommandBindingManager.GetCommandFromKey(Keys.OemPeriod | Keys.Shift) == "CmdMoveD")
		{
			E.AddAction("Descend", "descend", "Descend", null, 'd');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Descend" && !PullDown && E.Actor.IsPlayer() && CommandBindingManager.GetCommandFromKey(Keys.OemPeriod | Keys.Shift) == "CmdMoveD")
		{
			Popup.ShowFail("Use " + ControlManager.getCommandInputFormatted("CmdMoveD") + " to descend.");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (PullDown)
		{
			E.MinWeight(99);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (PullDown)
		{
			E.MinWeight(4);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckAttackableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (PullDown && The.Player.CanFall)
		{
			E.IndicateObject = ParentObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Cell.ParentZone.ZoneWorld == "Tzimtzlum")
		{
			E.Cell.RemoveObject(ParentObject);
			ParentObject.Obliterate();
			E.Cell.AddObject("Space-Time Rift");
			return false;
		}
		int i = 0;
		for (int count = E.Cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = E.Cell.Objects[i];
			if (gameObject != ParentObject && gameObject.HasPart<StairsDown>())
			{
				E.Cell.RemoveObject(ParentObject);
				ParentObject.Obliterate();
				return false;
			}
		}
		if (Connected)
		{
			E.Cell.ParentZone.AddZoneConnection("d", E.Cell.X, E.Cell.Y, "StairsUp", ConnectionObject);
		}
		else if (ConnectLanding)
		{
			E.Cell.ParentZone.AddZoneConnection("d", E.Cell.X, E.Cell.Y, PullDown ? "PullDownEnd" : "DownEnd", null);
		}
		if (PullDown && !E.IgnoreGravity && (E.Cell.Objects.Count > 1 || !E.Cell.Objects.Contains(ParentObject)))
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(E.Cell.Objects);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				if (list[j] != ParentObject)
				{
					CheckPullDown(list[j]);
				}
			}
		}
		E.Cell?.ClearWalls();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		if (PullDown)
		{
			Cell cell = currentCell;
			if (cell != null)
			{
				List<GameObject> list = Event.NewGameObjectList();
				list.AddRange(cell.Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					CheckPullDown(list[i]);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (PullDown && !E.Forced && !E.System && IsValidForPullDown(E.Object) && E.Object.IsPlayer() && !E.Object.IsConfused && !((IsLongFall() ? JumpPrompt : null) ?? E.Cell.ParentZone.GetZoneProperty("PullDownPrompt")).IsNullOrEmpty() && Popup.WarnYesNoCancel(JumpPrompt) != DialogResult.Yes)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (PullDown && !E.IgnoreGravity)
		{
			CheckPullDown(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GravitationEvent E)
	{
		if (PullDown)
		{
			CheckPullDown(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.CurrentCell == ParentObject.CurrentCell)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.CurrentCell == ParentObject.CurrentCell)
		{
			if (E.Actor.IsPlayer())
			{
				Keyboard.PushMouseEvent("Command:CmdMoveD");
			}
			else
			{
				for (int i = 0; i < Levels; i++)
				{
					E.Actor.Move("D", Forced: false, Levels > 1, IgnoreGravity: false, NoStack: false, AllowDashing: false);
				}
			}
		}
		return false;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (ParentObject.HasTagOrProperty("IdleStairs") && E.Actor.HasPart<Brain>() && Stat.Random(1, 2000) == 2000)
		{
			GameObject who = E.Actor;
			who.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
			{
				if (who.CurrentCell == ParentObject.CurrentCell)
				{
					for (int i = 0; i < Levels; i++)
					{
						who.Move("D", Forced: false, Levels > 1, IgnoreGravity: false, NoStack: false, AllowDashing: false);
					}
				}
				h.FailToParent();
			}));
			who.Brain.PushGoal(new MoveTo(ParentObject));
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ClimbDown");
		base.Register(Object, Registrar);
	}

	public bool IsValidForPullDown(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (Object == ParentObject)
		{
			return false;
		}
		return Object.CanFall;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ClimbDown")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("GO");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				string tag = ParentObject.GetTag("KeyObject");
				if (!string.IsNullOrEmpty(tag) && !gameObjectParameter.IsCarryingObject(tag))
				{
					DidX("are", "locked, and you don't have the key", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
					return false;
				}
			}
			if (!PullDown || gameObjectParameter.IsFlying)
			{
				bool flag = false;
				for (int i = 0; i < Levels; i++)
				{
					flag |= gameObjectParameter.Move("D", Forced: false, Levels > 1, IgnoreGravity: false, NoStack: false, AllowDashing: false);
				}
				if (flag)
				{
					PlayWorldSound(Sound);
				}
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public bool CheckPullDown(GameObject Object)
	{
		try
		{
			if (!PullDown)
			{
				return false;
			}
			if (!GameObject.Validate(ParentObject))
			{
				return false;
			}
			if (!IsValidForPullDown(Object))
			{
				return false;
			}
			Cell cell = ParentObject.CurrentCell;
			Cell DestinationCell = GetPullDownCell(cell, out var Distance);
			if (DestinationCell == null)
			{
				return false;
			}
			if (!BeforePullDownEvent.Check(ParentObject, Object, ref DestinationCell))
			{
				return false;
			}
			if (!GameObject.Validate(ParentObject))
			{
				return false;
			}
			if (DestinationCell == null)
			{
				return false;
			}
			if (Object.IsPlayer())
			{
				ZoneManager.ZoneTransitionCount -= Distance;
			}
			if (!DestinationCell.IsPassable(Object))
			{
				Cell closestPassableCellFor = DestinationCell.getClosestPassableCellFor(Object);
				if (closestPassableCellFor != null && closestPassableCellFor.RealDistanceTo(DestinationCell) <= 2.0)
				{
					DestinationCell = closestPassableCellFor;
				}
			}
			if (Distance > 1)
			{
				PlayWorldSound("sfx_characterTrigger_shaft_fall");
			}
			else
			{
				PlayWorldSound("fly_generic_fall");
			}
			string text = (ParentObject.HasPropertyOrTag("FallPreposition") ? ParentObject.GetPropertyOrTag("FallPreposition") : "down");
			bool flag = ((ParentObject.HasPropertyOrTag("FallUseDefiniteArticle") && ParentObject.GetPropertyOrTag("FallUseDefiniteArticle") == "true") ? true : false);
			if (Object.IsPlayerLed() && !Object.IsTrifling)
			{
				Popup.Show("Your companion, " + Object.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + "," + Object.GetVerb("have") + " fallen " + text + " " + ParentObject.t() + " " + The.Player.DescribeDirectionToward(ParentObject) + "!");
			}
			else
			{
				GameObject parentObject = ParentObject;
				bool indefiniteObject = !flag;
				IComponent<GameObject>.XDidYToZ(Object, "fall", text, parentObject, null, null, null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, indefiniteObject);
			}
			Object.SystemMoveTo(DestinationCell, 0, forced: true);
			if (!Object.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Object, "fall", "down from above", null, null, null, null, Object, UseFullNames: false, IndefiniteSubject: true);
			}
			DestinationCell = Object.CurrentCell ?? DestinationCell;
			List<GameObject> list = null;
			if (Object.GetMatterPhase() <= 2)
			{
				int phase = Object.GetPhase();
				int i = 0;
				for (int count = DestinationCell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = DestinationCell.Objects[i];
					if (gameObject != null && gameObject != Object && gameObject.IsCombat && gameObject.GetMatterPhase() <= 2 && gameObject.PhaseMatches(phase))
					{
						if (list == null)
						{
							list = Event.NewGameObjectList();
						}
						list.Add(gameObject);
					}
				}
			}
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					list[j].TakeDamage(Stat.Random(1, 4), Owner: Object, Message: "from " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " falling on " + list[j].them + ".", Attributes: "Crushing", DeathReason: Object.An(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " fell on " + list[j].them + ".", ThirdPersonDeathReason: Object.An(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " fell on " + list[j].them + ".", Attacker: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: true);
					if (Distance <= 1 && j == count2 - 1)
					{
						Object.TakeDamage(Stat.Random(1, 4), Owner: Object, Message: "from " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " falling on " + list[j].them + ".", Attributes: "Crushing", DeathReason: Object.An(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " fell on " + list[j].them + ".", ThirdPersonDeathReason: Object.An(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " fell on " + list[j].them + ".", Attacker: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: true);
						if (Object.IsPlayer() && Object.hitpoints <= 0)
						{
							Achievement.DIE_BY_FALLING.Unlock();
						}
					}
				}
			}
			if (Distance > 1)
			{
				InflictFallDamage(Object, Distance, GenericFall ? null : ("You fell down " + ParentObject.an() + "."), GenericFall ? null : (Object.It + " @@fell down " + ParentObject.an() + "."));
			}
			FellDownEvent.Send(Object, DestinationCell, cell, Distance);
			if (Object.IsValid())
			{
				GameObject partyLeader = Object.PartyLeader;
				if (partyLeader != null && !Object.InSameZone(partyLeader) && !Object.HasEffect<Incommunicado>())
				{
					Object.ApplyEffect(new Incommunicado());
				}
			}
			if (Object != null && Object.IsPlayer())
			{
				The.ZoneManager.SetActiveZone(DestinationCell.ParentZone);
				The.ZoneManager.ProcessGoToPartyLeader();
				if (Distance > 1)
				{
					IComponent<GameObject>.AddPlayerMessage("You fall downward!");
				}
				else if (!PullMessage.IsNullOrEmpty())
				{
					IComponent<GameObject>.AddPlayerMessage(PullMessage);
				}
			}
			if (Object?.CurrentCell?.IsSolid() == true)
			{
				Cell cell2 = Object?.CurrentCell?.GetFirstPassableConnectedAdjacentCell();
				if (cell2 != null)
				{
					Object.SystemMoveTo(cell2);
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("StairsDown::CheckPulldown", x);
		}
		return true;
	}

	public static void InflictFallDamage(GameObject Object, int Distance, string DeathReason = null, string ThirdPersonDeathReason = null)
	{
		if (Distance > 1)
		{
			if (DeathReason == null)
			{
				DeathReason = "You fell from a great height.";
			}
			if (ThirdPersonDeathReason == null)
			{
				ThirdPersonDeathReason = Object.It + " @@fell from a great height.";
			}
			Object.TakeDamage(Stat.Roll(Distance + "d20+" + (100 + Distance * 25)), "from " + Object.its + " fall.", "Crushing Falling", DeathReason, ThirdPersonDeathReason, Object, null, null, null, null, Accidental: true);
			if (Object.IsPlayer() && Object.hitpoints <= 0)
			{
				Achievement.DIE_BY_FALLING.Unlock();
			}
		}
	}

	public bool IsLongFall()
	{
		GetPullDownCell(out var Distance, 2);
		return Distance > 1;
	}

	public Cell GetPullDownCell(Cell CC, out int Distance, int MaxDistance = int.MaxValue)
	{
		Distance = 0;
		if (CC == null || CC.ParentZone == null || !CC.ParentZone.Built || CC.HasObjectWithTagOrProperty("SuspendedPlatform"))
		{
			return null;
		}
		Cell cell = CC.GetCellFromDirection("D", BuiltOnly: false);
		bool flag = cell.HasObjectWithPropertyOrTag("SuspendedPlatform");
		if (cell != null && !flag)
		{
			Distance++;
			if (Distance >= MaxDistance)
			{
				return null;
			}
			for (int i = 1; i < Levels; i++)
			{
				Cell cellFromDirection = cell.GetCellFromDirection("D", BuiltOnly: false);
				if (cellFromDirection == null)
				{
					break;
				}
				cell = cellFromDirection;
				Distance++;
				if (Distance >= MaxDistance)
				{
					return null;
				}
				if (cell.HasObjectWithPropertyOrTag("SuspendedPlatform"))
				{
					break;
				}
			}
		}
		if (cell != null && !flag)
		{
			GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("StairsDown");
			if (firstObjectWithPart != null)
			{
				StairsDown part = firstObjectWithPart.GetPart<StairsDown>();
				if (part != null && part.PullDown)
				{
					Cell pullDownCell = part.GetPullDownCell(out var Distance2, MaxDistance - Distance);
					if (pullDownCell != null)
					{
						cell = pullDownCell;
					}
					Distance += Distance2;
					if (Distance >= MaxDistance)
					{
						return null;
					}
				}
			}
		}
		return cell;
	}

	public Cell GetPullDownCell(out int Distance, int MaxDistance = int.MaxValue)
	{
		return GetPullDownCell(currentCell, out Distance, MaxDistance);
	}

	public Cell GetPullDownCell(int MaxDistance = int.MaxValue)
	{
		int Distance;
		return GetPullDownCell(out Distance, MaxDistance);
	}
}
