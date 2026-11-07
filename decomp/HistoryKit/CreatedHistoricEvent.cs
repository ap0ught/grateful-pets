using System;

namespace HistoryKit;

[Serializable]
public class CreatedHistoricEvent : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
	}
}
