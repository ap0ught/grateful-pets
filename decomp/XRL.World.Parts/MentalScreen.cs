using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class MentalScreen : IPoweredPart
{
	public string PercentageReduction;

	public string LinearReduction;

	public string IncludeTypes = "Psionic";

	public string ExcludeTypes;

	public float ComputePowerFactor;

	private int _IncludeBitTypes = int.MinValue;

	private int _ExcludeBitTypes = int.MinValue;

	public int IncludeBitTypes
	{
		get
		{
			if (_IncludeBitTypes == int.MinValue)
			{
				_IncludeBitTypes = Mental.TypeExpansion(IncludeTypes);
			}
			return _IncludeBitTypes;
		}
	}

	public int ExcludeBitTypes
	{
		get
		{
			if (_ExcludeBitTypes == int.MinValue)
			{
				_ExcludeBitTypes = Mental.TypeExpansion(IncludeTypes);
			}
			return _ExcludeBitTypes;
		}
	}

	public MentalScreen()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		MentalScreen mentalScreen = p as MentalScreen;
		if (mentalScreen.LinearReduction != LinearReduction)
		{
			return false;
		}
		if (mentalScreen.PercentageReduction != PercentageReduction)
		{
			return false;
		}
		if (mentalScreen.IncludeTypes != IncludeTypes)
		{
			return false;
		}
		if (mentalScreen.ExcludeTypes != ExcludeTypes)
		{
			return false;
		}
		if (mentalScreen.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == BeforeMentalDefendEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if ((WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier || WorksOnImplantee) && (!string.IsNullOrEmpty(PercentageReduction) || !string.IsNullOrEmpty(LinearReduction)))
		{
			E.Postfix.Append("\n{{rules|");
			bool flag = false;
			bool flag2 = false;
			if (!string.IsNullOrEmpty(PercentageReduction))
			{
				int num = PercentageReduction.RollMin();
				int num2 = PercentageReduction.RollMax();
				if (num >= 0 && num2 > 0)
				{
					E.Postfix.Append("Provides ").Append(num);
					if (num != num2)
					{
						E.Postfix.Append('-').Append(num2);
					}
					E.Postfix.Append('%');
					flag = true;
				}
				else if (num < 0 && num2 <= 0)
				{
					E.Postfix.Append("Causes ").Append(-num2).Append('%');
					if (num != num2)
					{
						E.Postfix.Append(" to ").Append(-num).Append('%');
					}
					flag2 = true;
				}
				else if (num != 0 || num2 != 0)
				{
					E.Postfix.Append("Confers ").Append(num).Append('%');
					if (num != num2)
					{
						E.Postfix.Append(" to ").Append(num2).Append('%');
					}
					flag = true;
					flag2 = true;
				}
			}
			if (!string.IsNullOrEmpty(LinearReduction))
			{
				int num3 = LinearReduction.RollMin();
				int num4 = LinearReduction.RollMax();
				if (num3 >= 0 && num4 > 0)
				{
					if (flag2)
					{
						E.Postfix.Append(" vulnerability");
					}
					E.Postfix.Append((flag | flag2) ? " plus " : "Provides ");
					E.Postfix.Append(num3);
					if (num3 != num4)
					{
						E.Postfix.Append('-').Append(num4);
					}
					E.Postfix.Append(' ').Append((num3 != 1 || num4 != 1) ? "points" : "point");
					if (!flag && !flag2)
					{
						E.Postfix.Append(" of");
					}
					E.Postfix.Append(" resistance");
					flag = true;
				}
				else if (num3 < 0 && num4 <= 0)
				{
					if (flag)
					{
						E.Postfix.Append(" resistance");
					}
					E.Postfix.Append((flag | flag2) ? " minus " : "Causes ");
					E.Postfix.Append(-num4);
					if (num3 != num4)
					{
						E.Postfix.Append('-').Append(-num3);
					}
					E.Postfix.Append(' ').Append((num3 != -1 || num4 != -1) ? "points" : "point");
					if (!flag && !flag2)
					{
						E.Postfix.Append(" of");
					}
					E.Postfix.Append(" vulnerability");
					flag2 = true;
				}
				else if (num3 != 0 || num4 != 0)
				{
					if (!flag || !flag2)
					{
						if (flag)
						{
							E.Postfix.Append(" resistance");
						}
						if (flag2)
						{
							E.Postfix.Append(" vulnerability");
						}
					}
					E.Postfix.Append((flag | flag2) ? " plus " : "Confers ");
					E.Postfix.Append(num3);
					if (num3 != num4)
					{
						E.Postfix.Append(" to ").Append(num4);
					}
					E.Postfix.Append(' ').Append((num3 != num4 && num4 != 1 && num4 != -1) ? "points" : "point");
					if (!flag && !flag2)
					{
						E.Postfix.Append(" of");
					}
					E.Postfix.Append(" resistance/vulnerability");
					flag = true;
					flag2 = true;
				}
			}
			else if (flag & flag2)
			{
				E.Postfix.Append(" resistance/vulnerability");
			}
			else
			{
				if (flag)
				{
					E.Postfix.Append(" resistance");
				}
				if (flag2)
				{
					E.Postfix.Append(" vulnerability");
				}
			}
			if (flag | flag2)
			{
				E.Postfix.Append(" to ");
				if (!string.IsNullOrEmpty(IncludeTypes))
				{
					E.Postfix.Append("some forms of ");
				}
				else if (!string.IsNullOrEmpty(ExcludeTypes))
				{
					E.Postfix.Append("most forms of ");
				}
				E.Postfix.Append("mental intrusion.");
			}
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeMentalDefendEvent E)
	{
		if (IsObjectActivePartSubject(E.Defender) && WasReady() && Applicable(E.Type) && E.Magnitude != int.MinValue)
		{
			if (!string.IsNullOrEmpty(PercentageReduction))
			{
				E.Magnitude = E.Magnitude * (100 - GetAvailableComputePowerEvent.AdjustUp(this, PercentageReduction.RollCached(), ComputePowerFactor)) / 100;
			}
			if (!string.IsNullOrEmpty(LinearReduction))
			{
				E.Magnitude -= GetAvailableComputePowerEvent.AdjustUp(this, LinearReduction.RollCached(), ComputePowerFactor);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool Applicable(int Type)
	{
		if (IncludeBitTypes.HasBit(Type))
		{
			return !ExcludeBitTypes.HasBit(Type);
		}
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}
}
