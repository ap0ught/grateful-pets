using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SecretObjectTalk : IPart
{
	public string id = "";

	public string text;

	public string message;

	public string adjectives;

	public string category;

	public string achieve = "";

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterConversationEvent E)
	{
		if (!revealed && E.SpeakingWith == ParentObject && E.Actor.IsPlayer())
		{
			revealed = true;
			if (!string.IsNullOrEmpty(achieve))
			{
				AchievementManager.SetAchievement(achieve);
			}
			JournalMapNote mapNote = JournalAPI.GetMapNote(id);
			if (mapNote != null)
			{
				if (!mapNote.Revealed)
				{
					JournalAPI.RevealMapNote(mapNote);
				}
			}
			else
			{
				Popup.Show(message);
				JournalAPI.AddMapNote(ParentObject.GetCurrentCell().ParentZone.ZoneID, text, category, adjectives.Split(','), id, revealed: true, sold: false, -1L);
			}
		}
		return base.HandleEvent(E);
	}
}
