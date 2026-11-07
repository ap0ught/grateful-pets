using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class GenericDeath : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string text = QudHistoryHelpers.ListAllCognomen(entity.GetSnapshotAtYear(entity.lastYear));
		string value = "In %" + year + "%, <entity.name>" + text + " died of natural causes. <entity.subjectPronoun.capitalize> was " + (int)(year - entity.firstYear) + " years old.";
		string value2 = string.Format("In the {0}, {1}{2} laid {3} body to rest and crossed into Brightsheol. {4} was {5} years old.", QudHistoryHelpers.GenerateSultanateYearName(), "<entity.name>", text, "<entity.possessivePronoun>", "<entity.subjectPronoun.capitalize>", (int)(year - entity.firstYear));
		SetEventProperty("gospel", value);
		SetEventProperty("tombInscription", value2);
		SetEventProperty("tombInscriptionCategory", "Dies");
	}
}
