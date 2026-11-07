using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class GameUnique : IPart
{
	public string State;

	public string DeathState;

	public bool Replace = true;

	public bool Activated;

	[NonSerialized]
	private bool Triggered;

	public override void Initialize()
	{
		if (State.IsNullOrEmpty())
		{
			State = ParentObject.Blueprint;
		}
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		GameUnique gameUnique = (GameUnique)base.DeepCopy(Parent);
		gameUnique.Activated = false;
		return gameUnique;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneDeactivatedEvent.ID && ID != AfterDieEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		OnCreated(E.Context);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneDeactivatedEvent E)
	{
		CheckState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		if (!Triggered)
		{
			SetDead(E.Killer);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override void Remove()
	{
		State = null;
	}

	public void OnCreated(string Context = null)
	{
		XRLGame game = The.Game;
		if (game == null || State.IsNullOrEmpty())
		{
			return;
		}
		string stringGameState = game.GetStringGameState(State, null);
		if (Context == "Wish" && stringGameState != null && Popup.ShowYesNo(ParentObject.T() + " (" + ParentObject.Blueprint + ") is considered unique, are you sure you want to create another?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) == DialogResult.Yes)
		{
			game.SetStringGameState(State, ParentObject.ID);
			if (!DeathState.IsNullOrEmpty())
			{
				game.RemoveBooleanGameState(DeathState);
			}
		}
		GameManager.Instance.gameQueue.queueTask(CheckState);
	}

	public void CheckState()
	{
		if (State.IsNullOrEmpty() || ParentObject == null || ParentObject.IsTemporary)
		{
			Activated = true;
			return;
		}
		string stringGameState = The.Game.GetStringGameState(State, null);
		if (Activated)
		{
			if (!stringGameState.HasDelimitedSubstring(',', ParentObject.ID))
			{
				Obliterate();
			}
			return;
		}
		if (stringGameState != null && (!Replace || stringGameState.HasDelimitedSubstring(',', "Dead") || (stringGameState != ParentObject.ID && The.ActiveZone.FindObjectByID(stringGameState) != null)))
		{
			Obliterate();
		}
		else
		{
			The.Game.SetStringGameState(State, ParentObject.ID);
		}
		Activated = true;
	}

	public bool IsBondedCompanion(GameObject Object)
	{
		if (Object.Brain != null)
		{
			return Object.IsBondedBy(ParentObject);
		}
		return false;
	}

	public void Obliterate()
	{
		List<GameObject> list = ParentObject.CurrentZone?.FindObjects(IsBondedCompanion);
		if (!list.IsNullOrEmpty())
		{
			foreach (GameObject item in list)
			{
				item.Obliterate(null, Silent: true);
			}
		}
		ParentObject.Obliterate(null, Silent: true);
	}

	public void SetDead(GameObject Killer = null)
	{
		if (!State.IsNullOrEmpty() && !ParentObject.IsTemporary)
		{
			The.Game.TryAddDelimitedGameState(State, ',', "Dead");
			if (Killer != null && Killer.IsPlayerControlled())
			{
				The.Game.TryAddDelimitedGameState(State, ',', "KilledByPlayer");
			}
			if (!DeathState.IsNullOrEmpty())
			{
				The.Game.SetBooleanGameState(DeathState, Value: true);
			}
		}
		Triggered = true;
	}
}
