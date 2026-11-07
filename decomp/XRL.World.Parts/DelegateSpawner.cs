using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DelegateSpawner : IPart
{
	public string Faction = "";

	public DelegateSpawner()
	{
	}

	public DelegateSpawner(string _faction)
	{
		Faction = _faction;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell cell = ParentObject.Physics.CurrentCell;
			List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(Faction);
			if (factionMembers.Count == 0)
			{
				foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
				{
					if (blueprint.Name == "Diplomacy Droid")
					{
						factionMembers.Add(blueprint);
					}
				}
			}
			Algorithms.RandomShuffleInPlace(factionMembers, Stat.Rand);
			GameObject gameObject = GameObject.Create(factionMembers[0].Name);
			gameObject.RequirePart<SocialRoles>().RequireRole("delegate for " + XRL.World.Faction.GetFormattedName(Faction));
			gameObject.RemovePartsDescendedFrom<IBondedLeader>();
			gameObject.Brain.Allegiance.Clear();
			gameObject.Brain.Allegiance.Add(Faction, 100);
			gameObject.Brain.Wanders = false;
			gameObject.Brain.WandersRandomly = false;
			gameObject.FireEvent("VillageInit");
			gameObject.SetIntProperty("IsDelegate", 1);
			gameObject.RequirePart<Calming>().Feeling = 100;
			XRLCore.Core.Game.ActionManager.AddActiveObject(gameObject);
			cell.AddObject(gameObject);
			ParentObject.Destroy();
		}
		return true;
	}
}
