using System;
using System.Text;

namespace XRL.World.Units;

[Serializable]
public class GameObjectAttributeUnit : GameObjectUnit
{
	public string Attribute;

	public int Value;

	public bool Percent;

	public override void Apply(GameObject Object)
	{
		if (Attribute == "*" || Attribute.EqualsNoCase("All"))
		{
			foreach (string attribute in Statistic.Attributes)
			{
				Apply(Object, attribute);
			}
			return;
		}
		Apply(Object, Attribute);
	}

	public void Apply(GameObject Object, string Attribute)
	{
		Statistic stat = Object.GetStat(Attribute);
		if (Percent)
		{
			stat.BaseValue += stat.BaseValue * Value / 100;
		}
		else
		{
			stat.BaseValue += Value;
		}
	}

	public override void Remove(GameObject Object)
	{
		if (Attribute == "*" || Attribute.EqualsNoCase("All"))
		{
			foreach (string attribute in Statistic.Attributes)
			{
				Remove(Object, attribute);
			}
			return;
		}
		Remove(Object, Attribute);
	}

	public void Remove(GameObject Object, string Attribute)
	{
		Statistic stat = Object.GetStat(Attribute);
		if (Percent)
		{
			stat.BaseValue = (int)((float)stat.BaseValue / (1f + (float)Value / 100f));
		}
		else
		{
			Object.GetStat(Attribute).BaseValue -= Value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Attribute = null;
		Value = 0;
		Percent = false;
	}

	public override string GetDescription(bool Inscription = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.AppendSigned(Statistic.IsInverseBenefit(Attribute) ? (-Value) : Value);
		if (Percent)
		{
			stringBuilder.Append("%");
		}
		if (Attribute == "*" || Attribute.EqualsNoCase("All"))
		{
			stringBuilder.Compound("to all stats");
		}
		else
		{
			stringBuilder.Compound(Statistic.GetStatDisplayName(Attribute));
		}
		return Event.FinalizeString(stringBuilder);
	}
}
