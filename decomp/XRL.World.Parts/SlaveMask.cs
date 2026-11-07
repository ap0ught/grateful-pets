using System;

namespace XRL.World.Parts;

[Serializable]
public class SlaveMask : IPart
{
	public bool AffiliationResetDone;

	public override bool WantTurnTick()
	{
		return !AffiliationResetDone;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckForAffiliationReset();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		ResetAffiliation(E.Actor);
		return base.HandleEvent(E);
	}

	public void ResetAffiliation(GameObject who = null)
	{
		if (AffiliationResetDone)
		{
			return;
		}
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		AffiliationResetDone = true;
		who.PartyLeader = null;
		who.Brain?.Goals.Clear();
		who.RemovePart<Preacher>();
		who.RemovePart<DomesticatedSlave>();
		string partParameter = who.GetBlueprint().GetPartParameter<string>("ConversationScript", "ConversationID");
		if (!string.IsNullOrEmpty(partParameter))
		{
			who.RequirePart<ConversationScript>().ConversationID = partParameter;
		}
		ParentObject.RemovePart(this);
	}

	public void CheckForAffiliationReset(GameObject who = null)
	{
		if (AffiliationResetDone)
		{
			return;
		}
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		GameObject Object = who.PartyLeader;
		if (!GameObject.Validate(ref Object))
		{
			ResetAffiliation(who);
		}
	}
}
