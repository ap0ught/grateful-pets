using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Snapjaw_Howl : BaseSkill
{
	public int Radius = 5;

	public string Duration = "10-20";

	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int COOLDOWN = 100;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Radius", Radius);
		stats.Set("Duration", Duration);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSnapjawHowl");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandSnapjawHowl");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSnapjawHowl")
		{
			if (!ParentObject.IsValid() || ParentObject.CurrentCell == null)
			{
				return false;
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_snapjaw_howl");
			DidX("howl", null, "!", null, null, ParentObject);
			List<GameObject> list = new List<GameObject>(16);
			foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(Radius))
			{
				GameObject firstObjectWithPropertyOrTag = localAdjacentCell.GetFirstObjectWithPropertyOrTag("Snapjaw", (GameObject obj) => !obj.HasEffect<Frenzied>());
				if (firstObjectWithPropertyOrTag == null)
				{
					continue;
				}
				bool flag = false;
				if (firstObjectWithPropertyOrTag.IsVisible())
				{
					if (firstObjectWithPropertyOrTag.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You are frenzied by the howl!", 'G');
					}
					else
					{
						flag = true;
					}
				}
				if (firstObjectWithPropertyOrTag.ApplyEffect(new Frenzied(Duration.RollCached())) & flag)
				{
					list.Add(firstObjectWithPropertyOrTag);
				}
			}
			if (list.Count > 0)
			{
				int num = 0;
				int num2 = 0;
				GameObject gameObject = null;
				Dictionary<string, int> dictionary = new Dictionary<string, int>(list.Count);
				Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>(list.Count);
				foreach (GameObject item in list)
				{
					string shortDisplayName = item.ShortDisplayName;
					if (dictionary.ContainsKey(shortDisplayName))
					{
						dictionary[shortDisplayName]++;
					}
					else
					{
						dictionary.Add(shortDisplayName, 1);
					}
					if (!dictionary2.ContainsKey(shortDisplayName))
					{
						dictionary2.Add(shortDisplayName, item);
					}
					if (gameObject == null)
					{
						gameObject = item;
					}
					num++;
					if (item.IsHostileTowards(IComponent<GameObject>.ThePlayer))
					{
						num2++;
					}
				}
				List<string> list2 = new List<string>(dictionary.Count);
				StringBuilder stringBuilder = Event.NewStringBuilder();
				foreach (string key in dictionary.Keys)
				{
					int num3 = dictionary[key];
					stringBuilder.Length = 0;
					if (num3 > 1)
					{
						stringBuilder.Append(Grammar.Cardinal(num3)).Append(' ').Append(Grammar.Pluralize(key));
					}
					else
					{
						stringBuilder.Append(dictionary2[key].a).Append(key);
					}
					list2.Add(stringBuilder.ToString());
				}
				string value;
				if (num2 >= num / 2)
				{
					value = "&R";
				}
				else
				{
					value = ((num2 <= 0) ? "&g" : "&r");
				}
				stringBuilder.Length = 0;
				stringBuilder.Append(value).Append(ColorUtility.CapitalizeExceptFormatting(Grammar.MakeAndList(list2))).Append(value)
					.Append((num == 1) ? gameObject.Is : " are")
					.Append(" frenzied by the howl!");
				IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString());
			}
			ParentObject.Soundwave();
			ParentObject.UseEnergy(1000, "Skill Howl");
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Howl", "CommandSnapjawHowl", "Skills", null, "\u000e");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
