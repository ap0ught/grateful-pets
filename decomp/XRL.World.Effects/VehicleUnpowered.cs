using System;
using System.Text;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class VehicleUnpowered : Effect
{
	public int ChargeMinimum;

	public VehicleUnpowered()
	{
		DisplayName = "{{K|unpowered}}";
		Duration = 1;
	}

	public VehicleUnpowered(int ChargeMinimum)
		: this()
	{
		this.ChargeMinimum = ChargeMinimum;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasEffect(typeof(VehicleUnpowered)))
		{
			return base.Apply(Object);
		}
		return false;
	}

	public override int GetEffectType()
	{
		return 1;
	}

	public override string GetDetails()
	{
		return "Can't take actions.";
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.GetCurrentFrameAtFPS(60) % 300;
		if (num >= 80 && num < 180)
		{
			E.Tile = "Items/sw_power_cut_small.png";
			E.ColorString = "&W";
			E.DetailColor = "R";
		}
		return base.Render(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CanMoveExtremities");
		Registrar.Register("IsMobile");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanChangeBodyPosition" || E.ID == "CanMoveExtremities")
		{
			if (E.HasFlag("ShowMessage"))
			{
				PreventActionMessage(Object);
			}
			return false;
		}
		if (E.ID == "IsMobile")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public void PreventActionMessage(GameObject Actor)
	{
		if (!Actor.IsPlayer())
		{
			return;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Object.TryGetPart<EnergyCellSocket>(out var Part))
		{
			if (GameObject.Validate(Part.Cell))
			{
				stringBuilder.Append(Part.Cell.Does("are")).Append(" drained or nearly drained.\n\n").Append("Recharge or replace " + Part.Cell.it + " to power ")
					.Append(Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: false))
					.Append('.');
			}
			else
			{
				stringBuilder.Append("Insert ").Append(Grammar.A(Part.GetSlotTypeName())).Append(" to power ")
					.Append(Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: false))
					.Append('.');
			}
		}
		else
		{
			stringBuilder.Append(Object.Does("lack", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: false)).Append(" the power to act.");
		}
		Popup.ShowFail(Event.FinalizeString(stringBuilder));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != CellChangedEvent.ID)
		{
			return ID == LeaveCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (E.Object == Object)
		{
			if (E.ShowMessage)
			{
				PreventActionMessage(E.Object);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == Object && !CheckCharge() && !E.Object.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckCharge();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeaveCellEvent E)
	{
		if (E.Object == Object && !E.Forced && !E.System && E.Dragging == null)
		{
			PreventActionMessage(E.Object);
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool CheckCharge()
	{
		if (ChargeMinimum > 0 && Object.TestCharge(ChargeMinimum, LiveOnly: false, 0L))
		{
			Object.RemoveEffect(this);
			return true;
		}
		return false;
	}
}
