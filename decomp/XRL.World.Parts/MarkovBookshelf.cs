using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class MarkovBookshelf : IPart
{
	public bool Opened;

	public string BookTable = "StiltCorpus";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && !Opened)
		{
			List<PopulationResult> list = PopulationManager.Generate(BookTable);
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list[i].Number; j++)
				{
					GameObject gameObject = GameObject.Create("MarkovBook");
					gameObject.GetPart<MarkovBook>().SetContents(Stat.Random(0, 2147483646), list[i].Blueprint);
					ParentObject.Inventory.AddObject(gameObject);
				}
			}
			Opened = true;
		}
		return base.FireEvent(E);
	}
}
