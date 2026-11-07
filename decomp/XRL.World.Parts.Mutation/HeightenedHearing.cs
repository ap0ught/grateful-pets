using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HeightenedHearing : BaseMutation
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
			if (cell != null && cell?.ParentZone?.IsWorldMap() != true)
			{
				List<GameObject> list = cell.ParentZone.FastSquareSearch(cell.X, cell.Y, GetRadius(), "Combat");
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					GameObject gameObject = list[i];
					if (gameObject.GetEffect(typeof(HeightenedHearingEffect), IsOurs) is HeightenedHearingEffect heightenedHearingEffect && heightenedHearingEffect.Listener == ParentObject && heightenedHearingEffect.Identified && ParentObject.IsRelevantHostile(gameObject))
					{
						E.Hostile = gameObject;
						E.PerceiveVerb = "hear";
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	private bool IsOurs(Effect FX)
	{
		return (FX as HeightenedHearingEffect).Listener == ParentObject;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You are possessed of unnaturally acute hearing.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "You detect the presence of creatures within a radius of {{rules|" + GetRadius(Level) + "}}.\n";
		if (Level == base.Level)
		{
			return text + "Chance to identify nearby detected creatures";
		}
		return text + "{{rules|Increased chance to identify nearby detected creatures}}";
	}

	public static int GetRadius(int Level)
	{
		if (Level < 10)
		{
			return 3 + Level * 2;
		}
		return 40;
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
				int radius = GetRadius();
				List<GameObject> list = cell.ParentZone.FastSquareSearch(cell.X, cell.Y, radius, "Combat");
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					GameObject gameObject = list[i];
					if (ParentObject.DistanceTo(gameObject) <= radius && !gameObject.HasEffect(typeof(HeightenedHearingEffect), IsOurs))
					{
						gameObject.ApplyEffect(new HeightenedHearingEffect(Level, ParentObject));
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
