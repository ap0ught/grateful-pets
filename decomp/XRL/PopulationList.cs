using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL;

public abstract class PopulationList : PopulationItem
{
	public struct ItemEnumerator : IDisposable
	{
		private static ConcurrentStack<Queue<PopulationList>> Queues = new ConcurrentStack<Queue<PopulationList>>();

		private Queue<PopulationList> Queue;

		private PopulationList List;

		private PopulationItem Item;

		private int Index;

		private int Count;

		public PopulationItem Current => Item;

		public ItemEnumerator(PopulationList List)
		{
			if (!Queues.TryPop(out Queue))
			{
				Queue = new Queue<PopulationList>();
			}
			this.List = List;
			Item = null;
			Index = 0;
			Count = List.Items.Count;
		}

		public ItemEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (Index == Count)
			{
				if (Queue.TryDequeue(out List))
				{
					Index = 0;
					Count = List.Items.Count;
					continue;
				}
				return false;
			}
			Item = List.Items[Index++];
			if (Item is PopulationList item)
			{
				Queue.Enqueue(item);
			}
			return true;
		}

		public void Dispose()
		{
			Queue.Clear();
			Queues.Push(Queue);
		}
	}

	public string Style;

	public List<PopulationItem> Items = new List<PopulationItem>();

	public ulong TotalWeight
	{
		get
		{
			ulong num = 0uL;
			foreach (PopulationItem item in Items)
			{
				num += item.Weight;
			}
			return num;
		}
	}

	public bool TryFindSimilar(PopulationItem Needle, out PopulationItem Item)
	{
		return TryFindSimilar(Needle.Name, Needle.GetType(), out Item);
	}

	public bool TryFindSimilar(string Name, Type Type, out PopulationItem Item)
	{
		if (!Name.IsNullOrEmpty())
		{
			foreach (PopulationItem item in Items)
			{
				if (item.Name == Name && item.GetType() == Type)
				{
					Item = item;
					return true;
				}
			}
		}
		Item = null;
		return false;
	}

	public override void Generate(List<PopulationResult> Result, Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		if (Style.EqualsNoCase("pickone"))
		{
			ulong totalWeight = TotalWeight;
			ulong num = Stat.NextULong(0uL, totalWeight);
			totalWeight = 0uL;
			{
				foreach (PopulationItem item in Items)
				{
					if (num >= totalWeight && num < totalWeight + item.Weight)
					{
						item.Generate(Result, Vars, Hint ?? DefaultHint);
						break;
					}
					totalWeight += item.Weight;
				}
				return;
			}
		}
		foreach (PopulationItem item2 in Items)
		{
			item2.Generate(Result, Vars, Hint ?? DefaultHint);
		}
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		if (Style.EqualsNoCase("pickone"))
		{
			ulong totalWeight = TotalWeight;
			ulong num = Stat.NextULong(0uL, totalWeight);
			totalWeight = 0uL;
			foreach (PopulationItem item in Items)
			{
				if (num >= totalWeight && num < totalWeight + item.Weight)
				{
					return item.GenerateOne(Vars, Hint ?? DefaultHint);
				}
				totalWeight += item.Weight;
			}
		}
		else
		{
			foreach (PopulationItem item2 in Items)
			{
				PopulationResult populationResult = item2.GenerateOne(Vars, Hint ?? DefaultHint);
				if (populationResult != null)
				{
					return populationResult;
				}
			}
		}
		return null;
	}

	public override void GenerateStructured(PopulationStructuredResult Result, Dictionary<string, string> Vars = null)
	{
		if (Style.EqualsNoCase("pickone"))
		{
			ulong totalWeight = TotalWeight;
			ulong num = Stat.NextULong(0uL, totalWeight);
			totalWeight = 0uL;
			{
				foreach (PopulationItem item in Items)
				{
					if (num >= totalWeight && num < totalWeight + item.Weight)
					{
						item.GenerateStructured(Result, Vars);
						break;
					}
					totalWeight += item.Weight;
				}
				return;
			}
		}
		foreach (PopulationItem item2 in Items)
		{
			item2.GenerateStructured(Result, Vars);
		}
	}

	public int Count(Predicate<PopulationItem> Predicate)
	{
		int num = 0;
		foreach (PopulationItem item in Items)
		{
			if (Predicate(item))
			{
				num++;
			}
			if (item is PopulationList populationList)
			{
				num += populationList.Count(Predicate);
			}
		}
		return num;
	}

	public void ForEach(Action<PopulationItem> Action)
	{
		foreach (PopulationItem item in Items)
		{
			Action(item);
			if (item is PopulationList populationList)
			{
				populationList.ForEach(Action);
			}
		}
	}

	public override PopulationItem Find(Predicate<PopulationItem> Predicate)
	{
		if (Predicate(this))
		{
			return this;
		}
		foreach (PopulationItem item in Items)
		{
			PopulationItem populationItem = item.Find(Predicate);
			if (populationItem != null)
			{
				return populationItem;
			}
		}
		return null;
	}

	public void AddItem(PopulationItem Item)
	{
		if (Item is PopulationGroup populationGroup)
		{
			populationGroup.Parent = this;
			if (!Item.Name.IsNullOrEmpty())
			{
				populationGroup.Info.GroupLookup.Add(Item.Name, populationGroup);
			}
		}
		Items.Add(Item);
	}

	public void RemoveItem(PopulationItem Item)
	{
		Items.Remove(Item);
	}

	public override void MergeFrom(PopulationItem Item, PopulationInfo Info)
	{
		base.MergeFrom(Item, Info);
		PopulationList populationList = (PopulationList)Item;
		if (populationList.Style != null)
		{
			Style = populationList.Style;
		}
		foreach (PopulationItem item in populationList.Items)
		{
			bool flag = !item.Remove;
			if (item.Merge)
			{
				PopulationItem Item2;
				if (item is PopulationGroup populationGroup)
				{
					if (!populationGroup.Name.IsNullOrEmpty() && Info.GroupLookup.TryGetValue(populationGroup.Name, out var value))
					{
						value.MergeFrom(populationGroup, Info);
						flag = false;
					}
				}
				else if (TryFindSimilar(item, out Item2))
				{
					Item2.MergeFrom(item, Info);
					flag = false;
				}
			}
			else if (item.Remove || item.Replace)
			{
				PopulationItem Item3;
				if (item is PopulationGroup populationGroup2)
				{
					if (!populationGroup2.Name.IsNullOrEmpty() && Info.GroupLookup.TryGetValue(populationGroup2.Name, out var value2))
					{
						value2.Parent.Items.Remove(value2);
						Info.GroupLookup.Remove(value2.Name);
						if (item.Replace)
						{
							value2.Parent.AddItem(item);
						}
						flag = false;
					}
				}
				else if (TryFindSimilar(item, out Item3))
				{
					Items.Remove(Item3);
				}
			}
			if (flag)
			{
				AddItem(item);
			}
		}
	}

	public ItemEnumerator IterateItems()
	{
		return new ItemEnumerator(this);
	}
}
