using System;
using System.Collections.Generic;
using XRL.World;

namespace XRL;

[Serializable]
public abstract class IQuestSystem : IPlayerSystem
{
	public string QuestID;

	[NonSerialized]
	private Quest _Quest;

	[NonSerialized]
	private Quest _Blueprint;

	public Quest Quest
	{
		get
		{
			return _Quest ?? (_Quest = The.Game.Quests.GetValue(QuestID));
		}
		set
		{
			_Quest = value;
			QuestID = value?.ID;
		}
	}

	public Quest Blueprint => _Blueprint ?? (_Blueprint = QuestLoader.Loader.QuestsByID.GetValue(QuestID));

	/// <summary>Removes the system from the game when the quest concludes.</summary>
	public virtual bool RemoveWithQuest => true;

	public virtual void ShowStartPopup()
	{
		Quest.ShowStartPopup();
	}

	public virtual void Start()
	{
	}

	public virtual void ShowFailPopup()
	{
		Quest.ShowFailPopup();
	}

	public virtual void Fail()
	{
	}

	public virtual void ShowFailStepPopup(QuestStep Step)
	{
		Quest.ShowFailStepPopup(Step);
	}

	public virtual void FailStep(QuestStep Step)
	{
	}

	public virtual void ShowFinishPopup()
	{
		Quest.ShowFinishPopup();
	}

	public virtual void Finish()
	{
	}

	public virtual void ShowFinishStepPopup(QuestStep Step)
	{
		Quest.ShowFinishStepPopup(Step);
	}

	public virtual void FinishStep(QuestStep Step)
	{
	}

	public virtual GameObject GetInfluencer()
	{
		return null;
	}

	public string GetProperty(string Key, string Default = null)
	{
		return Quest?.GetProperty(Key, Default) ?? Blueprint.GetProperty(Key, Default);
	}

	public int GetProperty(string Key, int Default)
	{
		return Quest?.GetProperty(Key, Default) ?? Blueprint.GetProperty(Key, Default);
	}

	public float GetProperty(string Key, float Default)
	{
		return Quest?.GetProperty(Key, Default) ?? Blueprint.GetProperty(Key, Default);
	}

	public T GetProperty<T>(string Key, T Default = null) where T : class
	{
		Quest quest = Quest;
		return ((quest != null) ? quest.GetProperty(Key, Default) : null) ?? Blueprint.GetProperty(Key, Default);
	}

	public List<string> GetList(string Key)
	{
		return Quest?.GetList(Key) ?? Blueprint.GetList(Key);
	}

	public List<T> GetList<T>(string Key)
	{
		return Quest?.GetList<T>(Key) ?? Blueprint.GetList<T>(Key);
	}

	public Dictionary<string, string> GetDictionary(string Key)
	{
		return Quest?.GetDictionary(Key) ?? Blueprint.GetDictionary(Key);
	}

	public Dictionary<string, T> GetDictionary<T>(string Key)
	{
		return Quest?.GetDictionary<T>(Key) ?? Blueprint.GetDictionary<T>(Key);
	}

	public void SetProperty(string Key, object Value)
	{
		Quest?.SetProperty(Key, Value);
	}

	public void SetProperty(string Key, int Value)
	{
		Quest?.SetProperty(Key, Value);
	}

	public void SetProperty(string Key, float Value)
	{
		Quest?.SetProperty(Key, Value);
	}

	public bool RemoveProperty(string Key)
	{
		return Quest?.RemoveProperty(Key) ?? false;
	}

	public bool RemoveNumericProperty(string Key)
	{
		return Quest?.RemoveNumericProperty(Key) ?? false;
	}
}
