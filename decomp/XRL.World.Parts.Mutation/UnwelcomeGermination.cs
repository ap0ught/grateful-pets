using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class UnwelcomeGermination : BaseMutation
{
	public int Germinating;

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You spasmodically engender wild plant growth around yourself for a short period.\n\nThere is a small chance each round that you enter into a compulsive state of mind for 30-39 rounds.\n\nDuring this time, there is a 25% chance each round that you summon several hostile plants nearby.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (ParentObject.CurrentZone == null || ParentObject.OnWorldMap())
			{
				return true;
			}
			if (2.in1000() && Germinating <= 0)
			{
				Popup.Show("{{r|Unwelcome images of {{G|budding plants}} fill your mind.}}");
				Germinating = Stat.Random(30, 39);
			}
			if (Germinating > 0 && 25.in100())
			{
				Germinate(ParentObject, Stat.Random(1, 10));
				UseEnergy(1000, "Mental Mutation");
			}
			if (Germinating > 0)
			{
				Germinating--;
			}
		}
		return base.FireEvent(E);
	}

	public static void Germinate(GameObject gameObject, int level, int range = 80, bool friendly = false, Cell targetCell = null)
	{
		List<Cell> list = new List<Cell>();
		if (targetCell != null)
		{
			list.Add(targetCell);
			list.AddRange(targetCell.GetAdjacentCells());
			Burgeoning.PlantSummoning(list, friendly, gameObject, level, bQudzuOnly: false);
			return;
		}
		List<Cell> list2 = (from c in gameObject.Physics.CurrentCell.ParentZone.GetEmptyVisibleCells()
			where c != null && c.DistanceTo(gameObject) <= range
			select c).ToList();
		if (list2.Count > 0)
		{
			list.Add(list2.GetRandomElement());
			list.AddRange(list[0].GetAdjacentCells());
			Burgeoning.PlantSummoning(list, friendly, gameObject, level, bQudzuOnly: false);
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
