using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Conversations.Parts;

public class WaterRitualFungusColonize : IWaterRitualPart
{
	public string Type;

	public string DisplayName = "a fungus";

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
		if (WaterRitual.Record.numFungusLeft > 0 && WaterRitual.RecordFaction.WaterRitualFungusInfect >= 0)
		{
			if (!WaterRitual.Record.TryGetAttribute("FungusType:", out Type))
			{
				Type = SporePuffer.InfectionObjectList.GetRandomElement();
				WaterRitual.Record.attributes.Add("FungusType:" + Type);
			}
			if (GameObjectFactory.Factory.Blueprints.TryGetValue(Type, out var value))
			{
				DisplayName = value.DisplayName();
			}
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "FungalInfection", WaterRitual.RecordFaction.WaterRitualFungusInfect);
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (FungalSporeInfection.ChooseLimbForInfection(DisplayName, out var Target, out var _) && UseReputation())
		{
			FungalSporeInfection.ApplyFungalInfection(The.Player, Type, Target);
			WaterRitual.Record.numFungusLeft--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[become infected with " + DisplayName + ": {{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
