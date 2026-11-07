using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.Anatomy;

[Serializable]
public class BodyPartPositionHint
{
	public BodyPart Self;

	public BodyPart Parent;

	public BodyPart Previous;

	public BodyPart Next;

	public BodyPartPositionHint()
	{
	}

	public BodyPartPositionHint(BodyPart Part)
		: this()
	{
		Self = Part.Clone();
		Parent = Part.GetParentPart()?.Clone();
		Previous = Part.GetPreviousPart()?.Clone();
		Next = Part.GetNextPart()?.Clone();
	}

	public List<BodyPartPositionSpec> GetRankedSpecs(List<BodyPart> List)
	{
		if (List == null || List.Count == 0)
		{
			return null;
		}
		List<BodyPartPositionSpec> list = new List<BodyPartPositionSpec>();
		foreach (BodyPart item in List)
		{
			int similarityScore = GetSimilarityScore(Parent, item);
			if (similarityScore <= 0)
			{
				continue;
			}
			if (item.Parts == null || item.Parts.Count == 0)
			{
				list.Add(new BodyPartPositionSpec(item, -1, similarityScore));
				continue;
			}
			int i = 0;
			for (int count = item.Parts.Count; i <= count; i++)
			{
				BodyPart bodyPart = ((i > 0) ? item.Parts[i - 1] : null);
				BodyPart bodyPart2 = ((i < count) ? item.Parts[i] : null);
				int position = bodyPart2?.Position ?? (-1);
				int num = 0;
				if (bodyPart != null && bodyPart2 != null)
				{
					num = (similarityScore + GetSimilarityScore(bodyPart, Previous) + GetSimilarityScore(bodyPart2, Next)) / 3;
				}
				else if (bodyPart != null)
				{
					num = (similarityScore + GetSimilarityScore(bodyPart, Previous)) / 2;
				}
				else if (bodyPart2 != null)
				{
					num = (similarityScore + GetSimilarityScore(bodyPart2, Next)) / 2;
				}
				if (num > 0)
				{
					list.Add(new BodyPartPositionSpec(item, position, num));
				}
			}
		}
		if (list.Count > 1)
		{
			list.Sort((BodyPartPositionSpec a, BodyPartPositionSpec b) => b.Score.CompareTo(a.Score));
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list;
	}

	public List<BodyPartPositionSpec> GetRankedSpecs(Body Body)
	{
		return GetRankedSpecs(Body?.GetParts());
	}

	public List<BodyPartPositionSpec> GetRankedSpecs(GameObject Object)
	{
		return GetRankedSpecs(Object?.Body);
	}

	public void GetBestParentAndPosition(List<BodyPart> List, out BodyPart Parent, out int Position)
	{
		List<BodyPartPositionSpec> rankedSpecs = GetRankedSpecs(List);
		if (rankedSpecs != null && rankedSpecs.Count > 0)
		{
			Parent = rankedSpecs[0].Parent;
			Position = rankedSpecs[0].Position;
		}
		else
		{
			Parent = null;
			Position = -1;
		}
	}

	public void GetBestParentAndPosition(Body Body, out BodyPart Parent, out int Position)
	{
		GetBestParentAndPosition(Body?.GetParts(), out Parent, out Position);
	}

	public void GetBestParentAndPosition(GameObject Object, out BodyPart Parent, out int Position)
	{
		GetBestParentAndPosition(Object?.Body, out Parent, out Position);
	}

	private static int GetSimilarityScore(BodyPart Part1, BodyPart Part2)
	{
		int num = 0;
		if (Part1 == null)
		{
			num = ((Part2 == null) ? (num + 1) : (num - 10));
		}
		else if (Part2 != null)
		{
			num++;
			if ((Part1.Parts?.Count ?? 0) == (Part2.Parts?.Count ?? 0) - 1)
			{
				num++;
			}
			if (Part1.VariantType == Part2.VariantType)
			{
				num += 20;
			}
			else if (Part1.Type == Part2.Type)
			{
				num += 10;
			}
			if (Part1.Name == Part2.Name)
			{
				num += 10;
			}
			if (Part1.Description == Part2.Description)
			{
				num += 10;
			}
			if (Part1.DefaultBehaviorBlueprint == Part2.DefaultBehaviorBlueprint)
			{
				num += 5;
			}
			num += Math.Max(10 - Math.Abs(Part1.Position - Part2.Position), 0);
			if (Part1.SupportsDependent != Part2.SupportsDependent)
			{
				num -= 5;
			}
			if (Part1.DependsOn != Part2.DependsOn)
			{
				num -= 5;
			}
			if (Part1.RequiresType != Part2.RequiresType)
			{
				num -= 5;
			}
			if (Part1.Category != Part2.Category)
			{
				num -= 2;
			}
			if (Part1.Laterality != Part2.Laterality)
			{
				num--;
			}
			if (Part1.Mobility != Part2.Mobility)
			{
				num--;
			}
			if (Part1.Native != Part2.Native)
			{
				num--;
			}
			if (Part1.Appendage != Part2.Appendage)
			{
				num -= 5;
			}
			if (Part1.Integral != Part2.Integral)
			{
				num -= 5;
			}
			if (Part1.Mortal != Part2.Mortal)
			{
				num -= 5;
			}
			if (Part1.Extrinsic != Part2.Extrinsic)
			{
				num -= 5;
			}
			if (Part1.Dynamic != Part2.Dynamic)
			{
				num -= 5;
			}
			if (Part1.Contact != Part2.Contact)
			{
				num -= 5;
			}
			if (Part1.IgnorePosition != Part2.IgnorePosition)
			{
				num -= 5;
			}
		}
		else
		{
			num -= 10;
		}
		return num;
	}
}
