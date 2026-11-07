using System;

namespace XRL.World.Parts;

[Serializable]
public class JumpBoost : IActivePart
{
	public string AbilityName;

	public int RangeModifier;

	public int MinimumRange;

	public int EventPriority;

	public bool CanJumpOverCreatures;

	public bool Stack;

	public JumpBoost()
	{
		WorksOnWearer = true;
		WorksOnEquipper = true;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != PooledEvent<GetJumpingBehaviorEvent>.ID && ID != PooledEvent<JumpedEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetJumpingBehaviorEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (Stack)
			{
				E.CanJumpOverCreatures |= CanJumpOverCreatures;
				E.RangeModifier += RangeModifier;
				E.MinimumRange += MinimumRange;
				E.Stats?.AddLinearBonusModifier("Range", RangeModifier, ParentObject.BaseDisplayName);
			}
			else if (E.Priority == 0 || E.Priority < EventPriority)
			{
				E.AbilityName = AbilityName.Coalesce(E.AbilityName);
				E.CanJumpOverCreatures = CanJumpOverCreatures;
				E.RangeModifier = RangeModifier;
				E.MinimumRange = MinimumRange;
				E.Priority = EventPriority;
				E.Stats?.AddLinearBonusModifier("Range", RangeModifier, ParentObject.BaseDisplayName);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(JumpedEvent E)
	{
		if (E.Pass == JumpedEvent.PASSES)
		{
			ConsumeChargeIfOperational();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (RangeModifier != 0)
		{
			E.Postfix.Compound("{{rules|", '\n').Append("This item ").Append((RangeModifier > 0) ? "increases" : "decreases")
				.Append(" jump distance by ")
				.Append(Math.Abs(RangeModifier))
				.Append(".}}");
		}
		if (MinimumRange != 0)
		{
			E.Postfix.Compound("{{rules|", '\n').AppendSigned(MinimumRange).Append(" minimum jump distance")
				.Append("}}");
		}
		if (CanJumpOverCreatures)
		{
			E.Postfix.Compound("{{rules|", '\n').Append("You can jump over creatures.").Append("}}");
		}
		return base.HandleEvent(E);
	}
}
