using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

[Serializable]
[HasModSensitiveStaticCache]
public class QuestLoader
{
	public class XMLParsing
	{
		public Dictionary<string, Action<XmlDataHelper>> rootNodes;

		public Dictionary<string, Action<XmlDataHelper>> questsNodes;

		public Dictionary<string, Action<XmlDataHelper>> questNodes;

		public Dictionary<string, Action<XmlDataHelper>> stepNodes;

		public Dictionary<string, Action<XmlDataHelper>> propertyNodes;

		public Quest currentQuest;

		public QuestStep currentQuestStep;

		private string TextValue;

		private object ComplexValue;

		private byte ComplexType;

		public Dictionary<string, Quest> QuestsByID => Loader.QuestsByID;

		public XMLParsing()
		{
			rootNodes = new Dictionary<string, Action<XmlDataHelper>> { { "quests", HandleQuests } };
			questsNodes = new Dictionary<string, Action<XmlDataHelper>> { { "quest", HandleQuest } };
			questNodes = new Dictionary<string, Action<XmlDataHelper>>
			{
				{ "step", HandleStep },
				{ "property", HandleProperty },
				{ "intproperty", HandleIntProperty },
				{ "floatproperty", HandleFloatProperty }
			};
			stepNodes = new Dictionary<string, Action<XmlDataHelper>> { { "text", HandleStepText } };
			propertyNodes = new Dictionary<string, Action<XmlDataHelper>>
			{
				{ "text", HandlePropertyText },
				{ "entry", HandlePropertyEntry },
				{ "value", HandlePropertyValue }
			};
		}

		public void HandleQuests(XmlDataHelper xml)
		{
			xml.HandleNodes(questsNodes);
		}

		public void HandleQuest(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute<string>("ID", null);
			if (text == null)
			{
				text = xml.ParseAttribute<string>("Name", null, required: true);
			}
			if (text != null)
			{
				if (!QuestsByID.TryGetValue(text, out currentQuest))
				{
					currentQuest = new Quest
					{
						ID = text
					};
				}
			}
			else
			{
				currentQuest = new Quest
				{
					ID = ""
				};
			}
			currentQuest.Name = xml.ParseAttribute("Name", currentQuest.Name);
			currentQuest.Level = xml.ParseAttribute("Level", currentQuest.Level);
			currentQuest.Achievement = xml.ParseAttribute("Achievement", currentQuest.Achievement);
			currentQuest.Accomplishment = xml.ParseAttribute("Accomplishment", currentQuest.Accomplishment);
			currentQuest.Hagiograph = xml.ParseAttribute("Hagiograph", currentQuest.Hagiograph);
			currentQuest.HagiographCategory = xml.ParseAttribute("HagiographCategory", currentQuest.HagiographCategory);
			currentQuest.Gospel = xml.ParseAttribute("Gospel", currentQuest.Gospel);
			currentQuest.BonusAtLevel = xml.ParseAttribute("BonusAtLevel", currentQuest.BonusAtLevel);
			currentQuest.Factions = xml.ParseAttribute("Factions", currentQuest.Factions);
			currentQuest.Reputation = xml.ParseAttribute("Reputation", currentQuest.Reputation);
			currentQuest.QuestGiverName = xml.ParseAttribute<string>("QuestGiverName", null);
			currentQuest.QuestGiverLocationName = xml.ParseAttribute<string>("QuestGiverLocationName", null);
			currentQuest.QuestGiverLocationZoneID = xml.ParseAttribute<string>("QuestGiverLocationZoneID", null);
			Type type = xml.ParseType("System", "XRL.World.Quests");
			if ((object)type != null)
			{
				currentQuest.SystemType = type;
			}
			Type type2 = xml.ParseType("Manager", "XRL.World.QuestManagers");
			if ((object)type2 != null)
			{
				currentQuest.Manager = ModManager.CreateInstance<QuestManager>(type2);
			}
			currentQuest.StepsByID = new Dictionary<string, QuestStep>();
			xml.HandleNodes(questNodes);
			QuestsByID[currentQuest.ID] = currentQuest;
			currentQuest = null;
		}

		public void HandleStep(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute<string>("ID", null);
			if (text == null)
			{
				text = xml.ParseAttribute<string>("Name", null, required: true);
			}
			if (!currentQuest.StepsByID.TryGetValue(text, out currentQuestStep))
			{
				currentQuestStep = new QuestStep();
			}
			currentQuestStep.ID = xml.ParseAttribute("ID", text);
			currentQuestStep.Name = xml.ParseAttribute("Name", currentQuestStep.Name);
			currentQuestStep.Value = xml.ParseAttribute("Value", currentQuestStep.Value);
			currentQuestStep.XP = xml.ParseAttribute("XP", currentQuestStep.XP);
			currentQuestStep.Optional = xml.ParseAttribute("Optional", currentQuestStep.Optional);
			currentQuestStep.Ordinal = xml.ParseAttribute("Ordinal", currentQuest.StepsByID.Count);
			currentQuestStep.Collapse = xml.ParseAttribute("Collapse", currentQuestStep.Collapse);
			currentQuestStep.Awarded = xml.ParseAttribute("Awarded", currentQuestStep.Awarded);
			currentQuestStep.Failed = xml.ParseAttribute("Failed", currentQuestStep.Failed);
			currentQuestStep.Hidden = xml.ParseAttribute("Hidden", currentQuestStep.Hidden);
			currentQuestStep.Base = xml.ParseAttribute("Base", currentQuestStep.Base);
			xml.HandleNodes(stepNodes);
			currentQuest.StepsByID[currentQuestStep.ID] = currentQuestStep;
			currentQuestStep = null;
		}

		public void HandleStepText(XmlDataHelper xml)
		{
			currentQuestStep.Text = xml.GetTextNode();
		}

		public void SetComplexType(byte Type)
		{
			TextValue = null;
			ComplexValue = null;
			ComplexType = Type;
		}

		public void HandleProperty(XmlDataHelper xml)
		{
			SetComplexType(0);
			string key = xml.ParseAttribute("Name", "");
			string text = xml.ParseAttribute("Value", "");
			xml.HandleNodes(propertyNodes);
			currentQuest.SetProperty(key, ComplexValue ?? TextValue ?? text);
		}

		public void HandleIntProperty(XmlDataHelper xml)
		{
			SetComplexType(1);
			string key = xml.ParseAttribute("Name", "");
			int value = xml.ParseAttribute("Value", 0);
			xml.HandleNodes(propertyNodes);
			int Value;
			if (ComplexValue != null)
			{
				currentQuest.SetProperty(key, ComplexValue);
			}
			else if (TextValue != null && xml.TryParse<int>(TextValue, out Value))
			{
				currentQuest.SetProperty(key, Value);
			}
			else
			{
				currentQuest.SetProperty(key, value);
			}
		}

		public void HandleFloatProperty(XmlDataHelper xml)
		{
			SetComplexType(2);
			string key = xml.ParseAttribute("Name", "");
			float value = xml.ParseAttribute("Value", 0f);
			xml.HandleNodes(propertyNodes);
			float Value;
			if (ComplexValue != null)
			{
				currentQuest.SetProperty(key, ComplexValue);
			}
			else if (TextValue != null && xml.TryParse<float>(TextValue, out Value))
			{
				currentQuest.SetProperty(key, Value);
			}
			else
			{
				currentQuest.SetProperty(key, value);
			}
		}

		public void HandlePropertyEntry(XmlDataHelper xml)
		{
			if (ComplexType == 0)
			{
				HandlePropertyEntry<string>(xml);
			}
			else if (ComplexType == 1)
			{
				HandlePropertyEntry<int>(xml);
			}
			else if (ComplexType == 2)
			{
				HandlePropertyEntry<float>(xml);
			}
		}

		public void HandlePropertyEntry<T>(XmlDataHelper xml)
		{
			if (ComplexValue == null)
			{
				ComplexValue = new Dictionary<string, T>();
			}
			Dictionary<string, T> obj = ComplexValue as Dictionary<string, T>;
			string text = xml.ParseAttribute("Key", "");
			if (text.IsNullOrEmpty())
			{
				text = xml.ParseAttribute("Name", text);
			}
			T value = xml.ParseAttribute<T>("Value", default);
			if (xml.TryParseTextNode<T>(out var Value))
			{
				value = Value;
			}
			obj[text] = value;
		}

		public void HandlePropertyValue(XmlDataHelper xml)
		{
			if (ComplexType == 0)
			{
				HandlePropertyValue<string>(xml);
			}
			else if (ComplexType == 1)
			{
				HandlePropertyValue<int>(xml);
			}
			else if (ComplexType == 2)
			{
				HandlePropertyValue<float>(xml);
			}
		}

		public void HandlePropertyValue<T>(XmlDataHelper xml)
		{
			if (ComplexValue == null)
			{
				ComplexValue = new List<T>();
			}
			List<T> obj = ComplexValue as List<T>;
			T item = xml.ParseAttribute<T>("Value", default);
			if (xml.TryParseTextNode<T>(out var Value))
			{
				item = Value;
			}
			obj.Add(item);
		}

		public void HandlePropertyText(XmlDataHelper xml)
		{
			TextValue = Strings.SB.Clear().Append(xml.Value).Unindent()
				.ToString();
		}
	}

	public Dictionary<string, Quest> QuestsByID;

	[ModSensitiveStaticCache(false)]
	private static QuestLoader _Loader = null;

	[ModSensitiveStaticCache(true)]
	public static XMLParsing parser = new XMLParsing();

	public static QuestLoader Loader
	{
		get
		{
			CheckInit();
			return _Loader;
		}
	}

	[PreGameCacheInit]
	public static void CheckInit()
	{
		if (_Loader == null)
		{
			_Loader = new QuestLoader();
			Loading.LoadTask("Loading Quests.xml", _Loader.LoadQuests);
		}
	}

	public void LoadQuests()
	{
		QuestsByID = new Dictionary<string, Quest>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Quests"))
		{
			item.HandleNodes(parser.rootNodes);
		}
	}
}
