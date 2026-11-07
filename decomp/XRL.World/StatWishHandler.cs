using System.Collections.Generic;
using System.Text;
using XRL.Messages;
using XRL.UI;
using XRL.Wish;

namespace XRL.World;

[HasWishCommand]
public class StatWishHandler
{
	[WishCommand(null, null)]
	public void ClearStatShifts()
	{
		foreach (Statistic value in The.Player.Statistics.Values)
		{
			if (value.Name != "MP" && value.Name != "SP" && value.Name != "AP")
			{
				value.Penalty = 0;
				value.Bonus = 0;
				value.Shifts = null;
			}
		}
		MessageQueue.AddPlayerMessage("Clearing player body stat shifts...");
	}

	[WishCommand(null, null)]
	public void ShowStatShifts()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		foreach (KeyValuePair<string, Statistic> statistic in The.Player.Statistics)
		{
			if (statistic.Value.Shifts == null)
			{
				continue;
			}
			stringBuilder.Append(statistic.Key);
			stringBuilder.Append(":");
			foreach (Statistic.StatShift shift in statistic.Value.Shifts)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("  ");
				if (shift.Amount > 0)
				{
					stringBuilder.Append('+');
				}
				stringBuilder.Append(shift.Amount);
				stringBuilder.Append(" from ");
				stringBuilder.Append(shift.DisplayName);
			}
			stringBuilder.AppendLine();
		}
		Popup.Show(stringBuilder.ToString());
	}
}
