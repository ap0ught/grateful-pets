using System;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class HelpingHands : IPart
{
	public string ManagerID => ParentObject.ID + "::HelpingHands";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDismemberEvent>.ID && ID != ExamineCriticalFailureEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDismemberEvent E)
	{
		if (E.IsObject && E.Part?.Manager != null && E.Part.Manager == ManagerID)
		{
			ParentObject.ApplyEffect(new Broken());
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsWorn())
		{
			AddArms(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		RemoveArms(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary && E.BodyPart?.Manager != null && E.BodyPart.Manager == ManagerID)
		{
			E.Chance -= 7;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void AddArms(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		if (Subject != null)
		{
			Body body = Subject.Body;
			if (body != null)
			{
				BodyPart body2 = body.GetBody();
				bool? extrinsic = true;
				string managerID = ManagerID;
				string[] orInsertBefore = new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" };
				BodyPart bodyPart = body2.AddPartAt("Robo-Arm", 2, null, null, null, null, managerID, null, null, null, null, null, null, null, extrinsic, null, null, null, null, null, "Arm", orInsertBefore);
				bodyPart.AddPart("Robo-Hand", 2, null, "Robo-Hands", null, null, Extrinsic: true, Manager: ManagerID);
				body2.AddPartAt(bodyPart, "Robo-Arm", 1, null, null, null, null, Extrinsic: true, Manager: ManagerID).AddPart("Robo-Hand", 1, null, "Robo-Hands", null, null, Extrinsic: true, Manager: ManagerID);
				extrinsic = true;
				string managerID2 = ManagerID;
				orInsertBefore = new string[3] { "Feet", "Roots", "Thrown Weapon" };
				body2.AddPartAt("Robo-Hands", 0, null, null, "Robo-Hands", null, managerID2, null, null, null, null, null, null, null, extrinsic, null, null, null, null, null, "Hands", orInsertBefore);
			}
		}
	}

	public void RemoveArms(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		Subject?.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		Subject?.WantToReequip();
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && !IsBroken() && !IsRusted())
		{
			EmitMessage("Two tubular sections of " + ParentObject.t() + " flail around and batter " + (E.Actor.IsPlayer() ? "you" : E.Actor.t()) + "!", ' ', FromDialog: false, E.Actor.IsPlayer());
			E.Actor.TakeDamage("3d4".RollCached(), "from %t pummeling!", "Melee Unarmed", null, null, E.Actor, null, ParentObject, null, null, Accidental: false, Environmental: false, Indirect: true, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, E.Actor.IsPlayer());
			E.Identify = true;
			return true;
		}
		return false;
	}
}
