using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Beguiling : BaseMutation
{
	public const string PROPERTY_ID = "BeguilingTargetID";

	public bool RealityDistortionBased;

	public Beguiling()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCompanionLimitEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCompanionLimitEvent E)
	{
		if (E.Means == "Beguiling" && E.Actor == ParentObject && ActivatedAbilityID != Guid.Empty)
		{
			E.Limit++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanCompanionRestoreParty");
		Registrar.Register("CommandBeguileCreature");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandBeguileCreature")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
			}
			else
			{
				Cast(ParentObject, this, E);
			}
		}
		else if (E.ID == "CanCompanionRestorePartyLeader")
		{
			if (ParentObject.SupportsFollower(E.GetGameObjectParameter("Companion"), 2))
			{
				return false;
			}
		}
		else if (E.ID == "GetMaxBeguiled")
		{
			E.ModParameter("Amount", 1);
		}
		return base.FireEvent(E);
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Beguiled beguiled)
		{
			return beguiled.Beguiler == ParentObject;
		}
		return false;
	}

	public override string GetDescription()
	{
		return "You beguile a nearby creature into serving you loyally.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Mental attack versus a creature with a mind\n" + "Success roll: {{rules|mutation rank}} or Ego mod (whichever is higher) + character level + 1d8 VS. Defender MA + character level\n", "Range: 1\n"), "Beguiled creature: +{{rules|", (Level * 5).ToString(), "}} bonus hit points\n"), "Cooldown: 50 rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		int num = Math.Max(ParentObject.StatMod("Ego"), Level) + ParentObject.GetStat("Level").Value;
		if (num == 0)
		{
			stats.Set("Attack", "1d8", !stats.mode.Contains("ability"));
		}
		else if (num > 0)
		{
			stats.Set("Attack", "1d8+" + num, !stats.mode.Contains("ability"));
		}
		else
		{
			stats.Set("Attack", "1d8" + num, !stats.mode.Contains("ability"));
		}
		stats.Set("BonusHP", Level * 5, !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static bool Cast(GameObject who, Beguiling mutation = null, Event ev = null, int genericLevel = 1)
	{
		bool independent = false;
		if (mutation == null)
		{
			mutation = new Beguiling();
			mutation.Level = genericLevel;
			mutation.ParentObject = who;
			independent = true;
		}
		Cell cell = mutation.PickDirection("[Select a direction to beguile a creature]");
		if (cell != null)
		{
			List<GameObject> objects = cell.GetObjects((GameObject x) => x.IsCombatObject() && x.IsValid() && x.HasStat("Hitpoints") && x.HasStat("Level"));
			if (objects.IsNullOrEmpty())
			{
				return who.ShowFailure("There are no valid targets in that square.");
			}
			GameObject gameObject = ((objects.Count > 2) ? Popup.PickGameObject("Who would you like to beguile?", objects, AllowEscape: true, ShowContext: false, UseYourself: false) : objects[0]);
			if (gameObject == null)
			{
				return false;
			}
			if (mutation.RealityDistortionBased)
			{
				Event e = Event.New("InitiateRealityDistortionTransit", "Object", who, "Mutation", mutation, "Cell", cell);
				if (!who.FireEvent(e, ev) || !cell.FireEvent(e, ev))
				{
					RealityStabilized.ShowGenericInterdictMessage(who);
					return false;
				}
			}
			if (gameObject != null && gameObject != who && gameObject.IsValid() && gameObject.HasStat("Level") && gameObject.HasStat("Hitpoints"))
			{
				if (gameObject.HasCopyRelationship(who) || gameObject.IsOriginalPlayerBody())
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You can't beguile " + who.itself + "!", 'R');
					}
					return false;
				}
				if (!gameObject.FireEvent("CanApplyBeguile") || !CanApplyEffectEvent.Check(gameObject, "Beguile"))
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(gameObject.Does("seem") + " utterly impervious to your charms.");
					}
					return false;
				}
				if (!gameObject.CheckInfluence(mutation.Name, who))
				{
					return false;
				}
				if (gameObject.HasEffect(typeof(Beguiled), mutation.OurEffect))
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You have already beguiled " + gameObject.t() + ".", 'R');
					}
					return false;
				}
				if (gameObject.IsLedBy(who) && who.IsPlayer() && Popup.ShowYesNo(gameObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " already your follower. Do you want to beguile " + gameObject.them + " anyway?") != DialogResult.Yes)
				{
					return false;
				}
				mutation?.UseEnergy(1000, "Mental Mutation Beguiling");
				mutation?.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, 50);
				if (Options.SifrahRecruitment)
				{
					int rating = who.Stat("Level") + who.StatMod("Ego");
					int num = gameObject.Stat("Level");
					if (gameObject.HasEffect<Proselytized>())
					{
						num++;
					}
					if (gameObject.HasEffect<Rebuked>())
					{
						num++;
					}
					if (gameObject.HasEffect<Beguiled>())
					{
						Beguiled effect = gameObject.GetEffect<Beguiled>();
						num += effect.LevelApplied;
					}
					BeguilingSifrah beguilingSifrah = new BeguilingSifrah(gameObject, mutation.Level, independent, rating, num);
					beguilingSifrah.Play(gameObject);
					return beguilingSifrah.Success;
				}
				if (gameObject.HasEffect<Beguiled>() && gameObject.GetEffect<Beguiled>().LevelApplied + "1d8".RollCached() > mutation.Level + "1d8".RollCached())
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You fail to outshine the current object of " + gameObject.poss("affection") + ".", 'R');
					}
					return false;
				}
				int attackModifier = who.Stat("Level") + Math.Max(who.StatMod("Ego"), mutation.Level);
				return Mental.PerformAttack(mutation.Beguile, who, gameObject, null, "Beguiling", "1d8", 1, int.MinValue, int.MinValue, attackModifier, gameObject.Stat("Level"));
			}
		}
		return false;
	}

	private bool Beguile(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		bool independent = ActivatedAbilityID == Guid.Empty;
		if (E.Penetrations <= 0 || !defender.FireEvent("CanApplyBeguile") || !CanApplyEffectEvent.Check(defender, "Beguile") || !defender.ApplyEffect(new Beguiled(E.Attacker, Level, independent)))
		{
			IComponent<GameObject>.AddPlayerMessage("Your coquetry infuriates " + defender.t() + ".", 'r');
			defender.AddOpinion<OpinionCoquetry>(E.Attacker);
			return false;
		}
		return true;
	}

	public IEnumerable<GameObject> YieldTargets()
	{
		if (ParentObject.Brain == null)
		{
			yield break;
		}
		foreach (KeyValuePair<int, PartyMember> partyMember in ParentObject.Brain.PartyMembers)
		{
			if (partyMember.Value.Flags.HasBit(2))
			{
				GameObject gameObject = partyMember.Value.Reference.Object ?? GameObject.FindByID(partyMember.Key);
				if (gameObject != null)
				{
					yield return gameObject;
				}
			}
		}
	}

	public static void SyncTarget(GameObject Beguiler, GameObject Target = null, bool Independent = false)
	{
		if (Beguiler.Brain == null)
		{
			return;
		}
		int num = GetCompanionLimitEvent.GetFor(Beguiler, "Beguiling");
		if (Target == null)
		{
			num++;
		}
		PartyCollection partyMembers = Beguiler.Brain.PartyMembers;
		int[] array = (from x in partyMembers
			where x.Value.Flags.HasBit(2)
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
			partyMembers[Target] = 2;
			if (Independent)
			{
				partyMembers[Target] |= 8388608;
			}
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		foreach (GameObject item in YieldTargets())
		{
			if (item.TryGetEffect<Beguiled>(out var Effect))
			{
				Effect.SyncToMutation();
			}
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Beguile Creature", "CommandBeguileCreature", "Mental Mutations", "Beguile", "\u0003", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		SyncTarget(GO);
		return base.Unmutate(GO);
	}

	public int GetCooldown(int Level)
	{
		return 50;
	}
}
