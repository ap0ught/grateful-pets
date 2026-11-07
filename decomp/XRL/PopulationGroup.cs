using System.Collections.Generic;

namespace XRL;

public class PopulationGroup : PopulationList
{
	public PopulationList Parent;

	public string Number;

	public string Chance;

	public PopulationInfo Info
	{
		get
		{
			for (PopulationList populationList = Parent; populationList != null; populationList = (populationList as PopulationGroup)?.Parent)
			{
				if (populationList is PopulationInfo result)
				{
					return result;
				}
			}
			return null;
		}
	}

	public override string ToString()
	{
		string text = "";
		if (!Name.IsNullOrEmpty())
		{
			text = text + Name + ", ";
		}
		text = text + Style + " of [";
		foreach (PopulationItem item in Items)
		{
			text = text + item.ToString() + ",";
		}
		return text + "]";
	}

	public override void MergeFrom(PopulationItem Item, PopulationInfo Info)
	{
		PopulationGroup populationGroup = (PopulationGroup)Item;
		if (populationGroup.Number != null)
		{
			Number = populationGroup.Number;
		}
		if (populationGroup.Chance != null)
		{
			Chance = populationGroup.Chance;
		}
		base.MergeFrom(Item, Info);
	}

	public override void GetEachUniqueObject(List<string> Ret)
	{
		foreach (PopulationItem item in Items)
		{
			item.GetEachUniqueObject(Ret);
		}
	}

	public override void GenerateStructured(PopulationStructuredResult Result, Dictionary<string, string> Vars = null)
	{
		PopulationStructuredResult populationStructuredResult = new PopulationStructuredResult();
		populationStructuredResult.Hint = Hint;
		Result.ChildGroups.Add(populationStructuredResult);
		int i = 0;
		for (int num = RollChance(Chance); i < num; i++)
		{
			int j = 0;
			for (int num2 = RollNumber(Number); j < num2; j++)
			{
				base.GenerateStructured(populationStructuredResult, Vars);
			}
		}
	}

	public override void Generate(List<PopulationResult> Result, Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		int i = 0;
		for (int num = RollChance(Chance); i < num; i++)
		{
			int j = 0;
			for (int num2 = RollNumber(Number); j < num2; j++)
			{
				base.Generate(Result, Vars, DefaultHint);
			}
		}
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		int i = 0;
		for (int num = RollChance(Chance); i < num; i++)
		{
			int j = 0;
			for (int num2 = RollNumber(Number); j < num2; j++)
			{
				PopulationResult populationResult = base.GenerateOne(Vars, DefaultHint);
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
}
