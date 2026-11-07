using System;

namespace XRL.World.ZoneParts;

[Serializable]
public class Memorialization : IZonePart
{
	public string Faction;

	public string RequiresObjectWithTagOrProperty;

	public string RequiresObjectWithOperationalPart;

	public string RequiresObjectOfFaction;

	public string Blueprint;

	public string BlueprintInorganic;

	public long Delay = 2400L;

	public long LastTurn;

	public int MinTurnsForCheck = 50;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckMemorialization();
		return base.HandleEvent(E);
	}

	public void CheckMemorialization()
	{
	}

	private string GetBlueprintFor(MemorialTracking Memorial)
	{
		if (BlueprintInorganic.IsNullOrEmpty())
		{
			return Blueprint;
		}
		string blueprint = Memorial.Blueprint;
		if (blueprint.IsNullOrEmpty())
		{
			return Blueprint;
		}
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(blueprint);
		if (blueprintIfExists == null)
		{
			return Blueprint;
		}
		if (!blueprintIfExists.GetPartParameter("Physics", "Organic", Default: false))
		{
			return BlueprintInorganic;
		}
		return Blueprint;
	}
}
