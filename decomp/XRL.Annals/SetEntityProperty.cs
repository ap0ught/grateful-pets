using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class SetEntityProperty : HistoricEvent
{
	public string _name;

	public string _value;

	public SetEntityProperty(string name, string value)
	{
		_name = name;
		_value = value;
	}

	public override void Generate()
	{
		SetEntityProperty(_name, _value);
		duration = 0L;
	}
}
