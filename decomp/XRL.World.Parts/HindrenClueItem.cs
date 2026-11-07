using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class HindrenClueItem : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		RevealHindrenClue(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		RevealHindrenClue(E.Actor);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		Registrar.Register("LookedAt");
		Registrar.Register("Open");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LookedAt" || E.ID == "AfterLookedAt")
		{
			RevealHindrenClue();
		}
		else if (E.ID == "Open")
		{
			RevealHindrenClue(E.GetGameObjectParameter("Opener"));
		}
		return base.FireEvent(E);
	}

	private void RevealHindrenClue(GameObject who = null)
	{
		if (The.Game != null && The.Game.HasQuest("Kith and Kin") && !The.Game.FinishedQuest("Kith and Kin") && (who == null || who.IsPlayer()))
		{
			JournalAPI.RevealObservation(ParentObject.Blueprint, onlyIfNotRevealed: true);
			KithAndKinGameState.Instance.foundClue();
		}
	}
}
