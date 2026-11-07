using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.API;
using XRL.Liquids;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitual : IConversationPart
{
	public static readonly int REL_LOVE = RuleSettings.REPUTATION_BASE_UNIT * 2;

	public static readonly int REL_LIKE = RuleSettings.REPUTATION_BASE_UNIT;

	public static readonly int REL_DISLIKE = -RuleSettings.REPUTATION_BASE_UNIT;

	public static readonly int REL_HATE = -RuleSettings.REPUTATION_BASE_UNIT * 2;

	private static BaseLiquid _Liquid;

	private static WaterRitualRecord _Record;

	private static Faction _RecordFaction;

	private static bool? _Alternative;

	public static BaseLiquid Liquid => _Liquid ?? (_Liquid = LiquidVolume.GetLiquid(The.Speaker.GetWaterRitualLiquid(The.Player)));

	public static string LiquidName => Liquid.GetName();

	public static string LiquidNameStripped => ColorUtility.StripFormatting(Liquid.GetName());

	public static WaterRitualRecord Record => _Record ?? (_Record = The.Speaker.RequirePart<WaterRitualRecord>());

	public static Faction RecordFaction => _RecordFaction ?? (_RecordFaction = Factions.Get(Record.faction));

	public static bool Alternative
	{
		get
		{
			bool valueOrDefault = _Alternative == true;
			if (!_Alternative.HasValue)
			{
				valueOrDefault = RecordFaction.UseAltBehavior(The.Speaker);
				_Alternative = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public static int Performance => The.Speaker.GetIntProperty("WaterRitualPerformance", 100);

	public static void Reset()
	{
		_Liquid = null;
		_Record = null;
		_RecordFaction = null;
		_Alternative = null;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != LeftElementEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == DisplayTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(DisplayTextEvent E)
	{
		E.Text.Append("\n\n{{C|-----}}\n{{y|Your reputation with ");
		if (RecordFaction.Visible)
		{
			E.Text.Append("{{C|").Append(RecordFaction.GetFormattedName()).Append("}}");
		}
		else
		{
			E.Text.Append(The.Speaker.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true));
		}
		E.Text.Append(" is {{C|").Append(RecordFaction.CurrentReputation).Append("}}.\n")
			.Append(RecordFaction.Visible ? The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true) : The.Speaker.It)
			.Append(" can award an additional {{C|")
			.Append(Record.totalFactionAvailable)
			.Append("}} reputation.}}");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		Reset();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		bool flag = !The.Speaker.HasIntProperty("WaterRitualed");
		if (flag)
		{
			PerformRitual();
		}
		WaterRitualStartEvent.Send(The.Player, The.Speaker, Record, flag);
		return base.HandleEvent(E);
	}

	public void PerformRitual()
	{
		The.Speaker.SetIntProperty("WaterRitualed", 1);
		if (!The.Speaker.HasIntProperty("SifrahWaterRitual"))
		{
			Popup.Show("You share your " + LiquidName + " with " + The.Speaker.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true) + " and begin the water ritual.");
		}
		HandleAchievements();
		AddAccomplishment();
		ModifyReputation();
		NameItems();
	}

	public void HandleAchievements()
	{
		if (The.Speaker.Blueprint == "Oboroqoru")
		{
			Achievement.WATER_RITUAL_OBOROQORU.Unlock();
		}
		if (The.Speaker.Blueprint == "Mamon")
		{
			Achievement.WATER_RITUAL_MAMON.Unlock();
		}
		Achievement.WATER_RITUAL_100_TIMES.Progress.Increment();
	}

	public void AddAccomplishment()
	{
		string referenceDisplayName = The.Speaker.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true);
		string referenceDisplayName2 = The.Speaker.GetReferenceDisplayName();
		IPronounProvider pronounProvider = The.Player.GetPronounProvider();
		JournalAPI.AddAccomplishment("In sacred ritual you shared your " + LiquidName + " with " + referenceDisplayName + ".", "In sacred ritual =name= shared " + pronounProvider.PossessiveAdjective + " holy " + LiquidNameStripped + " with noted luminary " + referenceDisplayName2 + ".", muralWeight: WanderSystem.WanderEnabled() ? MuralWeight.Low : MuralWeight.Medium, gospelText: "<spice.elements." + The.Player.GetMythicDomain() + ".weddingConditions.!random.capitalize>, =name= cemented " + The.Player.GetPronounProvider().PossessiveAdjective + " friendship with " + Factions.GetIfExists(The.Speaker?.GetPrimaryFaction())?.GetFormattedName() + " by marrying " + referenceDisplayName + ".", aggregateWith: null, category: "general", muralCategory: MuralCategory.Treats, secretId: null, time: -1L);
	}

	public void ModifyReputation()
	{
		string referenceDisplayName = The.Speaker.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true);
		int performance = Performance;
		Record.totalFactionAvailable = Record.totalFactionAvailable * performance / 100;
		GivesRep part = The.Speaker.GetPart<GivesRep>();
		int num = part?.repValue ?? REL_LOVE;
		int num2 = GetWaterRitualReputationAmountEvent.GetFor(The.Player, The.Speaker, Record, Record.faction, num);
		num2 = num2 * performance / 100;
		if (num2 != 0)
		{
			The.Game.PlayerReputation.Modify(RecordFaction, num2, null, null, "WaterRitualPrimaryAward");
		}
		if (part == null)
		{
			return;
		}
		part.wasParleyed = true;
		foreach (KeyValuePair<string, int> item in The.Speaker.Brain.GetBaseAllegiance())
		{
			if (!(item.Key == Record.faction))
			{
				Faction ifExists = Factions.GetIfExists(item.Key);
				if (ifExists != null)
				{
					int amount = GetWaterRitualReputationAmountEvent.GetFor(The.Player, The.Speaker, Record, ifExists.Name, num) * performance / 100;
					The.Game.PlayerReputation.Modify(ifExists, amount, "because they love " + referenceDisplayName, null, "WaterRitualFactionAward", Silent: false, Transient: false, SingleLine: false, Multiple: true);
				}
			}
		}
		foreach (FriendorFoe relatedFaction in part.relatedFactions)
		{
			Faction ifExists2 = Factions.GetIfExists(relatedFaction.faction);
			if (ifExists2 != null)
			{
				int baseAmount = 0;
				string text = "regard";
				string text2 = "WaterRitualAttitudeAward";
				switch (relatedFaction.status)
				{
				case "friend":
					baseAmount = num;
					text = "admire";
					text2 = "WaterRitualFriendAward";
					break;
				case "dislike":
					baseAmount = -num / 2;
					text = "dislike";
					text2 = "WaterRitualDislikeAward";
					break;
				case "hate":
					baseAmount = -num;
					text = "despise";
					text2 = "WaterRitualHateAward";
					break;
				}
				baseAmount = GetWaterRitualReputationAmountEvent.GetFor(The.Player, The.Speaker, Record, ifExists2.Name, baseAmount) * (100 + (performance - 100) / 10) / 100;
				if (baseAmount != 0)
				{
					Reputation playerReputation = The.Game.PlayerReputation;
					int amount2 = baseAmount;
					string type = text2;
					playerReputation.Modify(ifExists2, amount2, "because they " + text + " " + referenceDisplayName, null, type, Silent: false, Transient: false, SingleLine: false, Multiple: true);
				}
			}
		}
		The.Game.PlayerReputation.FinishModify();
	}

	public void NameItems()
	{
		int performance = Performance;
		ItemNaming.Opportunity(The.Speaker, null, The.Player, null, "WaterRitual", 7 - performance / 100, 0, 0, performance / 100);
		ItemNaming.Opportunity(The.Player, null, The.Speaker, null, "WaterRitual", 7 - performance / 100, 0, 0, performance / 100);
	}
}
