using System;

namespace XRL.World.Parts;

[Serializable]
public class DiThermoBeam : IPart
{
	public bool Flipped;

	public string HeatName = "R";

	public string ColdName = "C";

	public string HeatColorString = "&R";

	public string ColdColorString = "&C";

	public string HeatDetailColor = "C";

	public string ColdDetailColor = "R";

	public string ProjectileObject = "ProjectileDiThermoBeamCold";

	[NonSerialized]
	public bool Switch;

	[NonSerialized]
	public double LastAngle;

	[NonSerialized]
	public int LastMod = int.MinValue;

	[NonSerialized]
	public GameObjectBlueprint ProjectileBlueprint;

	public override void Attach()
	{
		ProjectileBlueprint = GameObjectFactory.Factory.GetBlueprint(ProjectileObject);
		base.Attach();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == PooledEvent<GetMissileWeaponPerformanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Flip", "flip", "FlipBeam", null, 'f', FireOnActor: false, 5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "FlipBeam")
		{
			FlipBeam(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		E.DamageColor = (Flipped ? "C" : "R");
		return base.HandleEvent(E);
	}

	public void FlipBeam(GameObject Actor)
	{
		Flipped = !Flipped;
		Render render = ParentObject.Render;
		if (Flipped && render.DisplayName == HeatName)
		{
			render.DisplayName = ColdName;
		}
		else if (!Flipped && render.DisplayName == ColdName)
		{
			render.DisplayName = HeatName;
		}
		if (Flipped && render.ColorString == HeatColorString)
		{
			render.ColorString = ColdColorString;
			render.DetailColor = ColdDetailColor;
		}
		else if (!Flipped && render.ColorString == ColdColorString)
		{
			render.ColorString = HeatColorString;
			render.DetailColor = HeatDetailColor;
		}
		if (Actor.IsPlayer())
		{
			PlayWorldSound("Sounds/Interact/sfx_interact_thermobeamGun_setting_flip");
			IComponent<GameObject>.XDidYToZ(Actor, "flip", "the thermal polarity on the", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ModifyMissileWeaponAngle");
		Registrar.Register("ProjectileSetup");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ModifyMissileWeaponAngle")
		{
			if (LastMod == int.MinValue)
			{
				LastAngle = (double)E.GetParameter("Angle");
				LastMod = E.GetIntParameter("Mod");
			}
			else
			{
				E.SetParameter("Angle", LastAngle);
				E.SetParameter("Mod", LastMod + 180);
				LastAngle = 0.0;
				LastMod = int.MinValue;
			}
		}
		else if (E.ID == "ProjectileSetup")
		{
			if (Switch == Flipped)
			{
				foreach (IPart parts in E.GetGameObjectParameter("Projectile").PartsList)
				{
					ProjectileBlueprint.ReinitializePart(parts);
				}
			}
			Switch = !Switch;
		}
		return base.FireEvent(E);
	}
}
