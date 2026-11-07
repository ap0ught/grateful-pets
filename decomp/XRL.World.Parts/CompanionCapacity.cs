using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class CompanionCapacity : IActivePart
{
	public int Proselytized;

	public int Beguiled;

	public CompanionCapacity()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<GetCompanionLimitEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Proselytized != 0)
		{
			string text = ((Math.Abs(Proselytized) == 1) ? "follower" : "followers");
			string text2 = ((Proselytized < 0) ? "fewer" : "additional");
			E.Postfix.Compound("{{rules|", '\n').Append("You may Proselytize " + Grammar.Cardinal(Math.Abs(Proselytized)) + " " + text2 + " " + text + ".}}");
		}
		if (Beguiled > 0)
		{
			string text3 = ((Math.Abs(Beguiled) == 1) ? "follower" : "followers");
			string text4 = ((Beguiled < 0) ? "fewer" : "additional");
			E.Postfix.Compound("{{rules|", '\n').Append("You may Beguile " + Grammar.Cardinal(Math.Abs(Beguiled)) + " " + text4 + " " + text3 + ".}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionLimitEvent E)
	{
		if (E.Means == "Beguiling")
		{
			if (Beguiled != 0 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.Limit += Beguiled;
			}
		}
		else if (E.Means == "Proselytize" && Proselytized != 0 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Limit += Proselytized;
		}
		return base.HandleEvent(E);
	}
}
