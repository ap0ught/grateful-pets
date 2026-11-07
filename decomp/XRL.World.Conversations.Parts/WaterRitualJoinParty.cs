using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Effects;

namespace XRL.World.Conversations.Parts;

public class WaterRitualJoinParty : IWaterRitualPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override void Awake()
	{
		if (WaterRitual.RecordFaction.WaterRitualJoin && !The.Speaker.IsPlayerLed() && !(The.Speaker.GetxTag("WaterRitual", "NoJoin") == "true"))
		{
			Reputation = Math.Max(50, 200 + (The.Speaker.Stat("Level") - The.Player.Stat("Level")) * (int)((double)RuleSettings.REPUTATION_BASE_UNIT * 0.25));
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Join", Reputation);
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			The.Speaker.SetAlliedLeader<AllyProselytize>(The.Player);
			if (The.Speaker.TryGetEffect<Lovesick>(out var Effect))
			{
				Effect.PreviousLeader = The.Player;
			}
			Popup.Show(The.Speaker.Does("join", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you!");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
