using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Rusted : Effect, ITierInitialized, IBusted
{
	public static readonly int ICON_COLOR_FOREGROUND_PRIORITY = 60;

	public static readonly int ICON_COLOR_DETAIL_PRIORITY = 120;

	public bool StartMessageUsePopup;

	public Rusted()
	{
		DisplayName = "{{r|rusted}}";
		Duration = 1;
	}

	public Rusted(bool StartMessageUsePopup = false)
		: this()
	{
		this.StartMessageUsePopup = StartMessageUsePopup;
	}

	public override int GetEffectType()
	{
		return 117441536;
	}

	public override string GetDetails()
	{
		if (Object.IsCreature)
		{
			return "-70 Quickness\nDoesn't function properly.";
		}
		return "Doesn't function properly and can't be equipped.\nWill erode into dust if rusted again.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart<Metal>())
		{
			return false;
		}
		if (!Object.FireEvent("ApplyRusted"))
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_rusted");
		if (Object.HasEffect<Rusted>())
		{
			if (Object.IsCreature)
			{
				return false;
			}
			GameObject gameObject = Object.Holder ?? Object;
			if (IComponent<GameObject>.Visible(gameObject))
			{
				gameObject.ParticleText("*" + Object.DisplayNameOnlyStripped + " reduced to dust*", 'r');
			}
			DidX("are", "reduced to dust", "!", null, null, null, gameObject, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
			Object.Die(null, null, "You were reduced to dust.", Object.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " @@reduced to dust.", Accidental: false, null, null, Force: false, AlwaysUsePopups: false, "");
		}
		else
		{
			GameObject equipped = Object.Equipped;
			GameObject gameObject2 = equipped ?? Object.InInventory ?? Object;
			if (IComponent<GameObject>.Visible(gameObject2))
			{
				gameObject2.ParticleText("*" + Object.DisplayNameOnlyStripped + " rusted*", 'r');
			}
			DidX("rust", null, null, null, null, null, gameObject2, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
			equipped?.FireEvent(Event.New("CommandUnequipObject", "BodyPart", equipped.FindEquippedObject(Object), "SemiForced", 1));
			ApplyStats();
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<IsRepairableEvent>.ID)
		{
			return ID == PooledEvent<RepairedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddTag("[{{r|rusted}}]", 20);
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

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeginBeingEquipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped")
		{
			string text = "You can't equip " + Object.t() + ", " + Object.itis + " badly rusted!";
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				E.SetParameter("FailureMessage", text);
			}
			else if (E.GetGameObjectParameter("Equipper").IsPlayer())
			{
				Popup.Show(text);
			}
			return false;
		}
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&r", "w", ICON_COLOR_FOREGROUND_PRIORITY, ICON_COLOR_DETAIL_PRIORITY);
		return base.Render(E);
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "Speed", -70);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}
}
