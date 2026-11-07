using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class WaterRitualSkillPoint : IWaterRitualPart
{
	public int Points;

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
		if (WaterRitual.RecordFaction.WaterRitualSkillPointCost >= 0 && WaterRitual.RecordFaction.WaterRitualSkillPointAmount > 0)
		{
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "SkillPoint", WaterRitual.RecordFaction.WaterRitualSkillPointCost);
			Points = WaterRitual.RecordFaction.WaterRitualSkillPointAmount;
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			Popup.Show("Talking to " + The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " rouses in you an inert truth. You once wore the frock of a child. You poured salt through the cracks of your fingers, and you watched worlds form. Can it be all so simple still?");
			Popup.Show("You gained {{C|" + Points + "}} skill points!");
			The.Player.GetStat("SP").BaseValue += Points;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[gain {{" + Numeric + "|" + Points + "}} {{W|skill points}}: {{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
