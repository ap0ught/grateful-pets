using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class PopulationLiquidFiller : IPart
{
	public string Table;

	public string Volume;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null)
		{
			PopulationResult populationResult = null;
			int num = 0;
			while (++num < 5)
			{
				List<PopulationResult> list = PopulationManager.Generate(Table);
				populationResult = ((list != null) ? list.GetRandomElement() : null);
				if (populationResult != null && (populationResult.Blueprint == "empty" || populationResult.Blueprint.StartsWith("!") || ParentObject.IsSafeContainerForLiquid(populationResult.Blueprint)))
				{
					break;
				}
			}
			string text = populationResult.Blueprint;
			if (text.StartsWith("!"))
			{
				text = text.Substring(1);
			}
			liquidVolume.Empty();
			if (text != "empty")
			{
				int drams = 1;
				if (populationResult.Number != 1 || string.IsNullOrEmpty(Volume))
				{
					drams = populationResult.Number;
				}
				else if (!string.IsNullOrEmpty(Volume))
				{
					drams = Volume.RollCached();
				}
				liquidVolume.AddDrams(text, drams);
			}
		}
		return base.HandleEvent(E);
	}
}
