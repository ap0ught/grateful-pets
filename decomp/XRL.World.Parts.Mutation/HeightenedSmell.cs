using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedSmell : BaseMutation
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<ExtraHostilePerceptionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ExtraHostilePerceptionEvent E)
	{
		if (E.Actor == ParentObject && ParentObject.IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				List<GameObject> list = cell.ParentZone.FastSquareSearch(cell.X, cell.Y, GetRadius(), "Combat");
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					GameObject gameObject = list[i];
					if (gameObject.GetEffect(typeof(HeightenedSmellEffect), IsOurs) is HeightenedSmellEffect heightenedSmellEffect && heightenedSmellEffect.Smeller == ParentObject && heightenedSmellEffect.Identified && ParentObject.IsRelevantHostile(gameObject))
					{
						E.Hostile = gameObject;
						E.PerceiveVerb = "smell";
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	private bool IsOurs(Effect FX)
	{
		return (FX as HeightenedSmellEffect).Smeller == ParentObject;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You are possessed of exceptionally acute smell.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "You detect the presence of creatures within a distance typically up to " + GetRadius(Level) + " squares depending on terrain\n";
		if (Level == base.Level)
		{
			return text + "Chance to identify nearby detected creatures";
		}
		return text + "{{rules|Increased chance to identify nearby detected creatures}}";
	}

	public static int GetRadius(int Level)
	{
		return 5 + Level * 4;
	}

	public int GetRadius()
	{
		return GetRadius(Level);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && ParentObject.IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				List<GameObject> list = cell.ParentZone.FastSquareSearch(cell.X, cell.Y, GetRadius() + 10, "Combat");
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					GameObject gameObject = list[i];
					if (gameObject.IsSmellable(ParentObject) && !gameObject.HasEffect(typeof(HeightenedSmellEffect), IsOurs))
					{
						gameObject.ApplyEffect(new HeightenedSmellEffect(Level, ParentObject));
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
