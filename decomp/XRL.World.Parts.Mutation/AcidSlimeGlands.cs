using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class AcidSlimeGlands : BaseMutation
{
	public AcidSlimeGlands()
	{
		Type = "Physical";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandSpitAcid");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You spit a puddle of corrosive acid.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("" + "Covers the area in acidic slime.\n", "Area: 3x3\n"), "Range: 8\n"), "Cooldown: 10 rounds\n");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Area", "3x3");
		stats.Set("Range", "8");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 10);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == BeforeApplyDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add("CommandSpitAcid");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && E.Damage.IsAcidDamage())
		{
			NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
			E.Damage.Amount = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSpitAcid")
		{
			List<Cell> list = PickBurst(1, 8, Locked: false, AllowVis.OnlyVisible, "Spit Acid");
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 9)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (8 squares)");
					}
					return false;
				}
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_creature_liquid_spit");
			SlimeGlands.SlimeAnimation("&G", ParentObject.CurrentCell, list[0]);
			int num = 0;
			foreach (Cell item2 in list)
			{
				if (num == 0 || 80.in100())
				{
					item2.AddObject("SaltyAcidPool");
				}
				num++;
			}
			DidX("spit", "a stream of corrosive acid", null, null, null, ParentObject);
			UseEnergy(1000, "Physical Mutation Acid Slime Glands");
			CooldownMyActivatedAbility(ActivatedAbilityID, 40);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spit Acid", "CommandSpitAcid", "Physical Mutations", null, "\u00ad");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
