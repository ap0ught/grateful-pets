using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TimeCube : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivateTimeCube", null, 'a', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTimeCube")
		{
			Activate(E.Actor);
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
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

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("time", 20);
		}
		return base.HandleEvent(E);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && Activate(E.Actor, SilentIfFailed: true, E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool Activate(GameObject Actor, bool SilentIfFailed = false, IExamineEvent Identify = null)
	{
		if (!IComponent<GameObject>.CheckRealityDistortionUsability(Actor, null, Actor, ParentObject) || !IComponent<GameObject>.CheckRealityDistortionUsability(ParentObject, null, Actor, ParentObject))
		{
			if (!SilentIfFailed)
			{
				if (Actor.IsPlayer())
				{
					IComponent<GameObject>.PlayUISound("Sounds/Abilities/sfx_ability_longBeam_attack_chargeUp");
					Popup.Show("{{R|Fraudulent}} {{W|ONEness}} is taught by {{O|evil}} {{G|educators}}! Nothing happens!");
				}
				Actor.UseEnergy(1000, "Item Failure");
			}
			return false;
		}
		Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_timeCube_activate");
		if (Actor.IsPlayer())
		{
			Achievement.ACTIVATE_TIMECUBE.Unlock();
		}
		Actor.ApplyEffect(new TimeCubed());
		Identify?.IdentifyImmediately();
		ParentObject.Destroy();
		return true;
	}
}
