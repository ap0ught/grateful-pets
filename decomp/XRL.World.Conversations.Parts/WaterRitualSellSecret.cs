using System;
using System.Collections.Generic;
using System.Text;
using Qud.API;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class WaterRitualSellSecret : IWaterRitualSecretPart
{
	public string Message;

	public int Bonus;

	public bool Secret = true;

	public bool Gossip
	{
		get
		{
			return !Secret;
		}
		set
		{
			Secret = !value;
		}
	}

	public override bool Affordable => WaterRitual.Record.totalFactionAvailable > 0;

	public override bool IsSell => true;

	public override bool Filter(IBaseJournalEntry Entry)
	{
		return Entry.CanSell();
	}

	public override int GetWeight(IBaseJournalEntry Entry)
	{
		int buySecretWeight = WaterRitual.RecordFaction.GetBuySecretWeight(Entry, The.Speaker);
		if (buySecretWeight <= 0)
		{
			return 0;
		}
		if (Entry is JournalObservation journalObservation)
		{
			if (journalObservation.Category != "Gossip" != Secret)
			{
				return 0;
			}
			return buySecretWeight;
		}
		if (!Secret)
		{
			return 0;
		}
		return buySecretWeight;
	}

	public virtual void Share()
	{
		List<IBaseJournalEntry> distinctNotes = GetDistinctNotes();
		HistoryAPI.OnWaterRitualSellSecret(WaterRitual.Record, distinctNotes);
		int num = Popup.PickOption(Secret ? "Choose a secret to share:" : "Choose some gossip to share:", null, "", "Sounds/UI/ui_notification", GetOptionsFor(distinctNotes), null, null, null, null, null, null, 1, 60, 0, -1, AllowEscape: true, RespectOptionNewlines: true);
		if (num >= 0)
		{
			SellEntry(distinctNotes[num]);
		}
	}

	public virtual void SellEntry(IBaseJournalEntry Entry)
	{
		Entry.Tradable = false;
		Entry.AppendHistory(" {{K|-shared with " + WaterRitual.RecordFaction.GetFormattedName() + "}}");
		Entry.Updated();
		Entry.Reveal(WaterRitual.RecordFaction.GetFormattedName());
		AwardReputation(Bonus);
	}

	public override void Awake()
	{
		base.Awake();
		Message = null;
		Reputation = (Secret ? 50 : 100);
		Bonus = 0;
		GetWaterRitualSellSecretBehaviorEvent.Send(The.Player, The.Speaker, ref Message, ref Reputation, ref Bonus, Secret, Gossip);
		Reputation = Math.Min(Reputation, WaterRitual.Record.totalFactionAvailable);
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (!Message.IsNullOrEmpty())
		{
			E.Text.Clear();
			E.Text.Append(Message);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (WaterRitual.Record.totalFactionAvailable <= 0)
		{
			Popup.ShowFail(The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " can't grant you any more reputation.");
		}
		else
		{
			Share();
			base.Awake();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder().Append("{{").Append(Lowlight)
			.Append("|[{{")
			.Append(Numeric)
			.Append("|+")
			.Append(GetReputationAward());
		if (Bonus != 0)
		{
			if (Affordable)
			{
				stringBuilder.Append("{{c|");
			}
			stringBuilder.Append((Bonus > 0) ? '+' : '-').Append(Bonus);
			if (Affordable)
			{
				stringBuilder.Append("}}");
			}
		}
		E.Tag = stringBuilder.Append("}} reputation]}}").ToString();
		return false;
	}
}
