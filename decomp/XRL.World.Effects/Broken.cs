using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Broken : Effect, ITierInitialized, IBusted
{
	public bool FromDamage;

	public bool FromExamine;

	public bool FromOverload;

	public bool FromModding;

	public Broken()
	{
		DisplayName = "{{r|broken}}";
		Duration = 1;
	}

	public Broken(bool FromDamage = false, bool FromExamine = false, bool FromOverload = false, bool FromModding = false)
		: this()
	{
		this.FromDamage = FromDamage;
		this.FromExamine = FromExamine;
		this.FromOverload = FromOverload;
		this.FromModding = FromModding;
	}

	public override int GetEffectType()
	{
		return 100664320;
	}

	public override string GetDetails()
	{
		return "Can't be equipped or used.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsBroken())
		{
			return false;
		}
		if (!Object.HasTagOrProperty("Breakable"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyBroken"))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_mechanicalRupture");
		GameObject holder = Object.Holder;
		if (holder != null)
		{
			holder.ParticleText("*" + Object.ShortDisplayNameStripped + " broken*", IComponent<GameObject>.ConsequentialColorChar(null, holder));
			Event obj = Event.New("CommandUnequipObject");
			obj.SetParameter("BodyPart", holder.FindEquippedObject(Object));
			obj.SetFlag("SemiForced", State: true);
			holder.FireEvent(obj);
		}
		else
		{
			Object.ParticleText("*" + Object.ShortDisplayNameStripped + " broken*", 'R');
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<IsRepairableEvent>.ID)
		{
			return ID == PooledEvent<RepairedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			E.AddTag("[{{r|broken}}]", 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.01);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginBeingEquipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped")
		{
			string text = "You can't equip " + Object.t() + ", " + Object.itis + " broken!";
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				E.SetParameter("FailureMessage", text);
			}
			else if (E.GetGameObjectParameter("Equipper").IsPlayer())
			{
				Popup.ShowFail(text);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
