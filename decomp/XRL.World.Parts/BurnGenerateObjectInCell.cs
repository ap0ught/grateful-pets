using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class BurnGenerateObjectInCell : IPart
{
	public int Chance;

	public string Table;

	public bool EvenIfDroppedByPlayer;

	public bool PerZone;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!Table.IsNullOrEmpty() && ParentObject.Physics != null && (ParentObject.Physics.LastDamagedByType == "Fire" || ParentObject.Physics.LastDamagedByType == "Light") && ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0 && (EvenIfDroppedByPlayer || ParentObject.GetIntProperty("DroppedByPlayer") == 0) && Chance.in100())
		{
			IInventory dropInventory = ParentObject.GetDropInventory();
			if (dropInventory != null)
			{
				string blueprint = GetBlueprint();
				if (blueprint != null)
				{
					GameObject gameObject = GameObject.Create(blueprint);
					DoCarryOvers(ParentObject, gameObject);
					dropInventory.AddObjectToInventory(gameObject, null, Silent: false, NoStack: false, FlushTransient: true, null, E);
					DroppedEvent.Send(ParentObject, gameObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void DoCarryOvers(GameObject From, GameObject To)
	{
		if (From.HasProperty("StoredByPlayer") || From.HasProperty("FromStoredByPlayer"))
		{
			To.SetIntProperty("FromStoredByPlayer", 1);
		}
		Temporary.CarryOver(From, To);
		Phase.carryOver(From, To);
	}

	public string GetBlueprint()
	{
		Zone zone = (PerZone ? ParentObject.CurrentZone : null);
		string name = (PerZone ? PerZoneResultKey() : null);
		string text = zone?.GetZoneProperty(name, null);
		if (text == null)
		{
			PopulationResult populationResult = PopulationManager.RollOneFrom(Table);
			if (populationResult != null)
			{
				text = populationResult.Blueprint;
				zone?.SetZoneProperty(name, text);
			}
		}
		return text;
	}

	private string PerZoneResultKey()
	{
		return "BurnGenerateObjectInCell_" + Table;
	}
}
