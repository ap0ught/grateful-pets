using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class CodaWorships : HistoricEvent
{
	public override void Generate()
	{
		string text = entity.GetListProperties("sacredThings", -1L).GetRandomElement() ?? entity.GetEntityProperty("defaultSacredThing", -1L);
		string entityProperty = history.GetEntity("CodaSultan").GetEntityProperty("name", -1L);
		AddEntityListItem("itemAdjectiveRoots", entityProperty);
		AddEntityListItem("sacredThings", entityProperty);
		AddEntityListItem("profaneThings", ExpandString("<spice.commonPhrases.profanity.!random> toward ") + entityProperty);
		string value = string.Format("The {0} of {1} was revealed to the people of {2} through {3}.|{4}", ExpandString("<spice.instancesOf.divineFeeling.!random>"), entityProperty, entity.GetEntityProperty("name", -1L), text, id);
		AddEntityListItem("Gospels", value);
	}
}
