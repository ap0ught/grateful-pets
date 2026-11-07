using System;
using HistoryKit;
using Qud.API;
using Qud.UI;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class OpeningStory : IPart
{
	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeTakeActionEvent>.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		if (!Triggered)
		{
			bool flag = The.Game.GetStringGameState("embark", "Joppa").EqualsNoCase("Joppa");
			Triggered = true;
			string text = null;
			if (The.Game.HasStringGameState("embarkIntroTextID"))
			{
				text = Data.GetText(The.Game.GetStringGameState("embarkIntroTextID"));
			}
			if (The.Game.HasStringGameState("embarkIntroText"))
			{
				text = The.Game.GetStringGameState("embarkIntroText");
			}
			if (text == null)
			{
				text = (flag ? Data.GetText("OpeningStoryJoppa") : Data.GetText("OpeningStoryAlternate"));
			}
			string displayName = ParentObject.GetCurrentCell().ParentZone.DisplayName;
			if (!flag)
			{
				The.Game.SetStringGameState("villageZeroName", displayName);
			}
			text = text.Replace("$day", Calendar.GetDay());
			text = text.Replace("$month", Calendar.GetMonth());
			text = text.Replace("$village", displayName);
			WorldGenerationScreen.HideWorldGenerationScreen();
			Popup.Show(text);
			TutorialManager.OnTrigger("OpeningStoryClosed");
			AddAccomplishment(displayName);
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public static void AddAccomplishment(string VillageName)
	{
		string text = HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".nouns.!random>");
		string text2 = HistoricStringExpander.ExpandString("<spice.colors.!random>");
		JournalAPI.AddAccomplishment("On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", you arrived at " + VillageName + ".", "On the auspicious " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", =name= arrived in " + VillageName + " and began " + The.Player.GetPronounProvider().PossessiveAdjective + " prodigious odyssey through Qud.", "At <spice.time.partsOfDay.!random> under <spice.commonPhrases.strange.!random.article> and " + text2 + " sky, the people of " + VillageName + " saw an image on the horizon that looked like a " + text + " bathed in " + text2 + ". It was =name=, and after " + The.Player.GetPronounProvider().Subjective + " came and left, the people of " + VillageName + " built a monument to =name= and thenceforth called " + The.Player.GetPronounProvider().Objective + " " + Grammar.MakeTitleCase(text) + "-in-" + Grammar.MakeTitleCase(text2) + ".", null, "general", MuralCategory.IsBorn, MuralWeight.Medium, null, -1L);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}
}
