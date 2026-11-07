using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class Adopted : HistoricEvent
{
	public override void Generate()
	{
		duration = Random(5, 8);
		SetEventProperty("gospel", "<$chosenSpice=spice.elements.entity$elements[random]><entity.name> was adopted by <$chosenSpice.professions.!random> who love <$chosenSpice.practices.!random>.");
	}
}
