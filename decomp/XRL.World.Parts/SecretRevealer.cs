using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SecretRevealer : IPart
{
	public string id;

	public string text;

	public string message;

	public string adjectives;

	public string category;

	public string extraprepopup;

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!revealed)
		{
			revealed = true;
			JournalMapNote mapNote = JournalAPI.GetMapNote(id);
			if (mapNote != null)
			{
				if (!mapNote.Revealed)
				{
					Popup.Show(message);
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
