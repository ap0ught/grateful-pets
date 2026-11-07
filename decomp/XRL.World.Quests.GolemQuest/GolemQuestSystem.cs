using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
public class GolemQuestSystem : IQuestSystem
{
	public const string QUEST_ID = "The Golem";

	[NonSerialized]
	public Dictionary<string, GolemQuestSelection> Selections = new Dictionary<string, GolemQuestSelection>
	{
		{
			"Body",
			new GolemBodySelection()
		},
		{
			"Catalyst",
			new GolemCatalystSelection()
		},
		{
			"Atzmus",
			new GolemAtzmusSelection()
		},
		{
			"Armament",
			new GolemArmamentSelection()
		},
		{
			"Incantation",
			new GolemIncantationSelection()
		},
		{
			"Hamsa",
			new GolemHamsaSelection()
		},
		{
			"Power",
			new GolemAscensionSelection()
		}
	};

	[NonSerialized]
	public GolemQuestMound Mound;

	[NonSerialized]
	private List<GameObject> _ValidHolders = new List<GameObject>(8);

	public GolemBodySelection Body => Selections.GetValue("Body") as GolemBodySelection;

	public GolemCatalystSelection Catalyst => Selections.GetValue("Catalyst") as GolemCatalystSelection;

	public GolemAtzmusSelection Atzmus => Selections.GetValue("Atzmus") as GolemAtzmusSelection;

	public GolemArmamentSelection Armament => Selections.GetValue("Armament") as GolemArmamentSelection;

	public static GolemQuestSystem Get()
	{
		return The.Game.GetSystem<GolemQuestSystem>();
	}

	public static GolemQuestSystem Require()
	{
		return The.Game.RequireSystem(() => new GolemQuestSystem());
	}

	public override void Start()
	{
		UpdateQuest();
	}

	public override void Write(SerializationWriter Writer)
	{
		Writer.Write(Selections.Count);
		foreach (KeyValuePair<string, GolemQuestSelection> selection in Selections)
		{
			Writer.Write(selection.Key);
			Writer.Write(selection.Value.GetType().FullName);
			selection.Value.Save(Writer);
		}
	}

	public override void Read(SerializationReader Reader)
	{
		GolemQuestSystem golemQuestSystem = null;
		if (Selections == null)
		{
			Selections = new Dictionary<string, GolemQuestSelection>();
		}
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			Type type = ModManager.ResolveType(Reader.ReadString());
			try
			{
				if (type != null && Activator.CreateInstance(type) is GolemQuestSelection golemQuestSelection)
				{
					golemQuestSelection.Load(Reader);
					Selections[key] = golemQuestSelection;
					continue;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Error deserializing golem selection", x);
			}
			if (golemQuestSystem == null)
			{
				golemQuestSystem = new GolemQuestSystem();
			}
			if (golemQuestSystem.Selections.TryGetValue(key, out var value))
			{
				Selections[key] = value;
			}
		}
	}

	public static bool IsValidHolder(GameObject Object)
	{
		if (Object?.Inventory == null || Object.Physics == null || !Object.Physics.IsReal)
		{
			return false;
		}
		if (Object.Brain != null)
		{
			return Object.IsPlayerControlled();
		}
		if (Object.HasPart(typeof(Container)))
		{
			return Object.Physics.Owner.IsNullOrEmpty();
		}
		return false;
	}

	public List<GameObject> GetValidHolders()
	{
		_ValidHolders.Clear();
		Cell cell = Mound?.ParentObject?.CurrentCell;
		if (cell != null)
		{
			Predicate<GameObject> filter = IsValidHolder;
			foreach (Cell item in cell.IterateAdjacent(2))
			{
				GameObject firstObject = item.GetFirstObject(filter);
				if (firstObject != null)
				{
					_ValidHolders.Add(firstObject);
				}
			}
		}
		else
		{
			_ValidHolders.Add(The.Player);
		}
		return _ValidHolders;
	}

	public bool AllValid()
	{
		foreach (KeyValuePair<string, GolemQuestSelection> selection in Selections)
		{
			if (!selection.Value.IsValid())
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateQuest()
	{
		if (base.Quest == null && The.Game.TryGetQuest("The Golem", out var Quest))
		{
			base.Quest = Quest;
		}
		if (base.Quest == null || base.Quest.Finished)
		{
			return;
		}
		StringBuilder stringBuilder = null;
		Quest value = QuestLoader.Loader.QuestsByID.GetValue("The Golem");
		foreach (KeyValuePair<string, QuestStep> item in base.Quest.StepsByID)
		{
			if (!Selections.TryGetValue(item.Key, out var value2))
			{
				continue;
			}
			if (value2.IsValid())
			{
				if (stringBuilder == null)
				{
					stringBuilder = Event.NewStringBuilder();
				}
				stringBuilder.Clear().Append(value2.GetOptionChoice());
				value2.AppendEffects(stringBuilder);
				item.Value.Text = stringBuilder.ToString();
				item.Value.Collapse = false;
				The.Game.FinishQuestStep("The Golem", item.Key, -1, CanFinishQuest: false);
			}
			else if (item.Value.Finished)
			{
				item.Value.XP = 0;
				item.Value.Finished = false;
				item.Value.Text = value.StepsByID.GetValue(item.Key).Text;
			}
		}
	}
}
