using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace Qud.API;

public static class MutationsAPI
{
	public static bool IsNewMutationValidFor(XRL.World.GameObject go, MutationEntry entry, Predicate<MutationEntry> filter = null)
	{
		if (go == null)
		{
			return false;
		}
		if (entry == null)
		{
			return false;
		}
		if ((go.HasTagOrProperty("Esper") || go.GetTagOrStringProperty("MutationLevel") == "Esper") && entry.Mutation != null && !string.IsNullOrEmpty(entry.Mutation.GetMutationType()) && !entry.Mutation.GetMutationType().Contains("Mental"))
		{
			return false;
		}
		if ((go.HasTagOrProperty("Chimera") || go.GetTagOrStringProperty("MutationLevel") == "Chimera") && entry.Mutation != null && !string.IsNullOrEmpty(entry.Mutation.GetMutationType()) && !entry.Mutation.GetMutationType().Contains("Physical"))
		{
			return false;
		}
		if (go.HasPart(entry.Class))
		{
			return false;
		}
		string[] exclusions = entry.GetExclusions();
		foreach (string text in exclusions)
		{
			if (!string.IsNullOrEmpty(text) && go.HasPart(text))
			{
				return false;
			}
		}
		if (filter != null && !filter(entry))
		{
			return false;
		}
		return true;
	}

	public static MutationEntry FindRandomMutationFor(XRL.World.GameObject go, Predicate<MutationEntry> filter = null, System.Random rng = null, bool allowMultipleDefects = false)
	{
		List<MutationEntry> list = new List<MutationEntry>(go.GetPart<Mutations>().GetMutatePool(filter, allowMultipleDefects));
		if (list.Count <= 0)
		{
			return null;
		}
		return list.GetRandomElement(rng);
	}

	public static bool ApplyMutationTo(XRL.World.GameObject go, MutationEntry entry)
	{
		Mutations part = go.GetPart<Mutations>();
		if (part == null)
		{
			return false;
		}
		part.AddMutation(entry.CreateInstance());
		return true;
	}

	public static MutationEntry RandomlyMutate(XRL.World.GameObject go, Predicate<MutationEntry> filter = null, System.Random rng = null, bool allowMultipleDefects = false, StringBuilder result = null)
	{
		MutationEntry mutationEntry = FindRandomMutationFor(go, filter, rng, allowMultipleDefects);
		if (mutationEntry == null)
		{
			return null;
		}
		ApplyMutationTo(go, mutationEntry);
		if (result != null && go.IsPlayer())
		{
			result.Append(" ");
			result.Append(" You gain " + mutationEntry.GetDisplayName() + "!");
		}
		if (go.IsChimera() && Stat.Random(1, 3) <= 1)
		{
			go.GetPart<Mutations>()?.AddChimericBodyPart();
		}
		return mutationEntry;
	}

	public static bool BuyRandomMutation(XRL.World.GameObject Object, int Cost = 4, bool Confirm = true, string MutationTerm = null)
	{
		try
		{
			if (MutationTerm == null)
			{
				MutationTerm = GetMutationTermEvent.GetFor(Object);
			}
			if (!StatusScreen.TreatAsMutant(Object))
			{
				return Object.ShowFailure("Your type of creature may not buy " + Grammar.Pluralize(MutationTerm) + ".");
			}
			if (Object.Stat("MP") < Cost)
			{
				return Object.ShowFailure("You don't have " + Cost + " mutation points!");
			}
			if (!Confirm || !Object.IsPlayer() || Popup.ShowYesNo("Are you sure you want to spend " + Cost + " mutation points to buy a new " + MutationTerm + "?") == DialogResult.Yes)
			{
				bool flag = false;
				if ((!Object.HasPart<IrritableGenome>()) ? StatusScreen.BuyRandomMutation(Object) : (RandomlyMutate(Object) != null))
				{
					Object.UseMP(Cost, "BuyNew");
					return true;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("BuyRandomMutation", x);
		}
		return false;
	}

	public static bool AddNewMentalMutation(List<BaseMutation> pMutations, int MinCost = -1, int MaxCost = 999)
	{
		List<MutationEntry> mutationsOfCategory = MutationFactory.GetMutationsOfCategory("Mental");
		mutationsOfCategory.ShuffleInPlace();
		foreach (MutationEntry item in mutationsOfCategory)
		{
			if (MutationOk(pMutations, item.CreateInstance(), MinCost, MaxCost))
			{
				pMutations.Add(item.CreateInstance());
				return true;
			}
		}
		return false;
	}

	public static bool AddNewPhysicalMutation(List<BaseMutation> pMutations, int MinCost = -1, int MaxCost = 999)
	{
		List<MutationEntry> mutationsOfCategory = MutationFactory.GetMutationsOfCategory("Physical");
		mutationsOfCategory.ShuffleInPlace();
		foreach (MutationEntry item in mutationsOfCategory)
		{
			if (MutationOk(pMutations, item.CreateInstance(), MinCost, MaxCost))
			{
				pMutations.Add(item.CreateInstance());
				return true;
			}
		}
		return false;
	}

	public static bool AddNewDefectMutation(List<BaseMutation> pMutations)
	{
		List<MutationEntry> mutationsOfCategory = MutationFactory.GetMutationsOfCategory("MentalDefects,PhysicalDefects");
		mutationsOfCategory.ShuffleInPlace();
		foreach (MutationEntry item in mutationsOfCategory)
		{
			if (MutationOk(pMutations, item.CreateInstance()))
			{
				pMutations.Add(item.CreateInstance());
				return true;
			}
		}
		return false;
	}

	public static bool MutationOk(List<BaseMutation> Current, BaseMutation New, int MinCost = -1, int MaxCost = 999)
	{
		if (New.GetMutationEntry() == null)
		{
			return true;
		}
		try
		{
			foreach (BaseMutation item in Current)
			{
				if (item.GetMutationEntry().Name == New.GetMutationEntry().Name)
				{
					return false;
				}
			}
			if (New.GetMutationEntry().Cost < MinCost || New.GetMutationEntry().Cost > MaxCost)
			{
				return false;
			}
			string[] exclusions = New.GetMutationEntry().GetExclusions();
			foreach (string text in exclusions)
			{
				if (!string.IsNullOrEmpty(text) && MutationInList(Current, text))
				{
					return false;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error with mutation " + New.Name + " cost: " + ex.ToString());
		}
		return true;
	}

	public static bool MutationInList(List<BaseMutation> List, string Name)
	{
		foreach (BaseMutation item in List)
		{
			if (item.GetMutationEntry().Name == Name)
			{
				return true;
			}
		}
		return false;
	}
}
