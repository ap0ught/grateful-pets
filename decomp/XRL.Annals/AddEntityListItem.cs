using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class AddEntityListItem : HistoricEvent
{
	private string _list;

	private string _value;

	public AddEntityListItem(string list, string value)
	{
		_list = list;
		_value = value;
	}

	public override void Generate()
	{
		AddEntityListItem(_list, _value);
		duration = 0L;
	}
}
