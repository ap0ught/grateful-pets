using Qud.API;

namespace XRL.World.Parts;

public class PariahSpawner : IPart
{
	public bool InheritName;

	public bool InheritDescription;

	public bool DoesWander = true;

	public bool IsUnique;

	public override bool SameAs(IPart p)
	{
		if ((p as PariahSpawner).DoesWander != DoesWander)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		GameObject gameObject = GeneratePariah(-1, !InheritName, IsUnique);
		if (gameObject.Brain != null)
		{
			if (DoesWander)
			{
				gameObject.Brain.Wanders = true;
				gameObject.Brain.WandersRandomly = true;
				gameObject.RequirePart<AIShopper>();
			}
			else
			{
				gameObject.Brain.Wanders = false;
				gameObject.Brain.WandersRandomly = false;
				gameObject.RequirePart<AISitting>();
			}
			gameObject.MakeActive();
		}
		if (InheritName && gameObject.Render != null)
		{
			gameObject.Render.DisplayName = ParentObject.Render.DisplayName;
		}
		if (InheritDescription && gameObject.TryGetPart<Description>(out var Part))
		{
			Part._Short = ParentObject?.GetPart<Description>()?._Short ?? Part._Short;
		}
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		gameObject.FireEvent("VillageInit");
		gameObject.SetIntProperty("Social", 1);
		GameObjectFactory.ApplyBuilders(gameObject, ParentObject.GetBlueprint());
		cell.AddObject(gameObject);
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}

	public static GameObject GeneratePariah(int level = -1, bool AlterName = true, bool IsUnique = false)
	{
		return GeneratePariah((level == -1) ? EncountersAPI.GetACreature() : EncountersAPI.GetCreatureAroundLevel(level), AlterName, IsUnique);
	}

	public static GameObject GeneratePariah(GameObject pariah, bool AlterName = true, bool IsUnique = false)
	{
		Pariah.MakePariah(pariah, AlterName, IsUnique);
		return pariah;
	}
}
