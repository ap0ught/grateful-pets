using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class VillageHistoryBook : IPart
{
	public string Title;

	public string Text;

	public string Events = "";

	public VillageHistoryBook()
	{
	}

	public VillageHistoryBook(string Title, string Text, string Events)
	{
		SetContents(Title, Text, Events);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<HasBeenReadEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && GetHasBeenRead())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, GetHasBeenRead() ? 15 : 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read" && E.Actor.IsPlayer())
		{
			BookUI.ShowBook(Text, Title);
			string[] array = Events.Split(',');
			foreach (string text in array)
			{
				if (!text.IsNullOrEmpty())
				{
					JournalAPI.RevealVillageNote(text);
				}
			}
			AfterReadBookEvent.Send(E.Actor, ParentObject, this, E);
		}
		return base.HandleEvent(E);
	}

	public void SetContents(string Title, string Text, string Events)
	{
		this.Title = Title;
		this.Text = Text;
		this.Events = Events;
	}

	public bool GetHasBeenRead()
	{
		string[] array = Events.Split(',');
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text) && !JournalAPI.IsMapOrVillageNoteRevealed(text))
			{
				return false;
			}
		}
		return true;
	}
}
