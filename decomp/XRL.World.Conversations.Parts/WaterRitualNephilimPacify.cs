using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitualNephilimPacify : IWaterRitualPart
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
		if (The.Speaker.TryGetPart<NephalProperties>(out var Part) && Part.Phase < 3 && !NephalProperties.IsFoiled(The.Speaker.Blueprint))
		{
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Pacify", 200);
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (The.Speaker.TryGetPart<NephalProperties>(out var Part) && UseReputation() && Part.TryPacify())
		{
			TryFinishNephalStep();
			TryGiveCircle();
		}
		return base.HandleEvent(E);
	}

	public bool TryFinishNephalStep()
	{
		if (The.Game.Quests.TryGetValue("Reclamation", out var Value) && Value.StepsByID.TryGetValue("Nephal", out var value) && !value.Finished && value.Value == The.Speaker.Blueprint)
		{
			The.Game.FinishQuestStep(Value, value.ID);
			return true;
		}
		return false;
	}

	public bool TryGiveCircle()
	{
		bool result = false;
		if (The.Speaker.TryGetPart<DropOnDeath>(out var Part) && !Part.Blueprints.IsNullOrEmpty())
		{
			GameObject gameObject = GameObject.Create(Part.Blueprints);
			result = The.Player.ReceiveObject(gameObject);
			Popup.Show("You receive " + gameObject.an() + "!");
			The.Speaker.RemovePart(Part);
		}
		return result;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
