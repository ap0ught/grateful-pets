using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class WaterRitualDiscount : IActivePart
{
	public string Types;

	public int Percent;

	private string[] _SplitTypes;

	public string[] SplitTypes
	{
		get
		{
			if (_SplitTypes == null)
			{
				if (string.IsNullOrEmpty(Types))
				{
					_SplitTypes = new string[0];
				}
				else
				{
					_SplitTypes = Types.Split(',');
				}
			}
			return _SplitTypes;
		}
	}

	public WaterRitualDiscount()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<GetWaterRitualCostEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (SplitTypes.Length != 0)
		{
			E.Postfix.Compound("{{rules|", '\n').AppendSigned(-Percent).Append("% reputation cost for ");
			int i = 0;
			for (int num = SplitTypes.Length; i < num; i++)
			{
				if (i != 0)
				{
					if (i == num - 1)
					{
						E.Postfix.Append(", and ");
					}
					else
					{
						E.Postfix.Append(", ");
					}
				}
				E.Postfix.Append(GetTypeDesc(SplitTypes[i]));
			}
			E.Postfix.Append(" via the water ritual");
		}
		return base.HandleEvent(E);
	}

	public static string GetTypeDesc(string Type)
	{
		return Type switch
		{
			"Join" => "recruiting creatures", 
			"CookingRecipe" => "learning cooking recipes", 
			"TinkerRecipe" => "learning tinker blueprints", 
			"Skill" => "learning skills", 
			"Mutation" => "gaining mutations", 
			"Item" => "buying items", 
			"Secret" => "sharing secrets", 
			_ => "buying " + Grammar.Pluralize(Type.ToLower()), 
		};
	}

	public override bool HandleEvent(GetWaterRitualCostEvent E)
	{
		if (SplitTypes.Contains(E.Type) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Cost = Math.Max(0, E.Cost * (100 - Percent) / 100);
		}
		return base.HandleEvent(E);
	}
}
