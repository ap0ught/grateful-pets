using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Tonic : IPart
{
	public bool CausesOverdose = true;

	public bool Eat;

	public string BehaviorDescription = "This item is a tonic. Applying one tonic while under the effects of another may produce undesired results.";

	public override bool SameAs(IPart p)
	{
		Tonic tonic = p as Tonic;
		if (tonic.CausesOverdose != CausesOverdose)
		{
			return false;
		}
		if (tonic.Eat != Eat)
		{
			return false;
		}
		if (tonic.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != GetInventoryActionsAlwaysEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(BehaviorDescription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		if (!E.Object.IsBroken() && !E.Object.IsRusted())
		{
			int num = 0;
			if (E.Object.HasPart<Empty_Tonic_Applicator>())
			{
				num = -100;
			}
			else if (E.Object.Equipped == E.Actor || E.Object.InInventory == E.Actor)
			{
				if (E.Object.IsImportant())
				{
					num = -1;
				}
				else if (E.Actor.FireEvent("CanApplyTonic") && (!Eat || E.Actor.HasPart<Stomach>()))
				{
					num = 100;
				}
			}
			if (Eat)
			{
				E.AddAction("Eat", "eat", "Apply", null, 'e', FireOnActor: false, num, 0, Override: true);
				if (!E.Actor.OnWorldMap())
				{
					E.AddAction("Feed To", "feed to", "ApplyTo", null, 'f', FireOnActor: false, -2, 0, Override: true);
				}
			}
			else
			{
				E.AddAction("Apply", "apply", "Apply", null, 'a', FireOnActor: false, num);
				if (!E.Actor.OnWorldMap())
				{
					E.AddAction("Apply To", "apply to", "ApplyTo", null, 'a', FireOnActor: false, -2);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply" || E.Command == "ApplyInvoluntarily" || E.Command == "ApplyExternally")
		{
			bool flag = E.Command == "ApplyInvoluntarily" || E.Command == "ApplyExternally";
			bool flag2 = E.Command == "ApplyInvoluntarily";
			GameObject gameObject = E.ObjectTarget ?? E.Actor;
			if (Eat && !gameObject.HasPart<Stomach>())
			{
				E.Actor.Fail(gameObject.Does("are") + " unable to consume tonics.");
				return false;
			}
			if (E.Item.IsBroken())
			{
				E.Actor.Fail(E.Item.Itis + " broken...");
				return false;
			}
			if (E.Item.IsRusted())
			{
				E.Actor.Fail(E.Item.Itis + " rusted...");
				return false;
			}
			if (!flag2 && !flag && !E.Item.ConfirmUseImportant(E.Actor, "use", "up", 1))
			{
				return false;
			}
			E.Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_applicator_apply");
			int value = GetTonicDosageEvent.GetFor(ParentObject, gameObject, E.Actor);
			Event obj = Event.New("ApplyingTonic");
			obj.SetParameter("Subject", gameObject);
			obj.SetParameter("Actor", E.Actor);
			obj.SetParameter("Tonic", ParentObject);
			obj.SetParameter("Dosage", value);
			obj.SetFlag("External", flag);
			obj.SetFlag("Involuntary", flag2);
			obj.SetFlag("ShowMessage", State: true);
			if (!gameObject.FireEvent(obj))
			{
				return false;
			}
			if (!BeforeConsumeEvent.Check(E.Actor, gameObject, ParentObject, Eat, Drink: false, !Eat, Inhale: false, Absorb: false, !flag2))
			{
				return false;
			}
			string Message = null;
			if (E.Actor.IsPlayer() || IComponent<GameObject>.Visible(E.Actor))
			{
				ParentObject.MakeUnderstood(out Message);
			}
			List<Effect> tonicEffects = gameObject.GetTonicEffects();
			int tonicCapacity = gameObject.GetTonicCapacity();
			if (tonicEffects.Count >= tonicCapacity && CausesOverdose)
			{
				foreach (Effect item in tonicEffects)
				{
					if (!gameObject.MakeSave("Toughness", 16 + 3 * (tonicEffects.Count - tonicCapacity), null, null, "Overdose", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
					{
						Event obj2 = Event.New("Overdose");
						obj2.SetParameter("Subject", gameObject);
						obj2.SetParameter("Actor", E.Actor);
						obj2.SetParameter("Tonic", ParentObject);
						obj2.SetParameter("Dosage", value);
						obj2.SetFlag("External", flag);
						obj2.SetFlag("Involuntary", flag2);
						item.FireEvent(obj2);
					}
				}
			}
			bool flag3 = false;
			string value2 = "No";
			if (gameObject.IsMutant())
			{
				int chance = 5;
				if (gameObject.HasPart<TonicAllergy>())
				{
					chance = 33;
				}
				if (gameObject.IsMutant() && chance.in100())
				{
					gameObject.SetLongProperty("Overdosing", 1L);
					flag3 = true;
				}
			}
			try
			{
				Event obj3 = Event.New("ApplyTonic");
				obj3.SetParameter("Subject", gameObject);
				obj3.SetParameter("Target", gameObject);
				obj3.SetParameter("Actor", E.Actor);
				obj3.SetParameter("Owner", E.Actor);
				obj3.SetParameter("Attacker", E.Actor);
				obj3.SetParameter("Overdose", value2);
				obj3.SetParameter("Dosage", value);
				obj3.SetFlag("External", flag);
				obj3.SetFlag("Involuntary", flag2);
				if (ParentObject.FireEvent(obj3))
				{
					AfterConsumeEvent.Send(E.Actor, gameObject, ParentObject, Eat, Drink: false, !Eat, Inhale: false, Absorb: false, !flag2);
					if (Eat)
					{
						Event e = Event.New("Eating", "Food", ParentObject, "Subject", gameObject);
						gameObject.FireEvent(e);
						if (!flag)
						{
							gameObject.UseEnergy(1000, "Item Eat");
						}
					}
					if (!flag && !gameObject.IsPlayer() && IComponent<GameObject>.Visible(gameObject))
					{
						IComponent<GameObject>.AddPlayerMessage(gameObject.Does(Eat ? "eat" : "apply") + " " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
					}
					ParentObject.Destroy(null, Silent: true);
				}
			}
			finally
			{
				if (flag3)
				{
					gameObject.SetLongProperty("Overdosing", 0L);
				}
				if (!Message.IsNullOrEmpty())
				{
					Popup.Show(Message);
				}
			}
		}
		else if (E.Command == "ApplyTo")
		{
			if (E.Item.IsBroken())
			{
				return E.Actor.Fail(E.Item.Itis + " broken...");
			}
			if (E.Item.IsRusted())
			{
				return E.Actor.Fail(E.Item.Itis + " rusted...");
			}
			if (E.Actor.OnWorldMap())
			{
				return E.Actor.Fail("You cannot do that on the world map.");
			}
			if (!E.Item.ConfirmUseImportant(E.Actor, "use", "up", 1))
			{
				return false;
			}
			Cell cell = PickDirection(ForAttack: false, POV: E.Actor, Label: "Apply " + E.Item.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true));
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(E.Actor, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false);
			if (combatTarget == null)
			{
				combatTarget = cell.GetCombatTarget(E.Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, null, null, null, AllowInanimate: false);
				if (combatTarget != null)
				{
					if (cell.GetCombatTarget(E.Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false) == null)
					{
						return E.Actor.Fail("You are out of phase with " + combatTarget.t() + ".");
					}
					return E.Actor.Fail("You cannot reach " + combatTarget.t() + ".");
				}
				return E.Actor.Fail("There is no one there you can " + (Eat ? "feed" : "apply") + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " to.");
			}
			Event obj4 = Event.New("CanApplyTonic");
			obj4.SetParameter("Subject", combatTarget);
			obj4.SetParameter("Actor", E.Actor);
			obj4.SetParameter("Tonic", ParentObject);
			obj4.SetFlag("External", State: true);
			obj4.SetFlag("ShowMessage", State: true);
			if (!combatTarget.FireEvent(obj4))
			{
				return false;
			}
			if (combatTarget == E.Actor)
			{
				if (Eat)
				{
					return E.Actor.Fail("If you want to eat " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " " + E.Actor.itself + ", you can do so through the eat action.");
				}
				return E.Actor.Fail("If you want to apply " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " to " + E.Actor.itself + ", you can do so through the apply action.");
			}
			if (combatTarget.IsHostileTowards(E.Actor) || (!combatTarget.IsLedBy(E.Actor) && GetUtilityScoreEvent.GetFor(combatTarget, ParentObject, null, ForPermission: true) <= 0))
			{
				if (Eat)
				{
					return E.Actor.Fail(combatTarget.Does("do") + " not want to consume " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				}
				return E.Actor.Fail(combatTarget.Does("do") + " not want " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " applied to " + combatTarget.them + ". You'll need to equip " + ParentObject.them + " as a weapon and attack with " + ParentObject.them + ".");
			}
			ParentObject.SplitFromStack();
			IComponent<GameObject>.WDidXToYWithZ(E.Actor, Eat ? "feed" : "apply", ParentObject, "to", combatTarget, null, null, null, null, combatTarget);
			GameObject parentObject = ParentObject;
			GameObject actor = E.Actor;
			GameObject parentObject2 = ParentObject;
			GameObject objectTarget = combatTarget;
			InventoryActionEvent.Check(parentObject, actor, parentObject2, "ApplyExternally", Auto: false, OwnershipHandled: false, !Eat, Forced: false, Silent: false, 0, 0, 0, objectTarget);
			E.Actor.UseEnergy(1000, "Item ApplyTo");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && 50.in100() && !Eat && !ParentObject.IsBroken() && !ParentObject.IsRusted())
		{
			IComponent<GameObject>.XDidY(E.Actor, "accidentally prick", E.Actor.itself + " with " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true), null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, E.Actor.IsPlayer());
			if (InventoryActionEvent.Check(ParentObject, E.Actor, ParentObject, "Apply"))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		Registrar.Register("ThrownProjectileHit");
		Registrar.Register("WeaponAfterDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "ProjectileHit" || E.ID == "ThrownProjectileHit" || E.ID == "WeaponAfterDamage") && !Eat)
		{
			GameObject Object = E.GetGameObjectParameter("Defender");
			if (GameObject.Validate(ref Object))
			{
				if (E.GetIntParameter("Penetrations") > 0 && !IsBroken() && !IsRusted())
				{
					if (Object.IsCreature)
					{
						GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
						InventoryActionEvent.Check(ParentObject, gameObjectParameter, ParentObject, "ApplyInvoluntarily", Auto: false, OwnershipHandled: false, OverrideEnergyCost: true, Forced: false, Silent: false, 0, 0, 0, Object);
					}
				}
				else
				{
					if (IComponent<GameObject>.Visible(Object))
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("fail") + " to penetrate " + Object.poss("armor") + " and" + ParentObject.Is + " destroyed.");
					}
					ParentObject.Destroy(null, Silent: true);
				}
			}
		}
		return base.FireEvent(E);
	}
}
