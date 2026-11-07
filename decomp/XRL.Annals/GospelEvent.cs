using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class GospelEvent : HistoricEvent
{
	[NonSerialized]
	private string Gospel;

	public GospelEvent()
	{
	}

	public GospelEvent(string Gospel)
	{
		this.Gospel = Gospel;
	}

	public override void Generate()
	{
		SetEventProperty("gospel", Gospel ?? "[NO GOSPEL]");
		AddEntityListItem("Gospels", string.Format("{0}|{1}", Gospel ?? "[NO GOSPEL]", id));
	}
}
