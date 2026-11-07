using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class ReceiveItem : IConversationPart
{
	public string Table;

	public string Blueprints;

	public string Identify;

	public string Mods = "0";

	public bool Pick;

	public bool FromSpeaker;

	public ReceiveItem()
	{
	}

	public ReceiveItem(string Blueprints)
	{
		this.Blueprints = Blueprints;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		List<PopulationResult> list = (Table.IsNullOrEmpty() ? new List<PopulationResult>() : PopulationManager.Generate(Table));
		if (!Blueprints.IsNullOrEmpty())
		{
			string[] array = Blueprints.Split(',');
			foreach (string text in array)
			{
				int num = text.IndexOf(':');
				PopulationResult item;
				if (num < 0)
				{
					item = new PopulationResult(text);
				}
				else
				{
					string text2 = text.Substring(num + 1);
					string blueprint = text.Substring(0, num);
					if (text2.Contains(":"))
					{
						item = new PopulationResult(blueprint, 1, text2);
					}
					else
					{
						DieRoll dieRoll = new DieRoll(text2, out var AnyUnknown);
						item = ((!AnyUnknown) ? new PopulationResult(blueprint, dieRoll.Resolve()) : new PopulationResult(blueprint, 1, text2));
					}
				}
				list.Add(item);
			}
		}
		List<GameObject> list2 = Event.NewGameObjectList();
		bool flag = Identify == "*" || Identify.EqualsNoCase("all");
		for (int j = 0; j < list.Count; j++)
		{
			bool flag2 = false;
			for (int num2 = list[j].Number; num2 > 0; num2--)
			{
				ParseItemHints(list[j], !flag2, out var Count, out var ModChance, out var Skip);
				if (!Skip)
				{
					if (!flag2 && Count != 0)
					{
						num2 = Count;
						flag2 = true;
					}
					int setModNumber = Mods.RollCached();
					GameObject gameObject = (FromSpeaker ? The.Speaker.HasItemWithBlueprint(list[j].Blueprint) : null) ?? GameObject.Create(list[j].Blueprint, ModChance, setModNumber);
					if (flag || (!Identify.IsNullOrEmpty() && Identify.CachedCommaExpansion().Contains(list[j].Blueprint)))
					{
						gameObject.MakeUnderstood();
					}
					if (num2 > 1 && gameObject.CanGenerateStacked())
					{
						gameObject.Count = num2;
						num2 = 0;
					}
					list2.Add(gameObject);
				}
			}
		}
		if (Pick && list2.Count > 1)
		{
			GameObject gameObject2 = Popup.PickGameObject("Choose a reward", list2);
			for (int num3 = list2.Count - 1; num3 >= 0; num3--)
			{
				if (list2[num3] != gameObject2)
				{
					list2[num3].Obliterate();
					list2.RemoveAt(num3);
				}
			}
		}
		if (list2.Count > 0)
		{
			List<string> list3 = new List<string>(list.Count);
			for (int k = 0; k < list2.Count; k++)
			{
				list3.Add(list2[k].an());
				The.Player.ReceiveObject(list2[k]);
			}
			Popup.Show("You receive " + Grammar.MakeAndList(list3) + "!");
		}
		return base.HandleEvent(E);
	}

	private void ParseItemHints(PopulationResult Result, bool AllowCount, out int Count, out int ModChance, out bool Skip)
	{
		Count = 0;
		ModChance = 0;
		Skip = false;
		if (Result.Hint == null)
		{
			return;
		}
		int num;
		if (AllowCount)
		{
			num = Result.Hint.IndexOf("Count:", StringComparison.Ordinal);
			if (num >= 0)
			{
				num += 6;
				int num2 = Result.Hint.IndexOf(',', num);
				string dice = ((num2 < 0) ? Result.Hint.Substring(num) : Result.Hint.Substring(num, num2 - num));
				Count = dice.RollCached();
			}
		}
		num = Result.Hint.IndexOf("SetBonusModChance:", StringComparison.Ordinal);
		if (num >= 0)
		{
			num += 18;
			int num3 = Result.Hint.IndexOf(',', num);
			string dice2 = ((num3 < 0) ? Result.Hint.Substring(num) : Result.Hint.Substring(num, num3 - num));
			ModChance = dice2.RollCached();
		}
		num = Result.Hint.IndexOf("IfSkillDominant:", StringComparison.Ordinal);
		if (num >= 0)
		{
			num += 16;
			int num4 = Result.Hint.IndexOf(',', num);
			string[] array = ((num4 < 0) ? Result.Hint.Substring(num) : Result.Hint.Substring(num, num4 - num)).Split('/');
			if (array.Length < 2 || The.Player.GetSkillAndPowerCountInSkill(array[0]) <= The.Player.GetSkillAndPowerCountInSkill(array[1]))
			{
				Skip = true;
			}
		}
		num = Result.Hint.IndexOf("IfNotSkillDominant:", StringComparison.Ordinal);
		if (num >= 0)
		{
			num += 19;
			int num5 = Result.Hint.IndexOf(',', num);
			string[] array2 = ((num5 < 0) ? Result.Hint.Substring(num) : Result.Hint.Substring(num, num5 - num)).Split('/');
			if (array2.Length < 2 || The.Player.GetSkillAndPowerCountInSkill(array2[0]) > The.Player.GetSkillAndPowerCountInSkill(array2[1]))
			{
				Skip = true;
			}
		}
	}
}
