using System;

namespace XRL;

[Serializable]
public class GenotypeStat
{
	public string Name = "";

	public string ChargenDescription = "";

	public int Minimum;

	public int Maximum;

	public int Bonus;

	public void MergeWith(GenotypeStat newStat)
	{
		if (newStat.Minimum != -999)
		{
			Minimum = newStat.Minimum;
		}
		if (newStat.Maximum != -999)
		{
			Maximum = newStat.Maximum;
		}
		if (newStat.Bonus != -999)
		{
			Bonus = newStat.Bonus;
		}
	}
}
