using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Discipline_Meditate : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public int RestCounter;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<UseEnergyEvent>.ID && ID != PooledEvent<CommandEvent>.ID)
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

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("DamageThreshold", Math.Max(0, ParentObject.GetStat("Willpower").Value * 3 - 60));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown());
	}

	public int GetCooldown()
	{
		return Math.Max(200 - ParentObject.GetIntProperty("Serene") * 40, 5);
	}

	public override bool HandleEvent(UseEnergyEvent E)
	{
		if (E.Passive && E.Type != null && E.Type.Contains("Pass"))
		{
			RestCounter++;
			if (RestCounter >= 10 && !ParentObject.HasEffect<Meditating>() && !ParentObject.HasEffect<Asleep>() && !ParentObject.HasEffect<Stasis>())
			{
				ParentObject.ApplyEffect(new Meditating(1, FromResting: true));
			}
		}
		else
		{
			RestCounter = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandDisciplineMeditate")
		{
			if (ParentObject.HasEffect<Meditating>())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are already meditating.");
				}
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown());
			ParentObject.ApplyEffect(new Meditating());
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Meditate", "CommandDisciplineMeditate", "Skills", null, "\u0001");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
