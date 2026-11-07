using System;

namespace XRL.World.Parts;

[Serializable]
public class CaverCorpseLoot : IPart
{
	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
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
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			Physics physics = ParentObject.Physics;
			physics.CurrentCell.AddObject(GameObject.Create("Miner's Helmet"));
			physics.CurrentCell.AddObject(GameObject.Create("Pickaxe"));
			physics.CurrentCell.AddObject(GameObject.Create("Sheaf1"));
			physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
			physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
			physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
			physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
			physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
			physics.CurrentCell.AddObject(GameObject.Create("Small Sphere of Negative Weight"));
			physics.CurrentCell.AddObject(GameObject.Create("CyberneticsCreditWedge"));
			ParentObject.UnregisterPartEvent(this, "EnteredCell");
			return true;
		}
		return base.FireEvent(E);
	}
}
