using System;
using XRL.Language;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class TurretTinker : IPart
{
	public int TurretCooldown;

	public string Cooldown = "5-10";

	public string TurretType;

	public int MaxTurretsPlaced = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		TurretTinker turretTinker = p as TurretTinker;
		if (turretTinker.TurretCooldown != TurretCooldown)
		{
			return false;
		}
		if (turretTinker.Cooldown != Cooldown)
		{
			return false;
		}
		if (turretTinker.TurretType != TurretType)
		{
			return false;
		}
		if (turretTinker.MaxTurretsPlaced != MaxTurretsPlaced)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		if (!TurretType.Contains("Turret"))
		{
			return;
		}
		TurretType = null;
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(TurretType);
		if (blueprint == null)
		{
			return;
		}
		foreach (InventoryObject item in blueprint.Inventory)
		{
			GameObjectBlueprint blueprint2 = GameObjectFactory.Factory.GetBlueprint(item.Blueprint);
			if (blueprint2 != null && blueprint2.DescendsFrom("MissileWeapon"))
			{
				TurretType = item.Blueprint;
				break;
			}
		}
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		string turretReferenceName = GetTurretReferenceName();
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
		stats.Set("TurretName", GetTurretReferenceName());
		stats.Set("An", Grammar.IndefiniteArticleShouldBeAn(turretReferenceName) ? "an" : "a");
	}

	public string GetTurretReferenceName()
	{
		if (TurretType.IsNullOrEmpty())
		{
			return "turret";
		}
		if (TurretType[0] == '*')
		{
			return "random turret";
		}
		if (TurretType[0] == '@')
		{
			return "dynamic turret";
		}
		return GameObjectFactory.Factory.GetBlueprint(GetTurretWeaponBlueprintInstance()).CachedDisplayNameStripped + " turret";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandTinkerTurret");
		base.Register(Object, Registrar);
	}

	public string GetRandomTurretWeaponBlueprint()
	{
		string populationName = "DynamicInheritsTable:MissileWeapon:Tier" + ParentObject.GetTier();
		int num = 0;
		PopulationResult populationResult;
		GameObjectBlueprint blueprint;
		do
		{
			populationResult = PopulationManager.RollOneFrom(populationName);
			if (populationResult == null)
			{
				return "Pump Shotgun";
			}
			if (++num > 10)
			{
				return "Pump Shotgun";
			}
			blueprint = GameObjectFactory.Factory.GetBlueprint(populationResult.Blueprint);
		}
		while (blueprint == null || !blueprint.GetPartParameter("MissileWeapon", "FiresManually", Default: true));
		return populationResult.Blueprint;
	}

	public string GetTurretWeaponBlueprintInstance()
	{
		string text = TurretType;
		if (text == "*")
		{
			text = GetRandomTurretWeaponBlueprint();
		}
		else if (text[0] == '@')
		{
			string text2 = text.Substring(1);
			if (!text2.Contains(":Tier"))
			{
				text2 = text2 + ":Tier" + ParentObject.GetTier();
			}
			if (text2.Contains("{zonetier}"))
			{
				text2 = text2.Replace("{zonetier}", ZoneManager.zoneGenerationContextTier.ToString());
			}
			text = PopulationManager.RollOneFrom(text2)?.Blueprint;
		}
		return text;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (MaxTurretsPlaced > 0)
			{
				if (ParentObject.IsPlayer())
				{
					TurretCooldown--;
				}
				else if (!IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
				{
					if (!ParentObject.IsPlayerControlled())
					{
						ParentObject.Brain.PushGoal(new WanderRandomly(5));
					}
				}
				else if (TurretCooldown > 0 && ParentObject.Brain != null && !ParentObject.Brain.HasGoal("PlaceTurretGoal"))
				{
					TurretCooldown--;
				}
				else if (TurretCooldown <= 0 && !ParentObject.Brain.HasGoal("PlaceTurretGoal"))
				{
					Cell cell = null;
					int num = 0;
					do
					{
						cell = ParentObject.CurrentCell.ParentZone.GetEmptyReachableCells().GetRandomElement();
						if (cell == null)
						{
							break;
						}
						if (cell.HasObjectWithTag("ExcavatoryTerrainFeature"))
						{
							cell = null;
						}
					}
					while (cell == null && ++num < 10);
					if (cell == null)
					{
						ParentObject.Brain.PushGoal(new WanderRandomly(5));
					}
					else
					{
						ParentObject.Brain.Goals.Clear();
						string turretWeaponBlueprintInstance = GetTurretWeaponBlueprintInstance();
						if (!string.IsNullOrEmpty(turretWeaponBlueprintInstance))
						{
							ParentObject.Brain.PushGoal(new PlaceTurretGoal(cell.Location, turretWeaponBlueprintInstance));
						}
					}
					TurretCooldown = Cooldown.RollCached();
				}
			}
		}
		else if (E.ID == "EnteredCell")
		{
			if (TurretType.IsNullOrEmpty())
			{
				TurretType = GetRandomTurretWeaponBlueprint();
			}
			if (TurretType != null && ActivatedAbilityID == Guid.Empty)
			{
				ActivatedAbilityID = AddMyActivatedAbility("Tinker Turret  [" + MaxTurretsPlaced + " remaining]", "CommandTinkerTurret", "Skills", null, "\u0012");
			}
		}
		else if (E.ID == "CommandTinkerTurret")
		{
			if (GetMyActivatedAbilityCooldown(ActivatedAbilityID) <= 0 && MaxTurretsPlaced > 0)
			{
				Cell cell2 = PickDirection("Tinker Turret");
				if (cell2 != null && cell2.IsEmpty())
				{
					MaxTurretsPlaced--;
					GameObject gameObject = IntegratedWeaponHosts.GenerateTurret(GameObject.Create(GetTurretWeaponBlueprintInstance()), ParentObject, overrideSupply: true);
					cell2.AddObject(gameObject);
					gameObject.MakeActive();
					gameObject.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", gameObject));
					CommandReloadEvent.Execute(gameObject, FreeAction: true);
					ParentObject.UseEnergy(1000, "Tinker Deploy Turret");
					SetMyActivatedAbilityDisplayName(ActivatedAbilityID, "Tinker Turret  [" + MaxTurretsPlaced + " remaining]");
					CooldownMyActivatedAbility(ActivatedAbilityID, Cooldown.RollCached());
				}
			}
			if (MaxTurretsPlaced <= 0)
			{
				ParentObject.ShowFailure("You are out of turrets to place.");
				DisableMyActivatedAbility(ActivatedAbilityID);
			}
		}
		return base.FireEvent(E);
	}
}
