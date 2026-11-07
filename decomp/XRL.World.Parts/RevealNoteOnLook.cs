using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealNoteOnLook : IPart
{
	public string NoteID;

	public bool LookedAt;

	public RevealNoteOnLook()
	{
	}

	public RevealNoteOnLook(string NoteID)
		: this()
	{
		this.NoteID = NoteID;
	}

	public override bool SameAs(IPart Part)
	{
		if (Part is RevealNoteOnLook revealNoteOnLook)
		{
			return revealNoteOnLook.NoteID == NoteID;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command != "Look" && !LookedAt && HasUnrevealedSecret())
		{
			E.Command = "Look";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !LookedAt && JournalAPI.TryRevealNote(NoteID))
		{
			LookedAt = true;
		}
		return base.FireEvent(E);
	}

	public bool HasUnrevealedSecret()
	{
		if (NoteID != null)
		{
			return JournalAPI.HasUnrevealedNote(NoteID);
		}
		return false;
	}
}
