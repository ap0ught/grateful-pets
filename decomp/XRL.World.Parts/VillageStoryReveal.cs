using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class VillageStoryReveal : IPart
{
	public string style = "engraving";

	public string villageName = "";

	public string villageId = "";

	public string eventText;

	public string secretId;

	public bool bLookedAt;

	public VillageStoryReveal()
	{
	}

	public VillageStoryReveal(JournalVillageNote note, string style)
		: this()
	{
		villageId = note.VillageID;
		villageName = HistoryAPI.GetEntityName(note.VillageID);
		eventText = note.Text.Split('|')[0];
		secretId = note.ID;
		this.style = style;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command != "Look" && IsVillageUnrevealed())
		{
			E.Command = "Look";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (style == "painting")
		{
			E.AddAdjective("{{painted|painted}}");
		}
		else if (style == "engraving")
		{
			E.AddAdjective("{{engraved|engraved}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AddStory(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		AddStory(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		GenerateStory();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !bLookedAt)
		{
			bLookedAt = true;
			if (eventText != null)
			{
				RevealVillage();
			}
		}
		return base.FireEvent(E);
	}

	private void GenerateStory()
	{
		if (eventText != null || The.Game == null)
		{
			return;
		}
		History sultanHistory = The.Game.sultanHistory;
		if (sultanHistory != null)
		{
			HistoricEntity randomElement = sultanHistory.GetEntitiesWherePropertyEquals("type", "village").GetRandomElement(Stat.Rand);
			villageName = randomElement.GetCurrentSnapshot().GetProperty("name");
			villageId = randomElement.id;
			List<string> list = randomElement.GetCurrentSnapshot().GetList("Gospels");
			if (list.Count > 0)
			{
				eventText = list[Stat.Random(0, list.Count - 1)];
			}
			else
			{
				eventText = "<marred and unreadable>";
			}
		}
	}

	public JournalVillageNote GetVillageNote()
	{
		return JournalAPI.VillageNotes.Find((JournalVillageNote n) => n.ID == secretId);
	}

	public bool IsVillageUnrevealed()
	{
		JournalVillageNote villageNote = GetVillageNote();
		if (villageNote != null)
		{
			return !villageNote.Revealed;
		}
		return false;
	}

	public void RevealVillage()
	{
		GetVillageNote()?.Reveal();
	}

	private void AddStory(IShortDescriptionEvent E)
	{
		if (eventText == null)
		{
			GenerateStory();
		}
		if (style == "painting")
		{
			E.Postfix.Append("\n{{C|Painted: This object is painted with a scene from the history of the village {{M|").Append(villageName).Append("}}:\n\n")
				.Append(eventText)
				.Append("}}\n");
		}
		else if (style == "engraving")
		{
			E.Postfix.Append("\n{{C|Engraved: This object is engraved with a scene from the history of the village {{M|").Append(villageName).Append("}}:\n\n")
				.Append(eventText)
				.Append("}}\n");
		}
		else if (style == "monument")
		{
			E.Postfix.Append("\n{{C|This object is a monument to a scene from the history of the village {{M|").Append(villageName).Append("}}:\n\n")
				.Append(eventText)
				.Append("}}\n");
		}
		else if (!(style == "tattoo"))
		{
			if (style == "light-pattern")
			{
				E.Postfix.Append("\n{{C|Holographic: This hologram depicts a scene from the history of the village {{M|").Append(villageName).Append("}}:\n\n")
					.Append(eventText)
					.Append("}}\n");
			}
			else if (style != null)
			{
				E.Postfix.Append("\n{{C|").Append(ColorUtility.CapitalizeExceptFormatting(style)).Append(": This object bears ")
					.Append(Grammar.A(style))
					.Append(" commemorating a scene from the history of the village {{M|")
					.Append(villageName)
					.Append("}}:\n\n")
					.Append(eventText)
					.Append("}}\n");
			}
		}
	}
}
