using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class RemoveEntityListItem : HistoricEvent
{
	private string _list;

	private string _value;

	public RemoveEntityListItem(string list, string value)
	{
		_list = list;
		_value = value;
	}

	public override void Generate()
	{
		RemoveEntityListItem(_list, _value);
		duration = 0L;
	}
}
