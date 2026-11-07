using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_Proselytize : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int COOLDOWN = 25;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCompanionLimitEvent>.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
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

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionLimitEvent E)
	{
		if (E.Means == "Proselytize" && E.Actor == ParentObject && ActivatedAbilityID != Guid.Empty)
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
		Registrar.Register("CommandProselytize");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandProselytize")
		{
			if (!AttemptProselytization())
			{
				return false;
			}
		}
		else if (E.ID == "CanCompanionRestorePartyLeader" && ParentObject.SupportsFollower(E.GetGameObjectParameter("Companion"), 1))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Proselytized proselytized)
		{
			return proselytized.Proselytizer == ParentObject;
		}
		return false;
	}

	public static void SyncTarget(GameObject Proselytizer, GameObject Target = null, bool Independent = false)
	{
		if (Proselytizer.Brain == null)
		{
			return;
		}
		int num = GetCompanionLimitEvent.GetFor(Proselytizer, "Proselytize");
		if (Target == null)
		{
			num++;
		}
		PartyCollection partyMembers = Proselytizer.Brain.PartyMembers;
		int[] array = (from x in partyMembers
			where x.Value.Flags.HasBit(1)
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
			partyMembers[Target] = 1;
			if (Independent)
			{
				partyMembers[Target] |= 8388608;
			}
		}
	}

	public override bool AddSkill(GameObject obj)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Proselytize", "CommandProselytize", "Skills", null, "\u0003");
		if (obj.IsPlayer())
		{
			SocialSifrah.AwardInsight();
		}
		return base.AddSkill(obj);
	}

	public override bool RemoveSkill(GameObject obj)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		SyncTarget(obj);
		return base.RemoveSkill(obj);
	}

	public bool AttemptProselytization()
	{
		bool flag = ParentObject.IsMissingTongue();
		if (flag && !ParentObject.HasPart<Telepathy>())
		{
			return ParentObject.Fail("You cannot proselytize without a tongue.");
		}
		if (!ParentObject.CheckFrozen(Telepathic: true))
		{
			return false;
		}
		Cell cell = PickDirection("Proselytize");
		if (cell == null)
		{
			return false;
		}
		List<GameObject> objects = cell.GetObjects((GameObject x) => x.IsCombatObject() && x.IsValid() && x.HasStat("MA") && x.HasStat("Level"));
		if (objects.IsNullOrEmpty())
		{
			return ParentObject.ShowFailure("There are no valid targets in that square.");
		}
		GameObject gameObject = ((objects.Count > 1) ? Popup.PickGameObject("Who would you like to proselytize?", objects, AllowEscape: true, ShowContext: false, UseYourself: false) : objects[0]);
		if (gameObject == null)
		{
			return false;
		}
		if (gameObject == ParentObject || gameObject.HasCopyRelationship(ParentObject) || gameObject.IsOriginalPlayerBody())
		{
			return ParentObject.Fail("You can't proselytize " + ParentObject.itself + "!");
		}
		if (flag && !ParentObject.CanMakeTelepathicContactWith(gameObject))
		{
			if (ParentObject.HasPart<Telepathy>())
			{
				return ParentObject.Fail("Without a tongue, you cannot proselytize " + gameObject.t() + ", as you cannot make telepathic contact with " + gameObject.them + ".");
			}
			return ParentObject.Fail("Without a tongue, you cannot proselytize " + gameObject.t() + ".");
		}
		if (!ParentObject.CheckFrozen(Telepathic: true, Telekinetic: false, Silent: true, gameObject))
		{
			return ParentObject.Fail("Frozen solid, you cannot proselytize " + gameObject.t() + ".");
		}
		if (gameObject.HasEffect(typeof(Proselytized), OurEffect))
		{
			return ParentObject.Fail("You have already proselytized " + gameObject.t() + ".");
		}
		if (!ConversationScript.IsPhysicalConversationPossible(ParentObject, gameObject, ShowPopup: true, AllowCombat: true, AllowFrozen: true) && !ConversationScript.IsMentalConversationPossible(ParentObject, gameObject, ShowPopup: true, AllowCombat: true))
		{
			return false;
		}
		if (gameObject.PartyLeader == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo(gameObject.Does("are") + " already your follower. Do you want to proselytize " + gameObject.them + " anyway?") != DialogResult.Yes)
		{
			return false;
		}
		if (!gameObject.CheckInfluence(By: ParentObject, Type: Name))
		{
			return false;
		}
		int num = Math.Max(gameObject.Stat("Level") - ParentObject.Stat("Level"), 0);
		if (gameObject.HasEffect<Proselytized>())
		{
			num++;
		}
		if (gameObject.HasEffect<Rebuked>())
		{
			num++;
		}
		Beguiled effect = gameObject.GetEffect<Beguiled>();
		if (effect != null)
		{
			num += effect.LevelApplied;
		}
		int num2 = ParentObject.StatMod("Ego");
		if (Options.SifrahRecruitment)
		{
			new ProselytizationSifrah(gameObject, num2, num).Play(gameObject);
		}
		else
		{
			PerformMentalAttack(Proselytize, ParentObject, gameObject, null, "Proselytize", "1d8-6", 2, int.MinValue, int.MinValue, num2, num);
		}
		ParentObject.UseEnergy(1000, "Skill Proselytize");
		CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		return true;
	}

	public static bool Proselytize(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		if (E.Penetrations <= 0 || !defender.ApplyEffect(new Proselytized(E.Attacker)))
		{
			return E.Attacker.Fail(defender.Does("are") + " unconvinced by your pleas.");
		}
		return true;
	}
}
