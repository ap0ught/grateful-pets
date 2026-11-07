using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ReduceEnergyCosts : IPoweredPart
{
	public string PercentageReduction;

	public string LinearReduction;

	public string IncludeTypes;

	public string ExcludeTypes;

	public string ScopeDescription;

	public bool UsesChargePerTurn;

	public bool UsesChargePerEffect;

	public bool GenerateShortDescription = true;

	public ReduceEnergyCosts()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		ReduceEnergyCosts reduceEnergyCosts = p as ReduceEnergyCosts;
		if (reduceEnergyCosts.LinearReduction != LinearReduction)
		{
			return false;
		}
		if (reduceEnergyCosts.PercentageReduction != PercentageReduction)
		{
			return false;
		}
		if (reduceEnergyCosts.IncludeTypes != IncludeTypes)
		{
			return false;
		}
		if (reduceEnergyCosts.ExcludeTypes != ExcludeTypes)
		{
			return false;
		}
		if (reduceEnergyCosts.UsesChargePerTurn != UsesChargePerTurn)
		{
			return false;
		}
		if (reduceEnergyCosts.UsesChargePerEffect != UsesChargePerEffect)
		{
			return false;
		}
		if (reduceEnergyCosts.GenerateShortDescription != GenerateShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetEnergyCostEvent>.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return GenerateShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (Applicable(E.Type) && IsObjectActivePartSubject(E.Actor) && IsReady(UsesChargePerEffect, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (!string.IsNullOrEmpty(PercentageReduction))
			{
				E.PercentageReduction += PercentageReduction.RollCached();
			}
			if (!string.IsNullOrEmpty(LinearReduction))
			{
				E.LinearReduction += LinearReduction.RollCached();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (GenerateShortDescription && (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier || WorksOnImplantee || WorksOnSelf) && (!string.IsNullOrEmpty(PercentageReduction) || !string.IsNullOrEmpty(LinearReduction)))
		{
			StringBuilder postfix = E.Postfix;
			postfix.Append("\n{{rules|");
			bool flag = false;
			bool flag2 = false;
			bool flag3 = (UsesChargePerTurn || UsesChargePerEffect) && ChargeUse > 0;
			if (flag3)
			{
				postfix.Append("When powered, ");
			}
			if (!string.IsNullOrEmpty(PercentageReduction))
			{
				int num = PercentageReduction.RollMin();
				int num2 = PercentageReduction.RollMax();
				if (num >= 0 && num2 > 0)
				{
					postfix.Append(flag3 ? "provides " : "Provides ").Append(num);
					if (num != num2)
					{
						postfix.Append('-').Append(num2);
					}
					postfix.Append('%');
					flag = true;
				}
				else if (num < 0 && num2 <= 0)
				{
					postfix.Append(flag3 ? "causes " : "Causes ").Append(-num2).Append('%');
					if (num != num2)
					{
						postfix.Append(" to ").Append(-num).Append('%');
					}
					flag2 = true;
				}
				else if (num != 0 || num2 != 0)
				{
					postfix.Append(flag3 ? "confers " : "Confers ").Append(num).Append('%');
					if (num != num2)
					{
						postfix.Append(" to ").Append(num2).Append('%');
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
						postfix.Append(" increase");
					}
					StringBuilder stringBuilder = postfix;
					string value;
					if (flag | flag2)
					{
						value = " plus ";
					}
					else
					{
						value = (flag3 ? "provides " : "Provides ");
					}
					stringBuilder.Append(value);
					postfix.Append(num3);
					if (num3 != num4)
					{
						postfix.Append('-').Append(num4);
					}
					postfix.Append(' ').Append((num3 != 1 || num4 != 1) ? "points" : "point");
					if (!flag && !flag2)
					{
						postfix.Append(" of");
					}
					postfix.Append(" reduction");
					flag = true;
				}
				else if (num3 < 0 && num4 <= 0)
				{
					if (flag)
					{
						postfix.Append(" reduction");
					}
					StringBuilder stringBuilder2 = postfix;
					string value2;
					if (flag | flag2)
					{
						value2 = " minus ";
					}
					else
					{
						value2 = (flag3 ? "causes " : "Causes ");
					}
					stringBuilder2.Append(value2);
					postfix.Append(-num4);
					if (num3 != num4)
					{
						postfix.Append('-').Append(-num3);
					}
					postfix.Append(' ').Append((num3 != -1 || num4 != -1) ? "points" : "point");
					if (!flag && !flag2)
					{
						postfix.Append(" of");
					}
					postfix.Append(" increase");
					flag2 = true;
				}
				else if (num3 != 0 || num4 != 0)
				{
					if (!flag || !flag2)
					{
						if (flag)
						{
							postfix.Append(" reduction");
						}
						if (flag2)
						{
							postfix.Append(" increase");
						}
					}
					StringBuilder stringBuilder3 = postfix;
					string value3;
					if (flag | flag2)
					{
						value3 = " plus ";
					}
					else
					{
						value3 = (flag3 ? "confers " : "Confers ");
					}
					stringBuilder3.Append(value3);
					postfix.Append(num3);
					if (num3 != num4)
					{
						postfix.Append(" to ").Append(num4);
					}
					postfix.Append(' ').Append((num3 != num4 && num4 != 1 && num4 != -1) ? "points" : "point");
					if (!flag && !flag2)
					{
						postfix.Append(" of");
					}
					postfix.Append(" reduction/increase");
					flag = true;
					flag2 = true;
				}
			}
			else if (flag & flag2)
			{
				postfix.Append(" reduction/increase");
			}
			else
			{
				if (flag)
				{
					postfix.Append(" reduction");
				}
				if (flag2)
				{
					postfix.Append(" increase");
				}
			}
			if (flag | flag2)
			{
				postfix.Append(" in ");
				if (!string.IsNullOrEmpty(ScopeDescription))
				{
					postfix.Append(ScopeDescription);
				}
				else if (!string.IsNullOrEmpty(IncludeTypes))
				{
					postfix.Append("the action costs of ").Append(Grammar.MakeAndList(IncludeTypes.CachedCommaExpansion())).Append(" actions");
					if (!string.IsNullOrEmpty(ExcludeTypes))
					{
						postfix.Append(", except for ").Append(Grammar.MakeAndList(ExcludeTypes.CachedCommaExpansion())).Append(" ones");
					}
				}
				else if (!string.IsNullOrEmpty(ExcludeTypes))
				{
					postfix.Append("the action costs of actions other than ").Append(Grammar.MakeAndList(ExcludeTypes.CachedCommaExpansion())).Append(" ones");
				}
				else
				{
					postfix.Append("action costs");
				}
				postfix.Append('.');
			}
			if (!E.NoStatus)
			{
				AddStatusSummary(postfix);
			}
			postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	private bool Applicable(string Type)
	{
		if (!string.IsNullOrEmpty(IncludeTypes))
		{
			if (Type == null)
			{
				return false;
			}
			List<string> list = IncludeTypes.CachedCommaExpansion();
			bool flag = false;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (Type.Contains(list[i]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (Type != null && !string.IsNullOrEmpty(ExcludeTypes))
		{
			List<string> list2 = ExcludeTypes.CachedCommaExpansion();
			int j = 0;
			for (int count2 = list2.Count; j < count2; j++)
			{
				if (Type.Contains(list2[j]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return UsesChargePerTurn;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (UsesChargePerTurn && !OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
