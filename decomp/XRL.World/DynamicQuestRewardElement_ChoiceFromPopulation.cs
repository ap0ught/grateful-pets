using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class DynamicQuestRewardElement_ChoiceFromPopulation : DynamicQuestRewardElement
{
	public string table;

	public int nChoices = 3;

	public DynamicQuestRewardElement_ChoiceFromPopulation()
	{
	}

	public DynamicQuestRewardElement_ChoiceFromPopulation(string table, int nChoices)
		: this()
	{
		this.table = table;
		this.nChoices = nChoices;
	}

	public override string getRewardConversationType()
	{
		return null;
	}

	public override string getRewardAcceptQuestText()
	{
		return null;
	}

	public override void award()
	{
		List<List<PopulationResult>> list = PopulationManager.RollDistinctFrom(table, nChoices);
		if (list.Count <= 0)
		{
			return;
		}
		List<string> list2 = new List<string>();
		List<char> list3 = new List<char>();
		char c = 'a';
		List<List<GameObject>> list4 = new List<List<GameObject>>();
		foreach (List<PopulationResult> item in list)
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<GameObject> list5 = new List<GameObject>();
			foreach (PopulationResult item2 in item)
			{
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(item2.Blueprint);
				for (int i = 0; i < item2.Number; i++)
				{
					list5.Add(gameObject.DeepCopy(CopyEffects: false, CopyID: true));
				}
				string text = (gameObject.HasTagOrProperty("DisplayFullNameAsReward") ? gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true) : gameObject.DisplayNameOnlyDirect);
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				if (item2.Number == 1)
				{
					stringBuilder.Append(text);
				}
				else
				{
					stringBuilder.Append(text + " &Wx" + item2.Number);
				}
			}
			list4.Add(list5);
			list2.Add(stringBuilder.ToString());
			list3.Add(c);
			c = (char)(c + 1);
		}
		int index = Popup.PickOption("Choose a reward", null, "", "Sounds/UI/ui_notification", list2.ToArray(), list3.ToArray(), null, null, null, null, null, 1);
		foreach (GameObject item3 in list4[index])
		{
			item3.MakeUnderstood();
			The.Player.ReceiveObject(item3);
		}
	}
}
