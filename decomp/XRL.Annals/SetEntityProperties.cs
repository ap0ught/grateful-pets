using System;
using System.Collections.Generic;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class SetEntityProperties : HistoricEvent
{
	private Dictionary<string, string> stringProperties;

	private Dictionary<string, string> listProperties;

	public SetEntityProperties(Dictionary<string, string> _stringProperties, Dictionary<string, string> _listProperties)
	{
		stringProperties = _stringProperties;
		listProperties = _listProperties;
	}

	public override void Generate()
	{
		if (stringProperties != null)
		{
			foreach (string key in stringProperties.Keys)
			{
				SetEntityProperty(key, stringProperties[key]);
			}
		}
		if (listProperties != null)
		{
			foreach (string key2 in listProperties.Keys)
			{
				AddEntityListItem(key2, listProperties[key2]);
			}
		}
		duration = 0L;
	}
}
