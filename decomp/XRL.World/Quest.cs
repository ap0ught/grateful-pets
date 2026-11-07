using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XRL.Messages;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class Quest
{
	public string ID;

	public string Name;

	public Type SystemType;

	public string Accomplishment;

	public string Achievement;

	public string BonusAtLevel;

	public int Level;

	public string Factions;

	public string Reputation;

	public string QuestGiverName;

	public string QuestGiverLocationName;

	public string QuestGiverLocationZoneID;

	public string Hagiograph;

	public string HagiographCategory;

	public string Gospel;

	public bool Finished;

	public DynamicQuestReward _dynamicReward;

	public Dictionary<string, object> Properties;

	public Dictionary<string, int> IntProperties;

	[NonSerialized]
	private IQuestSystem _System;

	public Dictionary<string, QuestStep> StepsByID;

	public QuestManager _Manager;

	public DynamicQuestReward dynamicReward
	{
		get
		{
			return _dynamicReward;
		}
		set
		{
			_dynamicReward = value;
			if (value == null)
			{
				return;
			}
			int count = StepsByID.Count;
			foreach (QuestStep value2 in StepsByID.Values)
			{
				value2.XP = value.StepXP / count;
			}
		}
	}

	public IQuestSystem System
	{
		get
		{
			if (_System == null && (object)SystemType != null)
			{
				_System = The.Game.GetSystem(SystemType) as IQuestSystem;
			}
			return _System;
		}
		set
		{
			_System = value;
			SystemType = value?.GetType();
			if (value != null)
			{
				value.Quest = this;
			}
		}
	}

	public QuestManager Manager
	{
		get
		{
			return _Manager;
		}
		set
		{
			_Manager = value;
			value.MyQuestID = ID;
		}
	}

	public string DisplayName => "{{W|" + Name + "}}";

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[Quest " + ID + "]");
		stringBuilder.AppendLine("Name=" + Name);
		stringBuilder.AppendLine("Accomplishment=" + Accomplishment);
		stringBuilder.AppendLine("Achievement=" + Achievement);
		stringBuilder.AppendLine("BonusAtLevel=" + BonusAtLevel);
		stringBuilder.AppendLine("Hagiograph=" + Hagiograph);
		stringBuilder.AppendLine("HagiographCategory=" + HagiographCategory);
		stringBuilder.AppendLine("Factions=" + Factions);
		stringBuilder.AppendLine("Reputation=" + Reputation);
		stringBuilder.AppendLine("Level=" + Level);
		stringBuilder.AppendLine("Finished=" + Finished);
		stringBuilder.AppendLine("System=" + SystemType?.FullName);
		stringBuilder.AppendLine("QuestGiverName=" + QuestGiverName);
		stringBuilder.AppendLine("QuestGiverLocationName=" + QuestGiverLocationName);
		stringBuilder.AppendLine("QuestGiverLocationZoneID=" + QuestGiverLocationZoneID);
		if (_dynamicReward == null)
		{
			stringBuilder.Append("DynamicReward=none");
		}
		else
		{
			stringBuilder.Append("DynamicReward=" + _dynamicReward.ToString());
		}
		stringBuilder.Append("nSteps=" + StepsByID.Count);
		foreach (KeyValuePair<string, QuestStep> item in StepsByID)
		{
			stringBuilder.Append(" step " + item.Key + " = " + item.Value.ToString());
		}
		return stringBuilder.ToString();
	}

	public GameObject GetInfluencer()
	{
		GameObject gameObject = System?.GetInfluencer();
		if (gameObject == null)
		{
			QuestManager manager = Manager;
			if (manager == null)
			{
				return null;
			}
			gameObject = manager.GetQuestInfluencer();
		}
		return gameObject;
	}

	public IQuestSystem InitializeSystem()
	{
		if ((object)SystemType == null)
		{
			return null;
		}
		return System = The.Game.RequireSystem(SystemType) as IQuestSystem;
	}

	public bool ReadyToTurnIn()
	{
		foreach (QuestStep value in StepsByID.Values)
		{
			if (!value.Finished)
			{
				return false;
			}
		}
		return true;
	}

	public void FinishPost()
	{
		if (dynamicReward != null)
		{
			dynamicReward.postaward();
		}
	}

	public void ShowStartPopup()
	{
		Popup.Show("You have received a new quest, " + DisplayName + "!");
	}

	public void ShowFailPopup()
	{
		Popup.Show("You have failed the quest " + DisplayName + "!");
	}

	public void ShowFailStepPopup(QuestStep Step)
	{
		Popup.Show("You have failed the step, {{R|" + Step.Name + "}}, of the quest " + DisplayName + "!");
	}

	public void ShowFinishPopup()
	{
		Popup.Show("You have completed the quest " + DisplayName + "!");
	}

	public void ShowFinishStepPopup(QuestStep Step)
	{
		string text = "You have finished the step, {{G|" + Step.Name + "}}, of the quest " + DisplayName + "!";
		if (Step.XP > 0)
		{
			Popup.ShowBlock(text + "\nYou gain {{C|" + Step.XP + "}} XP!", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			MessageQueue.AddPlayerMessage(text);
		}
		else
		{
			Popup.ShowBlock(text);
		}
	}

	public void Fail()
	{
		if (System != null)
		{
			_System.Fail();
			if (_System.RemoveWithQuest)
			{
				The.Game.FlagSystemForRemoval(_System);
			}
		}
	}

	public void FailStep(QuestStep Step)
	{
		System?.FailStep(Step);
	}

	public void Finish()
	{
		Finished = true;
		if (System != null)
		{
			_System.Finish();
			if (_System.RemoveWithQuest)
			{
				The.Game.FlagSystemForRemoval(_System);
			}
		}
		if (dynamicReward != null)
		{
			dynamicReward.award();
		}
	}

	public void FinishStep(QuestStep Step)
	{
		System?.FinishStep(Step);
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(ID);
		Writer.Write(Name);
		Writer.Write(Level);
		Writer.Write(Finished);
		Writer.Write(Accomplishment);
		Writer.Write(Achievement);
		Writer.Write(Hagiograph);
		Writer.Write(HagiographCategory);
		Writer.Write(Gospel);
		Writer.Write(BonusAtLevel);
		Writer.Write(Factions);
		Writer.Write(Reputation);
		Writer.Write(QuestGiverName);
		Writer.Write(QuestGiverLocationName);
		Writer.Write(QuestGiverLocationZoneID);
		Writer.WriteObject(_dynamicReward);
		if (StepsByID == null)
		{
			Writer.Write(-1);
		}
		else
		{
			Writer.Write(StepsByID.Count);
			foreach (KeyValuePair<string, QuestStep> item in StepsByID)
			{
				item.Value.Save(Writer);
			}
		}
		if (Manager == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(1);
			IPart.Save(Manager, Writer);
		}
		Writer.WriteTokenized(SystemType);
		Writer.Write(Properties);
		Writer.Write(IntProperties);
	}

	public static Quest Load(SerializationReader Reader)
	{
		Quest quest = new Quest();
		quest.ID = Reader.ReadString();
		quest.Name = Reader.ReadString();
		quest.Level = Reader.ReadInt32();
		quest.Finished = Reader.ReadBoolean();
		quest.Accomplishment = Reader.ReadString();
		quest.Achievement = Reader.ReadString();
		quest.Hagiograph = Reader.ReadString();
		quest.HagiographCategory = Reader.ReadString();
		quest.Gospel = Reader.ReadString();
		quest.BonusAtLevel = Reader.ReadString();
		quest.Factions = Reader.ReadString();
		quest.Reputation = Reader.ReadString();
		quest.QuestGiverName = Reader.ReadString();
		quest.QuestGiverLocationName = Reader.ReadString();
		quest.QuestGiverLocationZoneID = Reader.ReadString();
		quest._dynamicReward = Reader.ReadObject() as DynamicQuestReward;
		int num = Reader.ReadInt32();
		if (num >= 0)
		{
			quest.StepsByID = new Dictionary<string, QuestStep>(num);
			for (int i = 0; i < num; i++)
			{
				QuestStep questStep = new QuestStep(Reader);
				quest.StepsByID[questStep.ID] = questStep;
			}
		}
		if (Reader.ReadInt32() != 0)
		{
			quest.Manager = (QuestManager)IPart.Load(null, Reader);
		}
		quest.SystemType = Reader.ReadTokenizedType();
		quest.Properties = Reader.ReadDictionary<string, object>();
		quest.IntProperties = Reader.ReadDictionary<string, int>();
		return quest;
	}

	public Quest Copy()
	{
		Quest quest = new Quest();
		quest.ID = ID;
		quest.Name = Name;
		quest.Accomplishment = Accomplishment;
		quest.Achievement = Achievement;
		quest.Hagiograph = Hagiograph;
		quest.HagiographCategory = HagiographCategory;
		quest.Gospel = Gospel;
		quest.BonusAtLevel = BonusAtLevel;
		quest.Factions = Factions;
		quest.Reputation = Reputation;
		quest.Level = Level;
		quest.Finished = Finished;
		quest._Manager = _Manager;
		quest.SystemType = SystemType;
		quest._dynamicReward = _dynamicReward;
		quest.QuestGiverName = QuestGiverName;
		quest.QuestGiverLocationName = QuestGiverLocationName;
		quest.QuestGiverLocationZoneID = QuestGiverLocationZoneID;
		quest.StepsByID = new Dictionary<string, QuestStep>(StepsByID.Count);
		foreach (KeyValuePair<string, QuestStep> item in StepsByID)
		{
			QuestStep value = item.Value;
			if (!value.Base)
			{
				quest.StepsByID.Add(item.Key, new QuestStep
				{
					Finished = false,
					ID = value.ID,
					Name = value.Name,
					Text = value.Text,
					Value = value.Value,
					XP = value.XP,
					Ordinal = value.Ordinal,
					Flags = value.Flags
				});
			}
		}
		if (!Properties.IsNullOrEmpty())
		{
			quest.Properties = new Dictionary<string, object>(Properties);
			foreach (KeyValuePair<string, object> property in Properties)
			{
				if (property.Value is ICollection collection)
				{
					Type type = collection.GetType();
					quest.Properties[property.Key] = Activator.CreateInstance(type, collection);
				}
			}
		}
		if (!IntProperties.IsNullOrEmpty())
		{
			quest.IntProperties = new Dictionary<string, int>(IntProperties);
		}
		return quest;
	}

	public string GetProperty(string Key, string Default = null)
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value as string;
	}

	public int GetProperty(string Key, int Default)
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!IntProperties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value;
	}

	public float GetProperty(string Key, float Default)
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!IntProperties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return BitConverter.Int32BitsToSingle(value);
	}

	public T GetProperty<T>(string Key, T Default = null) where T : class
	{
		if (Key == null || Properties == null)
		{
			return Default;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value as T;
	}

	public List<string> GetList(string Key)
	{
		if (Key == null || Properties == null)
		{
			return null;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return null;
		}
		return value as List<string>;
	}

	public List<T> GetList<T>(string Key)
	{
		if (Key == null || Properties == null)
		{
			return null;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return null;
		}
		return value as List<T>;
	}

	public Dictionary<string, string> GetDictionary(string Key)
	{
		if (Key == null || Properties == null)
		{
			return null;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return null;
		}
		return value as Dictionary<string, string>;
	}

	public Dictionary<string, T> GetDictionary<T>(string Key)
	{
		if (Key == null || Properties == null)
		{
			return null;
		}
		if (!Properties.TryGetValue(Key, out var value))
		{
			return null;
		}
		return value as Dictionary<string, T>;
	}

	public void SetProperty(string Key, object Value)
	{
		if (Properties == null)
		{
			Properties = new Dictionary<string, object>(3);
		}
		Properties[Key] = Value;
	}

	public void SetProperty(string Key, int Value)
	{
		if (IntProperties == null)
		{
			IntProperties = new Dictionary<string, int>(3);
		}
		IntProperties[Key] = Value;
	}

	public void SetProperty(string Key, float Value)
	{
		if (IntProperties == null)
		{
			IntProperties = new Dictionary<string, int>(3);
		}
		IntProperties[Key] = BitConverter.SingleToInt32Bits(Value);
	}

	public bool HasProperty(string Key)
	{
		if (Properties == null || !Properties.ContainsKey(Key))
		{
			if (IntProperties != null)
			{
				return IntProperties.ContainsKey(Key);
			}
			return false;
		}
		return true;
	}

	public bool RemoveProperty(string Key)
	{
		if (Properties != null)
		{
			return Properties.Remove(Key);
		}
		return false;
	}

	public bool RemoveNumericProperty(string Key)
	{
		if (IntProperties != null)
		{
			return IntProperties.Remove(Key);
		}
		return false;
	}

	public bool IsStepFinished(string StepID)
	{
		if (StepsByID.TryGetValue(StepID, out var value))
		{
			return value.Finished;
		}
		return false;
	}

	public static string Consider(Quest Q)
	{
		int num = The.Player.Statistics["Level"].Value - Q.Level;
		if (num <= -15)
		{
			return "[{{R|Impossible}}]";
		}
		if (num <= -10)
		{
			return "[{{r|Very Tough}}]";
		}
		if (num <= -5)
		{
			return "[{{W|Tough}}]";
		}
		if (num < 5)
		{
			return "[{{w|Average}}]";
		}
		if (num <= 10)
		{
			return "[{{g|Easy}}]";
		}
		return "[{{G|Trivial}}]";
	}
}
