using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

[HasWishCommand]
public static class IntegratedWeaponHosts
{
	[NonSerialized]
	public const string DeployTurretContext = "DeployTurret";

	public static GameObject GenerateTurret(GameObject weapon, GameObject owner = null, bool overrideSupply = false)
	{
		GameObjectBlueprint gameObjectBlueprint = FindTurretBlueprint(weapon);
		weapon.RemoveFromContext();
		int num = Tier.Constrain(weapon?.GetTier() ?? 3);
		GameObject gameObject;
		if (gameObjectBlueprint != null)
		{
			List<GameObject> list = new List<GameObject> { weapon };
			gameObject = GameObject.Create(gameObjectBlueprint, 0, 0, null, null, null, "DeployTurret", list);
			if (list.Count > 0)
			{
				weapon.Obliterate();
				weapon = null;
			}
			gameObject.RemoveIntProperty("TurretWarmup");
		}
		else
		{
			gameObject = GameObject.Create("TinkerTurret", 0, 0, null, null, null, "DeployTurret");
			gameObject.Inventory.AddObject(weapon);
			gameObject.DisplayName = GetTurretNameFromWeapon(weapon);
			gameObject.Render.Tile = GetTurretTileFromWeapon(weapon);
			gameObject.Render.RenderString = num.ToString();
			gameObject.Render.SetForegroundColor(weapon.Render.GetForegroundColor());
			string propertyOrTag = weapon.GetPropertyOrTag("IntegratedWeaponHostDetailColor");
			if (!string.IsNullOrEmpty(propertyOrTag) && propertyOrTag.Length == 1)
			{
				gameObject.Render.DetailColor = propertyOrTag;
			}
			gameObject.GetStat("Hitpoints").BaseValue = num * 5;
			gameObject.GetStat("AV").BaseValue = num * 3 / 2;
			if (weapon.IsGiganticEquipment)
			{
				gameObject.IsGiganticCreature = true;
			}
		}
		PowerSwitch part = gameObject.GetPart<PowerSwitch>();
		if (part != null)
		{
			if (owner != null && owner.IsPlayer())
			{
				part.SecurityClearance = 0;
			}
			else
			{
				part.SecurityClearance = -Math.Min(Math.Max(num, 1), 5);
			}
		}
		gameObject.RemovePart<RandomLoot>();
		if (weapon != null)
		{
			weapon.FireEvent(Event.New("PrepIntegratedHostToReceiveAmmo", "Host", gameObject));
		}
		else
		{
			gameObject.FireEventOnBodyparts(Event.New("PrepIntegratedHostToReceiveAmmo", "Host", gameObject));
		}
		gameObject.GetStat("XPValue").BaseValue = 0;
		if (owner != null)
		{
			int num2 = owner.Stat("Level");
			if (num2 > gameObject.Stat("Level"))
			{
				gameObject.GetStat("Level").BaseValue = num2;
				gameObject.GetStat("XP").BaseValue = Leveler.GetXPForLevel(num2);
			}
			if (gameObject.HasStat("Agility"))
			{
				gameObject.GetStat("Agility").BaseValue = GetTurretAgilityByDeployer(owner);
			}
			gameObject.SetAlliedLeader<AllyConstructed>(owner);
		}
		gameObject.Brain.PerformEquip(Silent: true);
		if (owner != null)
		{
			Event obj = Event.New("SupplyIntegratedHostWithAmmo", "Host", gameObject, "Owner", owner);
			if (owner.IsPlayer() && !overrideSupply)
			{
				obj.SetParameter("TrackSupply", 1);
			}
			if (weapon != null)
			{
				weapon.FireEvent(obj);
			}
			else
			{
				gameObject.FireEventOnBodyparts(obj);
			}
			if (owner.IsPlayer() && !overrideSupply && obj.HasFlag("AnySupplyHandler") && !obj.HasFlag("AnySupplies"))
			{
				Popup.Show("You have no ammunition to supply " + gameObject.the + gameObject.DisplayNameOnly + " with. " + gameObject.it + " may be ineffective unless stocked.");
			}
		}
		return gameObject;
	}

	public static GameObjectBlueprint FindTurretBlueprint(GameObject weapon)
	{
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (IsBlueprintMatchingTurret(blueprint, weapon))
			{
				return blueprint;
			}
		}
		return null;
	}

	private static bool IsBlueprintMatchingTurret(GameObjectBlueprint BP, GameObject weapon)
	{
		if (!BP.DescendsFrom("TinkerTurret"))
		{
			return false;
		}
		if (BP.Inventory == null)
		{
			return false;
		}
		if (BP.HasTag("WeaponEmplacement"))
		{
			return false;
		}
		bool flag = false;
		foreach (InventoryObject item in BP.Inventory)
		{
			if (item.Blueprint == weapon.Blueprint)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
				continue;
			}
			if (!GameObjectFactory.Factory.Blueprints.TryGetValue(item.Blueprint, out var value))
			{
				return false;
			}
			if (!value.HasPart("MissileWeapon"))
			{
				continue;
			}
			return false;
		}
		return flag;
	}

	public static string GetTurretNameFromWeapon(GameObject weapon)
	{
		string tagOrStringProperty = weapon.GetTagOrStringProperty("TurretName");
		if (!string.IsNullOrEmpty(tagOrStringProperty))
		{
			return tagOrStringProperty;
		}
		string text = GenericTurretNameTweaks(weapon.Render.DisplayName);
		if (!text.Contains("turret"))
		{
			text += " turret";
		}
		return text;
	}

	private static string GenericTurretNameTweaks(string Name)
	{
		return Name.Replace("rifle", "turret").Replace(" gun", "").Replace(" pistol", "")
			.Replace(" pump", "")
			.Replace(" tube", "")
			.Replace(" rack", "");
	}

	public static string GetTurretTileFromWeapon(GameObject weapon)
	{
		MissileWeapon part = weapon.GetPart<MissileWeapon>();
		if (weapon.HasTagOrProperty("Bow"))
		{
			return "Creatures/bow-turret.png";
		}
		if (weapon.HasPart<LiquidAmmoLoader>())
		{
			return "Creatures/sw_flamethrower_turret.png";
		}
		if (weapon.HasPart<EnergyAmmoLoader>())
		{
			if (part != null && part.ShotsPerAction > 1)
			{
				return "Creatures/sw_chainlaser_emplacement.bmp";
			}
			return "Creatures/sw_laser_turret.bmp";
		}
		if (part != null && part.Skill == "HeavyWeapon" && part.ShotsPerAction == 1)
		{
			return "Creatures/sw_rocket_turret.bmp";
		}
		if (part != null && part.ShotsPerAction > 1)
		{
			return "Creatures/sw_chaingun_turret.bmp";
		}
		return "Creatures/sw_rifle_turret.bmp";
	}

	public static int GetTurretAgilityByDeployer(GameObject who)
	{
		if (who.HasSkill("Tinkering_Tinker3"))
		{
			return 34;
		}
		if (who.HasSkill("Tinkering_Tinker2"))
		{
			return 32;
		}
		if (who.HasSkill("Tinkering_Tinker1"))
		{
			return 30;
		}
		return 28;
	}

	[WishCommand(null, null, Regex = "^turret:\\s*(.*?)\\s*$")]
	public static bool HandleTurretWish(Match match)
	{
		string value = match.Groups[1].Value;
		try
		{
			GameObject gameObject = GenerateTurret(GameObject.Create(value));
			(The.Player.CurrentCell.GetFirstEmptyAdjacentCell() ?? The.Player.CurrentCell).AddObject(gameObject);
			gameObject.MakeActive();
		}
		catch (Exception ex)
		{
			Popup.ShowFail("Could not generate turret from blueprint \"" + value + "\"\n\n" + ex.ToString());
		}
		return true;
	}
}
