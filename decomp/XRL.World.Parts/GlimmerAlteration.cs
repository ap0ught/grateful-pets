using System;
using XRL.Language;

namespace XRL.World.Parts;

/// <remarks>
/// <para>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, glimmer alteration is increased in magnitude
/// by a percentage equal to ((power load - 100) / 10), i.e. 30% for the
/// standard overload power load of 400.
/// </para>
/// </remarks>
[Serializable]
public class GlimmerAlteration : IPoweredPart
{
	public int Amount;

	public bool ShowInShortDescription;

	public GlimmerAlteration()
	{
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		GlimmerAlteration glimmerAlteration = p as GlimmerAlteration;
		if (glimmerAlteration.Amount != Amount)
		{
			return false;
		}
		if (glimmerAlteration.ShowInShortDescription != ShowInShortDescription)
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
			ConsumeChargeIfOperational();
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != EquippedEvent.ID && (ID != PooledEvent<GetPsychicGlimmerEvent>.ID || Amount == 0) && (ID != GetShortDescriptionEvent.ID || !ShowInShortDescription))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		if (Amount != 0 && IsObjectActivePartSubject(E.Subject) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Level += GetEffectiveAmount();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			string text = null;
			int effectiveAmount = GetEffectiveAmount();
			if (effectiveAmount > 0)
			{
				text = "Increases psychic glimmer in the " + Grammar.Ordinal(effectiveAmount) + " degree.";
			}
			else if (effectiveAmount < 0)
			{
				text = "Reduces psychic glimmer in the " + Grammar.Ordinal(-effectiveAmount) + " degree.";
			}
			if (!string.IsNullOrEmpty(text))
			{
				E.Postfix.AppendRules(text);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (Amount != 0)
		{
			E.Actor.SyncMutationLevelAndGlimmer();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (Amount != 0)
		{
			E.Actor.SyncMutationLevelAndGlimmer();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		if (Amount != 0)
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				activePartSubject.SyncMutationLevelAndGlimmer();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int GetEffectiveAmount()
	{
		int num = Amount;
		int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return num;
	}
}
