using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_DeployTurret : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public static void Init()
	{
	}

	public override bool AllowStaticRegistration()
	{
		return true;
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
			E.Add("scholarship", 2);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandDeployTurret");
		base.Register(Object, Registrar);
	}

	private bool UsableForTurret(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return false;
		}
		MissileWeapon part = obj.GetPart<MissileWeapon>();
		if (part == null)
		{
			return false;
		}
		if (!part.FiresManually)
		{
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandDeployTurret")
		{
			if (OnWorldMap)
			{
				ParentObject.Fail("You cannot do that on the world map.");
				return false;
			}
			if (!ParentObject.CanMoveExtremities("Deploy Turret", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			List<GameObject> objects = ParentObject.Inventory.GetObjects(UsableForTurret);
			if (objects.Count == 0)
			{
				ParentObject.Fail("You have no missile weapons to deploy.");
				return false;
			}
			GameObject gameObject = Popup.PickGameObject("[Select a weapon to deploy on the turret]\n\n", objects, AllowEscape: true);
			if (gameObject == null)
			{
				return false;
			}
			Cell cell = PickDirection("Deploy " + gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: true));
			if (cell == null)
			{
				return false;
			}
			if (!cell.IsPassable() || cell.HasObjectWithTag("ExcavatoryTerrainFeature"))
			{
				ParentObject.Fail("You can't deploy there!");
				return false;
			}
			gameObject = gameObject.RemoveOne();
			GameObject gameObject2 = IntegratedWeaponHosts.GenerateTurret(gameObject, ParentObject);
			cell.AddObject(gameObject2);
			gameObject2.MakeActive();
			gameObject2.PlayWorldSound("Sounds/Robot/sfx_turret_deploy");
			DidXToY("deploy", gameObject2, null, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
			ParentObject.UseEnergy(2000, "Skill Tinkering DeployTurret");
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Deploy Turret", "CommandDeployTurret", "Tinkering", null, "\u009d");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
