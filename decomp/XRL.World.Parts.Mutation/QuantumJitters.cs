using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class QuantumJitters : BaseMutation
{
	public int counter;

	public QuantumJitters()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("chance", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("BeforeCooldownActivatedAbility");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "Your willful acts sometimes dent spacetime.\n\nWhenever you use an activated ability, there's a small chance your focus slips and you dent spacetime in the local region, causing 1-2 spacetime vortices to appear. This chance increases the longer you go without using an activated ability.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			counter++;
		}
		else if (E.ID == "BeforeCooldownActivatedAbility" && !E.HasFlag("Involuntary"))
		{
			if (Math.Min(counter, 250).in1000())
			{
				Sunder();
			}
			counter = 0;
		}
		return base.FireEvent(E);
	}

	public void Sunder()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your focus slips, causing you to dent spacetime in the local region.");
		}
		ParentObject.SpatialDistortionBlip();
		int num = Stat.Random(1, 2);
		foreach (Cell item in cell.GetLocalAdjacentCells().InRandomOrder())
		{
			SpaceTimeVortex part = item.AddObject("Space-Time Vortex").GetPart<SpaceTimeVortex>();
			if (part != null)
			{
				part.Delay = 1;
			}
			num--;
			if (num == 0)
			{
				break;
			}
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
