using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World.Parts.Skill;

[Serializable]
public class SingleWeaponFighting_OpportuneAttacks : ISingleWeaponFightingSkill
{
	public const int ABL_REFRESH = 2;

	[NonSerialized]
	private List<string> Skills;

	[NonSerialized]
	private List<ActivatedAbilityEntry> Abilities;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetAttackerMeleePenetrationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		if (AbilityEntry.ToggleState && E.Critical)
		{
			PopulateSkills(E.Weapon);
			if (PopulateRefreshable(E.Attacker))
			{
				Refresh(2);
			}
		}
		return base.HandleEvent(E);
	}

	public void PopulateSkills(GameObject Weapon)
	{
		if (Skills == null)
		{
			Skills = new List<string>(2);
		}
		Skills.Clear();
		string text = Weapon?.GetWeaponSkill();
		if (!text.IsNullOrEmpty())
		{
			Skills.Add(text);
		}
		Skills.Add("Shield");
	}

	public bool PopulateRefreshable(GameObject Actor)
	{
		Dictionary<Guid, ActivatedAbilityEntry> dictionary = Actor?.ActivatedAbilities?.AbilityByGuid;
		if (dictionary.IsNullOrEmpty())
		{
			return false;
		}
		if (Abilities == null)
		{
			Abilities = new List<ActivatedAbilityEntry>();
		}
		Abilities.Clear();
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in dictionary)
		{
			if (IsRefreshable(item.Value))
			{
				Abilities.Add(item.Value);
			}
		}
		return Abilities.Count > 0;
	}

	public bool IsRefreshable(ActivatedAbilityEntry Entry)
	{
		if (Entry.Cooldown > 0 && Entry.TryGetTag("Skill", out var Value) && Skills.Contains(Value))
		{
			return CanRefreshAbilityEvent.Check(ParentObject, Entry);
		}
		return false;
	}

	public void Refresh(int Amount)
	{
		Skills.Clear();
		for (int i = 0; i < Amount; i++)
		{
			ActivatedAbilityEntry activatedAbilityEntry = Abilities.RemoveRandomElement();
			if (activatedAbilityEntry == null)
			{
				break;
			}
			activatedAbilityEntry.Refresh(this);
			if (!Skills.Contains(activatedAbilityEntry.DisplayName))
			{
				Skills.Add(activatedAbilityEntry.DisplayName);
			}
		}
		int count = Skills.Count;
		if (count > 0)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			for (int j = 0; j < count; j++)
			{
				Skills[j] = stringBuilder.Append("{{W|").Append(Skills[j]).Append("}}")
					.ToString();
				stringBuilder.Clear();
			}
			stringBuilder.Append("advantage of ").Append(ParentObject.its).Append(" opponent's reaction to ")
				.Append(ParentObject.its)
				.Append(" attack! ")
				.Append(Grammar.MakeAndList(Skills))
				.Append((count == 1) ? " is " : " are ")
				.Append("refreshed");
			DidX("take", stringBuilder.ToString(), null, null, null, ParentObject);
		}
	}
}
