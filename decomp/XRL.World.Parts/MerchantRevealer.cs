using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MerchantRevealer : IPart
{
	public JournalMapNote merchantLoc;

	public string bookTitle;

	public string bookText;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != BeforeObjectCreatedEvent.ID && ID != EnteredCellEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<HasBeenReadEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		CheckMerchantLocation(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		CheckMerchantLocation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckMerchantLocation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckMerchantLocation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && (merchantLoc == null || merchantLoc.Revealed))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (merchantLoc != null && !merchantLoc.Revealed) ? 100 : 15);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read" && E.Actor.IsPlayer())
		{
			BookUI.ShowBook(bookText, bookTitle);
			AfterReadBookEvent.Send(E.Actor, ParentObject, this, E);
			if (merchantLoc != null)
			{
				JournalMapNote journalMapNote = JournalAPI.GetMapNotes((JournalMapNote n) => n.SameAs(merchantLoc)).FirstOrDefault();
				if (journalMapNote != null)
				{
					merchantLoc = journalMapNote;
				}
				else
				{
					JournalAPI.AddMapNote(merchantLoc);
				}
				merchantLoc?.Reveal();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void CheckMerchantLocation(IObjectCreationEvent E = null)
	{
		if (merchantLoc != null)
		{
			return;
		}
		GenerateMerchantLocation();
		if (merchantLoc == null)
		{
			GameObject gameObject = GameObject.Create(PopulationManager.RollOneFrom("Books").Blueprint);
			if (E != null)
			{
				E.ReplacementObject = gameObject;
			}
			else
			{
				ParentObject.ReplaceWith(gameObject);
			}
		}
	}

	private void GenerateMerchantLocation()
	{
		if (The.Game != null)
		{
			merchantLoc = JournalAPI.GetMapNotes((JournalMapNote mapnote) => mapnote.Has("merchant") && mapnote.Has("humanoid")).GetRandomElement();
			if (merchantLoc != null)
			{
				bookTitle = "advertisement for " + Grammar.InitLower(merchantLoc.Text);
				ParentObject.DisplayName = bookTitle;
				Dictionary<string, string> vars = new Dictionary<string, string>
				{
					{
						"$workshop",
						"{{|" + Grammar.InitLower(merchantLoc.Text) + "}}"
					},
					{
						"$location",
						LoreGenerator.GenerateLandmarkDirectionsTo(merchantLoc.ZoneID)
					}
				};
				bookText = HistoricStringExpander.ExpandString("<spice.advertisements.merchants.!random>", null, null, vars);
			}
		}
	}
}
