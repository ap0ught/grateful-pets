using System.Collections.Generic;
using HistoryKit;

namespace XRL.Annals;

public class RemoveEntityListProperties : HistoricEvent
{
	private Dictionary<string, string> listProperties;

	public RemoveEntityListProperties(Dictionary<string, string> _listProperties)
	{
		listProperties = _listProperties;
	}

	public override void Generate()
	{
		if (listProperties != null)
		{
			foreach (string key in listProperties.Keys)
			{
				RemoveEntityListItem(key, listProperties[key]);
			}
		}
		duration = 0L;
	}
}
