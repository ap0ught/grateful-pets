using System.Linq;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitualGainMutation : IWaterRitualPart
{
	public string Mutation;

	public MutationEntry Entry;

	public string DisplayName => Entry?.GetDisplayName() ?? Mutation;

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
		if (WaterRitual.Record.Has("SoldMutation") || The.Speaker.HasProperty("WaterRitualNoSellMutation"))
		{
			return;
		}
		Mutation = The.Speaker.GetStringProperty("WaterRitual_Mutation");
		if (Mutation == null)
		{
			Mutation = The.Speaker.GetxTag("WaterRitual", "SellMutation");
		}
		if (Mutation.IsNullOrEmpty())
		{
			if (WaterRitual.RecordFaction.WaterRitualMutation.IsNullOrEmpty())
			{
				return;
			}
			Mutation = WaterRitual.RecordFaction.WaterRitualMutation;
			Entry = MutationFactory.GetMutationEntryByName(Mutation);
			if (Entry == null)
			{
				Entry = MutationFactory.GetMutationEntries(Mutation).FirstOrDefault();
			}
			Reputation = WaterRitual.RecordFaction.WaterRitualMutationCost;
		}
		else
		{
			Entry = MutationFactory.GetMutationEntryByName(Mutation);
			if (Entry == null)
			{
				Entry = MutationFactory.GetMutationEntries(Mutation).FirstOrDefault();
			}
			MutationEntry entry = Entry;
			Reputation = ((entry != null) ? (entry.Cost * 100) : 100);
		}
		if (!The.Player.HasPart(Entry?.Class ?? Mutation))
		{
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Mutation", Reputation);
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			Popup.Show("Despite your genetic limitations, " + The.Speaker.does("teach", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you to improvise {{M|" + DisplayName + "}}!");
			Mutations mutations = The.Player.RequirePart<Mutations>();
			if (Entry != null)
			{
				mutations.AddMutation(Entry);
			}
			else
			{
				mutations.AddMutation(Mutation);
			}
			WaterRitual.Record.attributes.Add("SoldMutation");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[gain {{M|" + DisplayName + "}}: {{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
