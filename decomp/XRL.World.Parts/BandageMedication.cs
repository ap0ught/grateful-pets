using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BandageMedication : IPart
{
	public string Performance = "1d4+4";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != AIGetPassiveItemListEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply" && !PerformBandaging(E.Actor, E.ObjectTarget))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		AIBandageUsage(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetPassiveItemListEvent E)
	{
		AIBandageUsage(E);
		return base.HandleEvent(E);
	}

	public bool PerformBandaging(GameObject Actor, GameObject Target = null)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return false;
		}
		if (!Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		if (Target == null)
		{
			if (Actor.IsPlayer() && AnyBleedingNear(Actor))
			{
				Cell cell = Actor.Physics.PickDirection("Bandage whom?");
				if (cell == null)
				{
					return false;
				}
				Target = ((cell == Actor.CurrentCell) ? Actor : cell.GetCombatTarget(Actor, IgnoreFlight: false, IgnoreAttackable: true, IgnorePhase: true, 0, null, null, null, null, null, AllowInanimate: false));
				if (Target == null)
				{
					Target = cell.GetCombatTarget(Actor, IgnoreFlight: true, IgnoreAttackable: true, IgnorePhase: true, 0, null, null, null, null, null, AllowInanimate: false);
					if (Target != null)
					{
						Actor.Fail("You cannot reach " + Target.t() + " to bandage " + Target.its + " wounds.");
					}
					else
					{
						Actor.Fail("There's no one there.");
					}
					return false;
				}
			}
			else
			{
				Target = Actor;
			}
		}
		if (Target != Actor && Target.IsHostileTowards(Actor) && Target.CanMoveExtremities())
		{
			Actor.Fail(Target.Does("won't") + " let you bandage " + Target.them + ".");
			return false;
		}
		Bleeding bleeding = FindBandageableWound(Target, out var HasBandaged, out var HasUntreatable);
		if (bleeding == null)
		{
			if (HasBandaged & HasUntreatable)
			{
				Actor.Fail("All of " + Target.poss("wounds") + " that can be staunched have been already.");
			}
			else if (HasBandaged)
			{
				Actor.Fail(Target.Poss("wounds") + " have been bandaged.");
			}
			else if (HasUntreatable)
			{
				Actor.Fail(Target.Poss("wounds") + " are too deep to bandage.");
			}
			else
			{
				Actor.Fail(Target.Does("are") + " not bleeding.");
			}
			return false;
		}
		int basePerformance = Performance.RollCached();
		ParentObject.SplitFromStack();
		int reduceSaveTargetBy = GetBandagePerformanceEvent.GetFor(ParentObject, Actor, Target, basePerformance);
		bool flag = false;
		if (ParentObject.HasPropertyOrTag("MessageAsBandage"))
		{
			if (Actor != Target && !Actor.PhaseMatches(Target))
			{
				IComponent<GameObject>.XDidYToZ(Actor, "try", "to bandage", Target, "wounds, but " + ParentObject.does("pass", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " through " + Target.them, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			}
			else if (Actor != Target && Target.IsInStasis())
			{
				IComponent<GameObject>.XDidYToZ(Actor, "try", "to bandage", Target, "wounds, but cannot affect " + Target.them, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			}
			else
			{
				IComponent<GameObject>.XDidYToZ(Actor, "bandage", Target, "wounds", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
				flag = true;
			}
		}
		else if (Actor != Target && !Actor.PhaseMatches(Target))
		{
			IComponent<GameObject>.WDidXToYWithZ(Actor, "try", "to staunch", Target, "wounds with", ParentObject, ", but " + ParentObject.does("pass", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " through " + Target.them, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: true, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: true, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		}
		else if (Actor != Target && Target.IsInStasis())
		{
			IComponent<GameObject>.WDidXToYWithZ(Actor, "try", "to staunch", Target, "wounds with", ParentObject, ", but cannot affect " + Target.them, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: true, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: true, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		}
		else
		{
			IComponent<GameObject>.WDidXToYWithZ(Actor, "staunch", Target, "wounds with", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: true, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: true, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			flag = true;
		}
		if (flag)
		{
			Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_bandage_apply");
			bleeding.Bandaged = true;
			bool flag2 = false;
			if (!bleeding.StopMessageUsePopup && (Actor.IsPlayer() || Target.IsPlayer()))
			{
				bleeding.StopMessageUsePopup = false;
				flag2 = false;
			}
			if (!bleeding.RecoveryChance(reduceSaveTargetBy, ReduceFirst: true) & flag2)
			{
				bleeding.StopMessageUsePopup = false;
			}
			ParentObject.Destroy();
		}
		else
		{
			ParentObject.CheckStack();
		}
		Actor.UseEnergy(1000, "Physical Item Bandage");
		return true;
	}

	public static bool AnyBleedingIn(Cell Cell)
	{
		if (Cell != null)
		{
			int i = 0;
			for (int count = Cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = Cell.Objects[i];
				if (gameObject._Effects == null)
				{
					continue;
				}
				int j = 0;
				for (int count2 = gameObject.Effects.Count; j < count2; j++)
				{
					if (gameObject.Effects[j] is Bleeding)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool AnyBleedingNear(Cell Cell)
	{
		if (Cell != null)
		{
			foreach (Cell localAdjacentCell in Cell.GetLocalAdjacentCells())
			{
				if (AnyBleedingIn(localAdjacentCell))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AnyBleedingNear(GameObject Object)
	{
		return AnyBleedingNear(Object.CurrentCell);
	}

	public static Bleeding FindBandageableWound(GameObject Subject, out bool HasBandaged, out bool HasUntreatable)
	{
		HasBandaged = false;
		HasUntreatable = false;
		Bleeding bleeding = null;
		if (Subject._Effects != null)
		{
			int i = 0;
			for (int count = Subject.Effects.Count; i < count; i++)
			{
				if (Subject.Effects[i] is Bleeding bleeding2)
				{
					if (bleeding2.Bandaged)
					{
						HasBandaged = true;
					}
					else if (bleeding2.Internal)
					{
						HasUntreatable = true;
					}
					else if (bleeding == null || bleeding2.Damage.GetCachedDieRoll().Average() > bleeding.Damage.GetCachedDieRoll().Average())
					{
						bleeding = bleeding2;
					}
				}
			}
		}
		return bleeding;
	}

	public static bool HasBandageableWound(GameObject Subject)
	{
		if (Subject._Effects != null)
		{
			int i = 0;
			for (int count = Subject.Effects.Count; i < count; i++)
			{
				if (Subject.Effects[i] is Bleeding { Bandaged: false, Internal: false })
				{
					return true;
				}
			}
		}
		return false;
	}

	public static GameObject FindCreatureWithBandageableWound(GameObject Actor, Cell Cell)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return null;
		}
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = Cell.Objects[i];
			if (gameObject != Actor && gameObject.IsCreature && HasBandageableWound(gameObject) && (Actor.IsLedBy(gameObject) || gameObject.IsLedBy(Actor) || Actor.IsAlliedTowards(gameObject) || gameObject.IsAlliedTowards(Actor)) && gameObject.PhaseMatches(Actor) && !gameObject.IsInStasis() && (FungalVisionary.VisionLevel > 0 || !gameObject.HasPart<FungalVision>()))
			{
				return gameObject;
			}
		}
		return null;
	}

	public void AIBandageUsage(IAICommandListEvent E)
	{
		bool flag = false;
		if (HasBandageableWound(E.Actor))
		{
			if (!flag && !E.Actor.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
			{
				return;
			}
			flag = true;
			E.Add("Apply", 2, ParentObject, Inv: true);
		}
		Cell cell = E.Actor.CurrentCell;
		if (cell == null)
		{
			return;
		}
		GameObject gameObject = FindCreatureWithBandageableWound(E.Actor, cell);
		if (gameObject != null)
		{
			if (!flag && !E.Actor.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
			{
				return;
			}
			flag = true;
			E.Add("Apply", Object: ParentObject, Priority: (!E.Actor.IsInLoveWith(gameObject)) ? 1 : 10, Inv: true, Self: false, TargetOverride: gameObject);
		}
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
		{
			GameObject gameObject2 = FindCreatureWithBandageableWound(E.Actor, localAdjacentCell);
			if (gameObject2 != null)
			{
				if (!flag && !E.Actor.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
				{
					break;
				}
				flag = true;
				E.Add("Apply", Object: ParentObject, Priority: (!E.Actor.IsInLoveWith(gameObject2)) ? 1 : 10, Inv: true, Self: false, TargetOverride: gameObject2);
			}
		}
	}
}
