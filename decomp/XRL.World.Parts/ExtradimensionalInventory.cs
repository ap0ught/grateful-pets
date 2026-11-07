using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ExtradimensionalInventory : IPart
{
	public int Chance = 100;

	public bool Applied;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && !Applied)
		{
			Applied = true;
			foreach (GameObject item in ParentObject.GetInventory())
			{
				if (!item.HasPart<ModExtradimensional>() && Stat.Random(1, 100) <= Chance && !string.Equals(item.GetTag("Mods"), "None"))
				{
					item.AddPart(new ModExtradimensional());
				}
			}
		}
		return base.FireEvent(E);
	}
}
