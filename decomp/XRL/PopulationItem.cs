using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL;

public abstract class PopulationItem
{
	public string Name;

	public string Hint;

	public string Load;

	public uint Weight = 1u;

	public bool Merge => Load == "Merge";

	public bool Remove => Load == "Remove";

	public bool Replace => Load == "Replace";

	public abstract void Generate(List<PopulationResult> Result, Dictionary<string, string> Vars = null, string DefaultHint = null);

	public List<PopulationResult> Generate(Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		List<PopulationResult> result = new List<PopulationResult>();
		Generate(result, Vars, DefaultHint);
		return result;
	}

	public abstract PopulationResult GenerateOne(Dictionary<string, string> Vars = null, string DefaultHint = null);

	public abstract void GenerateStructured(PopulationStructuredResult Result, Dictionary<string, string> Vars = null);

	public abstract void GetEachUniqueObject(List<string> List);

	public abstract PopulationItem Find(Predicate<PopulationItem> Predicate);

	public virtual void MergeFrom(PopulationItem Item, PopulationInfo Info)
	{
		if (Item.Hint != null)
		{
			Hint = Item.Hint;
		}
		if (Item.Weight != 1)
		{
			Weight = Item.Weight;
		}
	}

	protected double CalculateProbability(string Chance)
	{
		if (Chance.IsNullOrEmpty())
		{
			return 1.0;
		}
		double num = 1.0;
		DelimitedEnumeratorChar enumerator = Chance.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (double.TryParse(enumerator.Current, out var result))
			{
				num *= (100.0 - Math.Clamp(result, 0.0, 100.0)) / 100.0;
			}
		}
		return 1.0 - num;
	}

	protected int RollChance(string Chance)
	{
		if (Chance.IsNullOrEmpty())
		{
			return 1;
		}
		int num = 0;
		DelimitedEnumeratorChar enumerator = Chance.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (!current.IsEmpty && double.TryParse(current, out var result) && !(Stat.Rnd.NextDouble() > result / 100.0))
			{
				num++;
			}
		}
		return num;
	}

	protected int RollNumber(string Number)
	{
		if (Number.IsNullOrEmpty())
		{
			return 1;
		}
		return Number.RollCached();
	}
}
