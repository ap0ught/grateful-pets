using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Quests.GolemQuest;

namespace XRL.World.Units;

[Serializable]
public class GameObjectGolemQuestRandomUnit : GameObjectUnit
{
	public string SelectionID;

	public string Description;

	public int Amount;

	public override void Apply(GameObject Object)
	{
		GolemQuestSystem golemQuestSystem = GolemQuestSystem.Get();
		object obj;
		if (golemQuestSystem == null)
		{
			obj = null;
		}
		else
		{
			Dictionary<string, GolemQuestSelection> selections = golemQuestSystem.Selections;
			obj = ((selections != null) ? selections.GetValue(SelectionID) : null);
		}
		GolemQuestSelection golemQuestSelection = (GolemQuestSelection)obj;
		if (golemQuestSelection == null)
		{
			return;
		}
		StringBuilder stringBuilder = (Description.IsNullOrEmpty() ? Event.NewStringBuilder() : null);
		int num = 0;
		for (int i = 0; i < 100; i++)
		{
			if (num >= Amount)
			{
				break;
			}
			IEnumerable<GameObjectUnit> enumerable = golemQuestSelection.YieldRandomEffects();
			int num2 = num;
			foreach (GameObjectUnit item in enumerable)
			{
				if (num2 == num)
				{
					num++;
				}
				item.Apply(Object);
				if (item.CanInscribe() && stringBuilder != null)
				{
					stringBuilder.Compound(item.GetDescription(Inscription: true), '\n');
				}
			}
		}
		if (stringBuilder != null)
		{
			Description = stringBuilder.ToString();
		}
	}

	public override void Reset()
	{
		base.Reset();
		SelectionID = null;
		Amount = 0;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Inscription)
		{
			return Description;
		}
		return Description ?? string.Format("{0} random effect{1}", Amount, (Amount == 1) ? "" : "s");
	}
}
