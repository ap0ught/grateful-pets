using System;
using XRL.Core;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Prone : Effect, ITierInitialized
{
	public bool Voluntary;

	public bool StartMessageUsePopup;

	public bool StopMessageUsePopup;

	public GameObject LyingOn;

	public Prone()
	{
		Duration = 1;
		DisplayName = "{{C|prone}}";
	}

	public Prone(bool Voluntary = false, bool StartMessageUsePopup = false, bool StopMessageUsePopup = false, GameObject LyingOn = null)
		: this()
	{
		this.Voluntary = Voluntary;
		this.StartMessageUsePopup = StartMessageUsePopup;
		this.StopMessageUsePopup = StopMessageUsePopup;
		this.LyingOn = LyingOn;
		if (LyingOn != null)
		{
			DisplayName = "{{C|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		}
	}

	public override int GetEffectType()
	{
		int num = 117440640;
		if (Voluntary)
		{
			num |= 0x8000000;
		}
		return num;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		CheckLyingOn();
		if (LyingOn != null)
		{
			return "{{C|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		}
		return "{{C|prone}}";
	}

	private string EffectSummary()
	{
		return "-6 Agility.\n-5 DV.\n-80 move speed.\nMust spend a turn to stand up.";
	}

	public override string GetDetails()
	{
		if (LyingOn != null)
		{
			return LyingOn.DisplayName + ":\n" + EffectSummary();
		}
		return EffectSummary();
	}

	public bool IsLyingOnValid()
	{
		if (!GameObject.Validate(ref LyingOn))
		{
			return false;
		}
		if (LyingOn.CurrentCell == null)
		{
			return false;
		}
		if (!GameObject.Validate(Object))
		{
			return false;
		}
		if (Object.CurrentCell == null)
		{
			return false;
		}
		if (LyingOn.CurrentCell != Object.CurrentCell)
		{
			return false;
		}
		return true;
	}

	public bool CheckLyingOn()
	{
		bool flag = IsLyingOnValid();
		if (!flag || LyingOn == null)
		{
			if (LyingOn != null)
			{
				LyingOn = null;
			}
			DisplayName = "{{C|prone}}";
			return flag;
		}
		DisplayName = "{{C|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
		return flag;
	}

	public static bool LimbSupportsProneness(BodyPart Part)
	{
		if (Part.Type != "Feet" && Part.Type != "Roots")
		{
			return false;
		}
		if (Part.Mobility <= 0)
		{
			return false;
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Prone>())
		{
			return false;
		}
		if (!Object.HasBodyPart(LimbSupportsProneness))
		{
			return false;
		}
		if (IsRootedInPlaceEvent.Check(Object))
		{
			return false;
		}
		if (!Object.CanChangeBodyPosition("Prone", ShowMessage: false, !Voluntary))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyProne"))
		{
			return false;
		}
		Object.RemoveEffect<Sitting>();
		if (Voluntary)
		{
			if (LyingOn != null && Object.IsPlayerControlled())
			{
				DidXToY("lie", "down on", LyingOn, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
			}
			else
			{
				DidX("lie", "down", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
			}
		}
		else
		{
			DidX("are", "knocked prone", "!", null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
			Object.ParticleText("*knocked prone*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		}
		Object.BodyPositionChanged(null, !Voluntary);
		Cell cell = Object.CurrentCell;
		if (cell != null)
		{
			ObjectGoingProneEvent.Send(Object, cell, Voluntary, StartMessageUsePopup);
		}
		ApplyStats();
		Object.ForfeitTurn(EnergyNeutral: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		foreach (Immobilized item in Object.YieldEffects<Immobilized>())
		{
			if (item != null && item.LinkedToProne)
			{
				item.EndImmobilization();
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != SingletonEvent<CommandTakeActionEvent>.ID || Voluntary) && (ID != LeaveCellEvent.ID || !Voluntary))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (Duration > 0 && !Voluntary && !Object.HasEffect<Asleep>() && Object.FireEvent("CanStandUp") && Object.CanChangeBodyPosition("Standing"))
		{
			Duration--;
			if (Duration > 0)
			{
				Object.UseEnergy(1000);
				return false;
			}
			StandUp();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeaveCellEvent E)
	{
		if (Voluntary && Duration > 0)
		{
			StandUp();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			CheckLyingOn();
			if (LyingOn != null)
			{
				E.AddTag("[{{B|lying on " + LyingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}]");
			}
			else
			{
				E.AddTag("[{{B|prone}}]");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "Agility", -6);
		StatShifter.SetStatShift(Object, "DV", -5);
		StatShifter.SetStatShift(Object, "MoveSpeed", 80);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	private void StandUp(bool UsePopup = false)
	{
		CheckLyingOn();
		if (LyingOn != null && Object.IsPlayerControlled())
		{
			if (LyingOn.HasPart(typeof(Bed)))
			{
				DidXToY("rise", "from", LyingOn, null, null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup || StopMessageUsePopup);
			}
			else
			{
				DidXToY("stand", "up from", LyingOn, null, null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup || StopMessageUsePopup);
			}
		}
		else
		{
			DidX("stand", "up", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup || StopMessageUsePopup);
		}
		Object.UseEnergy(1000, "Position");
		Object.RemoveEffect(this);
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_standUp");
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "_";
			E.ColorString = (Voluntary ? "&c" : "&R");
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanChangeBodyPosition")
		{
			if (!E.HasFlag("Involuntary") && E.GetStringParameter("To") != "Asleep" && E.GetStringParameter("To") != "Standing" && (!Object.FireEvent("CanStandUp") || !Object.FireEvent("CanStandUpFromProne")))
			{
				return false;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (Duration > 0 && E.GetStringParameter("To") != "Asleep" && E.GetStringParameter("To") != "Standing")
			{
				StandUp();
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
