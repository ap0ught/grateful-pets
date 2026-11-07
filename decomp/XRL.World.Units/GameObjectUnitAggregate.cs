using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Units;

[Serializable]
public class GameObjectUnitAggregate : GameObjectUnit
{
	public List<GameObjectUnit> Units;

	public string Description;

	public GameObjectUnitAggregate()
	{
		Units = new List<GameObjectUnit>();
	}

	public GameObjectUnitAggregate(string Description, params GameObjectUnit[] Units)
	{
		this.Description = Description;
		this.Units = new List<GameObjectUnit>(Units);
	}

	public GameObjectUnitAggregate(params GameObjectUnit[] Units)
	{
		this.Units = new List<GameObjectUnit>(Units);
	}

	public GameObjectUnitAggregate(IEnumerable<GameObjectUnit> Units)
	{
		this.Units = new List<GameObjectUnit>(Units);
	}

	public override void Apply(GameObject Object)
	{
		foreach (GameObjectUnit unit in Units)
		{
			unit.Apply(Object);
		}
	}

	public override void Remove(GameObject Object)
	{
		foreach (GameObjectUnit unit in Units)
		{
			unit.Remove(Object);
		}
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Description != null)
		{
			return Description;
		}
		if (Units.IsNullOrEmpty())
		{
			return "";
		}
		StringBuilder sB = Event.NewStringBuilder();
		foreach (GameObjectUnit unit in Units)
		{
			sB.Compound(unit.GetDescription(), '\n');
		}
		return Description = Event.FinalizeString(sB);
	}
}
