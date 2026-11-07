using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Shield_Slam : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandShieldSlam";

	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int Cooldown = 40;

	public static readonly int SlamSave = 20;

	public void CollectStats(Templates.StatCollector stats)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num = ParentObject.StatMod("Strength");
		if (num > 0)
		{
			stringBuilder.Append(num).Append("d2");
		}
		GameObject shieldWithHighestAV = ParentObject.GetShieldWithHighestAV();
		if (shieldWithHighestAV == null)
		{
			stringBuilder.Compound("shield AV", '+');
		}
		else
		{
			XRL.World.Parts.Shield part = shieldWithHighestAV.GetPart<XRL.World.Parts.Shield>();
			int Damage = part.AV;
			string Attributes = "Bludgeoning";
			string ExtraDesc = "";
			GetShieldSlamDamageEvent.GetFor(ParentObject, null, shieldWithHighestAV, ref Attributes, ref ExtraDesc, ref Damage, part.AV, num, Prospective: true);
			stringBuilder.CompoundSigned(Damage);
			if (!ExtraDesc.IsNullOrEmpty())
			{
				stringBuilder.Append(ExtraDesc);
			}
		}
		stats.Set("Damage", stringBuilder.ToString(), num > 0, num);
		if (num > 0)
		{
			stats.AddChangePostfix("Damage", num, num * 2, "high strength");
		}
		stats.Set("KnockdownSave", "Strength " + (SlamSave + num), num != 0, num);
		stats.AddChangePostfix("Knockdown save", num, (num > 0) ? "high strength" : "low strength");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !PerformShieldSlam())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (GameObject.Validate(E.Target) && E.Distance <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.CanMoveExtremities("ShieldSlam") && E.Actor.GetShield() != null && !E.Target.HasEffect<Prone>() && E.Actor.PhaseAndFlightMatches(E.Target))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ChargedTarget");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ChargedTarget")
		{
			GameObject shieldWithHighestAV = ParentObject.GetShieldWithHighestAV();
			if (shieldWithHighestAV != null)
			{
				GameObject Object = E.GetGameObjectParameter("Defender");
				if (GameObject.Validate(ref Object) && !Object.IsNowhere())
				{
					Slam(Object, shieldWithHighestAV, null, Free: true);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Shield Slam", COMMAND_NAME, "Skills", null, "\u0011", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}

	public bool Slam(GameObject TargetObject, GameObject ShieldObj, Cell TargetCell = null, bool Free = false)
	{
		if (ShieldObj == null || !GameObject.Validate(ParentObject))
		{
			return false;
		}
		if (TargetCell == null)
		{
			TargetCell = TargetObject.CurrentCell;
			if (TargetCell == null)
			{
				return false;
			}
		}
		Event obj = Event.New("BeginAttack");
		obj.SetParameter("TargetObject", TargetObject);
		obj.SetParameter("TargetCell", TargetCell);
		if (!ParentObject.FireEvent(obj))
		{
			return false;
		}
		if (!Free)
		{
			ParentObject.UseEnergy(1000, "Combat Melee Skill ShieldSlam");
		}
		if (ParentObject.IsPlayer())
		{
			ParentObject.Target = TargetObject;
		}
		if (!ParentObject.PhaseMatches(TargetObject))
		{
			DidXToY("try", "to shield slam", TargetObject, ", but " + ParentObject.its_(ShieldObj) + ShieldObj.GetVerb("pass") + " through " + TargetObject.them, "!", null, null, null, ParentObject);
		}
		else if (TargetObject.MakeSave("Strength", SlamSave, ParentObject, null, "ShieldSlam Slam", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ShieldObj))
		{
			DidXToYWithZ("slam", "into", TargetObject, "with", ShieldObj, null, "!", null, null, IndirectObjectPossessedBy: ParentObject, ColorAsGoodFor: ParentObject);
			TargetObject.Brain?.Attacked(ParentObject, ShieldObj);
			if (IComponent<GameObject>.Visible(TargetObject))
			{
				TargetObject.ParticleText("*resisted*", IComponent<GameObject>.ConsequentialColorChar(TargetObject));
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(TargetObject.Does("resist") + " your shield slam.", 'r');
			}
			else if (TargetObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You resist " + ParentObject.poss("shield slam") + ".", 'g');
			}
		}
		else
		{
			DidXToYWithZ("slam", "into", TargetObject, "with", ShieldObj, null, "!", null, null, IndirectObjectPossessedBy: ParentObject, ColorAsGoodFor: ParentObject);
			Event obj2 = Event.New("ObjectAttacking");
			obj2.SetParameter("Object", ParentObject);
			obj2.SetParameter("TargetObject", TargetObject);
			obj2.SetParameter("TargetCell", TargetCell);
			if (TargetCell.FireEvent(obj2) && !TargetObject.IsNowhere())
			{
				XRL.World.Parts.Shield part = ShieldObj.GetPart<XRL.World.Parts.Shield>();
				int num = ParentObject.StatMod("Strength");
				int Damage = ((num > 0) ? (num + "d2").RollCached() : 0) + part.AV;
				string Attributes = "Bludgeoning";
				string ExtraDesc = null;
				GetShieldSlamDamageEvent.GetFor(ParentObject, TargetObject, ShieldObj, ref Attributes, ref ExtraDesc, ref Damage, part.AV, num);
				TargetObject.ApplyEffect(new Prone());
				TargetObject.TakeDamage(ref Damage, Attributes, null, null, null, ParentObject, null, null, null, "from %t shield slam!");
				ShieldSlamPerformedEvent.Send(ParentObject, TargetObject, ShieldObj, part.AV, Damage);
			}
		}
		return true;
	}

	public bool PerformShieldSlam()
	{
		GameObject shieldWithHighestAV = ParentObject.GetShieldWithHighestAV();
		if (shieldWithHighestAV == null)
		{
			return ParentObject.Fail("You must have a shield equipped to perform a shield slam.");
		}
		if (!ParentObject.CanMoveExtremities("ShieldSlam", ShowMessage: true))
		{
			return false;
		}
		string text = PickDirectionS("Shield Slam");
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(text);
		if (cellFromDirection == null)
		{
			return false;
		}
		GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: true);
		if (combatTarget == null)
		{
			GameObject combatTarget2 = cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: true);
			if (combatTarget2 != null)
			{
				return ParentObject.Fail("You cannot reach " + combatTarget2.t() + " to shield slam " + combatTarget2.them + ".");
			}
			return ParentObject.Fail("There's nothing there you can shield slam.");
		}
		if (combatTarget == ParentObject)
		{
			return ParentObject.Fail("You cannot shield slam " + ParentObject.itself + ".");
		}
		if (!Slam(combatTarget, shieldWithHighestAV, cellFromDirection))
		{
			return false;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, Cooldown);
		return true;
	}
}
