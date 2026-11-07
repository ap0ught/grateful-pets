using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class WaterRitualRecord : IPart
{
	public int mySeed;

	public int secretsRemaining = Stat.Random(2, 3);

	public bool giftedItem;

	public bool soldItem;

	public bool canGenerateItem = true;

	public string faction;

	public int totalFactionAvailable = 100;

	public int hermitChatPenalty;

	public int numBlueprints;

	public int numGifts = 1;

	public int numItems;

	public int numFungusLeft = 1;

	public int LastSocialCoprocessorBonus;

	public List<string> attributes = new List<string>();

	public List<int> tinkerdata = new List<int>(2);

	public override void Initialize()
	{
		base.Initialize();
		Faction faction = Factions.Get(ParentObject.GetPrimaryFaction(Base: true));
		this.faction = faction.Name;
		totalFactionAvailable = ParentObject.GetPart<GivesRep>()?.repValue ?? (RuleSettings.REPUTATION_BASE_UNIT * 2);
		mySeed = Stat.SeededRandom("ritualRecord" + ParentObject.CurrentZone?.ZoneID + ParentObject.DisplayName, 0, 2147483646);
		GameObjectBlueprint blueprint = ParentObject.GetBlueprint();
		bool flag = faction.UseAltBehavior(ParentObject);
		string text = blueprint.GetxTag("WaterRitual", "numSecrets");
		if (!text.IsNullOrEmpty())
		{
			secretsRemaining = Math.Max(text.Roll(), 0);
		}
		if (blueprint.GetxTag("WaterRitual", "SellBlueprints") == "true")
		{
			string text2 = blueprint.GetxTag("WaterRitual", "numBlueprints");
			if (!text2.IsNullOrEmpty())
			{
				numBlueprints = Math.Max(text2.Roll(), 0);
			}
		}
		else if (flag)
		{
			if (!faction.WaterRitualAltBlueprints.IsNullOrEmpty())
			{
				numBlueprints = Math.Max(faction.WaterRitualAltBlueprints.Roll(), 0);
			}
			if (!faction.WaterRitualAltGifts.IsNullOrEmpty())
			{
				numGifts = Math.Max(faction.WaterRitualAltGifts.Roll(), 0);
			}
			if (!faction.WaterRitualAltItems.IsNullOrEmpty())
			{
				numItems = Math.Max(faction.WaterRitualAltItems.Roll(), 0);
			}
		}
		else
		{
			if (!faction.WaterRitualBlueprints.IsNullOrEmpty())
			{
				numBlueprints = Math.Max(faction.WaterRitualBlueprints.Roll(), 0);
			}
			if (!faction.WaterRitualGifts.IsNullOrEmpty())
			{
				numGifts = Math.Max(faction.WaterRitualGifts.Roll(), 0);
			}
			if (!faction.WaterRitualItems.IsNullOrEmpty())
			{
				numItems = Math.Max(faction.WaterRitualItems.Roll(), 0);
			}
		}
	}

	public bool Has(string attribute)
	{
		return attributes.Contains(attribute);
	}

	public string GetAttribute(string Prefix, string Default = null)
	{
		foreach (string attribute in attributes)
		{
			if (!attribute.StartsWith(Prefix))
			{
				return attribute.Substring(Prefix.Length);
			}
		}
		return Default;
	}

	public bool TryGetAttribute(string Prefix, out string Value)
	{
		foreach (string attribute in attributes)
		{
			if (attribute.StartsWith(Prefix))
			{
				Value = attribute.Substring(Prefix.Length);
				return true;
			}
		}
		Value = null;
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != BeginConversationEvent.ID || hermitChatPenalty <= 0))
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (hermitChatPenalty > 0 && E.SpeakingWith == ParentObject && E.Actor.IsPlayer())
		{
			Popup.Show("You bothered " + ParentObject.t() + " again.");
			The.Game.PlayerReputation.Modify(faction, -hermitChatPenalty, "WaterRitualHermitOathPunishment");
			hermitChatPenalty = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}
}
