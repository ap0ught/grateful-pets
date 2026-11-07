namespace XRL.World.Conversations.Parts;

public class WaterRitualHermitOath : IWaterRitualPart
{
	public override bool Affordable => true;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override void Awake()
	{
		if (WaterRitual.Record.hermitChatPenalty <= 0 && !WaterRitual.Record.Has("madeHermitOath") && WaterRitual.RecordFaction.WaterRitualHermitOath >= 0)
		{
			Reputation = WaterRitual.RecordFaction.WaterRitualHermitOath;
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		WaterRitual.Record.attributes.Add("madeHermitOath");
		WaterRitual.Record.hermitChatPenalty = Reputation * 2;
		The.Game.PlayerReputation.Modify(WaterRitual.RecordFaction, Reputation, null, null, "WaterRitualHermitOath");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Text.Replace("=hermit=", The.Speaker.GetPropertyOrTag("HermitOathAddressAs", "hermit"));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|+" + GetReputationAward("WaterRitualHermitOath") + "}} reputation]}}";
		return false;
	}
}
