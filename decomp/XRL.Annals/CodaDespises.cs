using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class CodaDespises : HistoricEvent
{
	public override void Generate()
	{
		string text = entity.GetListProperties("profaneThings", -1L).GetRandomElement() ?? entity.GetEntityProperty("defaultProfaneThing", -1L);
		string entityProperty = history.GetEntity("CodaSultan").GetEntityProperty("name", -1L);
		AddEntityListItem("profaneThings", entityProperty);
		string value = string.Format("The {0} of {1} was revealed to the people of {2} through {3}.|{4}", ExpandString("<spice.instancesOf.profaneFeeling.!random>"), entityProperty, entity.GetEntityProperty("name", -1L), text, id);
		AddEntityListItem("Gospels", value);
	}
}
