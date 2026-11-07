using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRL;

public class PopulationObject : PopulationItem
{
	public string Number;

	public string Chance;

	public string Builder;

	public string Blueprint
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	public PopulationObject()
	{
	}

	public PopulationObject(string blueprint, string number, uint weight, string builder)
		: this()
	{
		Blueprint = blueprint;
		Number = number;
		Weight = weight;
		Builder = builder;
	}

	public override string ToString()
	{
		return Blueprint + "[wt=" + Weight + "]";
	}

	public override void GetEachUniqueObject(List<string> List)
	{
		if (!List.CleanContains(Blueprint))
		{
			List.Add(Blueprint);
		}
	}

	public override void GenerateStructured(PopulationStructuredResult Result, Dictionary<string, string> Vars = null)
	{
		Generate(Result.Objects, Vars);
	}

	public override void Generate(List<PopulationResult> Result, Dictionary<string, string> Vars = null, string DefaultHint = null)
	{
		if (this.Blueprint == null)
		{
			Debug.LogError("NULL blueprint");
			return;
		}
		string Blueprint = this.Blueprint;
		if (!Vars.IsNullOrEmpty())
		{
			PopulationManager.ReplaceVariables(ref Blueprint, Vars);
		}
		if (Blueprint.StartsWith("$CALLBLUEPRINTMETHOD"))
		{
			Blueprint = PopulationManager.resolveCallBlueprintSlug(Blueprint);
		}
		int i = 0;
		for (int num = RollChance(Chance); i < num; i++)
		{
			Result.Add(new PopulationResult(Blueprint, RollNumber(Number), Hint ?? DefaultHint, Builder));
		}
	}

	public override PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint)
	{
		if (this.Blueprint == null)
		{
			Debug.LogError("NULL blueprint");
			return null;
		}
		string Blueprint = this.Blueprint;
		if (!Vars.IsNullOrEmpty())
		{
			PopulationManager.ReplaceVariables(ref Blueprint, Vars);
		}
		if (Blueprint.StartsWith("$CALLBLUEPRINTMETHOD"))
		{
			Blueprint = PopulationManager.resolveCallBlueprintSlug(Blueprint);
		}
		if (RollChance(Chance) > 0)
		{
			return new PopulationResult(Blueprint, RollNumber(Number), Hint ?? DefaultHint, Builder);
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

	public override void MergeFrom(PopulationItem Item, PopulationInfo Info)
	{
		base.MergeFrom(Item, Info);
		PopulationObject populationObject = (PopulationObject)Item;
		if (populationObject.Number != null)
		{
			Number = populationObject.Number;
		}
		if (populationObject.Chance != null)
		{
			Chance = populationObject.Chance;
		}
		if (populationObject.Builder != null)
		{
			Builder = populationObject.Builder;
		}
	}
}
