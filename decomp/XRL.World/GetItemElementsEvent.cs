using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetItemElementsEvent : PooledEvent<GetItemElementsEvent>
{
	public Dictionary<string, int> Weights = new Dictionary<string, int>();

	public BallBag<string> Bag = new BallBag<string>();

	public new int CascadeLevel;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Weights.Clear();
		Bag.Clear();
		CascadeLevel = 0;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public void Add(string Element, int Weight)
	{
		if (Weights.TryGetValue(Element, out var value))
		{
			int num = value + Weight;
			if (num != 0)
			{
				Weights[Element] = num;
			}
			else
			{
				Weights.Remove(Element);
			}
		}
		else if (Weight != 0)
		{
			Weights.Add(Element, Weight);
		}
	}

	public bool IsRelevantCreature(GameObject Subject)
	{
		return GameObject.Validate(ref Subject);
	}

	public bool IsRelevantObject(GameObject Subject)
	{
		return GameObject.Validate(ref Subject);
	}

	public bool HandleFor(GameObject Item)
	{
		if (GameObject.Validate(ref Item))
		{
			return Item.HandleEvent(this);
		}
		return true;
	}

	public BallBag<string> InitializeBag(Random Random = null)
	{
		BallBag<string> bag = Bag;
		bag.Clear();
		bag.Random = Random ?? Stat.Rnd;
		foreach (KeyValuePair<string, int> weight in Weights)
		{
			bag.Add(weight.Key, weight.Value);
		}
		return bag;
	}

	public static GetItemElementsEvent GetMythicFor(GameObject Item, int Range = 5, Random Random = null)
	{
		GetItemElementsEvent getItemElementsEvent = PooledEvent<GetItemElementsEvent>.FromPool();
		getItemElementsEvent.CascadeLevel = 17;
		getItemElementsEvent.HandleFor(Item);
		int num = 1;
		foreach (string attribute in Statistic.Attributes)
		{
			int statValue = Item.GetStatValue(attribute);
			if (statValue > num)
			{
				num = statValue;
			}
		}
		if (Item.GetStatValue("Strength") == num)
		{
			getItemElementsEvent.Add("might", 5);
		}
		if (Item.GetStatValue("Intelligence") == num)
		{
			getItemElementsEvent.Add("scholarship", 5);
		}
		if (Item.GetStatValue("Ego") == num)
		{
			getItemElementsEvent.Add("jewels", 5);
		}
		int num2 = 0;
		BallBag<string> ballBag = getItemElementsEvent.InitializeBag(Random);
		for (int num3 = ballBag.Count - 1; num3 >= 0; num3--)
		{
			int weight = ballBag.GetWeight(num3);
			if (weight > num2)
			{
				num2 = weight;
			}
		}
		for (int num4 = ballBag.Count - 1; num4 >= 0; num4--)
		{
			if (ballBag.GetWeight(num4) < num2 - Range)
			{
				ballBag.RemoveAt(num4);
			}
		}
		return getItemElementsEvent;
	}
}
