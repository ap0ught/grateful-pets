using System;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Amnesia : BaseMutation
{
	[NonSerialized]
	private long LastProccedTurn = -1L;

	public string currentZone;

	public Amnesia()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void RegisterActive(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(The.Game, PooledEvent<SecretVisibilityChangedEvent>.ID);
	}

	public override string GetDescription()
	{
		return "You forget things and places.\n\nWhenever you learn a new secret, there's a small chance you forget a secret.\nWhenever you return to a map you previously visited, there's a small chance you forget the layout.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public bool DoesSecretTrigger(IBaseJournalEntry Entry)
	{
		if (Entry is JournalSultanNote)
		{
			return true;
		}
		if (Entry is JournalMapNote)
		{
			return true;
		}
		if (Entry is JournalObservation)
		{
			return true;
		}
		if (Entry is JournalRecipeNote)
		{
			return true;
		}
		return false;
	}

	public static bool AffectsSecret(IBaseJournalEntry Note)
	{
		if (Note == null)
		{
			return false;
		}
		if (!Note.Forgettable())
		{
			return false;
		}
		if (!Note.Has("gossip") && !Note.Has("sultan") && !Note.Has("village"))
		{
			return Note is JournalMapNote;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(SecretVisibilityChangedEvent E)
	{
		if (ParentObject != null && ParentObject.IsPlayer() && LastProccedTurn != The.Game.Turns && E.Entry.Revealed && DoesSecretTrigger(E.Entry) && 5.in100())
		{
			LastProccedTurn = The.Game.Turns;
			IBaseJournalEntry randomElement = JournalAPI.GetKnownNotes(AffectsSecret).GetRandomElement();
			if (randomElement != null && randomElement != E.Entry)
			{
				randomElement.Forget();
				IComponent<GameObject>.AddPlayerMessage("You feel like you forgot something important.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (5.in100() && ParentObject.IsPlayer())
		{
			Zone zone = ParentObject.CurrentZone;
			if (zone != null && currentZone != zone.ZoneID && !zone.IsWorldMap() && zone.LastPlayerPresence != -1 && zone.LastPlayerPresence < XRLCore.CurrentTurn - 2)
			{
				zone.ClearExploredMap();
				ParentObject?.PlayWorldSound("sfx_ability_mutation_mentalDefect_generic_activate");
				IComponent<GameObject>.AddPlayerMessage("This place feels vaguely familiar.");
				GenericDeepNotifyEvent.Send(ParentObject, "AmnesiaTriggered", ParentObject, ParentObject);
			}
			currentZone = zone.ZoneID;
		}
		return base.HandleEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
