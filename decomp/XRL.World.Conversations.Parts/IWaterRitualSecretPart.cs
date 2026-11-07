using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Collections;

namespace XRL.World.Conversations.Parts;

public abstract class IWaterRitualSecretPart : IWaterRitualPart
{
	public const int REP_SECRET = 50;

	public const int REP_GOSSIP = 100;

	public BallBag<IBaseJournalEntry> Notes;

	public Rack<IEventHandler> Handlers;

	public StringMap<int> Tagged;

	public int Seed;

	public string Context;

	public virtual bool IsBuy => false;

	public virtual bool IsSell => false;

	public virtual bool Filter(IBaseJournalEntry Entry)
	{
		return true;
	}

	[Obsolete]
	public virtual bool SultanNote(JournalSultanNote Entry)
	{
		return true;
	}

	[Obsolete]
	public virtual bool Observation(JournalObservation Entry)
	{
		return true;
	}

	[Obsolete]
	public virtual bool MapNote(JournalMapNote Entry)
	{
		return true;
	}

	[Obsolete]
	public virtual bool RecipeNote(JournalRecipeNote Entry)
	{
		return true;
	}

	public virtual int GetWeight(IBaseJournalEntry Entry)
	{
		return Entry.Weight;
	}

	public override void Awake()
	{
		if (!The.Speaker.GetxTag("WaterRitual", "NoSecrets").EqualsNoCase("true"))
		{
			ShuffleNotes(WaterRitual.Record.mySeed);
			Visible = !Notes.Items.IsNullOrEmpty();
		}
	}

	/// <remarks>Used for prioritizing notes that share little in common with previously selected notes.</remarks>
	public virtual int CountShared(IBaseJournalEntry Entry, List<IBaseJournalEntry> Notes)
	{
		int num = 0;
		foreach (IBaseJournalEntry Note in Notes)
		{
			if (Entry.Text == Note.Text)
			{
				num++;
			}
			foreach (string attribute in Note.Attributes)
			{
				if (Entry.Attributes.Contains(attribute))
				{
					num++;
				}
			}
		}
		return num;
	}

	public virtual void InitializeTagWeights(GameObject Speaker)
	{
		DelimitedEnumeratorChar delimitedEnumeratorChar;
		DelimitedEnumeratorChar enumerator;
		if (IsBuy)
		{
			string value = Speaker.GetxTag("WaterRitual", "SellSecretWeight");
			if (value.IsNullOrEmpty())
			{
				return;
			}
			delimitedEnumeratorChar = value.DelimitedBy(';');
			enumerator = delimitedEnumeratorChar.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Split(':', out var First, out var Second);
				if (int.TryParse(Second, out var result))
				{
					Tagged[First] = result;
				}
			}
		}
		else
		{
			if (!IsSell)
			{
				return;
			}
			string value2 = Speaker.GetxTag("WaterRitual", "BuySecretWeight");
			if (value2.IsNullOrEmpty())
			{
				return;
			}
			delimitedEnumeratorChar = value2.DelimitedBy(';');
			enumerator = delimitedEnumeratorChar.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Split(':', out var First2, out var Second2);
				if (int.TryParse(Second2, out var result2))
				{
					Tagged[First2] = result2;
				}
			}
		}
	}

	public void Dispatch(GetWaterRitualSecretWeightEvent E, IBaseJournalEntry Secret)
	{
		int weight = 0;
		if (Filter(Secret))
		{
			weight = (Tagged.TryGetValue(Secret.ID, out var Value) ? Value : GetWeight(Secret));
		}
		E.Secret = Secret;
		E.BaseWeight = (E.Weight = weight);
		int i = 0;
		for (int count = Handlers.Count; i < count; i++)
		{
			Handlers[i].HandleEvent(E);
		}
		if (E.Weight > 0)
		{
			Notes.Add(E.Secret, E.Weight);
		}
	}

	public virtual void ShuffleNotes(int Seed)
	{
		this.Seed = Seed;
		if (Notes == null)
		{
			Notes = new BallBag<IBaseJournalEntry>(new Random(Seed), 16);
		}
		Notes.Clear();
		if (Handlers == null)
		{
			Handlers = new Rack<IEventHandler>();
		}
		Handlers.Clear();
		if (Tagged == null)
		{
			Tagged = new StringMap<int>();
		}
		Tagged.Clear();
		GetWaterRitualSecretWeightEvent getWaterRitualSecretWeightEvent = PooledEvent<GetWaterRitualSecretWeightEvent>.FromPool();
		getWaterRitualSecretWeightEvent.Actor = The.Player;
		getWaterRitualSecretWeightEvent.Speaker = The.Speaker;
		getWaterRitualSecretWeightEvent.Buy = IsBuy;
		getWaterRitualSecretWeightEvent.Sell = IsSell;
		getWaterRitualSecretWeightEvent.Source = this;
		getWaterRitualSecretWeightEvent.Context = Context;
		InitializeTagWeights(getWaterRitualSecretWeightEvent.Speaker);
		getWaterRitualSecretWeightEvent.Actor.GetWantEventHandlers(PooledEvent<GetWaterRitualSecretWeightEvent>.ID, MinEvent.CascadeLevel, Handlers);
		getWaterRitualSecretWeightEvent.Speaker.GetWantEventHandlers(PooledEvent<GetWaterRitualSecretWeightEvent>.ID, MinEvent.CascadeLevel, Handlers);
		foreach (JournalSultanNote sultanNote in JournalAPI.SultanNotes)
		{
			Dispatch(getWaterRitualSecretWeightEvent, sultanNote);
		}
		foreach (JournalObservation observation in JournalAPI.Observations)
		{
			Dispatch(getWaterRitualSecretWeightEvent, observation);
		}
		foreach (JournalMapNote mapNote in JournalAPI.MapNotes)
		{
			Dispatch(getWaterRitualSecretWeightEvent, mapNote);
		}
		foreach (JournalRecipeNote recipeNote in JournalAPI.RecipeNotes)
		{
			Dispatch(getWaterRitualSecretWeightEvent, recipeNote);
		}
		getWaterRitualSecretWeightEvent.Reset();
		Handlers.Clear();
		Tagged.Clear();
		HistoryAPI.OnWaterRitualShuffleSecrets(WaterRitual.Record, Notes);
	}

	public virtual List<IBaseJournalEntry> GetDistinctNotes(int Amount = 3)
	{
		if (Amount > Notes.Count)
		{
			Amount = Notes.Count;
		}
		Notes.Random = new Random(Seed);
		List<IBaseJournalEntry> list = new List<IBaseJournalEntry>(Amount);
		for (int i = 0; i < Amount; i++)
		{
			IBaseJournalEntry distinctNote = GetDistinctNote(list);
			if (distinctNote != null)
			{
				list.Add(distinctNote);
			}
		}
		return list;
	}

	public virtual IBaseJournalEntry GetDistinctNote(List<IBaseJournalEntry> Selected = null)
	{
		if (Selected == null)
		{
			Notes.Random = new Random(Seed);
			return Notes.PeekOne();
		}
		IBaseJournalEntry result = null;
		int num = int.MaxValue;
		int i = 0;
		for (int num2 = Notes.Count * 5; i < num2; i++)
		{
			IBaseJournalEntry baseJournalEntry = Notes.PeekOne();
			if (Selected.Contains(baseJournalEntry))
			{
				continue;
			}
			num2 = Math.Min(num2, 5);
			int num3 = CountShared(baseJournalEntry, Selected);
			if (num3 < num)
			{
				result = baseJournalEntry;
				num = num3;
				if (num3 == 0)
				{
					break;
				}
			}
		}
		return result;
	}

	public virtual string[] GetOptionsFor(List<IBaseJournalEntry> Secrets)
	{
		int count = Secrets.Count;
		string[] array = new string[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = Secrets[i].GetShareText();
		}
		return array;
	}
}
