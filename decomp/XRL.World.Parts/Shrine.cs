using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Shrine : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID && ID != PooledEvent<IdleQueryEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			string baseDisplayName = ParentObject.BaseDisplayName;
			string key = "Shrine " + (ParentObject.GetPropertyOrTag("WorshippedAs") ?? baseDisplayName);
			string explanation = null;
			bool flag = true;
			PointOfInterest pointOfInterest = E.Find(key);
			if (pointOfInterest != null)
			{
				if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
				{
					E.Remove(pointOfInterest);
					explanation = "nearest";
				}
				else
				{
					flag = false;
					pointOfInterest.Explanation = "nearest";
				}
			}
			if (flag)
			{
				E.Add(ParentObject, baseDisplayName, explanation, key, null, null, null, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Pray", "pray", "Pray", null, 'y', FireOnActor: false, -10);
		if (!ParentObject.HasPart<ModDesecrated>())
		{
			E.AddAction("Desecrate", "desecrate", "Desecrate", null, 'D', FireOnActor: false, -10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Pray")
		{
			if (PrayAtShrine(E.Actor, FromDialog: true))
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "Desecrate" && DesecrateShrine(E.Actor, FromDialog: true))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (E.Actor.HasPart<Brain>() && 1.in10000())
		{
			GameObject actor = E.Actor;
			if (actor.GetPrimaryFaction() == ParentObject.Owner)
			{
				actor.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
				{
					if (actor.isAdjacentTo(ParentObject))
					{
						PrayAtShrine(actor);
					}
					h.FailToParent();
				}));
				actor.Brain.PushGoal(new MoveTo(ParentObject));
			}
			else if (ParentObject.Owner != null && Factions.GetFeelingFactionToObject(ParentObject.Owner, actor) < 0)
			{
				actor.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
				{
					if (actor.isAdjacentTo(ParentObject))
					{
						DesecrateShrine(actor);
					}
					h.FailToParent();
				}));
				actor.Brain.PushGoal(new MoveTo(ParentObject));
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (GameObject.Validate(E.Actor) && E.Actor != ParentObject)
		{
			bool isCreature = ParentObject.IsCreature;
			if (!isCreature)
			{
				ParentObject.Physics.BroadcastForHelp(E.Actor);
			}
			if (!ParentObject.HasPart<ModDesecrated>() && ParentObject.isDamaged(0.75))
			{
				PerformDesecration(E.Actor, FromDialog: false, E.Actor.IsPlayer(), isCreature, DoHelpBroadcast: false);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool PrayAtShrine(GameObject Actor, bool FromDialog = false, bool UsePopup = false, bool Silent = false)
	{
		Actor.PlayWorldSound("Sounds/Interact/sfx_interact_shrine_pray");
		if (!Silent)
		{
			IComponent<GameObject>.XDidYToZ(Actor, "voice", "a short prayer beneath", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog, UsePopup);
		}
		Actor.UseEnergy(1000, "Item");
		Event e = Event.New("Prayed", "Actor", Actor, "Object", ParentObject);
		ParentObject.FireEvent(e);
		Actor.FireEvent(e);
		WorshipPerformedEvent.Send(Actor, ParentObject);
		return true;
	}

	public void PerformDesecration(GameObject Actor, bool FromDialog = false, bool UsePopup = false, bool Silent = false, bool DoHelpBroadcast = true)
	{
		PlayWorldSound("Sounds/Interact/sfx_interact_shrine_desecrate");
		if (!Silent)
		{
			IComponent<GameObject>.XDidYToZ(Actor, "desecrate", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog, UsePopup);
		}
		ParentObject.AddPart(new ModDesecrated());
		if (DoHelpBroadcast)
		{
			ParentObject.Physics.BroadcastForHelp(Actor);
		}
		Event e = Event.New("Desecrated", "Actor", Actor, "Object", ParentObject);
		ParentObject.FireEvent(e);
		Actor.FireEvent(e);
		BlasphemyPerformedEvent.Send(Actor, ParentObject);
	}

	public bool DesecrateShrine(GameObject Actor, bool FromDialog = false)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return false;
		}
		if (Actor.IsPlayer())
		{
			int freeDrams = Actor.GetFreeDrams("blood");
			int freeDrams2 = Actor.GetFreeDrams("putrid");
			if (freeDrams <= 0 && freeDrams2 <= 0)
			{
				return Actor.Fail("You need a dram of either {{r|blood}} or {{putrid|putrescence}} to desecrate a shrine.");
			}
			List<string> list = new List<string>(2);
			List<string> list2 = new List<string>(2);
			if (freeDrams > 0)
			{
				list.Add("blood");
				list2.Add("blood");
			}
			if (freeDrams2 > 0)
			{
				list.Add("putrescence");
				list2.Add("putrid");
			}
			if (list.Count == 0)
			{
				Debug.LogError("internal inconsistency");
				return Actor.Fail("You need a dram of either {{r|blood}} or {{putrid|putrescence}} to desecrate a shrine.");
			}
			int num = Popup.PickOption("Choose a desecration liquid", null, "", "Sounds/UI/ui_notification", list.ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			Actor.UseDrams(1, list2[num]);
		}
		PerformDesecration(Actor, FromDialog);
		Actor.UseEnergy(1000, "Item");
		return true;
	}
}
