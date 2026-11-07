using XRL.Language;
using XRL.UI;
using XRL.World.Quests;

namespace XRL.World.Conversations.Parts;

/// <summary>Adds the current location as a possible sanctuary for the slynth during the quest Landing Pads.</summary>
public class AddSlynthCandidate : IConversationPart
{
	/// <summary>Optional explicit sanctuary name to use instead of zone name.</summary>
	public string Sanctuary;

	/// <summary>Whether the explicit sanctuary name is plural.</summary>
	public bool Plural;

	public string SactuaryZoneID => The.Speaker?.Brain?.StartingCell?.ZoneID ?? The.Speaker?.CurrentZone?.ZoneID ?? The.ActiveZone.ZoneID;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public string GetSanctuaryName()
	{
		if (Sanctuary.IsNullOrEmpty())
		{
			Sanctuary = The.ZoneManager.GetZoneDisplayName(SactuaryZoneID, WithIndefiniteArticle: true, WithDefiniteArticle: false, WithStratum: false, Mutate: false);
		}
		return Sanctuary;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{W|[confirm " + GetSanctuaryName() + " as a sanctuary option]}}";
		return false;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		LandingPadsSystem system = The.Game.GetSystem<LandingPadsSystem>();
		Popup.Show(Grammar.InitCap(GetSanctuaryName()) + (Plural ? " are" : " is") + " now a sanctuary option for the slynth.");
		system.candidateFactions.Add(The.Speaker?.GetPropertyOrTag("Mayor"));
		system.candidateFactionZones.Add(SactuaryZoneID);
		system.updateQuestStatus();
		return base.HandleEvent(E);
	}
}
