using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class QuickenMind : IActivePart
{
	public const string ABL_NAME = "Quicken Mind";

	public const string ABL_CMD = "QuickenMind";

	public Guid AbilityID;

	public int Cooldown = 1000;

	public int MinAbilityCooldown = 1;

	public int MaxAbilityCooldown = int.MaxValue;

	public string IncludeClasses;

	public string ExcludeClasses;

	public string AbilityAmount = "*";

	public string AbilityCommand;

	public QuickenMind()
	{
		WorksOnEquipper = true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		if (AbilityAmount == "1")
		{
			stats.Set("RefreshDescription", "Refreshes a random cooldown.");
		}
		else
		{
			stats.Set("RefreshDescription", "Refreshes all your cooldowns.");
		}
		stats.CollectCooldownTurns(MyActivatedAbility(AbilityID, ParentObject.Equipped), Cooldown);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && (ID != AIGetOffensiveItemListEvent.ID || !WorksOnEquipper) && (ID != EquippedEvent.ID || !WorksOnEquipper) && (ID != UnequippedEvent.ID || !WorksOnEquipper))
		{
			if (ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
			{
				return WorksOnEquipper;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(AbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor) && E.Actor.IsActivatedAbilityAIUsable(AbilityID) && AnyRefreshableFor(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Add(AbilityCommand);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor))
		{
			AbilityID = E.Actor.AddDynamicCommand(out AbilityCommand, "CommandQuickenMind", "Quicken Mind", "Items");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (!IsObjectActivePartSubject(E.Actor))
		{
			RemoveMyActivatedAbility(ref AbilityID, E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == AbilityCommand && Activate(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool IsRefreshable(ActivatedAbilityEntry Ability)
	{
		if (Ability.Cooldown <= MaxAbilityCooldown && Ability.Cooldown >= MinAbilityCooldown && Ability.CommandForDescription != "CommandQuickenMind" && (IncludeClasses == null || IncludeClasses.CachedCommaExpansion().Contains(Ability.Class)))
		{
			if (ExcludeClasses != null)
			{
				return !ExcludeClasses.CachedCommaExpansion().Contains(Ability.Class);
			}
			return true;
		}
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool AnyRefreshableFor(GameObject Actor)
	{
		Dictionary<Guid, ActivatedAbilityEntry> dictionary = Actor?.ActivatedAbilities?.AbilityByGuid;
		if (dictionary.IsNullOrEmpty())
		{
			return false;
		}
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in dictionary)
		{
			if (IsRefreshable(item.Value))
			{
				return true;
			}
		}
		return false;
	}

	public List<ActivatedAbilityEntry> GetRefreshableOf(GameObject Actor)
	{
		Dictionary<Guid, ActivatedAbilityEntry> dictionary = Actor?.ActivatedAbilities?.AbilityByGuid;
		if (dictionary.IsNullOrEmpty())
		{
			return null;
		}
		List<ActivatedAbilityEntry> list = null;
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in dictionary)
		{
			if (IsRefreshable(item.Value))
			{
				if (list == null)
				{
					list = new List<ActivatedAbilityEntry>();
				}
				list.Add(item.Value);
			}
		}
		return list;
	}

	public bool Activate(GameObject Actor)
	{
		if (!Actor.IsActivatedAbilityUsable(AbilityID) || !IsObjectActivePartSubject(Actor))
		{
			return false;
		}
		switch (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
		case ActivePartStatus.Booting:
			return Actor.Fail(ParentObject.Does("are") + " still attuning.");
		default:
			return Actor.Fail(ParentObject.Does("are") + " dead still.");
		case ActivePartStatus.Operational:
		{
			List<ActivatedAbilityEntry> refreshableOf = GetRefreshableOf(Actor);
			if (refreshableOf.IsNullOrEmpty())
			{
				return Actor.Fail("None of your abilities need refreshing.");
			}
			ConsumeCharge();
			Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_massMind_activate");
			CooldownMyActivatedAbility(AbilityID, Cooldown, Actor);
			if (AbilityAmount == "*" || AbilityAmount.EqualsNoCase("All"))
			{
				foreach (ActivatedAbilityEntry item in refreshableOf)
				{
					item.Refresh(this);
				}
				return Actor.ShowSuccess("You are quickened with creative energy! All cooldowns are refreshed.");
			}
			int num = Stat.RollCached(AbilityAmount);
			List<string> list = new List<string>();
			for (int i = 0; i < num; i++)
			{
				if (refreshableOf.Count <= 0)
				{
					break;
				}
				int index = Stat.Rnd.Next(refreshableOf.Count);
				ActivatedAbilityEntry activatedAbilityEntry = refreshableOf[index];
				activatedAbilityEntry.Refresh(this);
				list.Add("{{W|" + activatedAbilityEntry.DisplayName + "}}");
				refreshableOf.RemoveAt(index);
			}
			if (!list.IsNullOrEmpty())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder("You are quickened with creative energy! The ").Append((list.Count == 1) ? "cooldown" : "cooldowns").Append(" on ")
					.Append(Grammar.MakeAndList(list))
					.Compound((list.Count == 1) ? "is" : "are")
					.Append(" refreshed.");
				return Actor.ShowSuccess(stringBuilder.ToString());
			}
			return true;
		}
		}
	}
}
