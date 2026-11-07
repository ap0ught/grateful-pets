using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsStatModifier : IPart
{
	public string Stats;

	public bool AddOn;

	public override bool SameAs(IPart p)
	{
		CyberneticsStatModifier cyberneticsStatModifier = p as CyberneticsStatModifier;
		if (cyberneticsStatModifier.Stats != Stats)
		{
			return false;
		}
		if (cyberneticsStatModifier.AddOn != AddOn)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject.Implantee))
		{
			foreach (string key in ParseStats(Stats).Keys)
			{
				switch (key)
				{
				case "Strength":
					E.Add("might", 1);
					break;
				case "Intelligence":
					E.Add("scholarship", 1);
					break;
				case "Ego":
					E.Add("jewels", 1);
					break;
				case "Willpower":
					E.Add("salt", 1);
					break;
				case "Speed":
					E.Add("time", 1);
					break;
				case "MoveSpeed":
					E.Add("travel", 1);
					break;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		foreach (KeyValuePair<string, int> item in ParseStats(Stats))
		{
			E.Actor.GetStat(item.Key).BaseValue += item.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		foreach (KeyValuePair<string, int> item in ParseStats(Stats))
		{
			E.Actor.GetStat(item.Key).BaseValue -= item.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		if (AddOn)
		{
			string[] array = Stats.Split(';');
			foreach (string text in array)
			{
				string[] array2 = text.Split(':');
				try
				{
					E.Add(Statistic.GetStatAdjustDescription(array2[0], Convert.ToInt32(array2[1])));
				}
				catch
				{
					E.Add("[error in " + text + "]");
				}
			}
		}
		else if (ParentObject.Implantee != null)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			bool flag = true;
			string[] array = Stats.Split(';');
			foreach (string text2 in array)
			{
				string[] array3 = text2.Split(':');
				try
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						stringBuilder.Append('\n');
					}
					Statistic.AppendStatAdjustDescription(stringBuilder, array3[0], Convert.ToInt32(array3[1]));
				}
				catch
				{
					E.Add("[error in " + text2 + "]");
				}
			}
			E.Description = stringBuilder.ToString();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public Dictionary<string, int> ParseStats(string Stats)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string[] array = Stats.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			dictionary.Add(array2[0], Convert.ToInt32(array2[1]));
		}
		return dictionary;
	}
}
