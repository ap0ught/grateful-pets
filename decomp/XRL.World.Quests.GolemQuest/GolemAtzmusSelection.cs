using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.Rules;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public class GolemAtzmusSelection : GolemGameObjectSelection
{
	public const string BASE_ID = "Atzmus";

	public static readonly string[] Blocklist = new string[6] { "NightVision", "DarkVision", "Invisibility", "OldElectricalGeneration", "WallWalker", "Metamorphosis" };

	public override string ID => "Atzmus";

	public override string DisplayName => "atzmus";

	public override char Key => 'a';

	public override bool IsCarried => true;

	public override int Consumed => 1;

	public override bool IsValid(GameObject Object)
	{
		if (base.IsValid(Object))
		{
			if (!Object.HasStringProperty("LimbSourceGameObjectBlueprint"))
			{
				return Object.HasPart(typeof(DismemberedProperties));
			}
			return true;
		}
		return false;
	}

	public override void Apply(GameObject Object)
	{
		base.Apply(Object);
		GameObject sourceOf = GetSourceOf(Material);
		if (GameObject.Validate(sourceOf))
		{
			Object.SetStringProperty("AtzmusSourceBlueprint", sourceOf.Blueprint);
			Object.SetStringProperty("AtzmusSourceDisplayName", sourceOf.Render.DisplayName);
			if (!sourceOf.HasProperName)
			{
				Object.SetStringProperty("AtzmusSourceIndefiniteArticle", sourceOf.a);
			}
		}
		Mutations mutations = Object.RequirePart<Mutations>();
		string[] blocklist = Blocklist;
		foreach (string text in blocklist)
		{
			if (text != "DarkVision" && Object.GetPart(text) is BaseMutation mutation)
			{
				mutations.RemoveMutation(mutation);
			}
		}
	}

	private GameObject GetSourceOf(GameObject Object)
	{
		if (!GameObject.Validate(Object))
		{
			return null;
		}
		DismemberedProperties part = Object.GetPart<DismemberedProperties>();
		string text = part.SourceID ?? Object.GetStringProperty("LimbSourceGameObjectID");
		if (!text.IsNullOrEmpty())
		{
			GameObject gameObject = The.ZoneManager.FindObjectByID(text);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		string text2 = part.SourceBlueprint ?? Object.GetStringProperty("LimbSourceGameObjectBlueprint") ?? Object.GetPart<RandomFigurine>()?.Creature;
		if (!text2.IsNullOrEmpty() && GameObjectFactory.Factory.Blueprints.TryGetValue(text2, out var value))
		{
			Stat.PushState(text.Coalesce(Object.ID));
			try
			{
				return GameObjectFactory.Factory.CreateSampleObject(value);
			}
			finally
			{
				Stat.PopState();
			}
		}
		return Object;
	}

	public override IEnumerable<GameObjectUnit> YieldAllEffects()
	{
		foreach (MutationEntry item in MutationFactory.AllMutationEntries())
		{
			if (!item.IsDefect() && !Blocklist.Contains(item.Class))
			{
				yield return new GameObjectMutationUnit
				{
					Name = item.Name,
					Level = 5
				};
			}
		}
	}

	public override IEnumerable<GameObjectUnit> YieldRandomEffects()
	{
		List<MutationEntry> list = (from x in MutationFactory.AllMutationEntries()
			where !x.IsDefect()
			select x).ToList();
		yield return new GameObjectMutationUnit
		{
			Name = list.GetRandomElement().Name,
			Level = 5
		};
	}

	public override IEnumerable<GameObjectUnit> YieldEffectsOf(GameObject Object)
	{
		GameObject obj = GetSourceOf(Object);
		if (obj == null)
		{
			yield break;
		}
		List<BaseMutation> list = obj.GetPart<Mutations>()?.MutationList;
		if (!list.IsNullOrEmpty())
		{
			bool yielded = false;
			foreach (BaseMutation item in list)
			{
				if (!item.IsDefect() && !Blocklist.Contains(item.Name))
				{
					yielded = true;
					yield return new GameObjectMutationUnit
					{
						Name = item.GetDisplayName(),
						Class = item.Name,
						Level = Mathf.Max(item.BaseLevel, 1),
						ShouldShowLevel = item.ShouldShowLevel()
					};
				}
			}
			if (yielded)
			{
				yield break;
			}
		}
		List<string> list2 = new List<string> { "Strength" };
		int num = -1;
		foreach (string attribute in Statistic.Attributes)
		{
			int statValue = obj.GetStatValue(attribute);
			if (statValue > num)
			{
				list2.Clear();
				list2.Add(attribute);
				num = statValue;
			}
			else if (statValue == num)
			{
				list2.Add(attribute);
			}
		}
		yield return new GameObjectAttributeUnit
		{
			Attribute = ((list2.Count == 1) ? list2[0] : list2.GetRandomElement(Stat.GetSeededRandomGenerator(Object.ID))),
			Value = 5
		};
	}

	[WishCommand("golemquest:atzmus", null)]
	private static void Wish()
	{
		ReceiveMaterials(The.Player);
	}

	public static void ReceiveMaterials(GameObject Object)
	{
		GameObjectFactory factory = GameObjectFactory.Factory;
		Dictionary<string, (GameObjectBlueprint, int)> dictionary = new Dictionary<string, (GameObjectBlueprint, int)>();
		foreach (GameObjectBlueprint blueprint in factory.BlueprintList)
		{
			foreach (KeyValuePair<string, GamePartBlueprint> mutation in blueprint.Mutations)
			{
				if (blueprint.IsBaseBlueprint() || blueprint.HasTag("Golem") || blueprint.DescendsFrom("BaseNephal"))
				{
					break;
				}
				if (Array.IndexOf(Blocklist, mutation.Key) < 0)
				{
					if (!mutation.Value.TryGetParameter<int>("Level", out var Value))
					{
						Value = 1;
					}
					if (!dictionary.TryGetValue(mutation.Key, out var value) || Value > value.Item2 || (Value == value.Item2 && blueprint.Mutations.Count < value.Item1.Mutations.Count))
					{
						dictionary[mutation.Key] = (blueprint, Value);
					}
				}
			}
		}
		GameObject gameObject = null;
		foreach (KeyValuePair<string, (GameObjectBlueprint, int)> item in dictionary)
		{
			try
			{
				gameObject = factory.CreateObject(item.Value.Item1);
				BodyPart dismemberableBodyPart = Axe_Dismember.GetDismemberableBodyPart(gameObject);
				if (dismemberableBodyPart != null)
				{
					GameObject gameObject2 = gameObject.Body.Dismember(dismemberableBodyPart);
					if (gameObject2 != null)
					{
						Object.ReceiveObject(gameObject2);
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("golemquest:atzmus", x);
			}
			finally
			{
				gameObject?.Pool();
			}
		}
	}
}
