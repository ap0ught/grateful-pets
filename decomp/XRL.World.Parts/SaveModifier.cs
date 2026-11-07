using System;
using System.Text;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SaveModifier : IActivePart
{
	public string Vs;

	public int Amount = 1;

	public bool ShowInShortDescription = true;

	public SaveModifier()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		SaveModifier saveModifier = p as SaveModifier;
		if (saveModifier.Vs != Vs)
		{
			return false;
		}
		if (saveModifier.Amount != Amount)
		{
			return false;
		}
		if (saveModifier.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount1)
	{
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription && (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier))
		{
			E.Postfix.AppendRules(AppendRulesDescription, GetEventSensitiveAddStatusSummary(E));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (IsObjectActivePartSubject(E.Defender) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && SavingThrows.Applicable(Vs, E))
		{
			E.Roll += Amount;
		}
		return base.HandleEvent(E);
	}

	private void AppendRulesDescription(StringBuilder SB)
	{
		SavingThrows.AppendSaveBonusDescription(SB, Amount, Vs, HighlightNumber: false, Highlight: false, LeadingNewline: false);
	}
}
