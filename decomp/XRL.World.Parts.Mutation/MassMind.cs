using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MassMind : BaseMutation
{
	public int nDuration;

	public MassMind()
	{
		Type = "Mental";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandMassMind");
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You tap into the aggregate mind and steal power from other espers.";
	}

	public int GetCooldown(int Level)
	{
		return Math.Max(100, 550 - 50 * Level);
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text += "Refreshes all mental mutations\n";
		text = text + "Cooldown: {{rules|" + GetCooldown(Level) + "}} rounds\n";
		text += "Cooldown is not affected by Willpower.\n";
		text += "Each use attracts slightly more attention from psychic interlopers.\n";
		text = ((Level != base.Level) ? (text + "{{rules|Decreased chance for another esper to steal your powers}}\n") : (text + "{{rules|Small chance each round for another esper to steal your powers}}\n"));
		return text + "-200 reputation with {{w|the Seekers of the Sightless Way}}";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("CooldownNoWillpower", GetCooldown(Level), !stats.mode.Contains("ability"));
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (nDuration > 0)
			{
				nDuration--;
			}
			else if ((double)Stat.Random(1, 10000) < (0.13 - 0.005 * (double)ParentObject.StatMod("Willpower") - 0.0065 * (double)Level) * 100.0 && ParentObject.IsPlayer() && ParentObject.CurrentCell != null && !ParentObject.OnWorldMap())
			{
				IEnumerable<ActivatedAbilityEntry> enumerable = MyActivatedAbilities.YieldAbilitiesWithClass("Mental Mutations");
				if (enumerable.Any())
				{
					if (ParentObject.HasEffect<FungalVisionary>())
					{
						IComponent<GameObject>.AddPlayerMessage("You feel a small ripple in space and time.");
					}
					else
					{
						nDuration = "8d10".RollCached();
						foreach (ActivatedAbilityEntry item in enumerable)
						{
							if (item.Enabled)
							{
								item.AddScaledCooldown(nDuration * 10);
							}
						}
						IComponent<GameObject>.AddPlayerMessage("{{R|Someone reaches through the aggregate mind and exhausts your power!}}");
						ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_massMind_cooldownReset");
					}
				}
			}
		}
		else if (E.ID == "CommandMassMind")
		{
			if (ParentObject.HasEffect<FungalVisionary>())
			{
				Popup.Show("Too far! The aggregate mind is stretched to gossamers, and even the closest mind is too far away.");
			}
			else
			{
				MessageQueue.AddPlayerMessage("{{G|You innervate your mind at someone's expense.}}");
				foreach (ActivatedAbilityEntry item2 in MyActivatedAbilities.YieldAbilitiesWithClass("Mental Mutations"))
				{
					if (item2.Cooldown > 0 && item2.ID != ActivatedAbilityID)
					{
						item2.Refresh(this);
					}
				}
				ParentObject.ModIntProperty("GlimmerModifier", 1);
				ParentObject.SyncMutationLevelAndGlimmer();
				UseEnergy(1000, "Mental Mutation MassMind");
				int cooldown = GetCooldown(Level);
				CooldownMyActivatedAbility(ActivatedAbilityID, cooldown);
				ParentObject.FireEvent("AfterMassMind");
				ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_massMind_activate");
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Tap the Mass Mind", "CommandMassMind", "Mental Mutations", null, "!", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: false);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
