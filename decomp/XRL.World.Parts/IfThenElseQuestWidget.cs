using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class IfThenElseQuestWidget : IPart
{
	public const string QUEST_ID = "If, Then, Else";

	public const string STEP_TAP_ID = "Go to the Bottom of Taproot";

	public const string STEP_CHIME_ID = "Reunite Tau with Her Chime";

	public int Stage;

	public int Timer;

	[NonSerialized]
	private GameObject _Tau;

	[NonSerialized]
	private GameObject _Companion;

	public GameObject Tau
	{
		get
		{
			if (!GameObject.Validate(ref _Tau) || _Tau.CurrentCell == null)
			{
				_Tau = ParentObject.CurrentZone.FindFirstObject(IsTau);
			}
			return _Tau;
		}
		set
		{
			_Tau = value;
		}
	}

	public GameObject Companion
	{
		get
		{
			if (!GameObject.Validate(ref _Companion) || _Companion.CurrentCell == null)
			{
				_Companion = ParentObject.CurrentZone.FindFirstObject(IsCompanion);
			}
			return _Companion;
		}
		set
		{
			_Companion = value;
		}
	}

	public string TauBlueprint
	{
		get
		{
			if (!The.Game.HasFinishedQuestStep("If, Then, Else", "Reunite Tau with Her Chime"))
			{
				return "TauSoft";
			}
			if (!The.Game.GetBooleanGameState("TauCompanionDead"))
			{
				return "TauNoLonger";
			}
			return "TauWanderer";
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (The.Game.HasUnfinishedQuest("If, Then, Else"))
		{
			if (Stage <= 0)
			{
				Stage = 1;
				The.Game.FinishQuestStep("If, Then, Else", "Go to the Bottom of Taproot");
				FindPath findPath = new FindPath(E.Zone.GetCell(40, 10), E.Zone.GetCell(35, 19));
				if (findPath.Usable)
				{
					foreach (Cell step in findPath.Steps)
					{
						step.ClearImpassableObjects();
					}
				}
			}
			if (Stage >= 1 && Stage < 5)
			{
				RequireSubjects();
			}
		}
		else
		{
			ParentObject.Destroy(null, Silent: true);
		}
		return base.HandleEvent(E);
	}

	public void RequireSubjects()
	{
		if (Tau == null)
		{
			GameObject gameObject = (Tau = ParentObject.CurrentZone.GetCell(40, 10).AddObject(TauBlueprint));
		}
		Tau.RegisterPartEvent(this, "AfterConversation");
		Tau.RegisterPartEvent(this, "TookDamage");
		Tau.RegisterPartEvent(this, "BeforeDie");
		if (Companion == null)
		{
			GameObject gameObject = (Companion = ParentObject.CurrentZone.GetCell(33, 19).AddObject("AoygNoLonger"));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (Stage == 1)
		{
			if (E.ID == "AfterConversation" && The.Game.HasFinishedQuestStep("If, Then, Else", "Reunite Tau with Her Chime"))
			{
				Timer = 0;
				Tau = Tau.ReplaceWith(TauBlueprint);
				Tau.TeleportSwirl();
				RequireSubjects();
				Stage = ((Tau.Blueprint == "TauNoLonger") ? 2 : 5);
			}
			else if (E.ID == "TookDamage")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				GameObject companion = Companion;
				if (companion != null && gameObjectParameter2 == Tau && (!gameObjectParameter2.IsHostileTowards(gameObjectParameter) || !companion.IsHostileTowards(gameObjectParameter)))
				{
					companion.AddOpinion<OpinionAttackAlly>(gameObjectParameter, gameObjectParameter2);
				}
			}
		}
		if (E.ID == "BeforeDie" && E.GetParameter("Killer") is GameObject gameObject && gameObject.IsPlayer())
		{
			List<GameObject> list = Event.NewGameObjectList();
			Zone.ObjectEnumerator enumerator = The.ActiveZone.IterateObjects().GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				if (current.Blueprint == "TauHeadpiece")
				{
					list.Add(current);
					continue;
				}
				if (current.Inventory != null)
				{
					foreach (GameObject @object in current.Inventory.Objects)
					{
						if (@object.Blueprint == "TauHeadpiece")
						{
							list.Add(@object);
						}
					}
				}
				List<GameObject> list2 = current.Body?.GetEquippedObjectsReadonly();
				if (list2 == null)
				{
					continue;
				}
				foreach (GameObject item in list2)
				{
					if (item.Blueprint == "TauHeadpiece")
					{
						list.Add(item);
					}
				}
			}
			foreach (GameObject item2 in list)
			{
				item2.ReplaceWith("TauDagger");
			}
		}
		return base.FireEvent(E);
	}

	public static bool IsTau(GameObject Object)
	{
		return Object.GetPart<GameUnique>()?.State == "TauElse";
	}

	public static bool IsCompanion(GameObject Object)
	{
		return Object.GetPart<GameUnique>()?.State == "TauCompanion";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		int stage = Stage;
		if (stage <= 0 || stage >= 5)
		{
			return;
		}
		GameObject tau = Tau;
		if (tau?.Brain?.PartyLeader != null)
		{
			tau.Brain.PartyLeader = null;
			tau.Brain.Goals.Clear();
		}
		GameObject companion = Companion;
		if (companion?.Brain?.PartyLeader != null)
		{
			companion.Brain.PartyLeader = null;
			companion.Brain.Goals.Clear();
		}
		if (Stage == 2 && Timer++ >= 5)
		{
			Timer = 0;
			if (tau?.Brain != null)
			{
				Stage = 3;
				Brain brain = tau.Brain;
				bool wanders = (tau.Brain.WandersRandomly = false);
				brain.Wanders = wanders;
				GlobalLocation startingCell = tau.Brain.StartingCell;
				if (startingCell == null || startingCell.CellX != 35 || startingCell.CellY != 19)
				{
					tau.Brain.StartingCell = ParentObject.CurrentZone.GetCell(35, 19).GetGlobalLocation();
					tau.Brain.Goals.Clear();
				}
			}
		}
		else if (Stage == 3)
		{
			if (The.Game.HasIntGameState("ElseingComplete"))
			{
				Stage = 4;
				Timer = 0;
			}
		}
		else
		{
			if (Stage != 4 || Timer++ < 5)
			{
				return;
			}
			Stage = 5;
			if (tau != null)
			{
				tau.RemoveEffect(typeof(Dominated));
				if (!tau.IsPlayer())
				{
					tau.TeleportSwirl();
					tau.Physics.DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
					tau.Obliterate(null, Silent: true);
				}
			}
			if (companion != null)
			{
				companion.RemoveEffect(typeof(Dominated));
				if (!companion.IsPlayer())
				{
					Zone.ObjectEnumerator enumerator = companion.CurrentZone.IterateObjects().GetEnumerator();
					while (enumerator.MoveNext())
					{
						GameObject current = enumerator.Current;
						if (current.Brain?.PartyLeader == companion)
						{
							current.Obliterate(null, Silent: true);
						}
					}
					companion.TeleportSwirl();
					companion.Physics.DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
					companion.Obliterate(null, Silent: true);
				}
			}
			ParentObject.Obliterate(null, Silent: true);
		}
	}

	[WishCommand("elseing:start", null)]
	public static void WishElseingStart()
	{
		Popup.Suppress = true;
		try
		{
			The.Game.StartQuest("If, Then, Else");
			if (The.Player.HasItemWithBlueprint("TauChime") == null)
			{
				The.Player.ReceiveObject("TauChime");
			}
		}
		finally
		{
			Popup.Suppress = false;
		}
	}

	[WishCommand("elseing:taproot", null)]
	public static void WishElseingTaproot()
	{
		Popup.Suppress = true;
		try
		{
			The.Game.StartQuest("If, Then, Else");
			if (The.Player.HasItemWithBlueprint("TauChime") == null)
			{
				The.Player.ReceiveObject("TauChime");
			}
			The.Player.ZoneTeleport("JoppaWorld.76.5.1.1.50", 41, 11);
		}
		finally
		{
			Popup.Suppress = false;
		}
	}

	[WishCommand("elseing:success", null)]
	public static void WishElseingSuccess()
	{
		Popup.Suppress = true;
		try
		{
			The.Game.StartQuest("If, Then, Else");
			The.Game.CompleteQuest("If, Then, Else");
			The.Game.SetIntGameState("ElseingComplete", 1);
		}
		finally
		{
			Popup.Suppress = false;
		}
	}

	[WishCommand("elseing:kill", null)]
	public static void WishElseingKill(string Name)
	{
		Popup.Suppress = true;
		try
		{
			The.Game.StartQuest("If, Then, Else");
			if (Name.EqualsNoCase("tau"))
			{
				GameObject gameObject = The.ActiveZone.FindFirstObject(IsTau);
				if (gameObject == null)
				{
					gameObject = GameObjectFactory.Factory.CreateSampleObject("TauNoLonger");
				}
				gameObject.Die(The.Player);
			}
			else if (Name.EqualsNoCase("companion"))
			{
				GameObject gameObject2 = The.ActiveZone.FindFirstObject(IsCompanion);
				if (gameObject2 == null)
				{
					gameObject2 = GameObjectFactory.Factory.CreateSampleObject("AoygNoLonger");
				}
				gameObject2.Die(The.Player);
			}
			else if (Name.EqualsNoCase("chime"))
			{
				GameObject gameObject3 = The.ActiveZone.FindFirstObject("TauChime");
				if (gameObject3 == null)
				{
					gameObject3 = GameObjectFactory.Factory.CreateSampleObject("TauChime");
				}
				gameObject3.Die(The.Player);
			}
		}
		finally
		{
			Popup.Suppress = false;
		}
	}
}
