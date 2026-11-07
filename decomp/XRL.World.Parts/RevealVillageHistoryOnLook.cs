using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealVillageHistoryOnLook : IPart
{
	public string HistoryId;

	public bool LookedAt;

	public RevealVillageHistoryOnLook()
	{
	}

	public RevealVillageHistoryOnLook(string HistoryId)
		: this()
	{
		this.HistoryId = HistoryId;
	}

	public override bool SameAs(IPart Part)
	{
		if ((Part as RevealVillageHistoryOnLook).HistoryId != HistoryId)
		{
			return false;
		}
		return base.SameAs(Part);
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
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !LookedAt)
		{
			LookedAt = true;
			if (HistoryId != null)
			{
				JournalAPI.RevealVillageNote(HistoryId);
			}
		}
		return base.FireEvent(E);
	}

	public bool HasUnrevealedSecret()
	{
		if (HistoryId != null)
		{
			return JournalAPI.HasUnrevealedVillageNote(HistoryId);
		}
		return false;
	}
}
