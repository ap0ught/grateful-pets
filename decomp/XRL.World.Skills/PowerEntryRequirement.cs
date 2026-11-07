using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Skills;

[Serializable]
public class PowerEntryRequirement
{
	public PowerEntry Entry;

	public List<string> Attributes = new List<string>();

	public List<int> Minimums = new List<int>();

	public PowerEntryRequirement(PowerEntry Entry)
	{
		this.Entry = Entry;
	}

	public bool MeetsRequirement(GameObject Object, bool ShowPopup = false)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			if (Object.BaseStat(Attributes[i]) < Minimums[i])
			{
				if (ShowPopup)
				{
					Popup.Show("Your " + Attributes[i] + " isn't high enough to buy " + Entry.Name + "!");
				}
				return false;
			}
		}
		return true;
	}

	public void Render(GameObject GO, StringBuilder sb)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			string value = "R";
			if (GO.BaseStat(Attributes[i]) >= Minimums[i])
			{
				value = "G";
			}
			if (i > 0)
			{
				sb.Append(", ");
			}
			sb.Append("{{C|").Append(Minimums[i]).Append("}} {{")
				.Append(value)
				.Append('|')
				.Append((GameManager.IsOnUIContext() && Media.sizeClass <= Media.SizeClass.Small) ? Statistic.GetStatTitleCaseShortNames(Attributes[i]) : Attributes[i])
				.Append("}}");
		}
	}
}
