using System;

namespace XRL.World.Parts;

[Serializable]
public class CaverCorpseLoot2 : IPart
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
			if (physics.CurrentCell.ParentZone.X == 1 && physics.currentCell.ParentZone.Y == 1)
			{
				physics.CurrentCell.AddObject(GameObject.Create("Laser Pistol"));
				physics.CurrentCell.AddObject(GameObject.Create("Solar Cell"));
				physics.CurrentCell.AddObject(GameObject.Create("DataDisk"));
				physics.CurrentCell.AddObject(GameObject.Create("Floating Glowsphere"));
				physics.CurrentCell.AddObject(GameObject.Create("Basic Toolkit"));
				physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.Create("Canned Have-It-All"));
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
