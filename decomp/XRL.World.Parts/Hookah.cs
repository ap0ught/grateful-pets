using System;

namespace XRL.World.Parts;

[Serializable]
public class Hookah : IPart
{
	public static readonly string COMMAND_NAME = "SmokeHookah";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<IdleQueryEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Smoke", "smoke", COMMAND_NAME, null, 's', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME && SmokeHookah(E.Actor, !E.Auto))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return !SmokeHookah(E.Actor, FromDialog: false);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (E.Actor.HasPart<Brain>() && !E.Actor.HasPart<Robot>() && E.Actor.DistanceTo(ParentObject) <= 1 && 10.in100() && SmokeHookah(E.Actor, FromDialog: false))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool CanPuff()
	{
		return ParentObject.LiquidVolume?.IsPureLiquid("water") ?? false;
	}

	public bool SmokeHookah(GameObject Actor, bool FromDialog)
	{
		if (!CanPuff())
		{
			return Actor.Fail(ParentObject.Does("need") + " water in " + ParentObject.them + ".");
		}
		if (!BeforeConsumeEvent.Check(Actor, Actor, ParentObject, Eat: false, Drink: false, Inject: false, Inhale: true))
		{
			return false;
		}
		PlayWorldSound("sfx_interact_hookah_smoke");
		IComponent<GameObject>.XDidYToZ(Actor, "take", "a puff on", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog);
		Actor.UseEnergy(1000, "Item");
		for (int i = 2; i < 5; i++)
		{
			ParentObject.SmokePuff();
		}
		if (Actor.IsPlayer())
		{
			Achievement.SMOKE_HOOKAH.Unlock();
		}
		Event e = Event.New("Smoked", "Actor", Actor, "Object", ParentObject);
		ParentObject.FireEvent(e);
		Actor.FireEvent(e);
		AfterConsumeEvent.Send(Actor, Actor, ParentObject, Eat: false, Drink: false, Inject: false, Inhale: true);
		return true;
	}
}
