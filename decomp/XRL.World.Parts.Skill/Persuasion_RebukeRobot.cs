using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_RebukeRobot : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCompanionLimitEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionLimitEvent E)
	{
		if (E.Means == "Rebuke" && E.Actor == ParentObject && ActivatedAbilityID != Guid.Empty)
		{
			E.Limit++;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanCompanionRestorePartyLeader");
		Registrar.Register("CommandRebukeRobot");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandRebukeRobot")
		{
			if (!AttemptRebuke())
			{
				return false;
			}
		}
		else if (E.ID == "CanCompanionRestorePartyLeader" && ParentObject.SupportsFollower(E.GetGameObjectParameter("Companion"), 8))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Rebuke Robot", "CommandRebukeRobot", "Skills", "You admonish a robot into following your commands. Level + Ego-based difficulty check.", "\u0003");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		SyncTarget(GO);
		return true;
	}

	public bool AttemptRebuke()
	{
		if (ParentObject.IsMissingTongue())
		{
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You cannot rebuke without a tongue.");
			}
			return false;
		}
		if (!ParentObject.CheckFrozen())
		{
			return false;
		}
		Cell cell = PickDirection("Rebuke what robot?");
		if (cell == null)
		{
			return false;
		}
		bool flag = false;
		foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
		{
			if (item != ParentObject && item.Statistics.ContainsKey("Level") && item.HasPart<Robot>())
			{
				if (!item.CheckInfluence(By: ParentObject, Type: Name))
				{
					return false;
				}
				flag = true;
				int num = item.Stat("Level") * 4 / 5;
				if (item.HasEffect<Proselytized>())
				{
					num++;
				}
				if (item.HasEffect<Rebuked>())
				{
					num++;
				}
				if (item.TryGetEffect<Beguiled>(out var Effect))
				{
					num += Effect.LevelApplied;
				}
				int num2 = GetRebukeLevelEvent.GetFor(ParentObject, item);
				num2 = ParentObject.StatMod("Ego") + num2 * 4 / 5;
				if (Options.SifrahRecruitment)
				{
					new RebukingSifrah(item, num2, num).Play(item);
				}
				else
				{
					PerformMentalAttack(Rebuke, ParentObject, item, null, "Rebuke Robot", null, 2, int.MinValue, int.MinValue, num2, num);
				}
				ParentObject.UseEnergy(1000, "Skill Rebuke Robot");
				CooldownMyActivatedAbility(ActivatedAbilityID, 100);
			}
		}
		if (!flag && ParentObject.IsPlayer())
		{
			Popup.Show("There is nothing there to rebuke.");
		}
		return true;
	}

	public bool Rebuke(MentalAttackEvent E)
	{
		int penetrations = E.Penetrations;
		GameObject defender = E.Defender;
		if (penetrations <= 0)
		{
			IComponent<GameObject>.AddPlayerMessage("Your argument does not compute.");
			return false;
		}
		if (penetrations <= 2)
		{
			IComponent<GameObject>.XDidY(defender, "wander", "away disinterestedly");
			Neutralize(defender);
		}
		else
		{
			FinalizeRebuke(E.Attacker, defender);
		}
		return true;
	}

	public bool FinalizeRebuke(GameObject Actor, GameObject Robot)
	{
		Neutralize(Robot);
		return Robot.ApplyEffect(new Rebuked(Actor));
	}

	public static bool Rebuke(GameObject Actor, GameObject Robot)
	{
		return Actor.GetPart<Persuasion_RebukeRobot>()?.FinalizeRebuke(Actor, Robot) ?? false;
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Rebuked rebuked)
		{
			return rebuked.Rebuker == ParentObject;
		}
		return false;
	}

	public static void SyncTarget(GameObject Rebuker, GameObject Target = null, bool Independent = false)
	{
		if (Rebuker.Brain == null)
		{
			return;
		}
		int num = GetCompanionLimitEvent.GetFor(Rebuker, "Rebuke");
		if (Target == null)
		{
			num++;
		}
		PartyCollection partyMembers = Rebuker.Brain.PartyMembers;
		int[] array = (from x in partyMembers
			where x.Value.Flags.HasBit(8)
			orderby Brain.PartyMemberOrder(x) descending
			select x.Key).ToArray();
		int num2 = 0;
		for (int num3 = array.Length; num3 >= num; num3--)
		{
			partyMembers.Remove(array[num2]);
			num2++;
		}
		if (Target != null)
		{
			partyMembers[Target] = 8;
			if (Independent)
			{
				partyMembers[Target] |= 8388608;
			}
		}
	}

	public static void Neutralize(GameObject Actor, GameObject Object)
	{
		Brain brain = Object.Brain;
		if (brain != null)
		{
			brain.StopFighting();
			brain.Goals.Clear();
			brain.Allegiance.Hostile = false;
			brain.Target = null;
			brain.Wanders = true;
			brain.AddOpinion<OpinionRebuke>(Actor);
		}
	}

	public void Neutralize(GameObject Object)
	{
		Neutralize(ParentObject, Object);
	}
}
