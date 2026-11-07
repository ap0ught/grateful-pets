using System;
using System.Collections.Generic;

namespace XRL;

public class PopulationTable : PopulationItem
{
	public string Number;

	public string Chance;

	public override string ToString()
	{
		return "<Table>";
	}

	public override void MergeFrom(PopulationItem Item, PopulationInfo Info)
	{
		base.MergeFrom(Item, Info);
		PopulationTable populationTable = (PopulationTable)Item;
		if (populationTable.Number != null)
		{
			Number = populationTable.Number;
		}
		if (populationTable.Chance != null)
		{
			Chance = populationTable.Chance;
		}
	}

	public override void GetEachUniqueObject(List<string> List)
	{
		if (PopulationManager.Populations.ContainsKey(Name))
		{
			PopulationManager.Populations[Name].GetEachUniqueObject(List);
		}
	}

	public override void GenerateStructured(PopulationStructuredResult Result, Dictionary<string, string> Vars = null)
	{
		Generate(Result.Objects, Vars);
	}

	public override void Generate(List<PopulationResult> Result, Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		if (!PopulationManager.TryResolvePopulation(Name, Vars, out var Population))
		{
			return;
		}
		int i = 0;
		for (int num = RollChance(Chance); i < num; i++)
		{
			int j = 0;
			for (int num2 = RollNumber(Number); j < num2; j++)
			{
				Population.Generate(Result, Vars, Hint ?? DefaultHint);
			}
		}
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (!PopulationManager.TryResolvePopulation(Name, Vars, out var Population))
		{
			return null;
		}
		int i = 0;
		for (int num = RollChance(Chance); i < num; i++)
		{
			int j = 0;
			for (int num2 = RollNumber(Number); j < num2; j++)
			{
				PopulationResult populationResult = Population.GenerateOne(Vars, Hint ?? DefaultHint);
				if (populationResult != null)
				{
					return populationResult;
				}
			}
		}
		return null;
	}

	public double Probability()
	{
		return CalculateProbability(Chance);
	}

	public double AverageNumber()
	{
		if (!Number.IsNullOrEmpty())
		{
			return Number.GetCachedDieRoll().Average();
		}
		return 1.0;
	}

	public override PopulationItem Find(Predicate<PopulationItem> Predicate)
	{
		if (!Predicate(this))
		{
			return null;
		}
		return this;
	}
}
