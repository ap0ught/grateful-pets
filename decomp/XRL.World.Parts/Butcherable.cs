using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Butcherable : IPart
{
	public string OnSuccessAmount = "1";

	public string OnSuccess = "";

	public override bool SameAs(IPart p)
	{
		Butcherable butcherable = p as Butcherable;
		if (butcherable.OnSuccessAmount != OnSuccessAmount)
		{
			return false;
		}
		if (butcherable.OnSuccess != OnSuccess)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsButcherable() && E.Actor.HasSkill("CookingAndGathering_Butchery"))
		{
			E.AddAction("Butcher", "butcher", "Butcher", null, 'b', FireOnActor: false, 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Butcher" && AttemptButcher(E.Actor, E.Auto, SkipSkill: false, IntoInventory: false, null, E.FromCell, E.Generated))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		AttemptButcher(E.Object, Automatic: true, SkipSkill: false, IntoInventory: false, null, E.Cell);
		return base.HandleEvent(E);
	}

	public bool AttemptButcher(GameObject Actor, bool Automatic = false, bool SkipSkill = false, bool IntoInventory = false, string Verb = null, Cell FromCell = null, List<GameObject> Tracking = null)
	{
		if (!IsButcherable())
		{
			return false;
		}
		if (!SkipSkill && !Actor.HasSkill("CookingAndGathering_Butchery"))
		{
			return false;
		}
		if (Automatic)
		{
			if (ParentObject.IsImportant())
			{
				return false;
			}
			if (Actor.IsPlayer())
			{
				if (Actor.ArePerceptibleHostilesNearby(logSpot: false, popSpot: false, null, null, null, Options.AutoexploreIgnoreEasyEnemies, Options.AutoexploreIgnoreDistantEnemies))
				{
					return false;
				}
			}
			else if (Actor.Target != null)
			{
				return false;
			}
			CookingAndGathering_Butchery part = Actor.GetPart<CookingAndGathering_Butchery>();
			if (part != null && !part.IsMyActivatedAbilityToggledOn(part.ActivatedAbilityID))
			{
				return false;
			}
			if (ParentObject.HasTagOrProperty("QuestItem"))
			{
				return false;
			}
		}
		if (!Actor.CheckFrozen())
		{
			return false;
		}
		if (!ParentObject.ConfirmUseImportant(Actor, "butcher"))
		{
			return false;
		}
		Cell cell = ParentObject.GetCurrentCell();
		Cell cell2 = FromCell ?? Actor.GetCurrentCell();
		string text = null;
		if (cell != null && cell != cell2)
		{
			text = Directions.GetDirectionDescription(Actor, cell2.GetDirectionFromCell(cell));
		}
		bool storedByPlayer = ParentObject.GetIntProperty("StoredByPlayer") > 0;
		Action<GameObject> action = (GameObject o) =>
		{
			if (storedByPlayer)
			{
				o.SetIntProperty("FromStoredByPlayer", 1);
			}
			Event obj = Event.New("ObjectExtracted");
			obj.SetParameter("Object", o);
			obj.SetParameter("Source", ParentObject);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Action", "Butcher");
			o.FireEvent(obj);
		};
		bool result = false;
		GameObject gameObject = ParentObject.RemoveOne();
		int num = ((!ParentObject.IsTemporary) ? OnSuccessAmount.RollCached() : 0);
		int num2 = 1000;
		if (num > 0)
		{
			num2 /= Math.Max(Actor.GetIntProperty("ButcheryToolEquipped") + 1, 1);
			GameObject gameObject2 = ((OnSuccess[0] != '@') ? GameObject.Create(OnSuccess, 0, 0, null, null, action) : GameObject.Create(PopulationManager.RollOneFrom(OnSuccess.Substring(1)).Blueprint, 0, 0, null, null, action));
			if (!IntoInventory && Actor.IsPlayer() && gameObject2.ShouldAutoget())
			{
				IntoInventory = true;
			}
			if (OnSuccess[0] != '@' && num > 1)
			{
				IComponent<GameObject>.XDidYToZ(Actor, Verb ?? "butcher", gameObject, (text.IsNullOrEmpty() ? "" : (text + " ")) + "into " + Grammar.Cardinal(num) + " " + Grammar.Pluralize(gameObject2.ShortDisplayName));
				if (IntoInventory)
				{
					GameObject gameObject3 = Actor;
					List<GameObject> tracking = Tracking;
					gameObject3.TakeObject(gameObject2, NoStack: false, Silent: true, 0, null, tracking);
					GameObject gameObject4 = Actor;
					string onSuccess = OnSuccess;
					int number = num - 1;
					tracking = Tracking;
					Action<GameObject> afterObjectCreated = action;
					gameObject4.TakeObject(onSuccess, number, NoStack: false, Silent: true, 0, null, 0, 0, null, tracking, null, afterObjectCreated);
				}
				else
				{
					cell.AddObject(gameObject2);
					cell.AddObject(OnSuccess, num - 1, null, null, action);
				}
			}
			else
			{
				IComponent<GameObject>.WDidXToYWithZ(Actor, Verb ?? "butcher", gameObject, ((text == null) ? "" : (text + " ")) + "into", gameObject2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: true);
				if (IntoInventory)
				{
					GameObject gameObject5 = Actor;
					List<GameObject> tracking = Tracking;
					gameObject5.TakeObject(gameObject2, NoStack: false, Silent: true, 0, null, tracking);
				}
				else
				{
					cell.AddObject(gameObject2);
				}
			}
			result = true;
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(Actor, "fail", "to " + (Verb ?? "butcher") + " anything useful from", gameObject, text, null, null, null, null, Actor);
		}
		gameObject.PlayWorldSound("Sounds/Interact/sfx_interact_corpse_butcher");
		gameObject.Bloodsplatter(SelfSplatter: false);
		gameObject.Destroy();
		Actor.UseEnergy(num2, "Skill");
		return result;
	}

	public bool IsButcherable()
	{
		if (!string.IsNullOrEmpty(OnSuccess) && ParentObject.Render.Visible)
		{
			return !ParentObject.HasPart<HologramMaterial>();
		}
		return false;
	}
}
