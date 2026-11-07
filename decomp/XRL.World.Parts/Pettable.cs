using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World.AI.GoalHandlers;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class Pettable : IPart
{
	public static readonly int DEFAULT_PET_GOAL_CHANCE = 10;

	public static readonly int DEFAULT_PET_GOAL_WAIT = 100;

	public string UseFactionForFeelingFloor;

	public bool PettableIfPositiveFeeling;

	public bool OnlyAllowIfLiked = true;

	public int MaxAIDistance = 5;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<IdleQueryEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Pet", "pet", "Pet", null, 'p', FireOnActor: false, 5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Pet" && Pet(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if ((!E.Actor.IsPlayer() || !ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && !PreferConversation())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if ((!E.Actor.IsPlayer() || !ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && !PreferConversation())
		{
			Pet(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (CheckAIPetting(E.Actor))
		{
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
		Registrar.Register("CanHaveSmartUseConversation");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanHaveSmartUseConversation" && !PreferConversation())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool Pet(GameObject Actor)
	{
		if (!Actor.CanMoveExtremities("Pet", ShowMessage: true))
		{
			return false;
		}
		if (!CanPet(Actor, out var FailureMessage))
		{
			if (!FailureMessage.IsNullOrEmpty())
			{
				Actor.Fail(FailureMessage);
			}
			return false;
		}
		if (!TryGetLimbs(Actor, ParentObject, out var ActorLimb, out var ObjectLimb))
		{
			return Actor.ShowFailure("You lack the means to do that.");
		}
		if (!WillingToBePettedBy(Actor))
		{
			Actor.Fail(ParentObject.Does("shy") + " away from you.");
			return false;
		}
		CombatJuiceEntryPunch entry = CombatJuice.punch(Actor, ParentObject, 0.2f, Easing.Functions.SineEaseInOut, 0f, 0f, 0f, 0f, 0.5f, "PetSound", "Sounds/Interact/sfx_interact_pet", AllowPlayerSound: true, Async: true);
		if (Actor.IsPlayer())
		{
			CombatJuice.BlockUntilFinished((CombatJuiceEntry)entry, (IList<GameObject>)null, 500, true);
		}
		IComponent<GameObject>.XDidYToZ(Actor, "pet", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		string text = ParentObject.GetPropertyOrTag("PetResponse", "=subject.T= =verb:stare= at =object.t= blankly.");
		if (!text.IsNullOrEmpty())
		{
			if (text.Contains(","))
			{
				text = text.CachedCommaExpansion().GetRandomElement();
			}
			if (!text.Contains("=") && !text.StartsWith("*"))
			{
				if (text.Contains(" you "))
				{
					text = text.Replace(" you ", " =object.t= ");
				}
				if (text.Contains(" you."))
				{
					text = text.Replace(" you.", " =object.t=.");
				}
				if (text.Contains(" you!"))
				{
					text = text.Replace(" you!", " =object.t=!");
				}
				if (text.Contains(" your "))
				{
					text = text.Replace(" your ", " =objpronouns.possessive= ");
				}
				if (text.Contains(" yours "))
				{
					text = text.Replace(" your ", " =objpronouns.substantivePossessive= ");
				}
				if (text.Contains(" yours."))
				{
					text = text.Replace(" yours.", " =objpronouns.substantivePossessive=.");
				}
				if (text.Contains(" yours!"))
				{
					text = text.Replace(" yours!", " =objpronouns.substantivePossessive=!");
				}
				text = "=subject.T= " + text;
				char c = ColorUtility.LastCharacterExceptFormatting(text);
				if (c != '.' && c != '!' && c != '?')
				{
					text += ".";
				}
			}
			text = GameText.VariableReplace(text, ParentObject, Actor, StripColors: true);
			if (!text.IsNullOrEmpty())
			{
				EmitMessage(text, ' ', FromDialog: false, Actor.IsPlayer() || ParentObject.IsPlayer());
			}
		}
		Actor.UseEnergy(1000, "Petting");
		AfterPetEvent.Send(ParentObject, Actor);
		PhysicalContactEvent.Send(Actor, ParentObject, ActorLimb, ObjectLimb);
		return true;
	}

	public static bool TryGetLimbs(GameObject Actor, GameObject Object, out BodyPart ActorLimb, out BodyPart ObjectLimb)
	{
		ActorLimb = Actor.Body.GetFirstPart("Hand") ?? Actor.Body.GetFirstPart((BodyPart x) => x.Appendage);
		ObjectLimb = Object.Body.GetBody();
		if (50.in100())
		{
			ObjectLimb = Object.Body.GetFirstPart("Head") ?? ObjectLimb;
		}
		if (ActorLimb != null)
		{
			return ObjectLimb != null;
		}
		return false;
	}

	public bool WillingToBePettedBy(GameObject Actor)
	{
		if (PettableIfPositiveFeeling && ParentObject.Brain.GetFeeling(Actor) < 0)
		{
			return false;
		}
		if (UseFactionForFeelingFloor == null)
		{
			if (OnlyAllowIfLiked && Actor != null && (ParentObject.Brain?.GetFeeling(Actor) ?? 0) < 50)
			{
				return false;
			}
		}
		else if (!PettableIfPositiveFeeling && (ParentObject.Brain.GetFeeling(Actor) < 0 || (Actor.IsPlayer() && Math.Max(The.Game.PlayerReputation.GetFeeling(UseFactionForFeelingFloor), ParentObject.Brain?.GetFeeling(Actor) ?? 0) < 50)))
		{
			return false;
		}
		return true;
	}

	public static bool CanPet(GameObject Actor, out string FailureMessage)
	{
		if (Actor.Brain == null || Actor.HasTagOrProperty("NoPetting"))
		{
			FailureMessage = "You cannot do that.";
			return false;
		}
		FailureMessage = null;
		return true;
	}

	public static bool CanPet(GameObject Actor)
	{
		string FailureMessage;
		return CanPet(Actor, out FailureMessage);
	}

	public static bool PreferConversation(GameObject Actor)
	{
		if (Actor.GetIntProperty("NamedVillager") > 0)
		{
			return true;
		}
		if (Actor.GetIntProperty("ParticipantVillager") > 0)
		{
			return true;
		}
		if (Actor.GetIntProperty("Hero") > 0)
		{
			return true;
		}
		if (Actor.GetIntProperty("Librarian") > 0)
		{
			return true;
		}
		if (Actor.HasTagOrProperty("PreferChatToPet"))
		{
			return true;
		}
		return false;
	}

	public bool PreferConversation()
	{
		return PreferConversation(ParentObject);
	}

	public bool CheckAIPetting(GameObject Subject)
	{
		if (!GameObject.Validate(Subject))
		{
			return false;
		}
		if (Subject == ParentObject)
		{
			return false;
		}
		if (ParentObject.CurrentZone == null)
		{
			return false;
		}
		if (!CanPet(Subject))
		{
			return false;
		}
		if (!WillingToBePettedBy(Subject))
		{
			return false;
		}
		if (!ParentObject.GetIntProperty("PetGoalChance", DEFAULT_PET_GOAL_CHANCE).in100())
		{
			return false;
		}
		if (Subject.InSameOrAdjacentCellTo(ParentObject))
		{
			InventoryActionEvent.Check(ParentObject, Subject, ParentObject, "Pet");
		}
		else if ((MaxAIDistance < 0 || Subject.DistanceTo(ParentObject) < MaxAIDistance) && Subject.IsMobile())
		{
			if (Subject.GetIntProperty("AIPetWait") > 0)
			{
				if (Subject.ModIntProperty("AIPetWait", -1, RemoveIfZero: true) > 0)
				{
					return false;
				}
			}
			else
			{
				Subject.Brain.PushGoal(new Pet(ParentObject, 5, MaxAIDistance));
				int intProperty = ParentObject.GetIntProperty("PetGoalWait", DEFAULT_PET_GOAL_WAIT);
				Subject.SetIntProperty("AIPetWait", intProperty);
			}
		}
		return true;
	}
}
